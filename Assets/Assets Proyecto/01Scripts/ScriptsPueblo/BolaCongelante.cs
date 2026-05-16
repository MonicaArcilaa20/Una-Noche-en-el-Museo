using UnityEngine;

public class BolaCongelante : MonoBehaviour
{
    public float velocidad = 10f;
    public float tiempoCongelacion = 3f;

    void Start()
    {
        // Se destruye sola tras 5 segundos si no toca nada para no llenar la escena de basura
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Buscamos si el objeto tocado es un enemigo acechador
        EntidadAcechadora enemigo = other.GetComponentInParent<EntidadAcechadora>();

        if (enemigo != null)
        {
            // Llamamos a una nueva función que añadiremos al enemigo
            enemigo.StartCoroutine(enemigo.CongelarEntidad(tiempoCongelacion));
            Destroy(gameObject); // La bolita desaparece al tocarlo
        }
    }
}