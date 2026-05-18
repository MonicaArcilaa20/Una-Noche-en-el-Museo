using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(Collider))]
public class BarcaEmbarqueXR : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private XROrigin xrOriginObjetivo;
    [SerializeField] private CharacterController characterControllerObjetivo;
    [SerializeField] private Transform seatAnchor;

    [Header("Locomoción a desactivar")]
    [SerializeField] private MonoBehaviour[] locomocionesADesactivar;

    [Header("Input VR")]
    [SerializeField] private InputActionReference accionConfirmarEmbarqueIzquierda;
    [SerializeField] private InputActionReference accionConfirmarEmbarqueDerecha;

    [Header("Debug Editor")]
    [SerializeField] private Key debugKey = Key.B;

    [Header("Prompt visual")]
    [SerializeField] private GameObject promptSubirBarco;

    [Header("Opciones")]
    [SerializeField] private bool alinearYawConAsiento = false;
    [SerializeField] private bool mostrarLogs = true;
    [SerializeField] private string idTramoBarca = "Pueblo";

    private bool jugadorEnZona = false;
    private bool jugadorAbordo = false;

    private Collider triggerEmbarque;

    public bool JugadorAbordo => jugadorAbordo;
    public XROrigin XrOriginActual => xrOriginObjetivo;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        if (accionConfirmarEmbarqueIzquierda != null && accionConfirmarEmbarqueIzquierda.action != null)
            accionConfirmarEmbarqueIzquierda.action.Enable();

        if (accionConfirmarEmbarqueDerecha != null && accionConfirmarEmbarqueDerecha.action != null)
            accionConfirmarEmbarqueDerecha.action.Enable();
    }

    private void OnDisable()
    {
        if (accionConfirmarEmbarqueIzquierda != null && accionConfirmarEmbarqueIzquierda.action != null)
            accionConfirmarEmbarqueIzquierda.action.Disable();

        if (accionConfirmarEmbarqueDerecha != null && accionConfirmarEmbarqueDerecha.action != null)
            accionConfirmarEmbarqueDerecha.action.Disable();
    }

    private void Start()
    {
        triggerEmbarque = GetComponent<Collider>();
        
        if (promptSubirBarco != null)
            promptSubirBarco.SetActive(false);

            
    }

    private void Update()
    {
        if (!jugadorEnZona || jugadorAbordo)
            return;

        bool confirmar = false;

        if (accionConfirmarEmbarqueIzquierda != null && accionConfirmarEmbarqueIzquierda.action != null)
            confirmar |= accionConfirmarEmbarqueIzquierda.action.WasPressedThisFrame();

        if (accionConfirmarEmbarqueDerecha != null && accionConfirmarEmbarqueDerecha.action != null)
            confirmar |= accionConfirmarEmbarqueDerecha.action.WasPressedThisFrame();

#if UNITY_EDITOR
        if (!confirmar && Keyboard.current != null && Keyboard.current[debugKey].wasPressedThisFrame)
            confirmar = true;
#endif

        if (confirmar)
            Embarcar();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (jugadorAbordo)
            return;

        if (characterControllerObjetivo == null)
            return;

        if (other != characterControllerObjetivo)
            return;

        jugadorEnZona = true;

        if (promptSubirBarco != null)
            promptSubirBarco.SetActive(true);

        if (mostrarLogs)
            Debug.Log("Jugador en zona de embarque.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (characterControllerObjetivo == null)
            return;

        if (other != characterControllerObjetivo)
            return;

        jugadorEnZona = false;

        if (promptSubirBarco != null)
            promptSubirBarco.SetActive(false);

        if (mostrarLogs)
            Debug.Log("Jugador salió de la zona de embarque.");
    }

    public void ActivarTriggerEmbarque(bool activo)
    {
        if (triggerEmbarque != null)
            triggerEmbarque.enabled = activo;

        if (!activo && promptSubirBarco != null)
            promptSubirBarco.SetActive(false);
    }

    public void Embarcar()
    {
        if (jugadorAbordo)
            return;

        if (xrOriginObjetivo == null || characterControllerObjetivo == null || seatAnchor == null)
        {
            Debug.LogWarning("Faltan referencias en BarcaEmbarqueXR.");
            return;
        }

        jugadorAbordo = true;
        jugadorEnZona = false;

        if (promptSubirBarco != null)
            promptSubirBarco.SetActive(false);

        foreach (MonoBehaviour locomocion in locomocionesADesactivar)
        {
            if (locomocion != null)
                locomocion.enabled = false;
        }

        characterControllerObjetivo.enabled = false;
        RecolocarEnAsiento();
        characterControllerObjetivo.enabled = true;

        EstadoGlobalBarca.Instance?.MarcarAbordo(idTramoBarca);

        if (mostrarLogs)
            Debug.Log("Jugador embarcado correctamente.");
    }

    public void Desembarcar()
    {
        if (!jugadorAbordo || xrOriginObjetivo == null || characterControllerObjetivo == null)
            return;

        characterControllerObjetivo.enabled = false;

        foreach (MonoBehaviour locomocion in locomocionesADesactivar)
        {
            if (locomocion != null)
                locomocion.enabled = true;
        }

        characterControllerObjetivo.enabled = true;

        jugadorAbordo = false;

        if (promptSubirBarco != null)
            promptSubirBarco.SetActive(false);

        EstadoGlobalBarca.Instance?.BajarDeBarca();

        if (mostrarLogs)
            Debug.Log("Jugador desembarcado.");
    }

    private void AlinearYawConAsiento()
    {
        Transform camara = xrOriginObjetivo.Camera != null ? xrOriginObjetivo.Camera.transform : Camera.main != null ? Camera.main.transform : null;
        if (camara == null)
            return;

        Vector3 forwardCamara = camara.forward;
        forwardCamara.y = 0f;

        Vector3 forwardObjetivo = seatAnchor.forward;
        forwardObjetivo.y = 0f;

        if (forwardCamara.sqrMagnitude < 0.001f || forwardObjetivo.sqrMagnitude < 0.001f)
            return;

        float angulo = Vector3.SignedAngle(forwardCamara, forwardObjetivo, Vector3.up);
        xrOriginObjetivo.transform.RotateAround(camara.position, Vector3.up, angulo);
    }

    public void RecolocarEnAsiento()
    {
        if (xrOriginObjetivo == null || characterControllerObjetivo == null || seatAnchor == null)
            return;

        bool ccEstabaActivo = characterControllerObjetivo.enabled;
        characterControllerObjetivo.enabled = false;

        xrOriginObjetivo.MoveCameraToWorldLocation(seatAnchor.position);

        if (alinearYawConAsiento)
            AlinearYawConAsiento();

        characterControllerObjetivo.enabled = ccEstabaActivo;
    }

}