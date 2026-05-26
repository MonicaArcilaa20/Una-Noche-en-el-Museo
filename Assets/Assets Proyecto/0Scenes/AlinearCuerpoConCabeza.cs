using UnityEngine;

public class AlinearCuerpoConCabeza : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private BarcaEmbarqueXR embarqueXR;

    [Header("Giro normal")]
    [SerializeField] private float velocidadGiro = 120f;
    [SerializeField] private float anguloMinimoParaGirar = 25f;

    [Header("Giro en barca")]
    [SerializeField] private bool usarAjustesEspecialesEnBarca = true;
    [SerializeField] private float velocidadGiroEnBarca = 45f;
    [SerializeField] private float anguloMinimoEnBarca = 40f;

    [Header("Opciones")]
    [SerializeField] private bool soloMientrasSeMueve = false;
    [SerializeField] private float magnitudMinimaMovimiento = 0.1f;
    [SerializeField] private bool girarAlrededorDeLaCabeza = true;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = false;

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

        bool aBordo = embarqueXR != null && embarqueXR.JugadorAbordo;

        float velocidadActual = velocidadGiro;
        float anguloMinimoActual = anguloMinimoParaGirar;

        if (aBordo && usarAjustesEspecialesEnBarca)
        {
            velocidadActual = velocidadGiroEnBarca;
            anguloMinimoActual = anguloMinimoEnBarca;
        }

        Vector3 forwardCabeza = headTransform.forward;
        forwardCabeza.y = 0f;

        if (forwardCabeza.sqrMagnitude < 0.001f)
            return;

        Vector3 forwardCuerpo = transform.forward;
        forwardCuerpo.y = 0f;

        float angulo = Vector3.SignedAngle(forwardCuerpo, forwardCabeza.normalized, Vector3.up);

        if (Mathf.Abs(angulo) < anguloMinimoActual)
            return;

        float paso = velocidadActual * Time.deltaTime;
        float giro = Mathf.Clamp(angulo, -paso, paso);

        if (girarAlrededorDeLaCabeza)
        {
            transform.RotateAround(headTransform.position, Vector3.up, giro);
        }
        else
        {
            transform.Rotate(0f, giro, 0f, Space.World);
        }

        if (mostrarLogs)
            Debug.Log($"[AlinearCuerpoConCabeza] Giro aplicado: {giro:F2} | Abordo: {aBordo}", this);
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