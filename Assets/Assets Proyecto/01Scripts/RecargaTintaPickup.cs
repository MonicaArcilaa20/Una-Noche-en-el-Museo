using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RecargaTintaPickup : MonoBehaviour
{
    [Header("Recarga")]
    [SerializeField] private float cantidadRecarga = 25f;
    [SerializeField] private bool destruirAlRecargar = false;
    [SerializeField] private bool mostrarLogs = true;

    [Header("Flotación")]
    [SerializeField] private bool flotar = true;
    [SerializeField] private float amplitudFlotacion = 0.08f;
    [SerializeField] private float velocidadFlotacion = 1.5f;
    [SerializeField] private bool rotar = true;
    [SerializeField] private float velocidadRotacionY = 35f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecarga;
    [Range(0f, 1f)]
    [SerializeField] private float volumenSonido = 1f;

    private Collider col;
    private Vector3 posicionInicial;
    private bool usado = false;

    private void Reset()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
        posicionInicial = transform.position;
    }

    private void Update()
    {
        if (usado)
            return;

        if (flotar)
        {
            float offsetY = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
            transform.position = posicionInicial + new Vector3(0f, offsetY, 0f);
        }

        if (rotar)
        {
            transform.Rotate(0f, velocidadRotacionY * Time.deltaTime, 0f, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (usado)
            return;

        ControlPincel control = other.GetComponentInParent<ControlPincel>();
        if (control == null)
            return;

        PincelTinta tinta = control.GetComponent<PincelTinta>();
        if (tinta == null)
            return;

        if (tinta.EstaLlena())
            return;

        tinta.Recargar(cantidadRecarga);
        usado = true;

        if (sonidoRecarga != null)
            AudioSource.PlayClipAtPoint(sonidoRecarga, transform.position, volumenSonido);

        if (mostrarLogs)
            Debug.Log("Pickup de tinta usado", this);

        col.enabled = false;

        if (destruirAlRecargar)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}