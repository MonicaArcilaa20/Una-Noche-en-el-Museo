using System.Collections;
using TMPro;
using UnityEngine;

public class MuseoFlowManager : MonoBehaviour
{
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
    [TextArea] [SerializeField] private string textoIntro1 = "Bienvenido al lado del museo que nunca has visto. Un museo fuera de lo común.";
    [TextArea] [SerializeField] private string textoIntro2 = "Este museo no es lo que parece. Cada pintura es una puerta. Cada puerta, un mundo. Y tú tienes el poder de abrirlos.";
    [TextArea] [SerializeField] private string textoIntro3 = "Explora el lugar y encuentra el Pincel Mágico que te permitirá abrir nuevos mundos y te acompañará durante todo el viaje.";

    [TextArea] [SerializeField] private string textoPincelEncontrado = "¡Has encontrado el Pincel Mágico!";
    [TextArea] [SerializeField] private string textoPincelPoder = "Ahora posees el poder de visitar los mundos posibles de las obras de este lugar.";
    [TextArea] [SerializeField] private string textoBuscarTinta = "Pero ten cuidado, necesitarás de tinta para activar los portales en cada mundo, busca tu primera carga de tinta y embárcate en esta artística aventura.";

    [TextArea] [SerializeField] private string textoTintaTomada = "Has conseguido la tinta, úsala con cuidado ya que no es infinita, es tu deber cuidar y buscar de ella.";
    [TextArea] [SerializeField] private string textoPintarPrimerCuadro = "Ahora tienes todo lo que se necesita para pintar tu primer cuadro. El pincel no dibuja lo que ves, dibuja lo que es posible. Pinta el marco y observa cómo sucede la magia.";

    [TextArea] [SerializeField] private string textoEntrarAlCuadro = "No tengas miedo. Posibles mundos esperan por ti para ser vistos y transformados.";

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
        PrepararEstadoInicial();
        secuenciaActual = StartCoroutine(SecuenciaInicio());
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
        if (pincelTomado)
            return;

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
        if (tintaTomada)
            return;

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
        if (avisoCuadroLanzado)
            return;

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
        if (canvas == null)
            yield break;

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