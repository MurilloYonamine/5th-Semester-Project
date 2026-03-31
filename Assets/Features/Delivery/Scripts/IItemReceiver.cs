// Author: Murillo Gomes Yonamine
// Date: 29/03/2026

using System;
using FifthSemester.Items;

namespace FifthSemester.Delivery {
    public interface IItemReceiver {
        event Action<IInteractable> OnItemReceived;
        void ReceiveItem(IInteractable item);
    }
}