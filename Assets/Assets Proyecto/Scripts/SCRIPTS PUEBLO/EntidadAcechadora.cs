using UnityEngine;
using UnityEngine.AI;

public class EntidadAcechadora : MonoBehaviour
{
    private Transform camaraObjetivo;
    private NavMeshAgent agent;
    
    [Header("Configuración de Acecho")]
    public float umbralVision = 0.5f; 
    public float distanciaDeteccion = 10f; // Solo te persiguen si estás a menos de 10 metros

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (Camera.main != null) camaraObjetivo = Camera.main.transform;
    }

    void Update()
    {
        if (camaraObjetivo == null) return;

        // 1. Calculamos la distancia actual entre la entidad y la cámara
        float distanciaAlJugador = Vector3.Distance(transform.position, camaraObjetivo.position);

        // 2. Si estás muy lejos, la entidad se queda quieta y no hace cálculos
        if (distanciaAlJugador > distanciaDeteccion)
        {
            agent.isStopped = true;
            return;
        }

        // 3. Si estás cerca, revisamos si estás de espaldas
        Vector3 direccionHaciaMi = (transform.position - camaraObjetivo.position).normalized;
        float dot = Vector3.Dot(camaraObjetivo.forward, direccionHaciaMi);

        if (dot < umbralVision) 
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