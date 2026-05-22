using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ControlPincel : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool estaAgarrado = false;
    [SerializeField] private bool estaActivo = false;
    [SerializeField] private bool estaEquipado = false;

    [Header("Equipamiento")]
    [SerializeField] private Transform anclaManoDerecha;
    [SerializeField] private Transform origenMagia;

    [Header("Raíces visuales opcionales")]
    [SerializeField] private Transform visualsRoot;
    [SerializeField] private bool forzarEscalaLocalAlEquipar = true;
    [SerializeField] private bool forzarEscalaLocalEnLateUpdate = true;

    [Header("Mano derecha")]
    [SerializeField] private Animator animadorManoDerecha;
    [SerializeField] private MonoBehaviour controladorGripManoDerecha;
    [SerializeField] private string parametroGrip = "Grip";
    [SerializeField] private float valorGripCerrado = 1f;

    [Header("XR Grab")]
    [SerializeField] private bool desactivarDynamicAttachAlIniciar = true;
    [SerializeField] private bool desactivarMatchAttachAlIniciar = true;
    [SerializeField] private bool desactivarGrabInteractableAlEquipar = true;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = true;

    [Header("Eventos")]
    public UnityEvent alAgarrar;
    public UnityEvent alEncender;
    public UnityEvent alApagar;
    public UnityEvent alEquipar;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private PincelTinta sistemaTinta;
    private PincelPersistenciaEscena persistencia;

    private Vector3 escalaLocalInicialRaiz;
    private Vector3 escalaLocalInicialVisuals;
    private bool escalaInicialCapturadaVisuals = false;

    public bool EstaAgarrado => estaAgarrado;
    public bool EstaActivo => estaActivo;
    public bool EstaEquipado => estaEquipado;
    public Transform OrigenMagia => origenMagia != null ? origenMagia : transform;

    private void Reset()
    {
        if (visualsRoot == null)
        {
            Transform hijoVisuals = transform.Find("Visuals");
            if (hijoVisuals != null)
                visualsRoot = hijoVisuals;
        }
    }

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        sistemaTinta = GetComponent<PincelTinta>();
        persistencia = GetComponent<PincelPersistenciaEscena>();

        escalaLocalInicialRaiz = transform.localScale;

        if (visualsRoot != null)
        {
            escalaLocalInicialVisuals = visualsRoot.localScale;
            escalaInicialCapturadaVisuals = true;
        }

        rb.useGravity = false;

        if (grabInteractable != null)
        {
            if (desactivarDynamicAttachAlIniciar)
                grabInteractable.useDynamicAttach = false;

            if (desactivarMatchAttachAlIniciar)
            {
                grabInteractable.matchAttachPosition = false;
                grabInteractable.matchAttachRotation = false;
            }
        }
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.AddListener(CuandoSeAgarra);
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveListener(CuandoSeAgarra);
    }

    private void CuandoSeAgarra(SelectEnterEventArgs args)
    {
        if (estaEquipado)
            return;

        estaAgarrado = true;

        if (mostrarLogs)
            Debug.Log("[ControlPincel] Pincel agarrado por primera vez", this);

        alAgarrar?.Invoke();
        StartCoroutine(EquiparAlSiguienteFrame());
    }

    private IEnumerator EquiparAlSiguienteFrame()
    {
        yield return null;
        EquiparEnManoDerecha();
    }

    private void LateUpdate()
    {
        if (!estaEquipado || anclaManoDerecha == null)
            return;

        transform.position = anclaManoDerecha.position;
        transform.rotation = anclaManoDerecha.rotation;

        if (forzarEscalaLocalEnLateUpdate)
            RestaurarEscalasLocales();
    }

    public void EncenderPincel()
    {
        if (estaActivo)
            return;

        if (sistemaTinta != null && !sistemaTinta.TieneTinta())
        {
            if (mostrarLogs)
                Debug.Log("[ControlPincel] No se puede encender el pincel: sin tinta", this);
            return;
        }

        estaActivo = true;

        if (mostrarLogs)
            Debug.Log("[ControlPincel] Pincel encendido", this);

        alEncender?.Invoke();
        persistencia?.GuardarAhora();
    }

    public void ApagarPincel()
    {
        if (!estaActivo)
            return;

        estaActivo = false;

        if (mostrarLogs)
            Debug.Log("[ControlPincel] Pincel apagado", this);

        alApagar?.Invoke();
        persistencia?.GuardarAhora();
    }

    public void EquiparEnManoDerecha()
    {
        if (estaEquipado)
            return;

        if (anclaManoDerecha == null)
        {
            Debug.LogError("[ControlPincel] Falta asignar anclaManoDerecha.", this);
            return;
        }

        estaEquipado = true;
        estaAgarrado = true;

        rb.useGravity = false;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector3.zero;
#else
        rb.velocity = Vector3.zero;
#endif
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Importante: false para adoptar el espacio local del ancla sin heredar poses raras.
        transform.SetParent(anclaManoDerecha, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (forzarEscalaLocalAlEquipar)
            RestaurarEscalasLocales();

        if (desactivarGrabInteractableAlEquipar && grabInteractable != null)
            grabInteractable.enabled = false;

        if (controladorGripManoDerecha != null)
            controladorGripManoDerecha.enabled = false;

        if (animadorManoDerecha != null)
            animadorManoDerecha.SetFloat(parametroGrip, valorGripCerrado);

        if (mostrarLogs)
        {
            Debug.Log("[ControlPincel] Pincel equipado permanentemente en la mano derecha", this);
            Debug.Log("[ControlPincel] Escala raíz actual: " + transform.localScale, this);

            if (visualsRoot != null)
                Debug.Log("[ControlPincel] Escala visuals actual: " + visualsRoot.localScale, this);
        }

        alEquipar?.Invoke();
        persistencia?.GuardarAhora();
    }

    public void ReasignarAnclaManoDerecha(Transform nuevaAncla)
    {
        anclaManoDerecha = nuevaAncla;

        if (mostrarLogs && nuevaAncla != null)
            Debug.Log("[ControlPincel] Nueva ancla asignada: " + nuevaAncla.name, this);
    }

    private void RestaurarEscalasLocales()
    {
        transform.localScale = escalaLocalInicialRaiz;

        if (visualsRoot != null && escalaInicialCapturadaVisuals)
            visualsRoot.localScale = escalaLocalInicialVisuals;
    }
}