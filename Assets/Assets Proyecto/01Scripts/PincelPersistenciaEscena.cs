using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ControlPincel))]
[RequireComponent(typeof(PincelTinta))]
public class PincelPersistenciaEscena : MonoBehaviour
{
    [Header("Opciones")]
    [SerializeField] private bool autoEquiparSiYaExiste = true;
    [SerializeField] private bool restaurarEstadoActivo = true;
    [SerializeField] private bool mostrarLogs = true;
    [SerializeField] private float retrasoRestauracion = 0.15f;

    private ControlPincel controlPincel;
    private PincelTinta sistemaTinta;

    private void Awake()
    {
        controlPincel = GetComponent<ControlPincel>();
        sistemaTinta = GetComponent<PincelTinta>();

        if (mostrarLogs)
            Debug.Log("[Persistencia] Awake en: " + gameObject.name);
    }

    private IEnumerator Start()
    {
        if (mostrarLogs)
            Debug.Log("[Persistencia] Esperando para restaurar...");

        yield return null;
        yield return new WaitForSeconds(retrasoRestauracion);

        RestaurarDesdeEstadoGlobal();
    }

    private void RestaurarDesdeEstadoGlobal()
    {
        if (EstadoGlobalPincel.Instance == null)
        {
            Debug.LogWarning("[Persistencia] No hay EstadoGlobalPincel.Instance");
            return;
        }

        var estado = EstadoGlobalPincel.Instance;

        if (mostrarLogs)
        {
            Debug.Log("[Persistencia] Restaurando. Adquirido=" + estado.PincelAdquirido +
                      " Activo=" + estado.PincelActivo +
                      " Tinta=" + estado.TintaActual + "/" + estado.TintaMaxima);
        }

        sistemaTinta.FijarEstado(estado.TintaActual, estado.TintaMaxima);

        if (estado.PincelAdquirido && autoEquiparSiYaExiste)
        {
            controlPincel.EquiparEnManoDerecha();

            if (mostrarLogs)
                Debug.Log("[Persistencia] Pincel autoequipado.");
        }

        if (restaurarEstadoActivo)
        {
            if (estado.PincelActivo && sistemaTinta.TieneTinta())
            {
                controlPincel.EncenderPincel();

                if (mostrarLogs)
                    Debug.Log("[Persistencia] Estado activo restaurado.");
            }
            else
            {
                controlPincel.ApagarPincel();
            }
        }
    }

    public void GuardarAhora()
    {
        if (EstadoGlobalPincel.Instance == null)
            return;

        EstadoGlobalPincel.Instance.GuardarEstado(
            controlPincel.EstaEquipado,
            controlPincel.EstaActivo,
            sistemaTinta.TintaActual,
            sistemaTinta.TintaMaxima
        );

        if (mostrarLogs)
        {
            Debug.Log("[Persistencia] Guardando. Adquirido=" + controlPincel.EstaEquipado +
                      " Activo=" + controlPincel.EstaActivo +
                      " Tinta=" + sistemaTinta.TintaActual + "/" + sistemaTinta.TintaMaxima);
        }
    }
}