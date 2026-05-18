using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BarcaMotorCanalizado : MonoBehaviour
{
    [System.Serializable]
    private struct PoseBarca
    {
        public Vector3 posicion;
        public Quaternion rotacion;

        public PoseBarca(Vector3 p, Quaternion r)
        {
            posicion = p;
            rotacion = r;
        }
    }

    [Header("Referencias")]
    [SerializeField] private BarcaEmbarqueXR embarqueXR;
    [SerializeField] private BoxCollider hullCastBox;
    [SerializeField] private Transform referenciaAvance;

    [Header("Input (APK / Gafas)")]
    [SerializeField] private InputActionReference accionMover; // Vector2

    [Header("Input Debug Editor")]
    [SerializeField] private bool usarTecladoDebugEnEditor = true;
    [SerializeField] private bool priorizarTecladoSobreAccionEnEditor = true;
    [SerializeField] private Key teclaAdelante = Key.I;
    [SerializeField] private Key teclaAtras = Key.K;
    [SerializeField] private Key teclaIzquierda = Key.J;
    [SerializeField] private Key teclaDerecha = Key.L;

    [Header("Movimiento")]
    [SerializeField] private float aceleracionAdelante = 1.5f;
    [SerializeField] private float velocidadMaxima = 2.5f;
    [SerializeField] private float desaceleracion = 2f;
    [SerializeField] private float velocidadGiro = 35f;
    [SerializeField] private float umbralInputHorizontal = 0.1f;
    [SerializeField] private bool girarSoloSiAvanza = true;
    [SerializeField] private bool permitirRetroceso = true;

    [Header("Corrección de Input")]
    [SerializeField] private bool invertirInputY = false;
    [SerializeField] private bool invertirInputX = false;

    [Header("Altura")]
    [SerializeField] private bool mantenerAlturaConstante = true;
    [SerializeField] private float alturaFijaY;

    [Header("Bloqueo")]
    [SerializeField] private LayerMask capasBloqueo;
    [SerializeField, Range(0.5f, 1f)] private float factorReduccionCaja = 0.9f;

    [Header("Historial seguro")]
    [SerializeField] private int maxPosesGuardadas = 12;
    [SerializeField] private int framesRetrocesoSeguridad = 4;
    [SerializeField] private float distanciaMinimaParaGuardar = 0.02f;
    [SerializeField] private float anguloMinimoParaGuardar = 1.5f;

    [Header("Estado global")]
    [SerializeField] private bool guardarEstadoGlobal = true;
    [SerializeField] private string idTramo = "Pueblo";

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = false;

    private Rigidbody rb;
    private float velocidadActual = 0f;
    private float inputXCache = 0f;
    private float inputYCache = 0f;

    private readonly List<PoseBarca> historialPoses = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (referenciaAvance == null)
            referenciaAvance = transform;

        if (mantenerAlturaConstante)
            alturaFijaY = transform.position.y;

        historialPoses.Clear();
        RegistrarPoseSiHaceFalta(true);
    }

    private void OnEnable()
    {
        if (accionMover != null && accionMover.action != null)
            accionMover.action.Enable();
    }

    private void OnDisable()
    {
        if (accionMover != null && accionMover.action != null)
            accionMover.action.Disable();
    }

    private void Update()
    {
        if (embarqueXR == null || !embarqueXR.JugadorAbordo)
        {
            inputXCache = 0f;
            inputYCache = 0f;
            return;
        }

        Vector2 input = LeerInputMovimiento();

        inputYCache = invertirInputY ? -input.y : input.y;
        inputXCache = invertirInputX ? -input.x : input.x;

        if (mostrarLogs)
            Debug.Log($"Input barca -> X:{inputXCache:F2} Y:{inputYCache:F2} | Vel:{velocidadActual:F2}");
    }

    private void FixedUpdate()
    {
        if (embarqueXR == null || !embarqueXR.JugadorAbordo)
            return;

        if (BloqueadoEnPose(transform.position, transform.rotation))
            RestaurarPoseSegura();

        float entradaAdelante = permitirRetroceso
            ? Mathf.Clamp(inputYCache, -1f, 1f)
            : Mathf.Clamp01(inputYCache);

        float entradaGiro = Mathf.Abs(inputXCache) >= umbralInputHorizontal ? inputXCache : 0f;

        if (Mathf.Abs(entradaAdelante) > 0.01f)
            velocidadActual = Mathf.MoveTowards(velocidadActual, entradaAdelante * velocidadMaxima, aceleracionAdelante * Time.fixedDeltaTime);
        else
            velocidadActual = Mathf.MoveTowards(velocidadActual, 0f, desaceleracion * Time.fixedDeltaTime);

        bool puedeGirar = !girarSoloSiAvanza || Mathf.Abs(velocidadActual) > 0.05f;

        if (puedeGirar && Mathf.Abs(entradaGiro) > 0.001f)
        {
            Quaternion rotacionObjetivo = Quaternion.Euler(0f, entradaGiro * velocidadGiro * Time.fixedDeltaTime, 0f) * transform.rotation;

            if (!BloqueadoEnPose(transform.position, rotacionObjetivo))
                transform.rotation = rotacionObjetivo;
        }

        Vector3 direccionAvance = referenciaAvance != null ? referenciaAvance.forward : transform.forward;
        direccionAvance.y = 0f;
        direccionAvance.Normalize();

        Vector3 nuevaPosicion = transform.position + direccionAvance * velocidadActual * Time.fixedDeltaTime;

        if (mantenerAlturaConstante)
            nuevaPosicion.y = alturaFijaY;

        if (!BloqueadoEnPose(nuevaPosicion, transform.rotation))
        {
            transform.position = nuevaPosicion;
            RegistrarPoseSiHaceFalta();
        }
        else
        {
            velocidadActual = 0f;
            RestaurarPoseSegura();

            if (mostrarLogs)
                Debug.Log("Movimiento bloqueado. Restaurando pose segura.");
        }

        if (guardarEstadoGlobal)
            EstadoGlobalBarca.Instance?.GuardarProgreso(idTramo, 0f);
    }

    private Vector2 LeerInputMovimiento()
    {
#if UNITY_EDITOR
        if (usarTecladoDebugEnEditor && priorizarTecladoSobreAccionEnEditor)
            return LeerTecladoDebug();
#endif

        Vector2 input = Vector2.zero;

        if (accionMover != null && accionMover.action != null)
            input = accionMover.action.ReadValue<Vector2>();

#if UNITY_EDITOR
        if (usarTecladoDebugEnEditor && input == Vector2.zero)
            input = LeerTecladoDebug();
#endif

        return input;
    }

    private Vector2 LeerTecladoDebug()
    {
        if (Keyboard.current == null)
            return Vector2.zero;

        float x = 0f;
        float y = 0f;

        if (Keyboard.current[teclaIzquierda].isPressed)
            x -= 1f;
        if (Keyboard.current[teclaDerecha].isPressed)
            x += 1f;

        if (Keyboard.current[teclaAdelante].isPressed)
            y += 1f;
        if (Keyboard.current[teclaAtras].isPressed)
            y -= 1f;

        return new Vector2(x, y);
    }

    private void RegistrarPoseSiHaceFalta(bool forzar = false)
    {
        PoseBarca actual = new PoseBarca(transform.position, transform.rotation);

        if (historialPoses.Count == 0 || forzar)
        {
            historialPoses.Add(actual);
            LimitarHistorial();
            return;
        }

        PoseBarca ultima = historialPoses[historialPoses.Count - 1];

        float dist = Vector3.Distance(ultima.posicion, actual.posicion);
        float ang = Quaternion.Angle(ultima.rotacion, actual.rotacion);

        if (dist >= distanciaMinimaParaGuardar || ang >= anguloMinimoParaGuardar)
        {
            historialPoses.Add(actual);
            LimitarHistorial();
        }
    }

    private void LimitarHistorial()
    {
        while (historialPoses.Count > maxPosesGuardadas)
            historialPoses.RemoveAt(0);
    }

    private void RestaurarPoseSegura()
    {
        if (historialPoses.Count == 0)
            return;

        int offset = Mathf.Clamp(framesRetrocesoSeguridad, 1, historialPoses.Count);
        int index = Mathf.Max(0, historialPoses.Count - offset);

        PoseBarca segura = historialPoses[index];
        transform.SetPositionAndRotation(segura.posicion, segura.rotacion);
    }

    private bool BloqueadoEnPose(Vector3 posicionBarca, Quaternion rotacionBarca)
    {
        if (hullCastBox == null || capasBloqueo.value == 0)
            return false;

        Vector3 centroMundo = CalcularCentroMundoCaja(posicionBarca, rotacionBarca);
        Quaternion rotacionCaja = rotacionBarca * hullCastBox.transform.localRotation;

        Vector3 halfExtents = Vector3.Scale(hullCastBox.size * 0.5f * factorReduccionCaja, hullCastBox.transform.lossyScale);

        Collider[] hits = Physics.OverlapBox(
            centroMundo,
            halfExtents,
            rotacionCaja,
            capasBloqueo,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null)
                continue;

            if (hits[i].transform.IsChildOf(transform))
                continue;

            return true;
        }

        return false;
    }

    private Vector3 CalcularCentroMundoCaja(Vector3 posicionBarca, Quaternion rotacionBarca)
    {
        Vector3 offsetLocal =
            hullCastBox.transform.localPosition +
            hullCastBox.transform.localRotation * Vector3.Scale(hullCastBox.center, hullCastBox.transform.localScale);

        return posicionBarca + rotacionBarca * offsetLocal;
    }
}