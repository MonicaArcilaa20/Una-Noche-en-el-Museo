using UnityEngine;
using TMPro; 
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class TriggerDialogoMascara : MonoBehaviour
{
    [Header("Configuración del Evento")]
    [TextArea(2, 5)] public string textoSubtitulo = "Escribe aquí lo que dirá la máscara...";
    public float segundosVisibles = 4.0f; 

    [Header("Referencias de Escena")]
    public MascaraFlotante scriptMascara;
    public TMP_Text componenteTextoUI; 
    
    [Header("Sonido")]
    [Tooltip("Arrastra aquí el AudioSource que está en la Máscara")]
    public AudioSource fuenteAudioMascara;
    public AudioClip clipDeVoz; // El archivo de audio (.mp3 o .wav) para este trigger

    private bool yaSeActivo = false;
    private BoxCollider miCollider;

    private void Start()
    {
        miCollider = GetComponent<BoxCollider>();
        miCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!yaSeActivo && (other.CompareTag("Player") || other.CompareTag("MainCamera")))
        {
            yaSeActivo = true; 
            Debug.Log("<color=green>¡Trigger activado!</color> Reproduciendo audio y texto.");

            if (miCollider != null) miCollider.enabled = false;
            StartCoroutine(SecuenciaMascara());
        }
    }

    private IEnumerator SecuenciaMascara()
    {
        // 1. ACTIVAR VISUALES Y SONIDO
        if (scriptMascara != null) scriptMascara.DefinirVisibilidad(true);
        
        // Reproducir el audio si están asignados
        if (fuenteAudioMascara != null && clipDeVoz != null)
        {
            fuenteAudioMascara.clip = clipDeVoz;
            fuenteAudioMascara.Play();
        }

        if (componenteTextoUI != null)
        {
            componenteTextoUI.text = textoSubtitulo;
            componenteTextoUI.gameObject.SetActive(true);
        }

        // 2. ESPERAR EL TIEMPO CONFIGURADO
        yield return new WaitForSeconds(segundosVisibles);

        // 3. OCULTAR TODO
        if (scriptMascara != null) scriptMascara.DefinirVisibilidad(false);
        if (componenteTextoUI != null)
        {
            componenteTextoUI.text = "";
            componenteTextoUI.gameObject.SetActive(false);
        }

        // 4. DESTRUIR TRIGGER
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }
}