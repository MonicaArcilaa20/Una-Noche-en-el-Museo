using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransicionEscena : MonoBehaviour
{
    [SerializeField] private Image imagenFade;
    [SerializeField] private float duracionFade = 1.5f;

    private bool transicionando = false;

    private void Awake()
    {
        if (imagenFade != null)
        {
            Color c = imagenFade.color;
            c.a = 1f;
            imagenFade.color = c;
        }
    }

    private IEnumerator Start()
    {
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
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(alphaInicial, alphaObjetivo, t / duracionFade);

            c.a = alpha;
            imagenFade.color = c;

            yield return null;
        }

        c.a = alphaObjetivo;
        imagenFade.color = c;
    }
}