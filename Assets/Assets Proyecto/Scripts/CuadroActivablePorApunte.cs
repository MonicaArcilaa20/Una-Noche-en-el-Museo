using System.Collections;
using UnityEngine;

public class CuadroActivablePorApunte : MonoBehaviour
{
    [Header("Activación")]
    [SerializeField] private float tiempoNecesario = 2f;
    [SerializeField] private float anguloMaximo = 35f;
    [SerializeField] private float velocidadPerdida = 1.5f;

    [Header("Bloqueo por tinta")]
    [SerializeField] private GameObject senaleticaSinTinta;
    [SerializeField] private bool resetearProgresoSinTinta = true;

    [Header("Feedback sin tinta")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoSinTinta;
    [Range(0f, 1f)]
    [SerializeField] private float volumenSonidoSinTinta = 1f;
    [SerializeField] private float cooldownFeedbackSinTinta = 1.25f;
    [SerializeField] private float duracionParpadeo = 0.5f;
    [SerializeField] private float intervaloParpadeo = 0.15f;

    [Header("Referencias visuales")]
    [SerializeField] private Transform objetivoCentro;
    [SerializeField] private GameObject selloMagico;
    [SerializeField] private ParticleSystem particulas;
    [SerializeField] private LineRenderer lineaMagica;
    [SerializeField] private Renderer rendererCuadro;
    [SerializeField] private string propiedadEmision = "_EmissionColor";
    [SerializeField] private Color colorEmisionReposo = Color.black;
    [SerializeField] private Color colorEmisionActivo = Color.cyan;

    [Header("Flores oníricas opcionales")]
    [SerializeField] private GameObject grupoFloresOniricas;
    [SerializeField] private Animator[] animadoresFlores;
    [SerializeField] private string estadoBloomFlor = "Scene";
    [SerializeField] private bool ocultarFloresCuandoNoApunta = true;
    [SerializeField] private bool resetearFloresAlCancelar = true;
    [SerializeField] private bool mantenerFloresAbiertasAlCompletar = true;

    [Header("Transición")]
    [SerializeField] private string nombreEscenaSiguiente;
    [SerializeField] private TransicionEscena transicionEscena;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = true;

    private ControlPincel pincelActual;
    private float progresoActual = 0f;
    private bool completado = false;
    private Material materialInstancia;

    private float ultimoTiempoFeedbackSinTinta = -999f;
    private Coroutine corutinaParpadeo;

    private void Start()
    {
        if (selloMagico != null)
            selloMagico.SetActive(false);

        if (particulas != null)
            particulas.Stop();

        if (lineaMagica != null)
            lineaMagica.enabled = false;

        if (senaleticaSinTinta != null)
            senaleticaSinTinta.SetActive(false);

        if (rendererCuadro != null)
        {
            materialInstancia = rendererCuadro.material;
            materialInstancia.EnableKeyword("_EMISSION");
            materialInstancia.SetColor(propiedadEmision, colorEmisionReposo);
        }

        PrepararFloresOniricas();
    }

    private void Update()
    {
        if (completado)
            return;

        bool activando = false;
        bool bloqueadoPorFaltaDeTinta = false;

        if (pincelActual != null && objetivoCentro != null && pincelActual.OrigenMagia != null)
        {
            Transform origen = pincelActual.OrigenMagia;
            Vector3 direccionAlCentro = (objetivoCentro.position - origen.position).normalized;
            float angulo = Vector3.Angle(origen.forward, direccionAlCentro);

            if (angulo <= anguloMaximo)
            {
                if (TieneTintaDisponible())
                    activando = true;
                else
                    bloqueadoPorFaltaDeTinta = true;
            }
        }

        if (bloqueadoPorFaltaDeTinta)
        {
            DispararFeedbackSinTinta();

            if (resetearProgresoSinTinta)
                progresoActual = 0f;
            else
                progresoActual = Mathf.Max(0f, progresoActual - Time.deltaTime * velocidadPerdida);

            if (pincelActual != null)
                pincelActual.ApagarPincel();
        }
        else if (activando)
        {
            progresoActual += Time.deltaTime;

            if (pincelActual != null)
                pincelActual.EncenderPincel();

            if (mostrarLogs)
                Debug.Log("Activando cuadro...", this);
        }
        else
        {
            progresoActual = Mathf.Max(0f, progresoActual - Time.deltaTime * velocidadPerdida);

            if (pincelActual != null)
                pincelActual.ApagarPincel();
        }

        float progresoNormalizado = Mathf.Clamp01(progresoActual / tiempoNecesario);
        ActualizarVisuales(progresoNormalizado, activando);

        if (progresoActual >= tiempoNecesario)
            CompletarActivacion();
    }

    private bool TieneTintaDisponible()
    {
        if (pincelActual == null)
            return false;

        PincelTinta tinta = pincelActual.GetComponent<PincelTinta>();

        if (tinta == null)
        {
            if (mostrarLogs)
                Debug.LogWarning("El pincel no tiene componente PincelTinta.", pincelActual);

            return false;
        }

        return tinta.TieneTinta();
    }

    private void DispararFeedbackSinTinta()
    {
        if (Time.time < ultimoTiempoFeedbackSinTinta + cooldownFeedbackSinTinta)
            return;

        ultimoTiempoFeedbackSinTinta = Time.time;

        if (mostrarLogs)
            Debug.Log("El cuadro no puede activarse: el pincel no tiene tinta.", this);

        if (audioSource != null && sonidoSinTinta != null)
            audioSource.PlayOneShot(sonidoSinTinta, volumenSonidoSinTinta);
        else if (sonidoSinTinta != null)
            AudioSource.PlayClipAtPoint(sonidoSinTinta, transform.position, volumenSonidoSinTinta);

        if (corutinaParpadeo != null)
            StopCoroutine(corutinaParpadeo);

        corutinaParpadeo = StartCoroutine(RutinaParpadeoSenaletica());
    }

    private IEnumerator RutinaParpadeoSenaletica()
    {

        if (senaleticaSinTinta == null)
        {
            corutinaParpadeo = null;
            yield break;
        }

        float tiempo = 0f;
        bool visible = true;

        while (tiempo < duracionParpadeo)
        {
            senaleticaSinTinta.SetActive(visible);
            visible = !visible;

            yield return new WaitForSeconds(intervaloParpadeo);
            tiempo += intervaloParpadeo;
        }

        senaleticaSinTinta.SetActive(false);
        corutinaParpadeo = null;
    }

    private void ActualizarVisuales(float progreso, bool activando)
    {
        if (selloMagico != null)
        {
            selloMagico.SetActive(activando || progreso > 0.01f);
            float escala = Mathf.Lerp(0.2f, 1.1f, progreso);
            selloMagico.transform.localScale = Vector3.one * escala;
        }

        if (particulas != null)
        {
            if (activando && !particulas.isPlaying)
                particulas.Play();
            else if (!activando && particulas.isPlaying)
                particulas.Stop();
        }

        if (lineaMagica != null && pincelActual != null && objetivoCentro != null)
        {
            lineaMagica.enabled = activando;

            if (activando)
            {
                lineaMagica.positionCount = 2;
                lineaMagica.SetPosition(0, pincelActual.OrigenMagia.position);
                lineaMagica.SetPosition(1, objetivoCentro.position);
            }
        }

        if (materialInstancia != null)
        {
            Color colorActual = Color.Lerp(colorEmisionReposo, colorEmisionActivo, progreso);
            materialInstancia.SetColor(propiedadEmision, colorActual);
        }

        ActualizarFloresOniricas(progreso, activando);
    }

    private void CompletarActivacion()
    {
        if (completado)
            return;

        completado = true;

        if (mostrarLogs)
            Debug.Log("Cuadro activado completamente.", this);

        if (lineaMagica != null)
            lineaMagica.enabled = false;

        if (senaleticaSinTinta != null)
            senaleticaSinTinta.SetActive(false);

        if (corutinaParpadeo != null)
        {
            StopCoroutine(corutinaParpadeo);
            corutinaParpadeo = null;
        }

        if (mantenerFloresAbiertasAlCompletar)
            ForzarFloresCompletas();

        StartCoroutine(RutinaFinalizacion());
    }

    private IEnumerator RutinaFinalizacion()
    {
        yield return new WaitForSeconds(0.5f);

        if (transicionEscena != null)
            transicionEscena.IniciarTransicion(nombreEscenaSiguiente);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscenaSiguiente);
    }

