using UnityEngine;

public class PasoTinta : MonoBehaviour
{
    public TutorialFlowManager manager;

    private bool hecho = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hecho && other.CompareTag("Pincel"))
        {
            hecho = true;
            manager.PasoCompletado(4);
        }
    }
}