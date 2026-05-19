using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PinturaVivaController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Renderer objetivoRenderer;

    [Header("Shader")]
    [SerializeField] private string propiedadIntensidad = "_VidaIntensidad";
    [SerializeField] private string propiedadBrillo = "_VidaBrillo";
    [SerializeField] private bool usarBrillo = false;

    [Header("Estados")]
    [SerializeField] private float intensidadReposo = 0f;
    [SerializeField] private float intensidadActiva = 1f;
    [SerializeField] private float brilloReposo = 0f;
    [SerializeField] private float brilloActivo = 1f;

    [Header("Suavizado")]
    [SerializeField] private float velocidadSuavizado = 3f;
    [SerializeField] private bool iniciarActiva = false;

    [Header("Pulso opcional")]
    [SerializeField] private bool usarPulso = false;
    [SerializeField] private float amplitudPulso = 0.08f;
    [SerializeField] private float velocidadPulso = 2f;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = false;

    private Material materialInstancia;
    private float intensidadActual;
    private float intensidadObjetivo;
    private float brilloActual;
    private float brilloObjetivo;
    private bool estaActiva;

    private int idIntensidad;
    private int idBrillo;

    public bool EstaActiva => estaActiva;
    public float IntensidadActual => intensidadActual;

    private void Reset()
    {
        objetivoRenderer = GetComponent<Renderer>();
    }

    private void Awake()
    {
        if (objetivoRenderer == null)
            objetivoRenderer = GetComponent<Renderer>();

        if (objetivoRenderer == null)
        {
            Debug.LogWarning("[PinturaVivaController] No se encontró Renderer.", this);
            enabled = false;
            return;
        }

        materialInstancia = objetivoRenderer.material;

        idIntensidad = Shader.PropertyToID(propiedadIntensidad);
        idBrillo = Shader.PropertyToID(propiedadBrillo);

        estaActiva = iniciarActiva;
        intensidadActual = iniciarActiva ? intensidadActiva : intensidadReposo;
        intensidadObjetivo = intensidadActual;

        brilloActual = iniciarActiva ? brilloActivo : brilloReposo;
        brilloObjetivo = brilloActual;

        AplicarValores();
    }

    private void Update()
    {
        float t = Time.deltaTime * velocidadSuavizado;

        intensidadActual = Mathf.Lerp(intensidadActual, intensidadObjetivo, t);
        brilloActual = Mathf.Lerp(brilloActual, brilloObjetivo, t);

        float intensidadFinal = intensidadActual;

        if (usarPulso && estaActiva)
            intensidadFinal += Mathf.Sin(Time.time * velocidadPulso) * amplitudPulso;

        intensidadFinal = Mathf.Max(0f, intensidadFinal);

        AplicarValores(intensidadFinal, brilloActual);
    }

    public void Activar()
    {
        estaActiva = true;
        intensidadObjetivo = intensidadActiva;
        brilloObjetivo = brilloActivo;

        if (mostrarLogs)
            Debug.Log("[PinturaVivaController] Pintura activada.", this);
    }

    public void Desactivar()
    {
        estaActiva = false;
        intensidadObjetivo = intensidadReposo;
        brilloObjetivo = brilloReposo;

        if (mostrarLogs)
            Debug.Log("[PinturaVivaController] Pintura desactivada.", this);
    }

    public void SetActiva(bool activa)
    {
        if (activa)
            Activar();
        else
            Desactivar();
    }

    // Compatibilidad con scripts viejos
    public void SetActivo(bool activo)
    {
        SetActiva(activo);
    }

    // Compatibilidad con CuadroActivablePorApunte
    public void DesactivarOndas()
    {
        Desactivar();
    }

    public void ActivarOndas()
    {
        Activar();
    }

    public void SetIntensidadManual(float valorNormalizado)
    {
        valorNormalizado = Mathf.Clamp01(valorNormalizado);

        estaActiva = valorNormalizado > 0.001f;
        intensidadObjetivo = Mathf.Lerp(intensidadReposo, intensidadActiva, valorNormalizado);
        brilloObjetivo = Mathf.Lerp(brilloReposo, brilloActivo, valorNormalizado);

        if (mostrarLogs)
            Debug.Log("[PinturaVivaController] Intensidad manual: " + valorNormalizado, this);
    }

    public void AplicarInstantaneoActivo()
    {
        estaActiva = true;
        intensidadActual = intensidadActiva;
        intensidadObjetivo = intensidadActiva;
        brilloActual = brilloActivo;
        brilloObjetivo = brilloActivo;
        AplicarValores();
    }

    public void AplicarInstantaneoReposo()
    {
        estaActiva = false;
        intensidadActual = intensidadReposo;
        intensidadObjetivo = intensidadReposo;
        brilloActual = brilloReposo;
        brilloObjetivo = brilloReposo;
        AplicarValores();
    }

    private void AplicarValores()
    {
        AplicarValores(intensidadActual, brilloActual);
    }

    private void AplicarValores(float intensidad, float brillo)
    {
        if (materialInstancia == null)
            return;

        if (materialInstancia.HasProperty(idIntensidad))
            materialInstancia.SetFloat(idIntensidad, intensidad);

        if (usarBrillo && materialInstancia.HasProperty(idBrillo))
            materialInstancia.SetFloat(idBrillo, brillo);
    }

    private void OnDestroy()
    {
        if (materialInstancia != null)
            Destroy(materialInstancia);
    }
}