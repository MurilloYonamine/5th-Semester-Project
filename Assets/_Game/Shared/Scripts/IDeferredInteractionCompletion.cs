namespace FifthSemester.Shared {
    public interface IDeferredInteractionCompletion {
        bool PublishInteractionOnInput { get; }
        bool TryCompleteDeferredInteraction(string sourceId);
    }
}