using UnityEngine;

namespace FifthSemester.Gameplay
{
    public class SpawnFlower : MonoBehaviour
    {
        private Animator _animator;
            
            private readonly int _activateFlowerHash = Animator.StringToHash("Activate");

            private void Awake()
            {
                _animator = GetComponent<Animator>();
            }

            private void OnTriggerEnter(Collider collision)
            {
                if (collision.CompareTag("Player"))
                {
                    _animator.SetTrigger(_activateFlowerHash);
                }
            }
    }
}
