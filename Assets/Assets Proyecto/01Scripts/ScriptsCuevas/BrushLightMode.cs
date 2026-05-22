using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ControlPincel))]
[RequireComponent(typeof(PincelTinta))]
public class BrushLightMode : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ControlPincel controlPincel;
    [SerializeField] private PincelTinta pincelTinta;
    [SerializeField] private Light luzPincel;
    [SerializeField] private Transform puntoLuzOverride;

    [Header("Condiciones")]
    [SerializeField] private bool requiereEquipado = true;
    [SerializeField] private bool requiereActivo = true;
    [SerializeField] private float tintaMinima = 0.01f;

    [Header("Modo de prueba")]
    [SerializeField] private bool permitirModoPrueba = true;
    [SerializeField] private bool usarTeclaPruebaEnEditor = true;
    [SerializeField] private Key teclaPrueba = Key.L;
    [SerializeField] private bool luzPruebaActiva = false;

    [Header("Intensidad")]
    [SerializeField] private float intensidadApagada = 0f;
    [SerializeField] private float intensidadEncendidaMaxima = 7f;
    [SerializeField] private float intensidadMinimaConPocaTinta = 2f;
    [SerializeField] private float velocidadCambio = 8f;

    [Header("Opciones")]
    [SerializeField] private bool seguirOrigenMagia = true;
    [SerializeField] private bool modularIntensidadConTinta = true;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = true;

    private bool estadoAnteriorIluminando = false;

    public bool EstaIluminando { get; private set; }

    public Transform PuntoLuzActual
    {
        get
        {
            if (puntoLuzOverride != null)
                return puntoLuzOverride;

            if (controlPincel != null && controlPincel.OrigenMagia != null)
                return controlPincel.OrigenMagia;

            if (luzPincel != null)
                return luzPincel.transform;

            return transform;
        }
    }

    private void Reset()
    {
        if (controlPincel == null)
            controlPincel = GetComponent<ControlPincel>();

        if (pincelTinta == null)
            pincelTinta = GetComponent<PincelTinta>();
    }

    private void Awake()
    {
        if (controlPincel == null)
            controlPincel = GetComponent<ControlPincel>();

        if (pincelTinta == null)
            pincelTinta = GetComponent<PincelTinta>();

        if (luzPincel != null)
        {
            luzPincel.enabled = true;
            luzPincel.intensity = intensidadApagada;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (permitirModoPrueba && usarTeclaPruebaEnEditor && Keyboard.current != null && Keyboard.current[teclaPrueba].wasPressedThisFrame)
        {
            luzPruebaActiva = !luzPruebaActiva;

            if (mostrarLogs)
                Debug.Log("[BrushLightMode] luzPruebaActiva = " + luzPruebaActiva, this);
        }
#endif

        EstaIluminando = PuedeIluminar();

        if (seguirOrigenMagia && luzPincel != null)
        {
            Transform refTransform = PuntoLuzActual;
            luzPincel.transform.position = refTransform.position;
            luzPincel.transform.rotation = refTransform.rotation;
        }

        ActualizarIntensidad();
        ReportarCambioEstado();
    }

    private bool PuedeIluminar()
    {
        if (permitirModoPrueba && luzPruebaActiva)
            return true;

        if (controlPincel == null)
        {
            if (mostrarLogs) Debug.LogWarning("[BrushLightMode] controlPincel es null", this);
            return false;
        }

        if (pincelTinta == null)
        {
            if (mostrarLogs) Debug.LogWarning("[BrushLightMode] pincelTinta es null", this);
            return false;
        }

        if (luzPincel == null)
        {
            if (mostrarLogs) Debug.LogWarning("[BrushLightMode] luzPincel es null", this);
            return false;
        }

        if (requiereEquipado && !controlPincel.EstaEquipado)
            return false;

        if (requiereActivo && !controlPincel.EstaActivo)
            return false;

        if (!pincelTinta.TieneTinta(tintaMinima))
            return false;

        return true;
    }

    private void ActualizarIntensidad()
    {
        if (luzPincel == null)
            return;

        float intensidadObjetivo = intensidadApagada;

        if (EstaIluminando)
        {
            if (modularIntensidadConTinta && pincelTinta != null)
            {
                float t = Mathf.Clamp01(pincelTinta.TintaNormalizada);
                intensidadObjetivo = Mathf.Lerp(intensidadMinimaConPocaTinta, intensidadEncendidaMaxima, t);
            }
            else
            {
                intensidadObjetivo = intensidadEncendidaMaxima;
            }
        }

        luzPincel.intensity = Mathf.Lerp(
            luzPincel.intensity,
            intensidadObjetivo,
            Time.deltaTime * velocidadCambio
        );
    }

    private void ReportarCambioEstado()
    {
        if (EstaIluminando == estadoAnteriorIluminando)
            return;

        estadoAnteriorIluminando = EstaIluminando;

        if (mostrarLogs)
            Debug.Log("[BrushLightMode] EstaIluminando = " + EstaIluminando, this);
    }
}