using UnityEngine;

public class MenuFlowManager : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string nombreEscenaTutorial = "1Tutorial";
    [SerializeField] private string nombreEscenaExperiencia = "1Museo_Monica";

    [Header("Transición")]
    [SerializeField] private TransicionEscena transicionEscena;

    [Header("Opciones")]
    [SerializeField] private bool mostrarLogs = true;

    public void IrATutorial()
    {
        if (mostrarLogs)
            Debug.Log("Botón Tutorial presionado.");

        CargarEscena(nombreEscenaTutorial);
    }

    public void IrAExperiencia()
    {
        if (mostrarLogs)
            Debug.Log("Botón Iniciar Experiencia presionado.");

        CargarEscena(nombreEscenaExperiencia);
    }

    public void SalirAplicacion()
    {
        if (mostrarLogs)
            Debug.Log("Salir aplicación.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CargarEscena(string nombreEscena)
    {
        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            Debug.LogWarning("No se asignó nombre de escena.");
            return;
        }

        if (transicionEscena != null)
            transicionEscena.IniciarTransicion(nombreEscena);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscena);
    }
}