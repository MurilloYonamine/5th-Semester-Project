// Autor: Generated
// Data: 05/05/2026

using System;

namespace FifthSemester.Gameplay.Quests {
    public interface IMissionService {
        MissionSO[] Missions { get; }
        int CurrentIndex { get; }
        MissionSO GetCurrentMission();
        void SkipToMission(int missionIndex);
        void CompleteCurrentMission();
    }
}
