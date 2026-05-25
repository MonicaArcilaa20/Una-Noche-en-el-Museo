using UnityEngine;
using System.Collections;

public class SirenaScript : MonoBehaviour
{
    [Header("Configuracion de Audio")]
    public AudioSource audioSource;
    public float retrasoAudio = 0f;

    [Header("Configuracion del Modelo 3D")]
    public GameObject modelo3D;
    public Vector3 distanciaSubida = new Vector3(0, 3f, 0); 
    public float tiempoParaSubir = 1.5f;
    public float tiempoArriba = 3f;
    public float tiempoParaBajar = 1.5f;

    private Vector3 posicionOriginal;
    private bool yaSeActivo = false;

    void Start()
    {
        if (modelo3D != null)
        {
            posicionOriginal = modelo3D.transform.localPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica que el objeto tenga el tag Barco
        if (other.CompareTag("Barco") && !yaSeActivo)
        {
            StartCoroutine(SecuenciaEvento());
        }
    }

    IEnumerator SecuenciaEvento()
    {
        yaSeActivo = true;

        // 1. Audio con retraso
        Invoke("PlayAudio", retrasoAudio);

        // 2. Subida
        yield return Mover(posicionOriginal + distanciaSubida, tiempoParaSubir);

        // 3. Espera arriba
        yield return new WaitForSeconds(tiempoArriba);

        // 4. Bajada
        yield return Mover(posicionOriginal, tiempoParaBajar);
    }

    void PlayAudio()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    IEnumerator Mover(Vector3 destino, float tiempo)
    {
        float transcurrido = 0;
        Vector3 inicio = modelo3D.transform.localPosition;

        while (transcurrido < tiempo)
        {
            modelo3D.transform.localPosition = Vector3.Lerp(inicio, destino, transcurrido / tiempo);
            transcurrido += Time.deltaTime;
            yield return null;
        }
        modelo3D.transform.localPosition = destino;
    }
}