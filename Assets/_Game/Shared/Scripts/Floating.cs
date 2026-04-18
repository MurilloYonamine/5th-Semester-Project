using UnityEngine;

namespace FifthSemester.Shared
{
    public class Floating : MonoBehaviour
    {
        public float amplitude = 0.5f; // The maximum distance the object moves up/down
        public float speed = 1f;       // The speed of the oscillation
        private float startY;
        private Vector3 tempPos;

        void Start()
        {
            startY = transform.position.y;
        }

        void Update()
        {
            tempPos = transform.position;
            // Use Sin to calculate the new Y position over time
            tempPos.y = startY + amplitude * Mathf.Sin(speed * Time.time);
            transform.position = tempPos;
        }
    }
}
