using UnityEngine;
using System.Collections;

public class TriggerDeParticulas : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("El sistema de partículas que quieres que desaparezca")]
    public GameObject sistemaParticulas;

    [Tooltip("Tiempo en segundos antes de que desaparezca")]
    public float tiempoParaDesaparecer = 0.5f;

    [Tooltip("Nombre exacto del objeto del barco (o parte del nombre)")]
    public string nombreDelBarco = "Barco";

    private bool yaSeActivo = false;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si es el barco y si no se ha activado ya
        if (!yaSeActivo && (other.name.Contains(nombreDelBarco) || other.CompareTag("Player")))
        {
            yaSeActivo = true;
            Debug.Log("¡El barco ha tocado el activador! Desapareciendo partículas en " + tiempoParaDesaparecer + " segundos.");
            
            // Iniciamos la cuenta atrás para desaparecer
            StartCoroutine(DesaparecerParticulas());
        }
    }

    IEnumerator DesaparecerParticulas()
    {
        // Espera el tiempo que tú elijas en el Inspector
        yield return new WaitForSeconds(tiempoParaDesaparecer);

        if (sistemaParticulas != null)
        {
            // Opción A: Desactivar el objeto por completo
            sistemaParticulas.SetActive(false);
            
            // Opción B: Si solo quieres que dejen de salir partículas pero las que ya están sigan vivas hasta morir:
            // var main = sistemaParticulas.GetComponent<ParticleSystem>().main;
            // main.loop = false; 
            
            Debug.Log("Sistema de partículas desactivado.");
        }
    }
}