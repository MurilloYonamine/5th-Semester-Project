using UnityEngine;
using UnityEngine.UI; // Necessário para acessar o componente Image

namespace FifthSemester.Gameplay.Menu {
    [RequireComponent(typeof(Image))]
    public class UnscaledTimeShader : MonoBehaviour {
        
        private Image _image;
        private static readonly int UnscaledTimeID = Shader.PropertyToID("_UnscaledTime");
        private static readonly int UnscaledDeltaTimeID = Shader.PropertyToID("_UnscaledDeltaTime");

        private void Awake() {
            _image = GetComponent<Image>();
        }

        private void Update() {
            Shader.SetGlobalFloat(UnscaledTimeID, Time.unscaledTime);
            Shader.SetGlobalFloat(UnscaledDeltaTimeID, Time.unscaledDeltaTime);

            if (_image != null) {
                _image.SetMaterialDirty();
            }
        }
    }
}