// autor: Murillo Gomes Yonamine
// data: 13/05/2026

namespace FifthSemester.Gameplay.Dialogue {
    public struct ParsedDialogueLine {
        public string speakerName;
        public string text;

        public ParsedDialogueLine(string speakerName, string text) {
            this.speakerName = speakerName;
            this.text = text;
        }
    }
}
