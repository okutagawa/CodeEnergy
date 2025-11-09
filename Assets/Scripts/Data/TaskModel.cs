namespace MyGame.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class TaskModel
    {
        public int id;
        public string title;

        // NPC who gives the task (name or identifier)
        public string giverNpc;

        // NPC who receives/completes the task
        public string receiverNpc;

        // Dialogue / text shown to the giver NPC
        public string textForGiver;

        // Dialogue / text shown to the receiver NPC
        public string textForReceiver;

        // Answer options (non-empty strings). Order matters.
        public List<string> answers = new List<string>();

        // Indexes of correct answers (0-based)
        public List<int> correctAnswerIndexes = new List<int>();

        // Flag whether this task awards stars
        public bool hasStars = false;
    }
}
