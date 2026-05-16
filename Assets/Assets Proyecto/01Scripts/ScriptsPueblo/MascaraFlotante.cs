using UnityEngine;

using UnityEngine;

public class MascaraFlotante : MonoBehaviour
{
    private Transform camaraJugador;
    private float tiempoLevitacion;
    private Vector3 posicionObjetivoActual;

    [Header("Configuración de Seguimiento")]
    public Vector3 offsetRespectoAlJugador = new Vector3(0.6f, -0.2f, 1.2f); // X=Lado, Y=Altura, Z=Adelante
    public float suavizadoMovimiento = 3f;
    public float suavizadoRotacion = 5f;

    [Header("Efecto de Levitación")]
    public float amplitudOnda = 0.05f; // Qué tanto sube y baja
    public float velocidadOnda = 2f;    // Qué tan rápido flota

    [Header("Componentes Visuales")]
    public GameObject modeloVisualMascara; // El hijo que contiene la malla/renderers de la máscara

    void Start()
    {
        if (Camera.main != null)
        {
            camaraJugador = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró la MainCamera en la escena.");
        }

        // Por defecto inicia apagada hasta que pise un collider
        DefinirVisibilidad(false);
    }

    void Update()
    {
        if (camaraJugador == null) return;

        // 1. CALCULAR POSICIÓN AL LADO DEL JUGADOR
        // TransformDirection hace que el offset rote junto con la cabeza del jugador en VR
        Vector3 posicionBase = camaraJugador.position + camaraJugador.TransformDirection(offsetRespectoAlJugador);

        // 2. EFECTO LEVITACIÓN (Arriba y Abajo)
        tiempoLevitacion += Time.deltaTime * velocidadOnda;
        float desfasadoY = Mathf.Sin(tiempoLevitacion) * amplitudOnda;
        posicionObjetivoActual = posicionBase + new Vector3(0, desfasadoY, 0);

        // 3. APLICAR MOVIMIENTO Y ROTACIÓN SUAVE
        transform.position = Vector3.Lerp(transform.position, posicionObjetivoActual, suavizadoMovimiento * Time.deltaTime);

        // Hace que la máscara siempre gire para mirar al jugador de frente
        Vector3 direccionAlJugador = camaraJugador.position - transform.position;
        if (direccionAlJugador != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionAlJugador);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, suavizadoRotacion * Time.deltaTime);
        }
    }

    public void DefinirVisibilidad(bool visible)
    {
        if (modeloVisualMascara != null)
        {
            modeloVisualMascara.SetActive(visible);
        }
    }
}
