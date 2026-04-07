// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Systems.DialogueSystem {
    [CreateAssetMenu(menuName = "Dialogue/Dialogue")]
    public class DialogueSO : ScriptableObject {
        public List<DialogueLine> lines;
    }
}
