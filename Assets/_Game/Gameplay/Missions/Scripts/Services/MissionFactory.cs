// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    public static class MissionFactory {
        public static IMission CreateMission(MissionDefinition definition) {
            if (definition == null) return null;

            GameObject missionGO = new($"{definition.MissionId}_Runtime");
            IMission mission = definition.Type switch {
                MissionType.CollectItems => missionGO.AddComponent<CollectItemsMission>(),
                MissionType.CollectAndDeliver => missionGO.AddComponent<CollectAndDeliverMission>(),
                _ => null
            };

            return mission;
        }
    }
}
