// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;

namespace FifthSemester.Items {
    public class MedicineItem : Item {
        public override string ToString() {
            var itemName = gameObject.name ?? "Unnamed Item";
            return $"{itemName}";
        }
    }
}
