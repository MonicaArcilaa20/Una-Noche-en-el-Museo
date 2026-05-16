using UnityEngine;

public class PincelDisparador : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject prefabBola;
    public float cadenciaFuego = 0.5f; // Tiempo entre disparos
    public float alcanceMaximo = 20f;  // Qué tan lejos detecta al enemigo

    private ControlPincel controlPincel;
    private float tiempoSiguienteDisparo;

    void Awake()
    {
        controlPincel = GetComponent<ControlPincel>();
    }

    void Update()
    {
        // Solo intenta disparar si el pincel está "encendido" según tu lógica de ControlPincel.cs
        if (controlPincel != null && controlPincel.EstaActivo)
        {
            ProcesarApuntado();
        }
    }

    void ProcesarApuntado()
    {
        if (Time.time < tiempoSiguienteDisparo) return;

        // Usamos el OrigenMagia definido en tu ControlPincel.cs para lanzar el rayo
        Transform origen = controlPincel.OrigenMagia;
        RaycastHit hit;

        // Lanzamos un rayo desde la punta del pincel hacia adelante
        if (Physics.Raycast(origen.position, origen.forward, out hit, alcanceMaximo))
        {
            // Verificamos si lo que estamos señalando tiene el script de la entidad
            EntidadAcechadora enemigo = hit.collider.GetComponentInParent<EntidadAcechadora>();

            if (enemigo != null)
            {
                Disparar();
                tiempoSiguienteDisparo = Time.time + cadenciaFuego;
            }
        }
    }

    void Disparar()
    {
        Transform puntoDisparo = controlPincel.OrigenMagia;
        Instantiate(prefabBola, puntoDisparo.position, puntoDisparo.rotation);
    }

    // Para ver el rayo en el editor (ayuda a calibrar la puntería)
    private void OnDrawGizmos()
    {
        if (controlPincel != null && controlPincel.OrigenMagia != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(controlPincel.OrigenMagia.position, controlPincel.OrigenMagia.forward * alcanceMaximo);
        }
    }
}