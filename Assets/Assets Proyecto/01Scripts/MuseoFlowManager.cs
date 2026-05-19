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

    [Header("Guías visuales")]
    [SerializeField] private GameObject senaleticaPasilloIzquierdo;
    [SerializeField] private GameObject objetoPincelReal;
    [SerializeField] private GameObject guiaVisualPincel;

    [SerializeField] private GameObject senaleticaPasilloDerecho;
    [SerializeField] private GameObject objetoTintaReal;
    [SerializeField] private GameObject guiaVisualTinta;

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

    [SerializeField] private AudioClip vozCuadroActivado;

    [Header("Textos")]
    [TextArea] [SerializeField] private string textoIntro1 = "Bienvenido al lado del museo que nunca has visto. Un museo fuera de lo común.";
    [TextArea] [SerializeField] private string textoIntro2 = "Este museo no es lo que parece. Las pinturas pueden parecer sólo representaciones visuales de imaginarios individuales, pero si observamos con suficiente detenimiento, notaremos que a veces, en algunos rincones, estas pinturas parecen cobrar vida…";
    [TextArea] [SerializeField] private string textoIntro3 = "Explora el lugar y encuentra el Pincel Mágico que te permitirá abrir nuevos mundos y te acompañará durante todo el viaje.";

    [TextArea] [SerializeField] private string textoPincelEncontrado = "¡Has encontrado el Pincel Mágico!";
    [TextArea] [SerializeField] private string textoPincelPoder = "Ahora posees el poder de visitar los mundos posibles de las obras de este mágico lugar!";
    [TextArea] [SerializeField] private string textoBuscarTinta = "Pero ten cuidado, necesitarás de tinta para activar los portales en cada mundo, busca tu primera carga de tinta y embárcate en esta artística aventura!";

    [TextArea] [SerializeField] private string textoTintaTomada = "Has conseguido la tinta, úsala con cuidado ya que no es infinita, es tu deber cuidar y buscar de ella.";
    [TextArea] [SerializeField] private string textoPintarPrimerCuadro = "¡Ahora tienes todo lo que se necesita para pintar tu primer cuadro! Activa el cuadro y observa cómo sucede la magia.";
    [TextArea] [SerializeField] private string textoCuadroActivado = "Observa la magia, mira como el cuadro cobra vida y te extiende una invitación a entrar.";

    [Header("Tiempos")]
    [SerializeField] private float duracionCanvasMovimiento = 8f;
    [SerializeField] private float duracionCanvasPincel = 8f;
    [SerializeField] private float pausaEntreLineas = 0.4f;
    [SerializeField] private float duracionFallbackLinea = 4f;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = true;

    private bool pincelTomado = false;
    private bool tintaTomada = false;
    private bool cuadroActivadoAvisado = false;

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
            senaleticaPasilloIzquierdo.SetActive(false);

        if (objetoPincelReal != null)
            objetoPincelReal.SetActive(false);

        if (guiaVisualPincel != null)
            guiaVisualPincel.SetActive(false);

        if (senaleticaPasilloDerecho != null)
            senaleticaPasilloDerecho.SetActive(false);

        if (objetoTintaReal != null)
            objetoTintaReal.SetActive(false);

        if (guiaVisualTinta != null)
            guiaVisualTinta.SetActive(false);

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

        if (mostrarLogs)
            Debug.Log("MuseoFlowManager: Pincel tomado.");

        PrepararNuevaSecuencia();

        if (canvasControlesMovimiento != null)
            canvasControlesMovimiento.SetActive(false);

        if (senaleticaPasilloIzquierdo != null)
            senaleticaPasilloIzquierdo.SetActive(false);

        if (guiaVisualPincel != null)
            guiaVisualPincel.SetActive(false);

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

        if (mostrarLogs)
            Debug.Log("MuseoFlowManager: Primera tinta tomada.");

        PrepararNuevaSecuencia();

        if (senaleticaPasilloDerecho != null)
            senaleticaPasilloDerecho.SetActive(false);

        if (guiaVisualTinta != null)
            guiaVisualTinta.SetActive(false);

        secuenciaActual = StartCoroutine(SecuenciaTintaTomada());
    }

    public void OnPrimerCuadroActivado()
    {
        if (cuadroActivadoAvisado)
            return;

        cuadroActivadoAvisado = true;

        if (mostrarLogs)
            Debug.Log("MuseoFlowManager: Primer cuadro activado.");

        PrepararNuevaSecuencia();

        if (senaleticaPrimerCuadro != null)
            senaleticaPrimerCuadro.SetActive(false);

        secuenciaActual = StartCoroutine(SecuenciaCuadroActivado());
    }

    public void OnLlegarPrimerCuadro()
    {
        OnPrimerCuadroActivado();
    }

    private void PrepararNuevaSecuencia()
    {
        if (secuenciaActual != null)
        {
            StopCoroutine(secuenciaActual);
            secuenciaActual = null;
        }

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

        if (senaleticaPasilloIzquierdo != null)
            senaleticaPasilloIzquierdo.SetActive(true);

        if (objetoPincelReal != null)
            objetoPincelReal.SetActive(true);

        if (guiaVisualPincel != null)
            guiaVisualPincel.SetActive(true);
    }

    private IEnumerator SecuenciaPincelTomado()
    {
        yield return ReproducirLinea(vozPincelEncontrado, textoPincelEncontrado);
        yield return ReproducirLinea(vozPincelPoder, textoPincelPoder);
        yield return ReproducirLinea(vozBuscarTinta, textoBuscarTinta);

        OcultarSubtitulos();

        if (senaleticaPasilloDerecho != null)
            senaleticaPasilloDerecho.SetActive(true);

        if (objetoTintaReal != null)
            objetoTintaReal.SetActive(true);

        if (guiaVisualTinta != null)
            guiaVisualTinta.SetActive(true);
    }

    private IEnumerator SecuenciaTintaTomada()
    {
        yield return ReproducirLinea(vozTintaTomada, textoTintaTomada);
        yield return ReproducirLinea(vozPintarPrimerCuadro, textoPintarPrimerCuadro);

        OcultarSubtitulos();

        if (senaleticaPrimerCuadro != null)
            senaleticaPrimerCuadro.SetActive(true);
    }

    private IEnumerator SecuenciaCuadroActivado()
    {
        yield return ReproducirLinea(vozCuadroActivado, textoCuadroActivado);
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