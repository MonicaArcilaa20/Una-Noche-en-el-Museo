using UnityEngine;

public class PincelSenaleticaIdle : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ControlPincel controlPincel;
    [SerializeField] private Camera camaraObjetivo;

    [Header("Visual")]
    [SerializeField] private Renderer[] renderersVisuales;
    [SerializeField] private ParticleSystem[] particulasOpcionales;

    [Header("Movimiento")]
    [SerializeField] private float amplitudVertical = 0.015f;
    [SerializeField] private float velocidadVertical = 2f;

    [Header("Pulso")]
    [SerializeField] private float escalaBase = 1f;
    [SerializeField] private float pulsoEscala = 0.08f;
    [SerializeField] private float velocidadPulso = 3f;

    [Header("Opciones")]
    [SerializeField] private bool mirarACamara = true;

    private Vector3 posicionLocalInicial;

    private void Awake()
    {
        if (controlPincel == null)
            controlPincel = GetComponentInParent<ControlPincel>();

        if (camaraObjetivo == null && Camera.main != null)
            camaraObjetivo = Camera.main;

        posicionLocalInicial = transform.localPosition;
    }

    private void Update()
    {
        bool mostrar = controlPincel != null && !controlPincel.EstaAgarrado && !controlPincel.EstaEquipado;

        AplicarVisibilidad(mostrar);

        if (!mostrar)
            return;

        float offsetY = Mathf.Sin(Time.time * velocidadVertical) * amplitudVertical;
        transform.localPosition = posicionLocalInicial + Vector3.up * offsetY;

        float pulso = escalaBase + Mathf.Sin(Time.time * velocidadPulso) * pulsoEscala;
        transform.localScale = Vector3.one * pulso;

        if (mirarACamara && camaraObjetivo != null)
        {
            Vector3 direccion = transform.position - camaraObjetivo.transform.position;
            if (direccion.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        }
    }

    private void AplicarVisibilidad(bool visible)
    {
        if (renderersVisuales != null)
        {
            for (int i = 0; i < renderersVisuales.Length; i++)
            {
                if (renderersVisuales[i] != null)
                    renderersVisuales[i].enabled = visible;
            }
        }

        if (particulasOpcionales != null)
        {
            for (int i = 0; i < particulasOpcionales.Length; i++)
            {
                if (particulasOpcionales[i] == null)
                    continue;

                if (visible)
                {
                    if (!particulasOpcionales[i].isPlaying)
                        particulasOpcionales[i].Play();
                }
                else
                {
                    if (particulasOpcionales[i].isPlaying)
                        particulasOpcionales[i].Stop();
                }
            }
        }
    }
}