using UnityEngine;

public class ObjetoMove : MonoBehaviour
{
    public TutorialFlowManager manager;

    private Vector3 posicionInicial;
    private bool hecho = false;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (!hecho && Vector3.Distance(transform.position, posicionInicial) > 0.2f)
        {
            hecho = true;

            Debug.Log("Objeto movido");
            manager.PasoCompletado(2);
        }
    }
}