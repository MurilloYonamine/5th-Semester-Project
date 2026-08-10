// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using System;

namespace FifthSemester.Gameplay.Missions {
    public interface IMissionService {
<<<<<<< HEAD
=======
        MissionDefinition[] Missions { get; }
>>>>>>> origin/main
        int CurrentIndex { get; }
        MissionDefinition GetCurrentMission();
        void StartMission(MissionDefinition mission);
        void SkipToMission(int missionIndex);
        void CompleteCurrentMission();
<<<<<<< HEAD
        void PlayMissionCompleteSFX();
        void UpdateCollectAndDeliverDoorState(MissionDefinition definition, int deliveredCount);
        void StartSequence(MissionSequenceSO sequence);
=======
>>>>>>> origin/main
    }
}
