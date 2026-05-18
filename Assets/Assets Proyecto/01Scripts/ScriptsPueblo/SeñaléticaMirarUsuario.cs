using UnityEngine;

public class SenaleticaMirarUsuario : MonoBehaviour
{
    [Header("Opciones")]
    [SerializeField] private bool soloEjeY = true;
    [SerializeField] private bool usarMainCamera = true;
    [SerializeField] private Transform objetivoManual;
    [SerializeField] private bool invertirFrente = false;

    private Transform objetivo;

    private void LateUpdate()
    {
        if (objetivo == null)
            BuscarObjetivo();

        if (objetivo == null)
            return;

        Vector3 direccion = objetivo.position - transform.position;

        if (soloEjeY)
            direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.0001f)
            return;

        Quaternion rotacion = Quaternion.LookRotation(direccion.normalized, Vector3.up);

        if (invertirFrente)
            rotacion *= Quaternion.Euler(0f, 180f, 0f);

        transform.rotation = rotacion;
    }

    private void BuscarObjetivo()
    {
        if (objetivoManual != null)
        {
            objetivo = objetivoManual;
            return;
        }

        if (usarMainCamera && Camera.main != null)
            objetivo = Camera.main.transform;
    }
}