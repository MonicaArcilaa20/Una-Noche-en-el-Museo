using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransicionEscena : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private Image imagenFade;
    [SerializeField] private float duracionFade = 1.5f;
    [SerializeField] private bool usarTiempoNoEscalado = true;

    [Header("Audio transición")]
    [SerializeField] private AudioSource audioSourceTransicion;
    [SerializeField] private AudioClip sonidoCambioMundo;
    [Range(0f, 1f)]
    [SerializeField] private float volumenSonido = 1f;
    [SerializeField] private bool reproducirEnFadeIn = true;
    [SerializeField] private bool reproducirEnFadeOut = true;

    private bool transicionando = false;

    private void Awake()
    {
        if (imagenFade != null)
        {
            if (!imagenFade.gameObject.activeSelf)
                imagenFade.gameObject.SetActive(true);

            Color c = imagenFade.color;
            c.a = 1f;
            imagenFade.color = c;
        }
    }

    private IEnumerator Start()
    {
        yield return null;

        if (reproducirEnFadeIn)
            ReproducirSonidoTransicion();

        yield return FadeA(0f);
    }

    public void IniciarTransicion(string nombreEscena)
    {
        if (transicionando)
            return;

        StartCoroutine(RutinaTransicion(nombreEscena));
    }

    private IEnumerator RutinaTransicion(string nombreEscena)
    {
        transicionando = true;

        if (reproducirEnFadeOut)
            ReproducirSonidoTransicion();

        yield return FadeA(1f);

        SceneManager.LoadScene(nombreEscena);
    }

    private IEnumerator FadeA(float alphaObjetivo)
    {
        if (imagenFade == null)
            yield break;

        Color c = imagenFade.color;
        float alphaInicial = c.a;
        float t = 0f;

        while (t < duracionFade)
        {
            t += usarTiempoNoEscalado ? Time.unscaledDeltaTime : Time.deltaTime;
            float alpha = Mathf.Lerp(alphaInicial, alphaObjetivo, t / duracionFade);

            c.a = alpha;
            imagenFade.color = c;

            yield return null;
        }

        c.a = alphaObjetivo;
        imagenFade.color = c;
    }

    private void ReproducirSonidoTransicion()
    {
        if (audioSourceTransicion != null && sonidoCambioMundo != null)
            audioSourceTransicion.PlayOneShot(sonidoCambioMundo, volumenSonido);
    }
}