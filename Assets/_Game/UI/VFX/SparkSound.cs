using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.VFX;
public class SparkSound : MonoBehaviour {
    private VisualEffect _vfx;
    [SerializeField] private AudioClip _audio;
    private readonly int sparkEventID = Shader.PropertyToID("SparkFired");
    private IAudioService _audioService;

    private void Awake() {
        _vfx = GetComponent<VisualEffect>();
    }
    private void Start() {
        _audioService = ServiceLocator.Get<IAudioService>();
    }
    private void OnEnable() {
        _vfx.outputEventReceived += OnOutputEventReceived;
    }
    private void OnDisable() {
        _vfx.outputEventReceived -= OnOutputEventReceived;
    }

    private void OnOutputEventReceived(VFXOutputEventArgs eventData) {
        if (eventData.nameId == sparkEventID) {
            _audioService.PlaySFX(_audio);
        }
    }
}
