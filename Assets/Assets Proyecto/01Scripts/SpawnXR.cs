using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class SpawnXRAlCargar : MonoBehaviour
{
    [Header("Opciones")]
    [SerializeField] private bool alinearRotacionYaw = true;
    [SerializeField] private bool mostrarLogs = true;

    private IEnumerator Start()
    {
        // Espera a que la escena y el XR rig terminen de inicializar
        yield return null;
        yield return new WaitForEndOfFrame();

        XROrigin xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogWarning("[SpawnXRAlCargar] No se encontró XROrigin en la escena.");
            yield break;
        }

        Transform cameraTransform = xrOrigin.Camera != null ? xrOrigin.Camera.transform : Camera.main != null ? Camera.main.transform : null;

        if (cameraTransform == null)
        {
            Debug.LogWarning("[SpawnXRAlCargar] No se encontró la cámara del XR rig.");
            yield break;
        }

        // Opcional: alinear rotación horizontal
        if (alinearRotacionYaw)
        {
            Vector3 forwardCam = cameraTransform.forward;
            forwardCam.y = 0f;

            Vector3 forwardSpawn = transform.forward;
            forwardSpawn.y = 0f;

            if (forwardCam.sqrMagnitude > 0.0001f && forwardSpawn.sqrMagnitude > 0.0001f)
            {
                float angulo = Vector3.SignedAngle(forwardCam, forwardSpawn, Vector3.up);
                xrOrigin.transform.RotateAround(cameraTransform.position, Vector3.up, angulo);
            }
        }

        // Mover el rig para que la cámara quede exactamente en el spawn
        Vector3 delta = transform.position - cameraTransform.position;
        xrOrigin.transform.position += delta;

        if (mostrarLogs)
            Debug.Log("[SpawnXRAlCargar] XR Origin colocado en SpawnXR.");
    }
}
