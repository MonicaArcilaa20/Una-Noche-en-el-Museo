using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventoTintaFloat : UnityEvent<float> { }

[RequireComponent(typeof(ControlPincel))]
public class PincelTinta : MonoBehaviour
{
    [Header("Tinta")]
    [SerializeField] private float tintaMaxima = 100f;
    [SerializeField] private float tintaActual = 100f;
    [SerializeField] private float consumoPorSegundo = 10f;

    [Header("Opciones")]
    [SerializeField] private bool consumirSoloCuandoEstaActivo = true;
    [SerializeField] private bool mostrarLogs = true;

    [Header("Visual simple")]
    [SerializeField] private GameObject puntaConPintura;
    [SerializeField] private float umbralVisual = 0.01f;

    [Header("Debug visual")]
    [SerializeField] private bool debugVisual = false;

    [Header("Eventos")]
    public UnityEvent alQuedarseSinTinta;
    public UnityEvent alRecargar;
    public EventoTintaFloat alCambiarTintaNormalizada;

    private ControlPincel controlPincel;
    private PincelPersistenciaEscena persistencia;

    public float TintaMaxima => tintaMaxima;
    public float TintaActual => tintaActual;
    public float TintaNormalizada => tintaMaxima <= 0f ? 0f : tintaActual / tintaMaxima;

    private void Awake()
    {
        controlPincel = GetComponent<ControlPincel>();
        persistencia = GetComponent<PincelPersistenciaEscena>();

        tintaMaxima = Mathf.Max(1f, tintaMaxima);
        tintaActual = Mathf.Clamp(tintaActual, 0f, tintaMaxima);

        if (debugVisual)
        {
            Debug.Log("[PincelTinta] Awake en " + gameObject.name, this);

            if (puntaConPintura != null)
                Debug.Log("[PincelTinta] puntaConPintura asignada: " + puntaConPintura.name, this);
            else
                Debug.Log("[PincelTinta] puntaConPintura NO asignada", this);
        }
    }

    private void Start()
    {
        NotificarCambio();
        ActualizarVisual();
    }

    private void Update()
    {
        if (!consumirSoloCuandoEstaActivo)
            return;

        if (controlPincel == null)
            return;

        if (!controlPincel.EstaActivo)
            return;

        Gastar(consumoPorSegundo * Time.deltaTime);
    }

    public bool TieneTinta(float cantidadMinima = 0.01f)
    {
        return tintaActual >= cantidadMinima;
    }

    public bool EstaLlena()
    {
        return tintaActual >= tintaMaxima;
    }

    public void Gastar(float cantidad)
    {
        if (cantidad <= 0f)
            return;

        if (tintaActual <= 0f)
            return;

        tintaActual -= cantidad;
        tintaActual = Mathf.Clamp(tintaActual, 0f, tintaMaxima);

        NotificarCambio();
        ActualizarVisual();
        persistencia?.GuardarAhora();

        if (tintaActual <= 0f)
        {
            tintaActual = 0f;

            if (controlPincel != null)
                controlPincel.ApagarPincel();

            if (mostrarLogs)
                Debug.Log("El pincel se quedó sin tinta");

            alQuedarseSinTinta?.Invoke();
        }
    }

    public void Recargar(float cantidad)
    {
        if (cantidad <= 0f)
            return;

        float tintaAntes = tintaActual;

        tintaActual += cantidad;
        tintaActual = Mathf.Clamp(tintaActual, 0f, tintaMaxima);

        if (!Mathf.Approximately(tintaActual, tintaAntes))
        {
            if (mostrarLogs)
                Debug.Log("Tinta recargada: " + tintaActual + " / " + tintaMaxima);

            alRecargar?.Invoke();
            NotificarCambio();
            ActualizarVisual();
            persistencia?.GuardarAhora();
        }
    }

    public void RellenarCompleta()
    {
        tintaActual = tintaMaxima;

        if (mostrarLogs)
            Debug.Log("Tinta rellenada al máximo");

        alRecargar?.Invoke();
        NotificarCambio();
        ActualizarVisual();
        persistencia?.GuardarAhora();
    }

    public void FijarTinta(float valor)
    {
        tintaActual = Mathf.Clamp(valor, 0f, tintaMaxima);
        NotificarCambio();
        ActualizarVisual();
    }

    public void FijarEstado(float actual, float maxima)
    {
        tintaMaxima = Mathf.Max(1f, maxima);
        tintaActual = Mathf.Clamp(actual, 0f, tintaMaxima);

        NotificarCambio();
        ActualizarVisual();
    }

    private void NotificarCambio()
    {
        alCambiarTintaNormalizada?.Invoke(TintaNormalizada);
    }

    private void ActualizarVisual()
    {
        if (puntaConPintura == null)
            return;

        bool mostrar = TintaNormalizada > umbralVisual;
        puntaConPintura.SetActive(mostrar);

        if (debugVisual)
        {
            Debug.Log(
                $"[PincelTinta] ActualizarVisual -> tintaNormalizada={TintaNormalizada:F3}, mostrar={mostrar}, activeSelf={puntaConPintura.activeSelf}, activeInHierarchy={puntaConPintura.activeInHierarchy}",
                puntaConPintura
            );
        }
    }
}