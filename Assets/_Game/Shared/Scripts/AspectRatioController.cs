using UnityEngine;

namespace FifthSemester.Shared {
    public class AspectRatioController : MonoBehaviour {
        private readonly float targetAspect = 4f / 3f;

        private void Start() {
            ApplyAspect();
        }

        private void ApplyAspect() {
            float windowAspect = (float)Screen.width / Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            Camera cam = GetComponent<Camera>();
            cam.backgroundColor = Color.black;

            if (scaleHeight < 1.0f) {
                Rect rect = cam.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                cam.rect = rect;
            }
            else {
                float scaleWidth = 1.0f / scaleHeight;

                Rect rect = cam.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                cam.rect = rect;
            }
        }
    }
}
