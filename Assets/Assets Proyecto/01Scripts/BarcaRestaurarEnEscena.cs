using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BarcaRestaurarEnEscena : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BarcaEmbarqueXR embarqueBarca;

    [Header("Configuración")]
    [SerializeField] private string idTramoActual = "Cuevas";
    [SerializeField] private int framesEspera = 8;
    [SerializeField] private float esperaExtraSegundos = 0.15f;
    [SerializeField] private bool autoEmbarcarSiNoHayEstadoGuardado = true;
    [SerializeField] private bool desactivarTriggerTrasEmbarcar = true;

    [Header("Corrección inicio directo")]
    [SerializeField] private bool reaplicarAsientoEnInicioDirecto = true;
    [SerializeField] private int framesEsperaReaplicacion = 2;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = true;

    private IEnumerator Start()
    {
        for (int i = 0; i < framesEspera; i++)
            yield return null;

        if (esperaExtraSegundos > 0f)
            yield return new WaitForSecondsRealtime(esperaExtraSegundos);

        yield return new WaitForEndOfFrame();

        if (embarqueBarca == null)
        {
            Debug.LogWarning("[BarcaRestaurarEnEscena] Falta asignar embarqueBarca.", this);
            yield break;
        }

        string escenaActual = SceneManager.GetActiveScene().name;

        bool vieneEnBarca = false;

        if (EstadoGlobalBarca.Instance != null)
            vieneEnBarca = EstadoGlobalBarca.Instance.DebeRestaurarseEnEscena(escenaActual);

        bool debeEmbarcar = vieneEnBarca || autoEmbarcarSiNoHayEstadoGuardado;

        if (!debeEmbarcar)
        {
            if (mostrarLogs)
                Debug.Log("[BarcaRestaurarEnEscena] No se autoembarca en esta escena.", this);
            yield break;
        }

        embarqueBarca.Embarcar();

        if (desactivarTriggerTrasEmbarcar)
            embarqueBarca.ActivarTriggerEmbarque(false);

        if (EstadoGlobalBarca.Instance != null)
        {
            if (vieneEnBarca)
                EstadoGlobalBarca.Instance.ConfirmarLlegadaEscenaBarca(idTramoActual);
            else
                EstadoGlobalBarca.Instance.MarcarAbordo(idTramoActual);
        }

        // Recolocación extra solo para el caso de inicio directo
        if (!vieneEnBarca && reaplicarAsientoEnInicioDirecto)
        {
            for (int i = 0; i < framesEsperaReaplicacion; i++)
                yield return null;

            yield return new WaitForEndOfFrame();
            embarqueBarca.RecolocarEnAsiento();

            if (mostrarLogs)
                Debug.Log("[BarcaRestaurarEnEscena] Reaplicando asiento por inicio directo en escena.", this);
        }

        if (mostrarLogs)
        {
            if (vieneEnBarca)
                Debug.Log("[BarcaRestaurarEnEscena] Jugador restaurado en barca desde transición.", this);
            else
                Debug.Log("[BarcaRestaurarEnEscena] Autoembarque por inicio directo en escena.", this);
        }
    }
}