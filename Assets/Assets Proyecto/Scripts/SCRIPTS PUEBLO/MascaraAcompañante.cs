using UnityEngine;

public class MascaraAcompañante : MonoBehaviour
{
    [Header("Seguimiento")]
    public Transform objetivo; // Arrastra aquí tu Cámara Principal
    public Vector3 offset = new Vector3(0.5f, -0.2f, 1.5f); // Posición relativa a la cámara
    public float suavidadSeguimiento = 5f;

    [Header("Levitación")]
    public float amplitudLevitacion = 0.1f;
    public float velocidadLevitacion = 2f;

    [Header("Estado")]
    public bool estaVisible = true;
    public float velocidadEscala = 5f;

    private Vector3 posicionInicialLevitacion;

    void Start()
    {
        if (objetivo == null) objetivo = Camera.main.transform;
    }

    void Update()
    {
        ManejarVisibilidad();

        if (estaVisible)
        {
            SeguirJugador();
        }
    }

    void SeguirJugador()
    {
        // 1. Calcular posición objetivo con el offset
        Vector3 posicionObjetivo = objetivo.TransformPoint(offset);

        // 2. Añadir efecto de levitación (arriba y abajo)
        float nuevoY = Mathf.Sin(Time.time * velocidadLevitacion) * amplitudLevitacion;
        posicionObjetivo.y += nuevoY;

        // 3. Mover suavemente la máscara
        transform.position = Vector3.Lerp(transform.position, posicionObjetivo, suavidadSeguimiento * Time.deltaTime);

        // 4. Que siempre mire a la cámara
        transform.LookAt(objetivo);
    }

    void ManejarVisibilidad()
    {
        // Efecto de aparecer/desaparecer mediante escala
        Vector3 escalaObjetivo = estaVisible ? Vector3.one : Vector3.zero;
        transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, velocidadEscala * Time.deltaTime);
    }

    public void MostrarTemporalmente(float segundos)
    {
        StopAllCoroutines();
        StartCoroutine(RutinaMostrar(segundos));
    }

    private System.Collections.IEnumerator RutinaMostrar(float segundos)
    {
        estaVisible = true;
        yield return new WaitForSeconds(segundos);
        estaVisible = false;
    }
}