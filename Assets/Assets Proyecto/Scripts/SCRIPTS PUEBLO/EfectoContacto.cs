using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ControlEfectosVR : MonoBehaviour
{
    [Header("Referencias de Volumen")]
    public Volume volumenGlobal;
    
    [Header("Audio")]
    public AudioSource fuenteSonido;
    public AudioClip sonidoSusto;

    private ChromaticAberration chromatic;
    private FilmGrain grain;
    private MotionBlur motionBlur; // Nuevo efecto
    private Coroutine rutinaEfecto;

    void Start()
    {
        if (volumenGlobal != null)
        {
            volumenGlobal.profile.TryGet(out chromatic);
            volumenGlobal.profile.TryGet(out grain);
            volumenGlobal.profile.TryGet(out motionBlur);

            // Inicializamos todo en 0
            ResetearEfectos();
        }

        if (fuenteSonido == null) fuenteSonido = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemigo"))
        {
            Debug.Log("¡CONTACTO! Activando Motion Blur y efectos.");

            // El truco de refrescar el volumen para evitar que se congele
            if (volumenGlobal != null)
            {
                volumenGlobal.enabled = false;
                volumenGlobal.enabled = true;
            }

            // Reproducir sonido
            if (fuenteSonido != null && sonidoSusto != null)
            {
                fuenteSonido.PlayOneShot(sonidoSusto);
            }

            if (rutinaEfecto != null) StopCoroutine(rutinaEfecto);
            rutinaEfecto = StartCoroutine(EjecutarEfectoVisual());
        }
    }

    IEnumerator EjecutarEfectoVisual()
    {
        float duracion = 2.5f;
        float tiempo = 0;

        // Forzamos que los estados de override estén activos
        ActivarOverrides(true);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float factor = 1 - (tiempo / duracion); // Va de 1 a 0

            // Aplicamos la intensidad a los tres efectos
            if (chromatic != null) chromatic.intensity.value = factor; // Máximo 1
            if (grain != null) grain.intensity.value = factor; // Máximo 1
            if (motionBlur != null) motionBlur.intensity.value = factor; // Máximo 1

            yield return null;
        }

        ResetearEfectos();
    }

    void ActivarOverrides(bool estado)
    {
        if (chromatic != null) { chromatic.active = estado; chromatic.intensity.overrideState = estado; }
        if (grain != null) { grain.active = estado; grain.intensity.overrideState = estado; }
        if (motionBlur != null) { motionBlur.active = estado; motionBlur.intensity.overrideState = estado; }
    }

    void ResetearEfectos()
    {
        if (chromatic != null) chromatic.intensity.value = 0f;
        if (grain != null) grain.intensity.value = 0f;
        if (motionBlur != null) motionBlur.intensity.value = 0f;
    }
}