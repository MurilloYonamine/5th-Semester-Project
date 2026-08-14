namespace FifthSemester.Core.Services {
    public interface IHUDService {
        bool IsHUDVisible { get; set; }
        void ToggleHUD();
        void SetHUDVisible(bool visible);
    }
}
