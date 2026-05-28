namespace FifthSemester.Gameplay.Map2 {
    public interface IMap2KeyService {
        bool HasCollectedAllKeys { get; }
        void RegisterKey(Map2KeyItem key);
        void UnregisterKey(Map2KeyItem key);
    }
}
