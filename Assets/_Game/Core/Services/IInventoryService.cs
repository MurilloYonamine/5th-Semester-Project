using System;
using System.Collections.Generic;

namespace FifthSemester.Core.Services
{
    public interface IInventoryService<TItem>
    {
        bool AddItem(TItem item);
        bool RemoveItem(TItem item);
        bool HasItem(TItem item);

        IReadOnlyList<TItem> GetItems();

        event Action<TItem> OnItemAdded;
        event Action<TItem> OnItemRemoved;
    }
}