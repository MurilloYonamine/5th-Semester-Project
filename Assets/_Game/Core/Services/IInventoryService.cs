using System;
using System.Collections.Generic;
using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Core.Services {
    public interface IInventoryService<TItem> {
        GameState CurrentState { get; set; }

        bool AddItem(TItem item);
        bool RemoveItem(TItem item);
        bool HasItem(TItem item);

        IReadOnlyList<TItem> GetItems();

        void OnGameStateChanged(GameStateChangedEvent evt);
    }
}