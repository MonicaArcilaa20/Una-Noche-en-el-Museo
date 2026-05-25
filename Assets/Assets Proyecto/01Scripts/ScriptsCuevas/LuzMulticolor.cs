using UnityEngine;

public class LuzMulticolor : MonoBehaviour
{
    [Header("Luz")]
    public Light pointLight;

    [Header("Velocidad de cambio")]
    public float velocidadColor = 0.2f;

    [Header("Saturación")]
    [Range(0f, 1f)]
    public float saturacion = 1f;

    [Header("Brillo")]
    [Range(0f, 1f)]
    public float brillo = 1f;

    private float hue;

    void Update()
    {
        // Avanza por el espectro de colores
        hue += Time.deltaTime * velocidadColor;

        // Reinicia cuando pasa de 1
        if (hue > 1f)
            hue = 0f;

        // Convierte HSV a RGB
        Color color = Color.HSVToRGB(hue, saturacion, brillo);

        // Asigna el color a la luz
        pointLight.color = color;
    }
}