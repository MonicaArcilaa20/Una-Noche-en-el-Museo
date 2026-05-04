using UnityEngine;

[RequireComponent(typeof(ControlPincel))]
public class PincelFlotanteIdle : MonoBehaviour
{
    [Header("Flotación")]
    [SerializeField] private float amplitudVertical = 0.05f;
    [SerializeField] private float velocidadVertical = 1.5f;

    [Header("Rotación")]
    [SerializeField] private float velocidadRotacion = 35f;

    [Header("Órbita opcional")]
    [SerializeField] private bool usarOrbita = false;
    [SerializeField] private float radioOrbita = 0.08f;
    [SerializeField] private float velocidadOrbita = 0.8f;

    [Header("Referencias")]
    [SerializeField] private Transform centroOrbita;

    [Header("Visual mientras flota")]
    [SerializeField] private GameObject objetoVisibleEnFlotacion;

    private ControlPincel controlPincel;
    private Vector3 posicionBase;
    private float anguloOrbita = 0f;

    private void Awake()
    {
        controlPincel = GetComponent<ControlPincel>();
        posicionBase = transform.position;
    }

    private void OnEnable()
    {
        posicionBase = transform.position;
        ActualizarObjetoFlotacion();
    }

    private void Update()
    {
        if (controlPincel == null)
            return;

        bool estaFlotando = !controlPincel.EstaAgarrado && !controlPincel.EstaEquipado;

        ActualizarObjetoFlotacion();

        if (!estaFlotando)
            return;

        float offsetY = Mathf.Sin(Time.time * velocidadVertical) * amplitudVertical;
        Vector3 nuevaPosicion = posicionBase + Vector3.up * offsetY;

        if (usarOrbita)
        {
            Vector3 centro = centroOrbita != null ? centroOrbita.position : posicionBase;

            anguloOrbita += velocidadOrbita * Time.deltaTime;
            float x = Mathf.Cos(anguloOrbita) * radioOrbita;
            float z = Mathf.Sin(anguloOrbita) * radioOrbita;

            nuevaPosicion = centro + new Vector3(x, offsetY, z);
        }

        transform.position = nuevaPosicion;
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime, Space.World);
    }

    public void RecalcularPosicionBase()
    {
        posicionBase = transform.position;
    }

    private void ActualizarObjetoFlotacion()
    {
        if (objetoVisibleEnFlotacion == null || controlPincel == null)
            return;

        bool debeEstarActivo = !controlPincel.EstaAgarrado && !controlPincel.EstaEquipado;
        objetoVisibleEnFlotacion.SetActive(debeEstarActivo);
    }
}