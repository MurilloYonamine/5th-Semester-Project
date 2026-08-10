using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Map {
    public class MapEntity : MonoBehaviour {
        [SerializeField] private string _id;

        private IMapService _mapService;

        private void Awake() {
            if (string.IsNullOrEmpty(_id)) {
                _id = gameObject.name;
            }
<<<<<<< HEAD

=======
        }

        private void Start() {
>>>>>>> origin/main
            _mapService = ServiceLocator.Get<IMapService>();
            _mapService.Register(_id, gameObject);
        }
    }
}
