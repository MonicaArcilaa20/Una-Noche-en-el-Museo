using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MuseoPasoTrigger : MonoBehaviour
{
    public enum TipoPaso
    {
        LlegarPrimerCuadro
    }

    [SerializeField] private MuseoFlowManager manager;
    [SerializeField] private TipoPaso tipoPaso;
    [SerializeField] private bool usarSoloUnaVez = true;

    private bool yaDisparado = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (usarSoloUnaVez && yaDisparado)
            return;

        if (!other.transform.root.CompareTag("Player"))
            return;

        if (manager == null)
            return;

        if (tipoPaso == TipoPaso.LlegarPrimerCuadro)
            manager.OnLlegarPrimerCuadro();

        yaDisparado = true;
    }
}