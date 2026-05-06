using UnityEngine;

public class MantenedorAltura : MonoBehaviour
{
    [Header("Configuración")]
    public float alturaFija = 1f; // ajusta según tu escena
    public float velocidadCorreccion = 10f;

    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Raycast hacia abajo para detectar el suelo
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            float alturaObjetivo = hit.point.y + alturaFija;
            if (transform.position.y < alturaObjetivo)
            {
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(transform.position.y, alturaObjetivo, Time.deltaTime * velocidadCorreccion);
                cc.enabled = false;
                transform.position = pos;
                cc.enabled = true;
            }
        }
    }
}