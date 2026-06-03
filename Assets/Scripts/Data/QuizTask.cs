using System.Collections.Generic;

[System.Serializable]
public class QuizTask
{
    public int taskId; // идентификатор задачи (соответствует TaskModel.id)
    public string title;
    public string textForReceiver;
    public string questionText;
    public List<string> answers = new List<string>();
    public List<int> correctAnswerIndexes = new List<int>();
    public bool hasStars = false;
    public bool rewardEnabled = true;
    public int maxStars = 3;
    public float timeLimitSeconds = 60f;
    public bool hintEnabled = false;
    public string hintText = "";
    public int hintCost = 1;
    public string worldEvent = WorldEventKey.None;
}
