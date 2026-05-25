using System.Collections;
using UnityEngine;

public class LianaLightTrigger : MonoBehaviour
{
    [Header("Luz")]
    public Light pointLight;

    [Header("Tiempo de encendido")]
    public float tiempoEncendido = 2f;

    [Header("Intensidad final")]
    public float intensidadFinal = 5f;

    private bool activada = false;

    private void Start()
    {
        // La luz empieza apagada
        pointLight.intensity = 0f;
        pointLight.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detecta si el objeto que entra es el barco
        if (other.CompareTag("Barco") && !activada)
        {
            Debug.Log("🚤 El barco ha entrado al trigger de la liana: " + gameObject.name);

            activada = true;
            StartCoroutine(EncenderLuz());
        }
        else
        {
            Debug.Log("Algo entró al trigger: " + other.name);
        }
    }

    IEnumerator EncenderLuz()
    {
        float tiempo = 0f;

        while (tiempo < tiempoEncendido)
        {
            tiempo += Time.deltaTime;

            pointLight.intensity = Mathf.Lerp(
                0f,
                intensidadFinal,
                tiempo / tiempoEncendido
            );

            yield return null;
        }

        pointLight.intensity = intensidadFinal;

        Debug.Log("💡 Luz encendida completamente en: " + gameObject.name);
    }
}