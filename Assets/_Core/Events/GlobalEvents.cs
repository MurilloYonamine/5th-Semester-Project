using System;
using UnityEngine;

namespace FifthSemester.Core {
    public static class GlobalPlayerEvents {
        public static event Action<bool> OnPlayerSprint;
        public static void RaisePlayerSprint(bool isSprinting) {
            OnPlayerSprint?.Invoke(isSprinting);
        }
    }
}
