using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider))]
public class FogZoneReactiva : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BrushLightMode brushLightMode;
    [SerializeField] private Transform puntoReaccion;
    [SerializeField] private Renderer[] renderersNiebla;
    [SerializeField] private ParticleSystem[] particulasNiebla;
    [SerializeField] private Volume volumenObjetivo;

    [Header("Detección del jugador")]
    [SerializeField] private string tagJugador = "Player";

    [Header("Detección de luz")]
    [SerializeField] private float distanciaMaxima = 16f;
    [SerializeField] private float radioReaccion = 2.2f;
    [SerializeField] private bool usarLineaDeVision = false;
    [SerializeField] private LayerMask mascaraBloqueo = ~0;

    [Header("Respuesta visual")]
    [SerializeField] private float alphaNieblaReposo = 1f;
    [SerializeField] private float alphaNieblaDisipada = 0.2f;
    [SerializeField] private float pesoVolumenReposo = 1f;
    [SerializeField] private float pesoVolumenDisipado = 0.25f;
    [SerializeField] private float velocidadCambio = 2.5f;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = true;
    [SerializeField] private bool dibujarDebug = true;

    private Collider triggerZona;
    private bool jugadorDentro = false;
    private float tActual = 0f;
    private MaterialPropertyBlock block;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        triggerZona = GetComponent<Collider>();
        triggerZona.isTrigger = true;
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        bool disipando = DebeDisiparse();

        float objetivo = disipando ? 1f : 0f;
        tActual = Mathf.MoveTowards(tActual, objetivo, velocidadCambio * Time.deltaTime);

        AplicarVisual(tActual);
    }

    private bool DebeDisiparse()
    {
        if (!jugadorDentro)
            return false;

        if (brushLightMode == null)
            return false;

        if (puntoReaccion == null)
            return false;

        if (!brushLightMode.EstaIluminando)
            return false;

        Transform origen = brushLightMode.PuntoLuzActual;
        if (origen == null)
            return false;

        Vector3 origenPos = origen.position;
        Vector3 dir = origen.forward.normalized;
        Vector3 haciaPunto = puntoReaccion.position - origenPos;

        float avanceSobreHaz = Vector3.Dot(haciaPunto, dir);

        if (avanceSobreHaz < 0f || avanceSobreHaz > distanciaMaxima)
            return false;

        Vector3 puntoMasCercanoEnHaz = origenPos + dir * avanceSobreHaz;
        float distanciaLateral = Vector3.Distance(puntoMasCercanoEnHaz, puntoReaccion.position);

        if (dibujarDebug)
        {
            Debug.DrawLine(origenPos, puntoMasCercanoEnHaz, Color.cyan);
            Debug.DrawLine(puntoMasCercanoEnHaz, puntoReaccion.position, Color.yellow);
        }

        if (distanciaLateral > radioReaccion)
            return false;

        if (usarLineaDeVision)
        {
            Vector3 dirAlPunto = (puntoReaccion.position - origenPos).normalized;
            float distAlPunto = Vector3.Distance(origenPos, puntoReaccion.position);

            if (Physics.Raycast(origenPos, dirAlPunto, out RaycastHit hit, distAlPunto, mascaraBloqueo))
            {
                if (hit.transform != puntoReaccion && !hit.transform.IsChildOf(transform))
                    return false;
            }
        }

        return true;
    }

    private void AplicarVisual(float t)
    {
        float alphaObjetivo = Mathf.Lerp(alphaNieblaReposo, alphaNieblaDisipada, t);

        if (renderersNiebla != null)
        {
            foreach (Renderer rend in renderersNiebla)
            {
                if (rend == null || rend.sharedMaterial == null)
                    continue;

                int colorProp;
                Color colorBase;

                if (!TryGetColorProperty(rend.sharedMaterial, out colorProp, out colorBase))
                    continue;

                rend.GetPropertyBlock(block);
                colorBase.a = alphaObjetivo;
                block.SetColor(colorProp, colorBase);
                rend.SetPropertyBlock(block);
            }
        }

        if (particulasNiebla != null)
        {
            foreach (ParticleSystem ps in particulasNiebla)
            {
                if (ps == null)
                    continue;

                var emission = ps.emission;
                emission.rateOverTimeMultiplier = Mathf.Lerp(25f, 4f, t);
            }
        }

        if (volumenObjetivo != null)
            volumenObjetivo.weight = Mathf.Lerp(pesoVolumenReposo, pesoVolumenDisipado, t);
    }

    private bool TryGetColorProperty(Material material, out int propertyId, out Color colorBase)
    {
        propertyId = -1;
        colorBase = Color.white;

        if (material.HasProperty(BaseColorID))
        {
            propertyId = BaseColorID;
            colorBase = material.GetColor(BaseColorID);
            return true;
        }

        if (material.HasProperty(ColorID))
        {
            propertyId = ColorID;
            colorBase = material.GetColor(ColorID);
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorDentro = true;

            if (mostrarLogs)
                Debug.Log("[FogZoneReactiva] Jugador entró en zona: " + name, this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorDentro = false;

            if (mostrarLogs)
                Debug.Log("[FogZoneReactiva] Jugador salió de zona: " + name, this);
        }
    }
}