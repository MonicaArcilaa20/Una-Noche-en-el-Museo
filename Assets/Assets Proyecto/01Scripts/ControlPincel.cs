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

    [Header("Mano derecha")]
    [SerializeField] private Animator animadorManoDerecha;
    [SerializeField] private MonoBehaviour controladorGripManoDerecha;
    [SerializeField] private string parametroGrip = "Grip";
    [SerializeField] private float valorGripCerrado = 1f;

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

    public bool EstaAgarrado => estaAgarrado;
    public bool EstaActivo => estaActivo;
    public bool EstaEquipado => estaEquipado;
    public Transform OrigenMagia => origenMagia != null ? origenMagia : transform;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        sistemaTinta = GetComponent<PincelTinta>();
        persistencia = GetComponent<PincelPersistenciaEscena>();

        rb.useGravity = false;
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(CuandoSeAgarra);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(CuandoSeAgarra);
    }

    private void CuandoSeAgarra(SelectEnterEventArgs args)
    {
        if (estaEquipado)
            return;

        estaAgarrado = true;

        if (mostrarLogs)
            Debug.Log("Pincel agarrado por primera vez");

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
    }

    public void EncenderPincel()
    {
        if (estaActivo)
            return;

        if (sistemaTinta != null && !sistemaTinta.TieneTinta())
        {
            if (mostrarLogs)
                Debug.Log("No se puede encender el pincel: sin tinta");
            return;
        }

        estaActivo = true;

        if (mostrarLogs)
            Debug.Log("Pincel encendido");

        alEncender?.Invoke();
        persistencia?.GuardarAhora();
    }

    public void ApagarPincel()
    {
        if (!estaActivo)
            return;

        estaActivo = false;

        if (mostrarLogs)
            Debug.Log("Pincel apagado");

        alApagar?.Invoke();
        persistencia?.GuardarAhora();
    }

    public void EquiparEnManoDerecha()
    {
        if (estaEquipado)
            return;

        if (anclaManoDerecha == null)
        {
            Debug.LogError("Falta asignar anclaManoDerecha en ControlPincel.");
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

        transform.SetParent(anclaManoDerecha, true);
        transform.position = anclaManoDerecha.position;
        transform.rotation = anclaManoDerecha.rotation;

        grabInteractable.enabled = false;

        if (controladorGripManoDerecha != null)
            controladorGripManoDerecha.enabled = false;

        if (animadorManoDerecha != null)
            animadorManoDerecha.SetFloat(parametroGrip, valorGripCerrado);

        if (mostrarLogs)
            Debug.Log("Pincel equipado permanentemente en la mano derecha");

        alEquipar?.Invoke();
        persistencia?.GuardarAhora();
    }
}