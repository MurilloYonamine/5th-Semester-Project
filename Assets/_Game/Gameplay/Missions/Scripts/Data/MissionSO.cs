// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using FifthSemester.Core.Enums;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    [CreateAssetMenu(menuName = "Mission/New Mission", fileName = "NewMission")]
    public class MissionDefinition : ScriptableObject {
        [Header("Identificação")]
        public string MissionId;
        public string Title;

        [TextArea(3, 6)]
        public string Description;

        [Header("Próxima Missão")]
        public MissionDefinition NextMission;

        [Header("Tipo e Conclusão")]
        public MissionType Type;

        [Header("Transição")]
        [Tooltip("Faz esta missão começar com fade." )]
        public bool UseFadeOnStart = false;

        [ShowIf("Type", MissionType.PlayCutscene)]
        [Tooltip("Qual cutscene deve ser tocada quando esta missão iniciar?")]
        public CutsceneType TargetCutscene;

        [ShowIf("IsTalkToNpc")]
        [Tooltip("ID do NPC com quem o jogador deve falar.")]
        public string NpcId;

        [ShowIf("IsCollectItems")]
        [Tooltip("Nome do item que deve ser coletado.")]
        public string TargetItemName;

        [ShowIf("IsCollectItems")]
        [Tooltip("Quantidade necessária para concluir a missão.")]
        public int RequiredCount = 1;

        [ShowIf("IsCollectAndDeliver")]
        [Tooltip("Nome do item que deve ser coletado.")]
        public string CollectItemName;

        [ShowIf("IsCollectAndDeliver")]
        [Tooltip("Quantidade necessária para coleta.")]
        public int CollectCount = 1;

        [ShowIf("IsCollectAndDeliver")]
        [Tooltip("IDs dos pontos de entrega da missão.")]
        public string[] DeliveryPointIds;

        [Header("Persistência")]
        [Tooltip("Salva o progresso desta missão.")]
        public bool PersistProgress = true;

        [Header("Configuração de Debug")]
        [Tooltip("Eventos aplicados ao pular para esta missão.")]
        public string[] DebugSetupEvents;

        [Header("Configuração de Fim de Jogo")]
        [ShowIf("Type", MissionType.EndGame)] 
        [Tooltip("Prefab ou objeto de UI/vídeo que deve ser instanciado.")]
        public GameObject EndGamePrefab;

        [Header("Efeitos no Mapa")]
        public List<MapAction> MapActions;

        private bool IsTalkToNpc() => Type == MissionType.TalkToNpc;
        private bool IsCollectItems() => Type == MissionType.CollectItems;
        private bool IsCollectAndDeliver() => Type == MissionType.CollectAndDeliver;
        private bool IsPlayCutscene() => Type == MissionType.PlayCutscene;
    }
    [System.Serializable]
    public struct MapAction {
        public enum ActionType { Activate, Deactivate, LockDoor, UnlockDoor, LockAllDoorsExcept }

        public ActionType Type;

        [ShowIf("IsSingleDoorAction")]
        public DoorType TargetDoor;

        [ShowIf("IsLockAllExceptAction")]
        public DoorType[] DoorsToKeepUnlocked;

        [HideIf("IsAnyDoorAction")]
        public string TargetObjectId;

        private bool IsSingleDoorAction() {
            return Type == ActionType.LockDoor || Type == ActionType.UnlockDoor;
        }

        private bool IsLockAllExceptAction() {
            return Type == ActionType.LockAllDoorsExcept;
        }

        private bool IsAnyDoorAction() {
            return IsSingleDoorAction() || IsLockAllExceptAction();
        }
    }
}
