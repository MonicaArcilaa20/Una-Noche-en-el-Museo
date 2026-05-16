using UnityEngine;
using UnityEngine.AI; // Por si usas NavMesh

public class ControlBarcoAutomatico : MonoBehaviour
{
    [Header("Rutas")]
    public Transform puntoA;
    public Transform puntoB;
    public float velocidadBarco = 3f;

    [Header("Configuración del Jugador")]
    public string tagJugador = "Player";
    
    private bool jugadorAbordo = false;
    private bool viajeFinalizado = false;
    private GameObject referenciaJugador;
    private CharacterController characterControllerJugador;
    private NavMeshAgent navMeshJugador;

    void Start()
    {
        // 1. POSICIÓN INICIAL
        if (puntoA != null) transform.position = puntoA.position;
        
        // 2. SOLUCIÓN AL HUNDIMIENTO
        // Al ser Kinematic, el barco ignora la gravedad y solo se mueve por código
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) 
        {
            rb.isKinematic = true; 
            rb.useGravity = false;
        }
    }

    void Update()
    {
        if (jugadorAbordo && !viajeFinalizado)
        {
            MoverBarco();
            BloquearMovimientoJugador();
        }
    }

    private void MoverBarco()
    {
        float distancia = Vector3.Distance(transform.position, puntoB.position);
        
        if (distancia > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, puntoB.position, velocidadBarco * Time.deltaTime);
        }
        else
        {
            viajeFinalizado = true;
            Debug.Log("Llegamos al destino. Barco inmovilizado.");
        }
    }

    private void BloquearMovimientoJugador()
    {
        if (referenciaJugador == null) return;

        // Si usas NavMesh, forzamos velocidad 0 y detenemos el agente
        if (navMeshJugador != null)
        {
            navMeshJugador.velocity = Vector3.zero;
            navMeshJugador.isStopped = true;
        }

        // Si usas CharacterController (típico en VR), forzamos su posición 
        // para que no se mueva respecto al barco
        // (Al ser hijo del barco, esto lo mantiene clavado en el sitio)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!viajeFinalizado && other.CompareTag(tagJugador))
        {
            SubirJugador(other.gameObject);
        }
    }

    private void SubirJugador(GameObject jugador)
    {
        jugadorAbordo = true;
        referenciaJugador = jugador;

        // Obtenemos las referencias una sola vez para optimizar
        characterControllerJugador = jugador.GetComponent<CharacterController>();
        navMeshJugador = jugador.GetComponent<NavMeshAgent>();

        // 1. Vínculo físico: El jugador ahora se mueve con el transform del barco
        jugador.transform.SetParent(this.transform);

        // 2. Si es un CharacterController, hay que desactivarlo un frame para teletransportarlo 
        // a la cubierta sin que las físicas lo reboten
        if (characterControllerJugador != null)
        {
            characterControllerJugador.enabled = false;
            // Lo posicionamos un poquito arriba de la cubierta para que no se entierre
            jugador.transform.localPosition = new Vector3(0, 1.2f, 0); 
            characterControllerJugador.enabled = true;
        }

        Debug.Log("Jugador a bordo. Velocidad bloqueada por jerarquía.");
    }
}