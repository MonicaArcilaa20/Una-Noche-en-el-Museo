using UnityEngine;

public class ObjetoGrab : MonoBehaviour
{
    public TutorialFlowManager manager;

    private bool hecho = false;

    public void OnGrab()
    {
        if (hecho) return;

        hecho = true;

        Debug.Log("Objeto agarrado");
        manager.PasoCompletado(1);
    }
}