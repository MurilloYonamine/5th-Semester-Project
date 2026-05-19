// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Missions {
    public interface IMission {
        void Initialize(MissionDefinition definition, IEventBus eventBus, ISaveService saveService);
        void StartMission();
        void Complete();
        void Cleanup();

        string Progress { get; }
        bool IsComplete { get; }
        string MissionId { get; }
    }
}
