using UnityEngine;
using Unity.XR.CoreUtils;

public class BarcaPasajeroXR : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BarcaEmbarqueXR embarqueXR;
    [SerializeField] private Transform seatAnchor;

    [Header("Comportamiento")]
    [SerializeField] private bool aplicarMovimientoPlataforma = true;
    [SerializeField] private bool aplicarRotacionYaw = true;

    [Header("Corrección Editor")]
    [SerializeField] private bool corregirDerivaEnEditor = true;
    [SerializeField] private bool corregirAlturaYEnEditor = true;
    [SerializeField] private float suavizadoCorreccionEditor = 18f;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = false;

    private bool inicializado = false;
    private Vector3 ultimaPosicionSeat;
    private float ultimoYawSeat;

    private void LateUpdate()
    {
        if (embarqueXR == null || !embarqueXR.JugadorAbordo || seatAnchor == null)
        {
            inicializado = false;
            return;
        }

        XROrigin xr = embarqueXR.XrOriginActual;
        if (xr == null)
        {
            inicializado = false;
            return;
        }

        Transform camara = xr.Camera != null ? xr.Camera.transform : null;
        if (camara == null)
        {
            inicializado = false;
            return;
        }

        if (!inicializado)
        {
            ultimaPosicionSeat = seatAnchor.position;
            ultimoYawSeat = seatAnchor.eulerAngles.y;
            inicializado = true;
            return;
        }

        if (aplicarMovimientoPlataforma)
        {
            Vector3 deltaPos = seatAnchor.position - ultimaPosicionSeat;
            xr.transform.position += deltaPos;
        }

        if (aplicarRotacionYaw)
        {
            float yawActual = seatAnchor.eulerAngles.y;
            float deltaYaw = Mathf.DeltaAngle(ultimoYawSeat, yawActual);

            if (Mathf.Abs(deltaYaw) > 0.001f)
                xr.transform.RotateAround(camara.position, Vector3.up, deltaYaw);

            ultimoYawSeat = yawActual;
        }
        else
        {
            ultimoYawSeat = seatAnchor.eulerAngles.y;
        }

#if UNITY_EDITOR
        if (corregirDerivaEnEditor)
        {
            Vector3 error = seatAnchor.position - camara.position;

            if (!corregirAlturaYEnEditor)
                error.y = 0f;

            float t = 1f - Mathf.Exp(-suavizadoCorreccionEditor * Time.unscaledDeltaTime);
            xr.transform.position += error * t;
        }
#endif

        ultimaPosicionSeat = seatAnchor.position;

        if (mostrarLogs)
            Debug.Log("BarcaPasajeroXR: XR siguiendo al barco.");
    }
}