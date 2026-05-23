using FifthSemester.Core.Enums;
using UnityEngine;

namespace FifthSemester.Core.Services {
    public interface IMapService {
        void Register(string id, GameObject obj);
        void Register(DoorType doorType, GameObject obj);

        void Unregister(string id);
        void Unregister(DoorType doorType);

        GameObject Get(string id);
        GameObject Get(DoorType doorType);
    }
}
