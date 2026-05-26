using System.Collections;
using UnityEngine;

public class ScreenFadeEffect : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el Quad que está frente a la cámara")]
    [SerializeField] private Renderer faderRenderer;

    [Header("Configuración")]
    [Tooltip("Duración de la transición a blanco")]
    [SerializeField] private float fadeDuration = 3.0f;
    
    [Tooltip("Tag de la cámara para activar el trigger")]
    [SerializeField] private string targetTag = "MainCamera";

    private bool hasStarted = false;

    private void Start()
    {
        // Al iniciar, nos aseguramos que sea invisible y esté activo el objeto
        if (faderRenderer != null)
        {
            faderRenderer.gameObject.SetActive(true);
            SetAlpha(0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detecta si la cámara entró al collider
        if (other.CompareTag(targetTag) && !hasStarted)
        {
            Debug.Log("Cámara detectada. Iniciando desvanecimiento a blanco...");
            hasStarted = true;
            StartFadeToWhite();
        }
    }

    public void StartFadeToWhite()
    {
        StartCoroutine(FadeRoutine(0f, 1f));
    }

    private IEnumerator FadeRoutine(float alphaStart, float alphaEnd)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Calculamos el progreso (0 a 1)
            float progress = timer / fadeDuration;
            // Interpolamos el valor alpha
            float newAlpha = Mathf.Lerp(alphaStart, alphaEnd, progress);
            
            SetAlpha(newAlpha);
            yield return null; // Espera al siguiente frame
        }

        SetAlpha(alphaEnd);
        Debug.Log("Fade completado.");
    }

    private void SetAlpha(float alpha)
    {
        if (faderRenderer != null)
        {
            // Accedemos al material y cambiamos el color con el nuevo alpha
            Color color = faderRenderer.material.color;
            color.a = alpha;
            faderRenderer.material.color = color;
        }
    }
}