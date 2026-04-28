using UnityEngine;

namespace FifthSemester.Framework.BehaviourTrees {
    public class ActionWait : Node {
        
        private float waitTime;
        private float startTime;
        private bool isWaiting;

        public ActionWait(float waitTime, string name = "Wait") : base(name) {
            this.waitTime = waitTime;
            this.isWaiting = false;
        }

        public override Status Process() {
            if (!isWaiting) {
                startTime = Time.time;
                isWaiting = true;
            }

            if (Time.time - startTime >= waitTime) {
                return Status.Success;
            }

            return Status.Running;
        }

        public override void Reset() {
            base.Reset();
            isWaiting = false; 
        }
    }
}