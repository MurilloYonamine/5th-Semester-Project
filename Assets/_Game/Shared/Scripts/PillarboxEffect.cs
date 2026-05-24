using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PillarboxEffect : MonoBehaviour
{
    private const float targetAspect = 4f / 3f;

    void Start()
    {
        Apply();
    }

    void Apply()
    {
        Camera cam = GetComponent<Camera>();

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleWidth = targetAspect / windowAspect;

        if (scaleWidth < 1.0f)
        {
            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;

            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
        else
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }

        cam.backgroundColor = Color.black;
    }
}