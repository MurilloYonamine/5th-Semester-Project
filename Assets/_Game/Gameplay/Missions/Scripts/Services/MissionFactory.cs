// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using FifthSemester.Core.Enums;
using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    public static class MissionFactory {
        public static IMission CreateMission(MissionDefinition definition) {
            if (definition == null) return null;

            GameObject missionGO = new($"{definition.MissionId}_Runtime");
            IMission mission = definition.Type switch {
                MissionType.CollectItems => missionGO.AddComponent<CollectItemsMission>(),
                MissionType.CollectAndDeliver => missionGO.AddComponent<CollectAndDeliverMission>(),
                MissionType.TalkToNpc => missionGO.AddComponent<TalkToNpcMission>(),
                MissionType.PlayCutscene => missionGO.AddComponent<CutsceneMission>(),
                MissionType.EndGame => missionGO.AddComponent<EndGameMission>(),
                _ => null
            };

            if (mission == null) {
                Object.Destroy(missionGO);
                Debug.LogError($"[MissionFactory] Mission type not supported: {definition.Type} ({definition.MissionId})");
            }

            return mission;
        }
    }
}
