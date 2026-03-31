using FifthSemester.Delivery;
using FifthSemester.DialogueSystem;
using FifthSemester.Items;
using FifthSemester.Player;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeliveryMissionManager : MonoBehaviour {
    private enum MissionState {
        Collecting,
        Delivering,
        Completed
    }

    public static DeliveryMissionManager Instance;

    [SerializeField] private List<BoxCollider> _lockedDoors;
    [SerializeField] private int _requiredItems = 6;

    [SerializeField] private TextMeshProUGUI _missionText;

    private int _collectedItems = 0;
    private MissionState _currentState = MissionState.Collecting;
    [SerializeField] private List<DeliveryPoint> _deliveryPoints;
    private int _deliveredItems = 0;

    [SerializeField] private GameObject _nurse;
    [SerializeField] private Transform _nurseSpawnPoint;
    [SerializeField] private DialogueSO _nurseDialogue;
    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach (var door in _lockedDoors) {
            door.enabled = false;
        }
        foreach (var point in _deliveryPoints) {
            point.OnItemReceived += OnItemDelivered;
        }
        UpdateMissionUI();

        PlayerInteraction.OnItemPickedUp += OnItemPicked;
    }

    private void OnItemPicked(IInteractable item) {
        if (_currentState != MissionState.Collecting) return;

        if (item is not MedicineItem) return;

        _collectedItems++;

        UpdateMissionUI();

        if (_collectedItems >= _requiredItems) {
            StartDeliveryMission();
        }
    }
    private void StartDeliveryMission() {
        _currentState = MissionState.Delivering;

        foreach (var door in _lockedDoors) {
            door.enabled = true;
        }

        _missionText.text = "Entregue os remédios aos pacientes";
    }
    private void OnItemDelivered(DeliveryPoint point, IInteractable item) {
        if (_currentState != MissionState.Delivering) return;

        _deliveredItems++;

        _missionText.text = $"Entregas: {_deliveredItems}/{_deliveryPoints.Count}";

        if (_deliveredItems >= _deliveryPoints.Count) {
            CompleteMission();
        }
    }
    private void CompleteMission() {
        _currentState = MissionState.Completed;

        _missionText.text = "✔ Todos os pacientes atendidos!";

        SpawnNurseEvent(); // 🔥 AQUI
    }
    private void UpdateMissionUI() {
        _missionText.text = $"Remédios: {_collectedItems}/{_requiredItems}";
    }
    private void UnlockDoors() {
        Debug.Log("TODOS ITENS COLETADOS! PORTAS LIBERADAS!");

        foreach (var door in _lockedDoors) {
            door.enabled = true;
        }
    }
    private void SpawnNurseEvent() {
        if (_nurse == null || _nurseSpawnPoint == null) return;

        _nurse.transform.position = _nurseSpawnPoint.position;
        _nurse.transform.rotation = _nurseSpawnPoint.rotation;

        _nurse.SetActive(true);

        _missionText.text = "Fale com a enfermeira";
        _nurse.GetComponent<DialogueTrigger>().SetDialogue(_nurseDialogue);
        _nurse.GetComponent<DialogueTrigger>().goBackToMenuOnEnd = true;
    }
    private void OnDestroy() {
        PlayerInteraction.OnItemPickedUp -= OnItemPicked;

        foreach (var point in _deliveryPoints) {
            point.OnItemReceived -= OnItemDelivered;
        }
    }
}
