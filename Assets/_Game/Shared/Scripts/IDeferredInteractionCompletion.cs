namespace FifthSemester.Gameplay.Shared {
    public interface IDeferredInteractionCompletion {
        bool PublishInteractionOnInput { get; }
        bool TryCompleteDeferredInteraction(string sourceId);
    }
}