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

    [Header("Overlay de ondas")]
    [SerializeField] private OverlayMovimientoCuadro overlayOndas;
    [SerializeField] private bool mostrarOndasSoloConTinta = true;

    [Header("Flores oníricas opcionales")]
    [SerializeField] private GameObject grupoFloresOniricas;
    [SerializeField] private Animator[] animadoresFlores;
    [SerializeField] private string estadoBloomFlor = "Bloom";
    [SerializeField] private bool ocultarFloresCuandoNoApunta = true;
    [SerializeField] private bool resetearFloresAlCancelar = true;
    [SerializeField] private bool mantenerFloresAbiertasAlCompletar = true;

    [Header("Difuminado / aparición suave de flores")]
    [SerializeField] private bool usarAparicionSuaveFlores = true;
    [SerializeField] private float duracionDifuminadoFlores = 0.35f;
    [SerializeField, Range(0f, 1f)] private float escalaInicialFlores = 0.82f;
    [SerializeField, Range(0f, 1f)] private float alphaInicialFlores = 0f;
    [SerializeField] private bool intentarControlarAlphaMateriales = true;

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

    private int hashEstadoBloomFlor;

    private float visibilidadFloresActual = 0f;
    private Transform[] floresTransform;
    private Vector3[] escalasOriginalesFlores;
    private Renderer[] renderersFlores;
    private MaterialPropertyBlock propertyBlockFlores;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");

    private void Start()
    {
        hashEstadoBloomFlor = Animator.StringToHash(estadoBloomFlor);

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

        if (overlayOndas != null)
            overlayOndas.DesactivarOndasInstantaneo();

        PrepararFloresOniricas();
    }

    private void Update()
    {
        if (completado)
            return;

        bool apuntandoAlCuadro = false;
        bool activando = false;
        bool bloqueadoPorFaltaDeTinta = false;

        if (pincelActual != null && objetivoCentro != null && pincelActual.OrigenMagia != null)
        {
            Transform origen = pincelActual.OrigenMagia;
            Vector3 direccionAlCentro = (objetivoCentro.position - origen.position).normalized;
            float angulo = Vector3.Angle(origen.forward, direccionAlCentro);

            if (angulo <= anguloMaximo)
            {
                apuntandoAlCuadro = true;

                if (TieneTintaDisponible())
                    activando = true;
                else
                    bloqueadoPorFaltaDeTinta = true;
            }
        }

        ActualizarOverlayOndas(apuntandoAlCuadro, bloqueadoPorFaltaDeTinta);

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

    private void ActualizarOverlayOndas(bool apuntandoAlCuadro, bool bloqueadoPorFaltaDeTinta)
    {
        if (overlayOndas == null)
            return;

        bool mostrar = apuntandoAlCuadro;

        if (mostrarOndasSoloConTinta && bloqueadoPorFaltaDeTinta)
            mostrar = false;

        overlayOndas.SetActivo(mostrar);
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

        if (overlayOndas != null && !mantenerFloresAbiertasAlCompletar)
            overlayOndas.DesactivarOndas();

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

            if (overlayOndas != null)
                overlayOndas.DesactivarOndas();

            ActualizarFloresOniricas(0f, false);

            if (mostrarLogs)
                Debug.Log("Pincel salió de la zona del cuadro.", this);
        }
    }

    private void PrepararFloresOniricas()
    {
        if (grupoFloresOniricas == null)
            return;

        int cantidadHijos = grupoFloresOniricas.transform.childCount;

        floresTransform = new Transform[cantidadHijos];
        escalasOriginalesFlores = new Vector3[cantidadHijos];

        for (int i = 0; i < cantidadHijos; i++)
        {
            Transform flor = grupoFloresOniricas.transform.GetChild(i);
            floresTransform[i] = flor;
            escalasOriginalesFlores[i] = flor.localScale;
        }

        renderersFlores = grupoFloresOniricas.GetComponentsInChildren<Renderer>(true);
        propertyBlockFlores = new MaterialPropertyBlock();

        visibilidadFloresActual = 0f;

        grupoFloresOniricas.SetActive(true);
        AplicarVisibilidadFlores(0f);

        if (ocultarFloresCuandoNoApunta)
            grupoFloresOniricas.SetActive(false);
    }

    private void ActualizarFloresOniricas(float progresoNormalizado, bool intentandoActivar)
    {
        if (grupoFloresOniricas == null || animadoresFlores == null || animadoresFlores.Length == 0)
            return;

        float objetivoVisibilidad = 0f;

        if (completado && mantenerFloresAbiertasAlCompletar)
        {
            objetivoVisibilidad = 1f;

            if (!grupoFloresOniricas.activeSelf)
                grupoFloresOniricas.SetActive(true);

            for (int i = 0; i < animadoresFlores.Length; i++)
                AplicarProgresoFlor(animadoresFlores[i], 1f);

            ActualizarVisibilidadSuaveFlores(objetivoVisibilidad);
            return;
        }

        if (intentandoActivar)
        {
            if (!grupoFloresOniricas.activeSelf)
                grupoFloresOniricas.SetActive(true);

            progresoNormalizado = Mathf.Clamp01(progresoNormalizado);

            for (int i = 0; i < animadoresFlores.Length; i++)
                AplicarProgresoFlor(animadoresFlores[i], progresoNormalizado);

            objetivoVisibilidad = progresoNormalizado;
        }
        else
        {
            if (resetearFloresAlCancelar)
            {
                bool estabaActivo = grupoFloresOniricas.activeSelf;

                if (!estabaActivo)
                    grupoFloresOniricas.SetActive(true);

                for (int i = 0; i < animadoresFlores.Length; i++)
                    AplicarProgresoFlor(animadoresFlores[i], 0f);

                if (!estabaActivo && ocultarFloresCuandoNoApunta && !usarAparicionSuaveFlores)
                    grupoFloresOniricas.SetActive(false);
            }

            objetivoVisibilidad = 0f;
        }

        ActualizarVisibilidadSuaveFlores(objetivoVisibilidad);
    }

    private void ForzarFloresCompletas()
    {
        if (grupoFloresOniricas == null || animadoresFlores == null || animadoresFlores.Length == 0)
            return;

        if (!grupoFloresOniricas.activeSelf)
            grupoFloresOniricas.SetActive(true);

        for (int i = 0; i < animadoresFlores.Length; i++)
            AplicarProgresoFlor(animadoresFlores[i], 1f);

        visibilidadFloresActual = 1f;
        AplicarVisibilidadFlores(1f);
    }

    private void AplicarProgresoFlor(Animator animador, float progresoNormalizado)
    {
        if (animador == null)
            return;

        if (animador.runtimeAnimatorController == null)
        {
            if (mostrarLogs)
                Debug.LogWarning("Una flor no tiene Runtime Animator Controller asignado.", animador);
            return;
        }

        if (!animador.HasState(0, hashEstadoBloomFlor))
        {
            if (mostrarLogs)
            {
                Debug.LogWarning(
                    "El Animator '" + animador.gameObject.name + "' no tiene un estado llamado '" + estadoBloomFlor + "' en la capa 0.",
                    animador
                );
            }
            return;
        }

        progresoNormalizado = Mathf.Clamp01(progresoNormalizado);

        animador.speed = 0f;
        animador.Play(hashEstadoBloomFlor, 0, progresoNormalizado);
        animador.Update(0f);
    }

    private void ActualizarVisibilidadSuaveFlores(float objetivoVisibilidad)
    {
        if (grupoFloresOniricas == null)
            return;

        objetivoVisibilidad = Mathf.Clamp01(objetivoVisibilidad);

        if (!usarAparicionSuaveFlores)
        {
            visibilidadFloresActual = objetivoVisibilidad;
            AplicarVisibilidadFlores(visibilidadFloresActual);

            if (objetivoVisibilidad > 0.001f && !grupoFloresOniricas.activeSelf)
                grupoFloresOniricas.SetActive(true);
            else if (objetivoVisibilidad <= 0.001f && ocultarFloresCuandoNoApunta)
                grupoFloresOniricas.SetActive(false);

            return;
        }

        if (objetivoVisibilidad > 0.001f && !grupoFloresOniricas.activeSelf)
            grupoFloresOniricas.SetActive(true);

        float velocidad = duracionDifuminadoFlores <= 0.0001f
            ? 999f
            : 1f / duracionDifuminadoFlores;

        visibilidadFloresActual = Mathf.MoveTowards(
            visibilidadFloresActual,
            objetivoVisibilidad,
            velocidad * Time.deltaTime
        );

        AplicarVisibilidadFlores(visibilidadFloresActual);

        if (visibilidadFloresActual <= 0.001f &&
            objetivoVisibilidad <= 0.001f &&
            ocultarFloresCuandoNoApunta)
        {
            grupoFloresOniricas.SetActive(false);
        }
    }

    private void AplicarVisibilidadFlores(float t)
    {
        if (grupoFloresOniricas == null)
            return;

        float valorSuave = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

        if (floresTransform != null && escalasOriginalesFlores != null)
        {
            for (int i = 0; i < floresTransform.Length; i++)
            {
                if (floresTransform[i] == null)
                    continue;

                float factorEscala = Mathf.Lerp(escalaInicialFlores, 1f, valorSuave);
                floresTransform[i].localScale = escalasOriginalesFlores[i] * factorEscala;
            }
        }

        if (intentarControlarAlphaMateriales && renderersFlores != null)
        {
            for (int i = 0; i < renderersFlores.Length; i++)
            {
                Renderer rend = renderersFlores[i];

                if (rend == null || rend.sharedMaterial == null)
                    continue;

                if (!TryGetColorProperty(rend.sharedMaterial, out int colorPropertyId, out Color colorBase))
                    continue;

                float alpha = Mathf.Lerp(alphaInicialFlores, 1f, valorSuave);
                colorBase.a = alpha;

                propertyBlockFlores.Clear();
                rend.GetPropertyBlock(propertyBlockFlores);
                propertyBlockFlores.SetColor(colorPropertyId, colorBase);
                rend.SetPropertyBlock(propertyBlockFlores);
            }
        }
    }

    private bool TryGetColorProperty(Material material, out int propertyId, out Color colorBase)
    {
        propertyId = -1;
        colorBase = Color.white;

        if (material == null)
            return false;

        if (material.HasProperty(BaseColorID))
        {
            propertyId = BaseColorID;
            colorBase = material.GetColor(BaseColorID);
            return true;
        }

        if (material.HasProperty(ColorID))
        {
            propertyId = ColorID;
            colorBase = material.GetColor(ColorID);
            return true;
        }

        return false;
    }
}