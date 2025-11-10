namespace MyGame.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class TaskModel
    {
        public int id;
        public string title;

        // NPC, который даёт задание (имя или идентификатор)
        public string giverNpc;

        // NPC, который получает/выполняет задание
        public string receiverNpc;

        // Диалог / текст, отображаемый для дающего NPC
        public string textForGiver;

        // Диалог / текст, отображаемый для получателя NPC
        public string textForReceiver;

        // Варианты ответов (непустые строки). Порядок имеет значение.
        public List<string> answers = new List<string>();

        // Indexes of correct answers (0-based)
        public List<int> correctAnswerIndexes = new List<int>();

        // за выполнение этого задания начисляются звёзды
        public bool hasStars = false;
    }
}
