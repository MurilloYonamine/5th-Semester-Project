// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

namespace FifthSemester.Core.Services {
    public interface IItemRegistry<TItem> {
        void RegisterItemPrefab(string itemId, TItem prefab);
        TItem GetItemPrefab(string itemId);
        TItem InstantiateItem(string itemId);
    }
}
