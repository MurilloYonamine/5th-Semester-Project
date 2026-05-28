using UnityEngine;

namespace FifthSemester.UI
{
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)] // Executa um pouco depois para garantir que Camera.main esteja inicializada
    public class CanvasCameraSetup : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Se nulo, tentará pegar o Canvas do próprio GameObject.")]
        [SerializeField] private Canvas _canvas;

        [Tooltip("Se verdadeiro, continuará tentando encontrar a câmera principal caso ela ainda não tenha sido instanciada no Start.")]
        [SerializeField] private bool _autoRetryIfCameraNull = true;

        private bool _setupCompleted = false;

        private void Awake()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }
        }

        private void Start()
        {
            SetupCanvas();
        }

        private void LateUpdate()
        {
            if (!_setupCompleted && _autoRetryIfCameraNull)
            {
                SetupCanvas();
            }
        }

        [ContextMenu("Configurar Canvas")]
        public void SetupCanvas()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }

            if (_canvas == null)
            {
                Debug.LogWarning($"[{nameof(CanvasCameraSetup)}] Nenhum componente Canvas encontrado no GameObject '{gameObject.name}'.", this);
                return;
            }

            if (_canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                Debug.Log($"[{nameof(CanvasCameraSetup)}] Render Mode do Canvas '{_canvas.name}' alterado para Screen Space - Camera.", _canvas);
            }

            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                _canvas.worldCamera = mainCamera;
                _setupCompleted = true;
                Debug.Log($"[{nameof(CanvasCameraSetup)}] Câmera principal '{mainCamera.name}' associada com sucesso ao Canvas '{_canvas.name}'.", _canvas);
            }
            else
            {
                if (!Application.isPlaying)
                {
                    Debug.LogWarning($"[{nameof(CanvasCameraSetup)}] Camera.main não foi encontrada no Editor. Certifique-se de que sua câmera principal possui a tag 'MainCamera'.", this);
                }
                else if (!_autoRetryIfCameraNull)
                {
                    Debug.LogWarning($"[{nameof(CanvasCameraSetup)}] Camera.main não encontrada. O Canvas '{_canvas.name}' permaneceu sem render camera associada.", this);
                }
            }
        }
    }
}