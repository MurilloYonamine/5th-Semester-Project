// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using System;

namespace FifthSemester.Gameplay.Missions {
    public interface IMissionService {
        MissionDefinition[] Missions { get; }
        int CurrentIndex { get; }
        MissionDefinition GetCurrentMission();
        void StartMission(MissionDefinition mission);
        void SkipToMission(int missionIndex);
        void CompleteCurrentMission();
    }
}
