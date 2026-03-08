using FifthSemester.Items;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable {
    public bool IsInteractable => true;

    public void Interact() {
        Debug.Log("Pode Interagir com o Item!");
    }

    public void StopInteract() {
        Debug.Log("Não pode interagir com o Item!");
    }
}
