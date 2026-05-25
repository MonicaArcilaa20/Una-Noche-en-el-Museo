using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PinturaVivaController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Renderer objetivoRenderer;

    [Header("Textura")]
    [SerializeField] private string propiedadTextura = "_BaseMap";
    [SerializeField] private Vector2 velocidadUVReposo = Vector2.zero;
    [SerializeField] private Vector2 velocidadUVActiva = new Vector2(0.03f, 0.015f);
    [SerializeField] private bool resetearOffsetAlApagar = false;

    [Header("Color / Alpha")]
    [SerializeField] private string propiedadColor = "_BaseColor";
    [SerializeField] private float alphaReposo = 0f;
    [SerializeField] private float alphaActivo = 1f;

    [Header("Escala")]
    [SerializeField] private bool usarPulsoEscala = true;
    [SerializeField] private float multiplicadorEscalaReposo = 1f;
    [SerializeField] private float multiplicadorEscalaActiva = 1f;
    [SerializeField] private float amplitudPulsoEscala = 0.03f;

    [Header("Pulso visual")]
    [SerializeField] private bool usarPulso = true;
    [SerializeField] private float amplitudPulsoAlpha = 0.12f;
    [SerializeField] private float velocidadPulso = 2f;

    [Header("Suavizado")]
    [SerializeField] private float velocidadSuavizado = 4f;
    [SerializeField] private bool iniciarActiva = false;

    [Header("Ocultación")]
    [SerializeField] private bool ocultarRendererAlFinalDelFadeOut = true;
    [SerializeField] private float umbralOcultacion = 0.02f;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = false;

    private Material materialInstancia;
    private int idTextura;
    private int idColor;
    private bool tienePropiedadTextura;
    private bool tienePropiedadColor;

    private Vector2 offsetBase;
    private Vector2 offsetActual;
    private Color colorBase = Color.white;

    private float alphaActual;
    private float alphaObjetivo;
    private float factorMovimientoActual;
    private float factorMovimientoObjetivo;

    private bool estaActiva;
    private Coroutine corutinaOcultacion;

    private Vector3 escalaInicialLocal;

    public bool EstaActiva => estaActiva;

    private void Reset()
    {
        if (objetivoRenderer == null)
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

        escalaInicialLocal = transform.localScale;

        materialInstancia = objetivoRenderer.material;

        idTextura = Shader.PropertyToID(propiedadTextura);
        idColor = Shader.PropertyToID(propiedadColor);

        tienePropiedadTextura = materialInstancia != null && materialInstancia.HasProperty(idTextura);
        tienePropiedadColor = materialInstancia != null && materialInstancia.HasProperty(idColor);

        if (tienePropiedadTextura)
            offsetBase = materialInstancia.GetTextureOffset(idTextura);

        offsetActual = offsetBase;

        if (tienePropiedadColor)
            colorBase = materialInstancia.GetColor(idColor);

        estaActiva = iniciarActiva;
        alphaActual = iniciarActiva ? alphaActivo : alphaReposo;
        alphaObjetivo = alphaActual;
        factorMovimientoActual = iniciarActiva ? 1f : 0f;
        factorMovimientoObjetivo = factorMovimientoActual;

        objetivoRenderer.enabled = iniciarActiva || !ocultarRendererAlFinalDelFadeOut;

        AplicarColor(alphaActual);
        AplicarEscala(1f);

        if (mostrarLogs)
            Debug.Log("[PinturaVivaController] Escala inicial capturada: " + escalaInicialLocal, this);
    }

    private void Update()
    {
        float t = Time.deltaTime * velocidadSuavizado;

        alphaActual = Mathf.Lerp(alphaActual, alphaObjetivo, t);
        factorMovimientoActual = Mathf.Lerp(factorMovimientoActual, factorMovimientoObjetivo, t);

        float pulso = 0f;
        if (usarPulso && estaActiva)
            pulso = Mathf.Sin(Time.time * velocidadPulso);

        ActualizarUV(factorMovimientoActual, pulso);

        float alphaFinal = alphaActual;
        if (usarPulso && estaActiva)
            alphaFinal += pulso * amplitudPulsoAlpha;

        alphaFinal = Mathf.Clamp01(alphaFinal);
        AplicarColor(alphaFinal);

        float factorEscala = 1f;
        if (usarPulsoEscala && estaActiva)
            factorEscala += pulso * amplitudPulsoEscala;

        AplicarEscala(factorEscala);

        if (ocultarRendererAlFinalDelFadeOut && !estaActiva && alphaActual <= umbralOcultacion)
            objetivoRenderer.enabled = false;
    }

    public void Activar()
    {
        if (corutinaOcultacion != null)
        {
            StopCoroutine(corutinaOcultacion);
            corutinaOcultacion = null;
        }

        estaActiva = true;
        alphaObjetivo = alphaActivo;
        factorMovimientoObjetivo = 1f;
        objetivoRenderer.enabled = true;

        if (mostrarLogs)
            Debug.Log("[PinturaVivaController] Activar()", this);
    }

    public void Desactivar()
    {
        estaActiva = false;
        alphaObjetivo = alphaReposo;
        factorMovimientoObjetivo = 0f;

        if (ocultarRendererAlFinalDelFadeOut)
        {
            if (corutinaOcultacion != null)
                StopCoroutine(corutinaOcultacion);

            corutinaOcultacion = StartCoroutine(OcultarAlFinalDelFade());
        }

        if (mostrarLogs)
            Debug.Log("[PinturaVivaController] Desactivar()", this);
    }

    public void SetActiva(bool activa)
    {
        if (activa) Activar();
        else Desactivar();
    }

    public void SetActivo(bool activo)
    {
        SetActiva(activo);
    }

    public void ActivarOndas()
    {
        Activar();
    }

    public void DesactivarOndas()
    {
        Desactivar();
    }

    public void AplicarInstantaneoActivo()
    {
        if (corutinaOcultacion != null)
        {
            StopCoroutine(corutinaOcultacion);
            corutinaOcultacion = null;
        }

        estaActiva = true;
        alphaActual = alphaActivo;
        alphaObjetivo = alphaActivo;
        factorMovimientoActual = 1f;
        factorMovimientoObjetivo = 1f;
        objetivoRenderer.enabled = true;

        AplicarColor(alphaActual);
        AplicarEscala(1f);
    }

    public void AplicarInstantaneoReposo()
    {
        estaActiva = false;
        alphaActual = alphaReposo;
        alphaObjetivo = alphaReposo;
        factorMovimientoActual = 0f;
        factorMovimientoObjetivo = 0f;

        if (resetearOffsetAlApagar && tienePropiedadTextura)
        {
            offsetActual = offsetBase;
            materialInstancia.SetTextureOffset(idTextura, offsetActual);
        }

        AplicarColor(alphaActual);
        AplicarEscala(1f);

        if (ocultarRendererAlFinalDelFadeOut)
            objetivoRenderer.enabled = false;
    }

    private void ActualizarUV(float factorMovimiento, float pulso)
    {
        if (materialInstancia == null || !tienePropiedadTextura || !objetivoRenderer.enabled)
            return;

        Vector2 velocidad = Vector2.Lerp(velocidadUVReposo, velocidadUVActiva, factorMovimiento);

        if (usarPulso && estaActiva)
            velocidad *= 1f + (pulso * 0.25f);

        offsetActual += velocidad * Time.deltaTime;
        materialInstancia.SetTextureOffset(idTextura, offsetActual);
    }

    private void AplicarColor(float alpha)
    {
        if (materialInstancia == null || !tienePropiedadColor)
            return;

        Color c = colorBase;
        c.a = alpha;
        materialInstancia.SetColor(idColor, c);
    }

    private void AplicarEscala(float factorPulso)
    {
        float multiplicadorBase = estaActiva ? multiplicadorEscalaActiva : multiplicadorEscalaReposo;
        Vector3 escalaBase = escalaInicialLocal * multiplicadorBase;

        if (!usarPulsoEscala)
        {
            transform.localScale = escalaBase;
            return;
        }

        transform.localScale = escalaBase * factorPulso;
    }

    private IEnumerator OcultarAlFinalDelFade()
    {
        while (!estaActiva && alphaActual > umbralOcultacion)
            yield return null;

        if (!estaActiva)
            objetivoRenderer.enabled = false;

        corutinaOcultacion = null;
    }

    private void OnDestroy()
    {
        if (materialInstancia != null)
            Destroy(materialInstancia);
    }
}