    public void RegistrarPincel(ControlPincel pincel)
    {
        pincelActual = pincel;

        if (mostrarLogs)
            Debug.Log("Pincel entró en la zona del cuadro.", this);
    }

    public void QuitarPincel(ControlPincel pincel)
    {
        if (pincelActual == pincel)
        {
            pincelActual.ApagarPincel();
            pincelActual = null;

            if (senaleticaSinTinta != null)
                senaleticaSinTinta.SetActive(false);

            if (corutinaParpadeo != null)
            {
                StopCoroutine(corutinaParpadeo);
                corutinaParpadeo = null;
            }

            ActualizarFloresOniricas(0f, false);

            if (mostrarLogs)
                Debug.Log("Pincel salió de la zona del cuadro.", this);
        }
    }

    private void PrepararFloresOniricas()
    {
        if (grupoFloresOniricas == null)
            return;

        grupoFloresOniricas.SetActive(false);
    }

    private void ActualizarFloresOniricas(float progresoNormalizado, bool intentandoActivar)
    {
        if (grupoFloresOniricas == null || animadoresFlores == null || animadoresFlores.Length == 0)
            return;

        if (completado && mantenerFloresAbiertasAlCompletar)
        {
            ForzarFloresCompletas();
            return;
        }

        if (intentandoActivar)
        {
            if (!grupoFloresOniricas.activeSelf)
                grupoFloresOniricas.SetActive(true);

            progresoNormalizado = Mathf.Clamp01(progresoNormalizado);

            for (int i = 0; i < animadoresFlores.Length; i++)
            {
                if (animadoresFlores[i] == null)
                    continue;

                animadoresFlores[i].speed = 0f;
                animadoresFlores[i].Play(estadoBloomFlor, 0, progresoNormalizado);
                animadoresFlores[i].Update(0f);
            }
        }
        else
        {
            if (resetearFloresAlCancelar)
            {
                bool estabaActivo = grupoFloresOniricas.activeSelf;

                if (!estabaActivo)
                    grupoFloresOniricas.SetActive(true);

                for (int i = 0; i < animadoresFlores.Length; i++)
                {
                    if (animadoresFlores[i] == null)
                        continue;

                    animadoresFlores[i].speed = 0f;
                    animadoresFlores[i].Play(estadoBloomFlor, 0, 0f);
                    animadoresFlores[i].Update(0f);
                }

                if (!estabaActivo && ocultarFloresCuandoNoApunta)
                    grupoFloresOniricas.SetActive(false);
            }

            if (ocultarFloresCuandoNoApunta)
                grupoFloresOniricas.SetActive(false);
        }
    }

    private void ForzarFloresCompletas()
    {
        if (grupoFloresOniricas == null || animadoresFlores == null || animadoresFlores.Length == 0)
            return;

        if (!grupoFloresOniricas.activeSelf)
            grupoFloresOniricas.SetActive(true);

        for (int i = 0; i < animadoresFlores.Length; i++)
        {
            if (animadoresFlores[i] == null)
                continue;

            animadoresFlores[i].speed = 0f;
            animadoresFlores[i].Play(estadoBloomFlor, 0, 1f);
            animadoresFlores[i].Update(0f);
        }
    }
}