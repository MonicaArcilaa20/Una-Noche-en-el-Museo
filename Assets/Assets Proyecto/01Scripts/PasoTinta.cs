using UnityEngine;

public class PasoTinta : MonoBehaviour
{
    [SerializeField] private TutorialFlowManager manager;

    private bool hecho = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hecho)
            return;

        // Más robusto que CompareTag solo del collider
        ControlPincel pincel = other.GetComponentInParent<ControlPincel>();
        if (pincel == null)
            return;

        hecho = true;
        manager.PasoCompletado(3);
    }
}