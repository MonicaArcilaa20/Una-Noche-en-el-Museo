using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialFlowManager : MonoBehaviour
{
    public string EscenaInicio = "1Museo_Susana";

    [Header("Textos")]
    public GameObject textoCaminar;
    public GameObject textoAgarrar;
    public GameObject textoPincel;
    public GameObject textoTinta;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoPasoCorrecto;

    private int paso = 0;

    void Start()
    {
        MostrarTexto(0);
    }

    public void PasoCompletado(int numeroPaso)
    {
        if (numeroPaso != paso) return;


        if (audioSource != null && sonidoPasoCorrecto != null)
            audioSource.PlayOneShot(sonidoPasoCorrecto);

        OcultarTexto(paso);

        paso++;

        MostrarTexto(paso);


        if (paso == 5)
        {
            StartCoroutine(FinalTutorial());
        }
    }

    void MostrarTexto(int p)
    {
        if (p == 0 && textoCaminar != null) textoCaminar.SetActive(true);
        if (p == 1 && textoAgarrar != null) textoAgarrar.SetActive(true);
        if (p == 3 && textoPincel != null) textoPincel.SetActive(true);
        if (p == 4 && textoTinta != null) textoTinta.SetActive(true);
    }

    void OcultarTexto(int p)
    {
        if (p == 0 && textoCaminar != null) textoCaminar.SetActive(false);
        if (p == 1 && textoAgarrar != null) textoAgarrar.SetActive(false);
        if (p == 3 && textoPincel != null) textoPincel.SetActive(false);
        if (p == 4 && textoTinta != null) textoTinta.SetActive(false);
    }

    IEnumerator FinalTutorial()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(EscenaInicio);
    }
}