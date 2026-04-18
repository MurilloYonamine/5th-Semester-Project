// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using System;
using UnityEngine;

namespace FifthSemester.Systems.DialogueSystem {
    [Serializable]
    public class DialogueLine {
        public CharacterSO speaker;
        [TextArea]
        public string text;
    }
}
