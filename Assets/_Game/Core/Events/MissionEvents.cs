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

    // Published when the active mission changes (UI listens to this)
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

    // Published when mission progress changes (UI listens to show count)
    public readonly struct MissionProgressEvent {
        public readonly string MissionId;
        public readonly int Current;
        public readonly int Required;

        public MissionProgressEvent(string missionId, int current, int required) {
            MissionId = missionId;
            Current = current;
            Required = required;
        }
    }
}
