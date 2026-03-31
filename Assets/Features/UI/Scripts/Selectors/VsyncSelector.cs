using FifthSemester.Framework.UI;
using UnityEngine;

namespace FifthSemester.UI {
    public class VsyncSelector : ToggleSelector {
        protected override void OnItemSelected(ToggleState selectedItem) {
            QualitySettings.vSyncCount = (selectedItem == ToggleState.On) ? 1 : 0;
        }
    }
}
