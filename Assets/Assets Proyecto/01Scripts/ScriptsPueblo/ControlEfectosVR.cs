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
    private MotionBlur motionBlur;
    private Coroutine rutinaEfecto;

    void Start()
    {
        if (volumenGlobal != null)
        {
            volumenGlobal.profile.TryGet(out chromatic);
            volumenGlobal.profile.TryGet(out grain);
            volumenGlobal.profile.TryGet(out motionBlur);
            ResetearEfectos();
        }

        if (fuenteSonido == null) fuenteSonido = GetComponent<AudioSource>();
    }

    // Esta función permite que la Entidad Acechadora active todo
    public void ActivarSustoManual()
    {
        Debug.Log("¡Susto Activado!");

        if (volumenGlobal != null)
        {
            volumenGlobal.enabled = false;
            volumenGlobal.enabled = true;
        }

        if (fuenteSonido != null && sonidoSusto != null)
        {
            fuenteSonido.PlayOneShot(sonidoSusto);
        }

        if (rutinaEfecto != null) StopCoroutine(rutinaEfecto);
        rutinaEfecto = StartCoroutine(EjecutarEfectoVisual());
    }

    IEnumerator EjecutarEfectoVisual()
    {
        float duracion = 2.5f;
        float tiempo = 0;

        ActivarOverrides(true);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float factor = 1 - (tiempo / duracion);

            if (chromatic != null) chromatic.intensity.value = factor;
            if (grain != null) grain.intensity.value = factor;
            if (motionBlur != null) motionBlur.intensity.value = factor;

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