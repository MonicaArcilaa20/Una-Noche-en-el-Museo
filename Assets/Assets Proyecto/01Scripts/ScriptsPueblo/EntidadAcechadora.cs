using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EntidadAcechadora : MonoBehaviour
{
    private Transform camaraObjetivo;
    private NavMeshAgent agent;
    private Renderer renderizadorHijo;
    private Material materialPropio;
    private float umbralOriginal;
    
    [Header("Configuración de Acecho")]
    public float umbralVision = 0.5f; 
    public float rangoDeteccion = 15f; 

    [Header("Efectos Visuales (Texturas)")]
    public Texture texturaNormal;
    public Texture texturaAcecho; 

    [Header("Sonido de Movimiento")]
    public AudioSource audioMovimiento; 

    [Header("Conexiones")]
    public ControlEfectosVR scriptEfectos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        umbralOriginal = umbralVision;
        renderizadorHijo = GetComponentInChildren<Renderer>();
        
        if (renderizadorHijo != null) materialPropio = renderizadorHijo.material;
        if (Camera.main != null) camaraObjetivo = Camera.main.transform;
    }

    void Update()
    {
        if (camaraObjetivo == null || agent == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, camaraObjetivo.position);
        Vector3 direccionHaciaMi = (transform.position - camaraObjetivo.position).normalized;
        float dot = Vector3.Dot(camaraObjetivo.forward, direccionHaciaMi);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh && umbralVision <= 1.0f)
        {
            if (distanciaAlJugador <= rangoDeteccion && dot < umbralVision) 
            {
                agent.isStopped = false;
                agent.SetDestination(camaraObjetivo.position);
                CambiarTextura(texturaNormal);
                GestionarSonido(true);
            }
            else 
            {
                agent.isStopped = true;
                if (distanciaAlJugador <= rangoDeteccion && dot >= umbralVision) 
                    CambiarTextura(texturaAcecho);
                else 
                    CambiarTextura(texturaNormal);
                
                GestionarSonido(false);
            }
        }
    }

    public void CambiarTextura(Texture nuevaTextura)
    {
        if (materialPropio != null && nuevaTextura != null)
        {
            materialPropio.SetTexture("_BaseMap", nuevaTextura);
            materialPropio.SetTexture("_MainTex", nuevaTextura);
        }
    }

    void GestionarSonido(bool debeSonar)
    {
        if (audioMovimiento == null) return;
        if (debeSonar && !audioMovimiento.isPlaying) audioMovimiento.Play();
        else if (!debeSonar && audioMovimiento.isPlaying) audioMovimiento.Pause();
    }

    public IEnumerator CongelarEntidad(float tiempo)
    {
        if (agent == null) yield break;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        CambiarTextura(texturaAcecho);
        GestionarSonido(false);

        umbralVision = 2.0f; 
        yield return new WaitForSeconds(tiempo);
        umbralVision = umbralOriginal;
        CambiarTextura(texturaNormal);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            if (scriptEfectos != null) scriptEfectos.ActivarSustoManual();
        }
    }
}