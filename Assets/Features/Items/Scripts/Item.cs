using UnityEngine;

namespace FifthSemester.Items {
    public class Item : MonoBehaviour, IInteractable
    {
        public bool IsInteractable => true;

        public void Interact()
        {
            Debug.Log("Pode Interagir com o Item!");
              Destroy(gameObject);
        }

        public void StopInteract()
        {
            Debug.Log("Não pode interagir com o Item!");
        }

        public override string ToString()
        {
            var itemName = gameObject.name ?? "Unnamed Item";
            return $"{itemName}";
        }
    }
}
