using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ZonaActivacionCuadro : MonoBehaviour
{
    [SerializeField] private CuadroActivablePorApunte cuadro;

    private void Reset()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        ControlPincel pincel = other.GetComponentInParent<ControlPincel>();

        if (pincel == null)
            return;

        if (cuadro != null)
            cuadro.RegistrarPincel(pincel);
    }

    private void OnTriggerExit(Collider other)
    {
        ControlPincel pincel = other.GetComponentInParent<ControlPincel>();

        if (pincel == null)
            return;

        if (cuadro != null)
            cuadro.QuitarPincel(pincel);
    }
}