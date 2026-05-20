// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    [CreateAssetMenu(fileName = "NewMissionSequence", menuName = "Missions/Sequence")]
    public class MissionSequenceSO : ScriptableObject {
        public List<MissionDefinition> Sequence;
    }
}
