using System.Collections.Generic;
using UnityEngine;

public class OverlayMovimientoCuadro : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Renderer[] renderersObjetivo;

    [Header("Fade")]
    [SerializeField] private float alphaMin = 0f;
    [SerializeField] private float alphaMax = 0.9f;
    [SerializeField] private float velocidadEntrada = 4f;
    [SerializeField] private float velocidadSalida = 5f;

    [Header("Pulso")]
    [SerializeField] private float velocidadPulso = 2f;
    [SerializeField] private float amplitudPulso = 0.25f;

    [Header("Escala opcional")]
    [SerializeField] private bool pulsarEscala = true;
    [SerializeField] private float amplitudEscala = 0.05f;

    [Header("Comportamiento")]
    [SerializeField] private bool iniciarOculto = true;
    [SerializeField] private bool ocultarCuandoTermina = true;
    [SerializeField] private bool desactivarRenderersAlOcultar = true;
    [SerializeField] private bool mostrarLogs = false;

    private readonly List<Material> materiales = new List<Material>();
    private readonly List<int> propiedadesColor = new List<int>();
    private readonly List<Color> coloresBase = new List<Color>();
    private readonly List<Renderer> renderersCache = new List<Renderer>();

    private float alphaActual = 0f;
    private bool activo = false;
    private Vector3 escalaBase;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int TintColorID = Shader.PropertyToID("_TintColor");

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        if (renderersObjetivo == null || renderersObjetivo.Length == 0)
            renderersObjetivo = visualRoot.GetComponentsInChildren<Renderer>(true);

        escalaBase = visualRoot.localScale;

        CachearMateriales();

        alphaActual = iniciarOculto ? alphaMin : alphaMax;
        AplicarAlpha(alphaActual);

        if (iniciarOculto && desactivarRenderersAlOcultar)
            SetRenderersEnabled(false);
    }

    private void Update()
    {
        float alphaObjetivo = activo ? alphaMax : alphaMin;
        float velocidad = activo ? velocidadEntrada : velocidadSalida;

        alphaActual = Mathf.MoveTowards(alphaActual, alphaObjetivo, velocidad * Time.deltaTime);

        float alphaFinal = alphaActual;

        if (activo)
        {
            if (desactivarRenderersAlOcultar)
                SetRenderersEnabled(true);

            float tPulso = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f;
            float multiplicadorPulso = Mathf.Lerp(1f - amplitudPulso, 1f, tPulso);
            alphaFinal *= multiplicadorPulso;

            if (pulsarEscala && visualRoot != null)
            {
                float escalaPulso = 1f + Mathf.Lerp(0f, amplitudEscala, tPulso);
                visualRoot.localScale = escalaBase * escalaPulso;
            }
        }
        else
        {
            if (visualRoot != null)
                visualRoot.localScale = escalaBase;
        }

        AplicarAlpha(alphaFinal);

        if (!activo && ocultarCuandoTermina && alphaActual <= 0.001f)
        {
            if (desactivarRenderersAlOcultar)
                SetRenderersEnabled(false);

            if (mostrarLogs)
                Debug.Log("Ondas ocultas", this);
        }
    }

    public void ActivarOndas()
    {
        activo = true;

        if (desactivarRenderersAlOcultar)
            SetRenderersEnabled(true);

        if (mostrarLogs)
            Debug.Log("Ondas activadas", this);
    }

    public void DesactivarOndas()
    {
        activo = false;

        if (mostrarLogs)
            Debug.Log("Ondas desactivadas", this);
    }

    public void DesactivarOndasInstantaneo()
    {
        activo = false;
        alphaActual = alphaMin;

        AplicarAlpha(alphaMin);

        if (visualRoot != null)
            visualRoot.localScale = escalaBase;

        if (desactivarRenderersAlOcultar)
            SetRenderersEnabled(false);

        if (mostrarLogs)
            Debug.Log("Ondas desactivadas instantáneamente", this);
    }

    public void SetActivo(bool valor)
    {
        if (valor)
            ActivarOndas();
        else
            DesactivarOndas();
    }

    private void CachearMateriales()
    {
        materiales.Clear();
        propiedadesColor.Clear();
        coloresBase.Clear();
        renderersCache.Clear();

        if (renderersObjetivo == null)
            return;

        foreach (Renderer rend in renderersObjetivo)
        {
            if (rend == null)
                continue;

            renderersCache.Add(rend);

            Material[] mats = rend.materials;

            foreach (Material mat in mats)
            {
                if (mat == null)
                    continue;

                int prop = DetectarPropiedadColor(mat);
                if (prop == -1)
                    continue;

                materiales.Add(mat);
                propiedadesColor.Add(prop);
                coloresBase.Add(mat.GetColor(prop));
            }
        }
    }

    private int DetectarPropiedadColor(Material mat)
    {
        if (mat.HasProperty(BaseColorID))
            return BaseColorID;

        if (mat.HasProperty(ColorID))
            return ColorID;

        if (mat.HasProperty(TintColorID))
            return TintColorID;

        return -1;
    }

    private void AplicarAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        for (int i = 0; i < materiales.Count; i++)
        {
            Material mat = materiales[i];
            int prop = propiedadesColor[i];
            Color baseColor = coloresBase[i];

            Color c = baseColor;
            c.a = alpha;
            mat.SetColor(prop, c);
        }
    }

    private void SetRenderersEnabled(bool enabledState)
    {
        for (int i = 0; i < renderersCache.Count; i++)
        {
            if (renderersCache[i] != null)
                renderersCache[i].enabled = enabledState;
        }
    }
}