using UnityEngine;

public class ZonaCamina : MonoBehaviour
{
    public TutorialFlowManager manager;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.PasoCompletado(0);


            if (rend != null)
                rend.enabled = false;

             gameObject.SetActive(false);
        }
    }
}