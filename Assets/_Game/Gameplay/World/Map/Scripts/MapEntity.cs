using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class MapEntity : MonoBehaviour {
        [SerializeField] private string _id;

        private IMapService _mapService;

        private void Awake() {
            if (string.IsNullOrEmpty(_id)) {
                _id = gameObject.name;
            }

            _mapService = ServiceLocator.Get<IMapService>();
            _mapService.Register(_id, gameObject);
        }
    }
}
