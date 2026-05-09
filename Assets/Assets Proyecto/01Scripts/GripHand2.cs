using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GripHand2 : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NearFarInteractor interactor;
    [SerializeField] private string gripParameter = "Grip";

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (interactor != null)
        {
            interactor.selectEntered.AddListener(OnSelectEntered);
            interactor.selectExited.AddListener(OnSelectExited);
        }

        SetGrip(0f);
    }

    void OnDisable()
    {
        if (interactor != null)
        {
            interactor.selectEntered.RemoveListener(OnSelectEntered);
            interactor.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        SetGrip(1f);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (interactor == null || !interactor.hasSelection)
            SetGrip(0f);
    }

    private void SetGrip(float value)
    {
        if (animator != null)
            animator.SetFloat(gripParameter, value);
    }
}