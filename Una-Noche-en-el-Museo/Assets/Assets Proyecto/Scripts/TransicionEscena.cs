using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransicionEscena : MonoBehaviour
{
    [SerializeField] private Image imagenFade;
    [SerializeField] private float duracionFade = 1.5f;

    private bool transicionando = false;

    private void Start()
    {
        if (imagenFade != null)
        {
            Color c = imagenFade.color;
            c.a = 0f;
            imagenFade.color = c;
        }
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

        float t = 0f;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / duracionFade);

            if (imagenFade != null)
            {
                Color c = imagenFade.color;
                c.a = alpha;
                imagenFade.color = c;
            }

            yield return null;
        }

        SceneManager.LoadScene(nombreEscena);
    }
}