using UnityEngine;
using TMPro; 
using System.Collections;
using System.Collections.Generic; // Para usar listas

public class TextosSecuencia : MonoBehaviour
{
    [System.Serializable]
    public class PasoDialogo // Clase para organizar cada frase con su audio
    {
        [TextArea(2, 4)] public string texto;
        public AudioClip audio;
        public float duracionEnPantalla = 3.0f;
        public float pausaAntesDelSiguiente = 0.5f;
    }

    [Header("Configuración de Inicio")]
    public float esperaInicial = 3.0f; // El tiempo que pediste antes de empezar

    [Header("Lista de Diálogos")]
    public List<PasoDialogo> secuenciaDeDialogos;

    [Header("Referencias")]
    public MascaraFlotante scriptMascara;
    public TMP_Text componenteTextoUI; 
    public AudioSource fuenteAudioMascara;

    private bool yaSeActivo = false;
    private BoxCollider miCollider;

    private void Start()
    {
        miCollider = GetComponent<BoxCollider>();
        if(miCollider) miCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!yaSeActivo && (other.CompareTag("Player") || other.CompareTag("MainCamera")))
        {
            yaSeActivo = true;
            Debug.Log("<color=cyan>¡Secuencia Iniciada!</color> Esperando " + esperaInicial + " segundos...");
            
            if (miCollider != null) miCollider.enabled = false;
            StartCoroutine(EjecutarSecuenciaCompleta());
        }
    }

    private IEnumerator EjecutarSecuenciaCompleta()
    {
        // 1. ESPERA INICIAL ANTES DE QUE TODO EMPIECE
        yield return new WaitForSeconds(esperaInicial);

        // 2. APARECE LA MÁSCARA (se queda activa durante toda la lista)
        if (scriptMascara != null) scriptMascara.DefinirVisibilidad(true);

        // 3. RECORRER LA LISTA DE DIÁLOGOS
        foreach (PasoDialogo paso in secuenciaDeDialogos)
        {
            // Ponemos el texto y el audio de este paso
            if (componenteTextoUI != null)
            {
                componenteTextoUI.text = paso.texto;
                componenteTextoUI.gameObject.SetActive(true);
            }

            if (fuenteAudioMascara != null && paso.audio != null)
            {
                fuenteAudioMascara.clip = paso.audio;
                fuenteAudioMascara.Play();
            }

            // Esperamos el tiempo que definiste para este audio específico
            yield return new WaitForSeconds(paso.duracionEnPantalla);

            // Quitamos el texto brevemente para dar efecto de "cambio de frase"
            if (componenteTextoUI != null) componenteTextoUI.text = "";
            
            // Pausa pequeña entre frases si quieres
            yield return new WaitForSeconds(paso.pausaAntesDelSiguiente);
        }

        // 4. FINALIZAR: Desaparece máscara y texto
        if (scriptMascara != null) scriptMascara.DefinirVisibilidad(false);
        if (componenteTextoUI != null) componenteTextoUI.gameObject.SetActive(false);

        // 5. DESTRUIR TRIGGER (después de 3 segundos como los anteriores)
        yield return new WaitForSeconds(3.0f);
        Debug.Log("Secuencia finalizada y objeto destruido.");
        Destroy(gameObject);
    }
}