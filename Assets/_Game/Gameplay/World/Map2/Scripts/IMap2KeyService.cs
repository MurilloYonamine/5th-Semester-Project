namespace FifthSemester.Gameplay {
    public interface IMap2KeyService {
        bool HasCollectedAllKeys { get; }
        void RegisterKey(Map2KeyItem key);
        void UnregisterKey(Map2KeyItem key);
        bool TryPrepareForLastKey(Map2KeyItem lastKey);
        void CheatSetKeysCollected();
    }
}
