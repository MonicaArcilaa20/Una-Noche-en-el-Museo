using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MuseoFlowManager : MonoBehaviour
{
    [Header("Botones físicos (ocultar al presionar)")]
    [SerializeField] private GameObject[] botonesInicio;

    [Header("UI")]
    [SerializeField] private GameObject canvasControlesMovimiento;
    [SerializeField] private GameObject canvasControlesPincel;
    [SerializeField] private GameObject panelSubtitulos;
    [SerializeField] private TMP_Text textoSubtitulos;

    [Header("Señaléticas")]
    [SerializeField] private GameObject senaleticaPasilloIzquierdo;
    [SerializeField] private GameObject senaleticaPasilloDerecho;
    [SerializeField] private GameObject senaleticaPrimerCuadro;

    [Header("Audio Voz Museo")]
    [SerializeField] private AudioSource audioSourceVoz;

    [SerializeField] private AudioClip vozIntro1;
    [SerializeField] private AudioClip vozIntro2;
    [SerializeField] private AudioClip vozIntro3;

    [SerializeField] private AudioClip vozPincelEncontrado;
    [SerializeField] private AudioClip vozPincelPoder;
    [SerializeField] private AudioClip vozBuscarTinta;

    [SerializeField] private AudioClip vozTintaTomada;
    [SerializeField] private AudioClip vozPintarPrimerCuadro;

    [SerializeField] private AudioClip vozEntrarAlCuadro;

    [Header("Textos")]
    [TextArea][SerializeField] private string textoIntro1 = "Bienvenido al museo.";
    [TextArea][SerializeField] private string textoIntro2 = "Este museo no es lo que parece.";
    [TextArea][SerializeField] private string textoIntro3 = "Encuentra el pincel mágico.";

    [TextArea][SerializeField] private string textoPincelEncontrado = "¡Has encontrado el pincel!";
    [TextArea][SerializeField] private string textoPincelPoder = "Puedes abrir mundos.";
    [TextArea][SerializeField] private string textoBuscarTinta = "Busca tinta.";

    [TextArea][SerializeField] private string textoTintaTomada = "Tienes tinta.";
    [TextArea][SerializeField] private string textoPintarPrimerCuadro = "Pinta el cuadro.";

    [TextArea][SerializeField] private string textoEntrarAlCuadro = "Entra al mundo.";

    [Header("Tiempos")]
    [SerializeField] private float duracionCanvasMovimiento = 8f;
    [SerializeField] private float duracionCanvasPincel = 8f;
    [SerializeField] private float pausaEntreLineas = 0.4f;
    [SerializeField] private float duracionFallbackLinea = 4f;

    private bool pincelTomado = false;
    private bool tintaTomada = false;
    private bool avisoCuadroLanzado = false;

    private Coroutine secuenciaActual;
    private Coroutine rutinaCanvasMovimiento;
    private Coroutine rutinaCanvasPincel;

    private void Start()
    {
        ApagarTodo();
    }

    private void ApagarTodo()
    {
        if (canvasControlesMovimiento != null)
            canvasControlesMovimiento.SetActive(false);

        if (canvasControlesPincel != null)
            canvasControlesPincel.SetActive(false);

        OcultarSubtitulos();

        if (senaleticaPasilloIzquierdo != null)
            senaleticaPasilloIzquierdo.SetActive(false);

        if (senaleticaPasilloDerecho != null)
            senaleticaPasilloDerecho.SetActive(false);

        if (senaleticaPrimerCuadro != null)
            senaleticaPrimerCuadro.SetActive(false);
    }

    public void OnClickEmpezar()
    {
        OcultarBotonesInicio();

        PrepararEstadoInicial();
        secuenciaActual = StartCoroutine(SecuenciaInicio());
    }

    public void OnClickTutorial()
    {
        OcultarBotonesInicio();

        SceneManager.LoadScene("0Tutorial_Inicio");
    }

    private void OcultarBotonesInicio()
    {
        foreach (GameObject boton in botonesInicio)
        {
            if (boton != null)
                boton.SetActive(false);
        }
    }

    private void PrepararEstadoInicial()
    {
        if (canvasControlesMovimiento != null)
            canvasControlesMovimiento.SetActive(true);

        if (canvasControlesPincel != null)
            canvasControlesPincel.SetActive(false);

        OcultarSubtitulos();

        if (senaleticaPasilloIzquierdo != null)
            senaleticaPasilloIzquierdo.SetActive(true);

        if (senaleticaPasilloDerecho != null)
            senaleticaPasilloDerecho.SetActive(false);

        if (senaleticaPrimerCuadro != null)
            senaleticaPrimerCuadro.SetActive(false);

        if (rutinaCanvasMovimiento != null)
            StopCoroutine(rutinaCanvasMovimiento);

        rutinaCanvasMovimiento = StartCoroutine(MostrarCanvasTemporal(canvasControlesMovimiento, duracionCanvasMovimiento));
    }

    public void OnPincelTomado()
    {
        if (pincelTomado) return;

        pincelTomado = true;

        PrepararNuevaSecuencia();

        if (canvasControlesMovimiento != null)
            canvasControlesMovimiento.SetActive(false);

        if (senaleticaPasilloIzquierdo != null)
            senaleticaPasilloIzquierdo.SetActive(false);

        if (senaleticaPasilloDerecho != null)
            senaleticaPasilloDerecho.SetActive(true);

        if (rutinaCanvasPincel != null)
            StopCoroutine(rutinaCanvasPincel);

        rutinaCanvasPincel = StartCoroutine(MostrarCanvasTemporal(canvasControlesPincel, duracionCanvasPincel));
        secuenciaActual = StartCoroutine(SecuenciaPincelTomado());
    }

    public void OnPrimeraTintaTomada()
    {
        if (tintaTomada) return;

        tintaTomada = true;

        PrepararNuevaSecuencia();

        if (senaleticaPasilloDerecho != null)
            senaleticaPasilloDerecho.SetActive(false);

        if (senaleticaPrimerCuadro != null)
            senaleticaPrimerCuadro.SetActive(true);

        secuenciaActual = StartCoroutine(SecuenciaTintaTomada());
    }

    public void OnLlegarPrimerCuadro()
    {
        if (avisoCuadroLanzado) return;

        avisoCuadroLanzado = true;

        PrepararNuevaSecuencia();

        if (senaleticaPrimerCuadro != null)
            senaleticaPrimerCuadro.SetActive(false);

        secuenciaActual = StartCoroutine(SecuenciaLlegadaCuadro());
    }

    private void PrepararNuevaSecuencia()
    {
        if (secuenciaActual != null)
            StopCoroutine(secuenciaActual);

        if (audioSourceVoz != null && audioSourceVoz.isPlaying)
            audioSourceVoz.Stop();

        OcultarSubtitulos();
    }

    private IEnumerator SecuenciaInicio()
    {
        yield return ReproducirLinea(vozIntro1, textoIntro1);
        yield return ReproducirLinea(vozIntro2, textoIntro2);
        yield return ReproducirLinea(vozIntro3, textoIntro3);
        OcultarSubtitulos();
    }

    private IEnumerator SecuenciaPincelTomado()
    {
        yield return ReproducirLinea(vozPincelEncontrado, textoPincelEncontrado);
        yield return ReproducirLinea(vozPincelPoder, textoPincelPoder);
        yield return ReproducirLinea(vozBuscarTinta, textoBuscarTinta);
        OcultarSubtitulos();
    }

    private IEnumerator SecuenciaTintaTomada()
    {
        yield return ReproducirLinea(vozTintaTomada, textoTintaTomada);
        yield return ReproducirLinea(vozPintarPrimerCuadro, textoPintarPrimerCuadro);
        OcultarSubtitulos();
    }

    private IEnumerator SecuenciaLlegadaCuadro()
    {
        yield return ReproducirLinea(vozEntrarAlCuadro, textoEntrarAlCuadro);
        OcultarSubtitulos();
    }

    private IEnumerator ReproducirLinea(AudioClip clip, string texto)
    {
        MostrarSubtitulos(texto);

        float espera = duracionFallbackLinea;

        if (audioSourceVoz != null && clip != null)
        {
            audioSourceVoz.clip = clip;
            audioSourceVoz.Play();
            espera = clip.length;
        }

        yield return new WaitForSeconds(espera + pausaEntreLineas);
    }

    private IEnumerator MostrarCanvasTemporal(GameObject canvas, float duracion)
    {
        if (canvas == null) yield break;

        canvas.SetActive(true);
        yield return new WaitForSeconds(duracion);
        canvas.SetActive(false);
    }

    private void MostrarSubtitulos(string texto)
    {
        if (panelSubtitulos != null)
            panelSubtitulos.SetActive(true);

        if (textoSubtitulos != null)
            textoSubtitulos.text = texto;
    }

    private void OcultarSubtitulos()
    {
        if (textoSubtitulos != null)
            textoSubtitulos.text = "";

        if (panelSubtitulos != null)
            panelSubtitulos.SetActive(false);
    }
}