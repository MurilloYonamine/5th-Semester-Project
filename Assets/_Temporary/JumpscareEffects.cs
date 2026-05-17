using UnityEngine;

public class JumpscareEffects : MonoBehaviour
{
    public Camera mainCam;
    private CameraClearFlags originalFlags;
    private Color originalColor;

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
    }

    public void StartJumpscare()
    {
        originalFlags = mainCam.clearFlags;
        originalColor = mainCam.backgroundColor;

        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = Color.black;
    }

    public void StopJumpscare()
    {
        mainCam.clearFlags = originalFlags;
        mainCam.backgroundColor = originalColor;
    }
}
