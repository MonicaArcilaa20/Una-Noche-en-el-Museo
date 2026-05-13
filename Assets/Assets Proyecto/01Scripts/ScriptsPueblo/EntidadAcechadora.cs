using UnityEngine;
using UnityEngine.AI;

public class EntidadAcechadora : MonoBehaviour
{
    private Transform camaraObjetivo;
    private NavMeshAgent agent;
    
    [Header("Configuración de Acecho")]
    public float umbralVision = 0.5f; 
    public float rangoDeteccion = 15f; 

    [Header("Conexiones")]
    public ControlEfectosVR scriptEfectos;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (Camera.main != null)
        {
            camaraObjetivo = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró una cámara con el Tag 'MainCamera'");
        }

        if (agent != null && !agent.isOnNavMesh)
        {
            Debug.LogWarning("La entidad no inició sobre un NavMesh válido.");
        }
    }

    void Update()
    {
        if (camaraObjetivo == null || agent == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, camaraObjetivo.position);
        Vector3 direccionHaciaMi = (transform.position - camaraObjetivo.position).normalized;
        float dot = Vector3.Dot(camaraObjetivo.forward, direccionHaciaMi);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            // Solo persigue si está en rango Y el jugador no lo mira
            if (distanciaAlJugador <= rangoDeteccion && dot < umbralVision) 
            {
                agent.isStopped = false;
                agent.SetDestination(camaraObjetivo.position);
            }
            else 
            {
                agent.isStopped = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el enemigo toca a la cámara (MainCamera)
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            if (scriptEfectos != null)
            {
                // Activamos la corrutina de efectos directamente
                scriptEfectos.ActivarSustoManual();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}