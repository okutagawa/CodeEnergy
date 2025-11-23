using System.Collections.Generic;

[System.Serializable]
public class QuizTask
{
    public string title;
    public string textForReceiver;
    public List<string> answers = new List<string>();
    public List<int> correctAnswerIndexes = new List<int>();
    public bool hasStars = false;
}
