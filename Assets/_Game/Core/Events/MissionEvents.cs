// Autor: Generated
// Data: 05/05/2026

using FifthSemester.Core.Enums;
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
    public struct MissionCompletedEvent {
        public string MissionId;
    }
    public struct CutsceneEndedEvent {
        public CutsceneType CutsceneID;
    }
    public struct PuzzlePartPlacedEvent {
        public string PuzzleId;
    }

    public struct ObjectSuccessfullyInteractedEvent {
        public string ObjectId;
    }

    public struct PlayerReachedZoneEvent {
        public string ZoneId;
    }
}
