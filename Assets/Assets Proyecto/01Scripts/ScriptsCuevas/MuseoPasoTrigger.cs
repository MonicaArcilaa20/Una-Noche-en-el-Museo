using UnityEngine;

namespace UnaNocheEnElMuseo
{
    [RequireComponent(typeof(Collider))]
    public class MuseoPasoTrigger : MonoBehaviour
    {
        public enum TipoPaso
        {
            PincelTomado,
            TintaTomada
        }

        [Header("Configuración")]
        [SerializeField] private MuseoFlowManager museoFlowManager;
        [SerializeField] private TipoPaso tipoPaso = TipoPaso.PincelTomado;
        [SerializeField] private bool soloUnaVez = true;
        [SerializeField] private bool iniciarDesactivado = true;
        [SerializeField] private bool mostrarLogs = true;

        [Header("Objetos opcionales")]
        [SerializeField] private GameObject[] objetosAlHabilitarTrigger;
        [SerializeField] private GameObject[] objetosAlDeshabilitarTrigger;
        [SerializeField] private GameObject[] objetosAlActivarPaso;

        private bool yaActivado = false;
        private Collider col;

        private void Awake()
        {
            col = GetComponent<Collider>();
            col.isTrigger = true;

            if (iniciarDesactivado)
                col.enabled = false;

            AplicarEstadoObjetos(objetosAlHabilitarTrigger, false);
        }

        public void HabilitarTrigger()
        {
            if (col == null)
                col = GetComponent<Collider>();

            col.enabled = true;

            AplicarEstadoObjetos(objetosAlHabilitarTrigger, true);

            if (mostrarLogs)
                Debug.Log("[MuseoPasoTrigger] Trigger habilitado: " + tipoPaso, this);
        }

        public void DeshabilitarTrigger()
        {
            if (col == null)
                col = GetComponent<Collider>();

            col.enabled = false;

            AplicarEstadoObjetos(objetosAlDeshabilitarTrigger, false);

            if (mostrarLogs)
                Debug.Log("[MuseoPasoTrigger] Trigger deshabilitado: " + tipoPaso, this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (soloUnaVez && yaActivado)
                return;

            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc == null)
                return;

            if (museoFlowManager == null)
            {
                Debug.LogWarning("[MuseoPasoTrigger] Falta asignar MuseoFlowManager.", this);
                return;
            }

            AplicarEstadoObjetos(objetosAlActivarPaso, true);

            switch (tipoPaso)
            {
                case TipoPaso.PincelTomado:
                    museoFlowManager.OnPincelTomado();
                    break;

                case TipoPaso.TintaTomada:
                    museoFlowManager.OnPrimeraTintaTomada();
                    break;
            }

            yaActivado = true;

            if (soloUnaVez)
                col.enabled = false;

            if (mostrarLogs)
                Debug.Log("[MuseoPasoTrigger] Paso activado: " + tipoPaso, this);
        }

        private void AplicarEstadoObjetos(GameObject[] objetos, bool estado)
        {
            if (objetos == null)
                return;

            for (int i = 0; i < objetos.Length; i++)
            {
                if (objetos[i] != null)
                    objetos[i].SetActive(estado);
            }
        }
    }
}