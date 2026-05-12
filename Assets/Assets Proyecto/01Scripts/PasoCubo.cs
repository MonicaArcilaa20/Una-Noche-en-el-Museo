using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PasoCubo : MonoBehaviour
{
    public TutorialFlowManager manager;

    private XRGrabInteractable grabInteractable;
    private bool yaFueAgarrado = false;
    private bool completado = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(AlAgarrar);
        grabInteractable.selectExited.AddListener(AlSoltar);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(AlAgarrar);
        grabInteractable.selectExited.RemoveListener(AlSoltar);
    }

    private void AlAgarrar(SelectEnterEventArgs args)
    {
        if (completado)
            return;

        yaFueAgarrado = true;
    }

    private void AlSoltar(SelectExitEventArgs args)
    {
        if (completado)
            return;

        if (!yaFueAgarrado)
            return;

        completado = true;
        manager.PasoCompletado(1);
    }
}