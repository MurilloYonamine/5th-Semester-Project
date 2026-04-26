namespace FifthSemester.Core.Services 
{
    public interface IMenuService 
    {
        bool IsAnyMenuOpen { get; }
        void TogglePauseMenu();
        void CloseAllMenus();
    }
}