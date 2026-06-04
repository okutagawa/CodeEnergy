using System.Collections.Generic;
using System.Linq;
using MyGame.Data;
using MyGame.Models;
using UnityEngine;

public class FinalTestController : MonoBehaviour
{
    [SerializeField] private QuizPanelController quizPanel;
    [SerializeField] private FinalTestResultPanelController resultPanel;
    [SerializeField] private string activationWorldEvent = WorldEventKey.ActivatePortal;

    private FinalTestModel _currentTest;
    private int _currentQuestionIndex;
    private int _correctAnswers;

    public void StartFinalTestForSelectedCourse()
    {
        var gameState = GameState.Instance;
        int courseId = gameState != null ? gameState.GetData().selectedCourseId : -1;
        StartFinalTestForCourse(courseId);
    }

    public void StartFinalTestForCourse(int courseId)
    {
        EnsureRefs();
        var course = DataManager.LoadCourses()?.courses?.FirstOrDefault(c => c != null && c.id == courseId);
        if (course == null)
        {
            Debug.LogError($"[FinalTest] Course not found: {courseId}");
            return;
        }

        if (!AreAllCourseTasksCompleted(course))
        {
            Debug.LogWarning($"[FinalTest] Course {courseId} final test is locked until all course tasks are completed.");
            return;
        }

        DataManager.NormalizeFinalTestDefaults(course.finalTest);
        if (course.finalTest == null || course.finalTest.questions == null || course.finalTest.questions.Count == 0)
        {
            Debug.LogWarning($"[FinalTest] Course {courseId} has no final test questions.");
            return;
        }

        _currentTest = course.finalTest;
        _currentQuestionIndex = 0;
        _correctAnswers = 0;
        ShowCurrentQuestion();
    }

    public void RetryFinalTest()
    {
        if (_currentTest == null)
        {
            StartFinalTestForSelectedCourse();
            return;
        }

        _currentQuestionIndex = 0;
        _correctAnswers = 0;
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        EnsureRefs();
        if (_currentTest == null || _currentQuestionIndex >= _currentTest.questions.Count)
        {
            FinishTest();
            return;
        }

        if (quizPanel == null)
        {
            Debug.LogError("[FinalTest] QuizPanelController not found in scene.");
            return;
        }

        var question = _currentTest.questions[_currentQuestionIndex];
        var quizTask = new QuizTask
        {
            taskId = -100000 - _currentQuestionIndex,
            title = string.IsNullOrWhiteSpace(_currentTest.title) ? "Финальный тест" : _currentTest.title,
            questionText = question.questionText,
            answers = question.answers != null ? new List<string>(question.answers.Take(question.answerCount)) : new List<string>(),
            correctAnswerIndexes = question.correctAnswerIndexes != null ? new List<int>(question.correctAnswerIndexes) : new List<int>(),
            rewardEnabled = false,
            hintEnabled = false,
            worldEvent = WorldEventKey.None
        };

        quizPanel.ShowFinalTestQuestion(quizTask, HandleQuestionAnswered);
    }

    private void HandleQuestionAnswered(bool isCorrect)
    {
        if (isCorrect) _correctAnswers++;
        _currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void FinishTest()
    {
        if (quizPanel != null) quizPanel.ForceCloseFromReward();

        int total = _currentTest?.questions?.Count ?? 0;
        int required = _currentTest != null ? Mathf.Clamp(_currentTest.requiredCorrectAnswers, 0, total) : 0;
        bool passed = total > 0 && _correctAnswers >= required;

        if (passed && GameState.Instance != null)
        {
            GameState.Instance.MarkWorldEventCompleted(activationWorldEvent);
        }

        EnsureRefs();
        if (resultPanel != null)
            resultPanel.Show(this, _correctAnswers, total, required, passed);
    }

    public static bool AreAllCourseTasksCompleted(CourseModel course)
    {
        if (course == null || GameState.Instance == null) return false;

        var requiredTaskIds = course.taskIds != null
            ? course.taskIds.Where(id => id >= 0).Distinct().ToList()
            : new List<int>();

        if (requiredTaskIds.Count == 0) return true;

        var completedTaskIds = GameState.Instance.GetData().completedTaskIds;
        return completedTaskIds != null && requiredTaskIds.All(id => completedTaskIds.Contains(id));
    }

    private void EnsureRefs()
    {
        if (quizPanel == null) quizPanel = FindObjectOfType<QuizPanelController>(true);
        if (resultPanel == null) resultPanel = FindObjectOfType<FinalTestResultPanelController>(true);
        if (resultPanel == null)
        {
            var resultPanelObject = FindSceneGameObject("FinalTestResultPanel");
            if (resultPanelObject != null)
                resultPanel = resultPanelObject.GetComponent<FinalTestResultPanelController>() ?? resultPanelObject.AddComponent<FinalTestResultPanelController>();
        }
    }

    private static GameObject FindSceneGameObject(string objectName)
    {
        var transforms = FindObjectsOfType<Transform>(true);
        foreach (var t in transforms)
        {
            if (t != null && t.name == objectName) return t.gameObject;
        }

        return null;
    }
}