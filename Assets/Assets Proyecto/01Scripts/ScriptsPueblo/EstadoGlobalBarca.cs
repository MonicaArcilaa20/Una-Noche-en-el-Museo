using UnityEngine;

public class EstadoGlobalBarca : MonoBehaviour
{
    public static EstadoGlobalBarca Instance { get; private set; }

    [Header("Estado")]
    [SerializeField] private bool jugadorEnBarca = false;
    [SerializeField] private string tramoActual = "";
    [SerializeField] private string siguienteEscenaBarca = "";
    [SerializeField] private float progresoTramo = 0f;

    public bool JugadorEnBarca => jugadorEnBarca;
    public string TramoActual => tramoActual;
    public string SiguienteEscenaBarca => siguienteEscenaBarca;
    public float ProgresoTramo => progresoTramo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarcarAbordo(string tramo)
    {
        jugadorEnBarca = true;
        tramoActual = tramo;
    }

    public void GuardarProgreso(string tramo, float progreso)
    {
        tramoActual = tramo;
        progresoTramo = Mathf.Clamp01(progreso);
    }

    public void PrepararCambioEscenaEnBarca(string nombreEscenaDestino, string tramo, float progreso)
    {
        jugadorEnBarca = true;
        siguienteEscenaBarca = nombreEscenaDestino;
        tramoActual = tramo;
        progresoTramo = Mathf.Clamp01(progreso);
    }

    public bool DebeRestaurarseEnEscena(string nombreEscenaActual)
    {
        if (!jugadorEnBarca)
            return false;

        if (string.IsNullOrEmpty(siguienteEscenaBarca))
            return false;

        return siguienteEscenaBarca == nombreEscenaActual;
    }

    public void ConfirmarLlegadaEscenaBarca(string nuevoTramo)
    {
        if (!jugadorEnBarca)
            return;

        tramoActual = nuevoTramo;
        siguienteEscenaBarca = "";
    }

    public void BajarDeBarca()
    {
        jugadorEnBarca = false;
        siguienteEscenaBarca = "";
        tramoActual = "";
        progresoTramo = 0f;
    }
}