using System;
using System.Collections.Generic;

namespace MyGame.Models
{
    [Serializable]
    public class FinalTestQuestionModel
    {
        public string questionText;
        public int answerCount = 4;
        public List<string> answers = new List<string>();
        public List<int> correctAnswerIndexes = new List<int>();
    }

    [Serializable]
    public class FinalTestModel
    {
        public string title;
        public int requiredCorrectAnswers = 1;
        public List<FinalTestQuestionModel> questions = new List<FinalTestQuestionModel>();
    }
}