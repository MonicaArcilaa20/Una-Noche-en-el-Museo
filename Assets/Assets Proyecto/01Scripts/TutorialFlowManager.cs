using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class PasoTutorialData
{
    [TextArea]
    public string nombrePaso;

    public GameObject texto;
    public GameObject objetoPaso1;
    public GameObject objetoPaso2;

    [Header("Mantener activos al completar")]
    public bool mantenerObjetoPaso1Activo;
    public bool mantenerObjetoPaso2Activo;
}

public class TutorialFlowManager : MonoBehaviour
{
    [Header("Escena al terminar")]
    [SerializeField] private string escenaInicio = "1Museo_Monica";
    [SerializeField] private float esperaAntesDeCargar = 3f;

    [Header("Pasos del tutorial")]
    [SerializeField] private PasoTutorialData[] pasos;

    [Header("Final del tutorial")]
    [SerializeField] private GameObject canvasFinalTutorial;
    [SerializeField] private TMP_Text textoFinalTutorial;
    [TextArea]
    [SerializeField] private string mensajeFinal = "¡Has terminado el tutorial! Ahora disfrutarás de la experiencia.";

    [Header("Transición")]
    [SerializeField] private TransicionEscena transicionEscena;

    [Header("Sonido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoPasoCorrecto;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = true;

    private int pasoActual = 0;
    private bool tutorialTerminando = false;

    private void Start()
    {
        OcultarTodosLosPasos();

        if (canvasFinalTutorial != null)
            canvasFinalTutorial.SetActive(false);

        MostrarPaso(0);
    }

    public void PasoCompletado(int numeroPaso)
    {
        if (tutorialTerminando)
            return;

        if (numeroPaso != pasoActual)
            return;

        if (audioSource != null && sonidoPasoCorrecto != null)
            audioSource.PlayOneShot(sonidoPasoCorrecto);

        OcultarPaso(pasoActual);

        pasoActual++;

        if (pasoActual >= pasos.Length)
        {
            tutorialTerminando = true;

            if (mostrarLogs)
                Debug.Log("Tutorial completado. Mostrando mensaje final.");

            StartCoroutine(FinalTutorial());
            return;
        }

        MostrarPaso(pasoActual);
    }

    private void MostrarPaso(int indice)
    {
        if (!IndiceValido(indice))
            return;

        PasoTutorialData paso = pasos[indice];

        if (paso.texto != null) paso.texto.SetActive(true);
        if (paso.objetoPaso1 != null) paso.objetoPaso1.SetActive(true);
        if (paso.objetoPaso2 != null) paso.objetoPaso2.SetActive(true);

        if (mostrarLogs)
            Debug.Log("Mostrando paso: " + indice + " - " + paso.nombrePaso);
    }

    private void OcultarPaso(int indice)
    {
        if (!IndiceValido(indice))
            return;

        PasoTutorialData paso = pasos[indice];

        if (paso.texto != null)
            paso.texto.SetActive(false);

        if (paso.objetoPaso1 != null && !paso.mantenerObjetoPaso1Activo)
            paso.objetoPaso1.SetActive(false);

        if (paso.objetoPaso2 != null && !paso.mantenerObjetoPaso2Activo)
            paso.objetoPaso2.SetActive(false);
    }

    private void OcultarTodosLosPasos()
    {
        if (pasos == null)
            return;

        for (int i = 0; i < pasos.Length; i++)
        {
            if (pasos[i].texto != null)
                pasos[i].texto.SetActive(false);

            if (pasos[i].objetoPaso1 != null)
                pasos[i].objetoPaso1.SetActive(false);

            if (pasos[i].objetoPaso2 != null)
                pasos[i].objetoPaso2.SetActive(false);
        }
    }

    private bool IndiceValido(int indice)
    {
        return pasos != null && indice >= 0 && indice < pasos.Length;
    }

    private IEnumerator FinalTutorial()
    {
        if (canvasFinalTutorial != null)
            canvasFinalTutorial.SetActive(true);

        if (textoFinalTutorial != null)
            textoFinalTutorial.text = mensajeFinal;

        yield return new WaitForSeconds(esperaAntesDeCargar);

        // Limpia el estado global para que el museo empiece fresco
        if (EstadoGlobalPincel.Instance != null)
            EstadoGlobalPincel.Instance.ResetearEstadoTutorial();

        if (transicionEscena != null)
            transicionEscena.IniciarTransicion(escenaInicio);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(escenaInicio);
    }
}