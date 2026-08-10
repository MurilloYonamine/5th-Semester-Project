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
<<<<<<< HEAD
        [Header("Identificação")]
=======
        [Header("Identity")]
>>>>>>> origin/main
        public string MissionId;
        public string Title;

        [TextArea(3, 6)]
        public string Description;

        [Header("Próxima Missão")]
        public MissionDefinition NextMission;

<<<<<<< HEAD
        [Header("Tipo e Conclusão")]
        public MissionType Type;

        [Header("Transição")]
        [Tooltip("Faz esta missão começar com fade." )]
        public bool UseFadeOnStart = false;

=======
        [Header("Type & Completion")]
        public MissionType Type;

>>>>>>> origin/main
        [ShowIf("Type", MissionType.PlayCutscene)]
        [Tooltip("Qual cutscene deve ser tocada quando esta missão iniciar?")]
        public CutsceneType TargetCutscene;

        [ShowIf("IsTalkToNpc")]
<<<<<<< HEAD
        [Tooltip("ID do NPC com quem o jogador deve falar.")]
        public string NpcId;

        [ShowIf("IsInteract")]
        [Tooltip("ID do objeto interagível que conclui a missão.")]
        public string InteractableTargetId;

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
=======
        [Tooltip("NPC ID to talk to")]
        public string NpcId;

        [ShowIf("IsCollectItems")]
        [Tooltip("Item name to collect")]
        public string TargetItemName;

        [ShowIf("IsCollectItems")]
        [Tooltip("Number of items to collect")]
        public int RequiredCount = 1;

        [ShowIf("IsCollectAndDeliver")]
        [Tooltip("Item name to collect")]
        public string CollectItemName;

        [ShowIf("IsCollectAndDeliver")]
        [Tooltip("Number of items to collect")]
        public int CollectCount = 1;

        [ShowIf("IsCollectAndDeliver")]
        [Tooltip("Delivery point IDs for deliver missions")]
        public string[] DeliveryPointIds;

        [Header("Persistence")]
        [Tooltip("Save progress for this mission")]
        public bool PersistProgress = true;

        [Header("Debug Setup")]
        [Tooltip("Events to apply when skipping to this mission")]
        public string[] DebugSetupEvents;

        [Header("End Game Setup")]
        [ShowIf("Type", MissionType.EndGame)] 
        [Tooltip("Prefab ou objeto da UI/Video que deve ser instanciado.")]
>>>>>>> origin/main
        public GameObject EndGamePrefab;

        [Header("Efeitos no Mapa")]
        public List<MapAction> MapActions;

        private bool IsTalkToNpc() => Type == MissionType.TalkToNpc;
<<<<<<< HEAD
        private bool IsInteract() => Type == MissionType.Interact;
=======
>>>>>>> origin/main
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
