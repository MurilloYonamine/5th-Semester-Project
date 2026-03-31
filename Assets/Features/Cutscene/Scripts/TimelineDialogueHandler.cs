using FifthSemester.DialogueSystem; 
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Cutscene {
    public class TimelineDialogueHandler : MonoBehaviour {
        [SerializeField] private PlayableDirector _director;

        public void PauseTimelineAndStartDialogue() {
            _director.Pause();
        }

        private void OnEnable() {
            DialogueManager.Instance.OnDialogueEnd += ResumeTimeline;
        }

        private void OnDisable() {
            DialogueManager.Instance.OnDialogueEnd -= ResumeTimeline;
        }

        public void ResumeTimeline() {
            _director.Resume();
        }
    }
}
