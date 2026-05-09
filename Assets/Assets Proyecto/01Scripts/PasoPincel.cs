using UnityEngine;

public class PasoPincel : MonoBehaviour
{
    public TutorialFlowManager manager;

    private bool hecho = false;

    public void OnGrab()
    {
        if (hecho) return;

        hecho = true;
        manager.PasoCompletado(3);
    }
}