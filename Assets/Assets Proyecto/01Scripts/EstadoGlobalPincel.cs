using UnityEngine;

public class EstadoGlobalPincel : MonoBehaviour
{
    public static EstadoGlobalPincel Instance { get; private set; }

    [Header("Estado global")]
    [SerializeField] private bool pincelAdquirido = false;
    [SerializeField] private bool pincelActivo = false;
    [SerializeField] private float tintaActual = 100f;
    [SerializeField] private float tintaMaxima = 100f;

    [Header("Valores por defecto")]
    [SerializeField] private float tintaActualPorDefecto = 100f;
    [SerializeField] private float tintaMaximaPorDefecto = 100f;

    public bool PincelAdquirido => pincelAdquirido;
    public bool PincelActivo => pincelActivo;
    public float TintaActual => tintaActual;
    public float TintaMaxima => tintaMaxima;

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

    public void GuardarEstado(bool adquirido, bool activo, float tintaActualNueva, float tintaMaximaNueva)
    {
        pincelAdquirido = adquirido;
        pincelActivo = activo;
        tintaMaxima = Mathf.Max(1f, tintaMaximaNueva);
        tintaActual = Mathf.Clamp(tintaActualNueva, 0f, tintaMaxima);
    }

    public void MarcarPincelAdquirido()
    {
        pincelAdquirido = true;
    }

    public void ResetearEstadoTutorial()
    {
        pincelAdquirido = false;
        pincelActivo = false;
        tintaMaxima = Mathf.Max(1f, tintaMaximaPorDefecto);
        tintaActual = Mathf.Clamp(tintaActualPorDefecto, 0f, tintaMaxima);

        Debug.Log("[EstadoGlobalPincel] Estado reseteado al salir del tutorial.");
    }
}