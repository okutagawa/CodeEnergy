namespace MyGame.Models
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class TaskModel
    {
        public int id;
        public int courseId = -1;
        public string title;

        // NPC, который даёт задание (имя или идентификатор)
        public string giverNpcGuid;

        // NPC, который получает/выполняет задание
        public string receiverNpcGuid;

        // Диалог / текст, отображаемый для дающего NPC
        public string textForGiver;

        // Диалог / текст, отображаемый для получателя NPC
        public string textForReceiver;

        public string questionText;

        public int answerCount = 4;

        // Варианты ответов (непустые строки). Порядок имеет значение.
        public List<string> answers = new List<string>();

        // Indexes of correct answers (0-based)
        public List<int> correctAnswerIndexes = new List<int>();

        // за выполнение этого задания начисляются звёзды
        public bool hasStars = false;

        // New reward settings.
        public bool rewardEnabled = true;
        public int maxStars = 3;
        public float timeLimitSeconds = 60f;

        public string worldEvent = WorldEventKey.None;
    }
}
