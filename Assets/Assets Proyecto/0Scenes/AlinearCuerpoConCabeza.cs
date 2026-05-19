using UnityEngine;

public class AlinearCuerpoConCabeza : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private float velocidadGiro = 120f;
    [SerializeField] private float anguloMinimoParaGirar = 25f;
    [SerializeField] private bool soloMientrasSeMueve = false;
    [SerializeField] private float magnitudMinimaMovimiento = 0.1f;

    [Header("Opcional")]
    [SerializeField] private CharacterController characterController;

    private Vector3 ultimaPosicion;

    private void Start()
    {
        if (headTransform == null && Camera.main != null)
            headTransform = Camera.main.transform;

        ultimaPosicion = transform.position;
    }

    private void Update()
    {
        if (headTransform == null)
            return;

        if (soloMientrasSeMueve && !SeEstaMoviendo())
            return;

        Vector3 forwardCabeza = headTransform.forward;
        forwardCabeza.y = 0f;

        if (forwardCabeza.sqrMagnitude < 0.001f)
            return;

        Vector3 forwardCuerpo = transform.forward;
        forwardCuerpo.y = 0f;

        float angulo = Vector3.SignedAngle(forwardCuerpo, forwardCabeza.normalized, Vector3.up);

        if (Mathf.Abs(angulo) < anguloMinimoParaGirar)
            return;

        float paso = velocidadGiro * Time.deltaTime;
        float giro = Mathf.Clamp(angulo, -paso, paso);

        transform.Rotate(0f, giro, 0f, Space.World);
    }

    private bool SeEstaMoviendo()
    {
        Vector3 posicionActual = transform.position;
        Vector3 delta = posicionActual - ultimaPosicion;
        ultimaPosicion = posicionActual;

        delta.y = 0f;
        return delta.magnitude > magnitudMinimaMovimiento * Time.deltaTime;
    }
}