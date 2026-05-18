using UnityEngine;

public class BalanceoBarcaVisual : MonoBehaviour
{
    [Header("Movimiento vertical")]
    [SerializeField] private float amplitudVertical = 0.025f;
    [SerializeField] private float velocidadVertical = 1.2f;

    [Header("Balanceo lateral")]
    [SerializeField] private float amplitudRoll = 2.2f;
    [SerializeField] private float velocidadRoll = 1.05f;

    [Header("Balanceo frontal")]
    [SerializeField] private float amplitudPitch = 1.2f;
    [SerializeField] private float velocidadPitch = 0.85f;

    [Header("Desfase opcional")]
    [SerializeField] private float desfase = 0f;

    [Header("Opcional")]
    [SerializeField] private bool usarTiempoNoEscalado = false;

    private Vector3 posicionLocalInicial;
    private Quaternion rotacionLocalInicial;

    private void Awake()
    {
        posicionLocalInicial = transform.localPosition;
        rotacionLocalInicial = transform.localRotation;
    }

    private void LateUpdate()
    {
        float t = usarTiempoNoEscalado ? Time.unscaledTime : Time.time;
        t += desfase;

        float offsetY = Mathf.Sin(t * velocidadVertical) * amplitudVertical;
        float roll = Mathf.Sin(t * velocidadRoll) * amplitudRoll;
        float pitch = Mathf.Cos(t * velocidadPitch) * amplitudPitch;

        transform.localPosition = posicionLocalInicial + new Vector3(0f, offsetY, 0f);
        transform.localRotation = rotacionLocalInicial * Quaternion.Euler(pitch, 0f, roll);
    }
}