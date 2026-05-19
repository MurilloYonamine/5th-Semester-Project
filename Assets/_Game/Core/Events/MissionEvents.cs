// Autor: Generated
// Data: 05/05/2026

using UnityEngine;

namespace FifthSemester.Core.Events {
    // Generic event that systems can publish with a string identifier.
    public readonly struct GenericGameEvent {
        public readonly string Name;
        public readonly object Payload;

        public GenericGameEvent(string name, object payload = null) {
            Name = name;
            Payload = payload;
        }
    }

    public readonly struct MissionUpdatedEvent {
        public readonly string MissionId;
        public readonly string Title;
        public readonly string Description;

        public MissionUpdatedEvent(string missionId, string title, string description) {
            MissionId = missionId;
            Title = title;
            Description = description;
        }
    }

    public readonly struct MissionProgressEvent {
        public readonly string MissionId;
        public readonly string Progress;

        public MissionProgressEvent(string missionId, string progress) {
            MissionId = missionId;
            Progress = progress;
        }
    }
}
