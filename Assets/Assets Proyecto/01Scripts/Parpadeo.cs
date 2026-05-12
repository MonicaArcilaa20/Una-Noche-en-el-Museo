using UnityEngine;
using UnityEngine.UI;

public class ParpadeoAtencion : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private GameObject objetivoVisual;

    [Header("Fade")]
    [SerializeField] private bool autoIniciar = true;
    [SerializeField, Range(0f, 1f)] private float alphaMin = 0.15f;
    [SerializeField, Range(0f, 1f)] private float alphaMax = 0.85f;
    [SerializeField] private float velocidadEntrada = 1.5f;
    [SerializeField] private float velocidadSalida = 1.5f;

    [Header("Luz opcional")]
    [SerializeField] private bool afectarLuces = true;
    [SerializeField] private float multiplicadorIntensidadLuz = 1f;

    private Renderer[] renderers3D;
    private SpriteRenderer[] spriteRenderers;
    private Graphic[] uiGraphics;
    private Light[] luces;

    private Material[][] materialesRenderer;
    private Color[][] coloresOriginalesRenderer;
    private Color[] coloresOriginalesSprite;
    private Color[] coloresOriginalesUI;
    private float[] intensidadesOriginalesLuces;

    private bool activo = false;
    private bool subiendo = true;
    private float alphaActual;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (objetivoVisual == null)
            objetivoVisual = gameObject;

        renderers3D = objetivoVisual.GetComponentsInChildren<Renderer>(true);
        spriteRenderers = objetivoVisual.GetComponentsInChildren<SpriteRenderer>(true);
        uiGraphics = objetivoVisual.GetComponentsInChildren<Graphic>(true);
        luces = objetivoVisual.GetComponentsInChildren<Light>(true);

        GuardarReferenciasOriginales();

        alphaActual = alphaMax;
        AplicarAlpha(alphaActual);
    }

    private void OnEnable()
    {
        if (autoIniciar)
            ActivarParpadeo();
        else
            AplicarAlpha(alphaMax);
    }

    private void Update()
    {
        if (!activo)
            return;

        float velocidad = subiendo ? velocidadEntrada : velocidadSalida;
        float objetivo = subiendo ? alphaMax : alphaMin;

        alphaActual = Mathf.MoveTowards(alphaActual, objetivo, velocidad * Time.deltaTime);
        AplicarAlpha(alphaActual);

        if (Mathf.Approximately(alphaActual, objetivo))
            subiendo = !subiendo;
    }

    public void ActivarParpadeo()
    {
        activo = true;
        subiendo = false;
        alphaActual = alphaMax;
        AplicarAlpha(alphaActual);
    }

    public void DetenerParpadeo(bool dejarVisible = true)
    {
        activo = false;
        alphaActual = dejarVisible ? alphaMax : 0f;
        AplicarAlpha(alphaActual);
    }

    private void GuardarReferenciasOriginales()
    {
        materialesRenderer = new Material[renderers3D.Length][];
        coloresOriginalesRenderer = new Color[renderers3D.Length][];

        for (int i = 0; i < renderers3D.Length; i++)
        {
            if (renderers3D[i] == null)
                continue;

            materialesRenderer[i] = renderers3D[i].materials;
            coloresOriginalesRenderer[i] = new Color[materialesRenderer[i].Length];

            for (int j = 0; j < materialesRenderer[i].Length; j++)
            {
                Material mat = materialesRenderer[i][j];
                coloresOriginalesRenderer[i][j] = ObtenerColorMaterial(mat);
            }
        }

        coloresOriginalesSprite = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                coloresOriginalesSprite[i] = spriteRenderers[i].color;
        }

        coloresOriginalesUI = new Color[uiGraphics.Length];
        for (int i = 0; i < uiGraphics.Length; i++)
        {
            if (uiGraphics[i] != null)
                coloresOriginalesUI[i] = uiGraphics[i].color;
        }

        intensidadesOriginalesLuces = new float[luces.Length];
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null)
                intensidadesOriginalesLuces[i] = luces[i].intensity;
        }
    }

    private void AplicarAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        for (int i = 0; i < renderers3D.Length; i++)
        {
            if (renderers3D[i] == null || materialesRenderer[i] == null)
                continue;

            for (int j = 0; j < materialesRenderer[i].Length; j++)
            {
                Material mat = materialesRenderer[i][j];
                if (mat == null)
                    continue;

                Color baseColor = coloresOriginalesRenderer[i][j];
                baseColor.a = alpha;
                AsignarColorMaterial(mat, baseColor);
            }
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
                continue;

            Color c = coloresOriginalesSprite[i];
            c.a = alpha;
            spriteRenderers[i].color = c;
        }

        for (int i = 0; i < uiGraphics.Length; i++)
        {
            if (uiGraphics[i] == null)
                continue;

            Color c = coloresOriginalesUI[i];
            c.a = alpha;
            uiGraphics[i].color = c;
        }

        if (afectarLuces)
        {
            for (int i = 0; i < luces.Length; i++)
            {
                if (luces[i] == null)
                    continue;

                luces[i].intensity = intensidadesOriginalesLuces[i] * alpha * multiplicadorIntensidadLuz;
            }
        }
    }

    private Color ObtenerColorMaterial(Material mat)
    {
        if (mat == null)
            return Color.white;

        if (mat.HasProperty(BaseColorID))
            return mat.GetColor(BaseColorID);

        if (mat.HasProperty(ColorID))
            return mat.GetColor(ColorID);

        return Color.white;
    }

    private void AsignarColorMaterial(Material mat, Color color)
    {
        if (mat == null)
            return;

        if (mat.HasProperty(BaseColorID))
            mat.SetColor(BaseColorID, color);
        else if (mat.HasProperty(ColorID))
            mat.SetColor(ColorID, color);
    }
}