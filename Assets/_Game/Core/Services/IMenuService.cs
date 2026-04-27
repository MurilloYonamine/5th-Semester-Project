using FifthSemester.Core.Enums;
using UnityEngine;

namespace FifthSemester.Core.Services {
    public interface IMenuService {
        void Show(MenuScreen screen);
        void Hide();

        void Register(MenuScreen screen, GameObject view);
        void Unregister(MenuScreen screen);

        GameObject GetView(MenuScreen screen);
    }
}