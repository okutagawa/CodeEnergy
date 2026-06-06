using System.Collections.Generic;
using System.Linq;
using MyGame.Data;
using MyGame.Models;
using UnityEngine;
using UnityEngine.UI;

public class FinalTestEditorPanelController : MonoBehaviour
{
    private const int MinAnswerCount = 2;
    private const int MaxAnswerCount = 5;
    private const int MaxQuestionPreviewCharacters = 8;

    [SerializeField] private InputField inputTestTitle;
    [SerializeField] private Dropdown dropdownRequiredCorrect;
    [SerializeField] private RectTransform questionsContent;
    [SerializeField] private GameObject questionItemPrefab;
    [SerializeField] private InputField inputQuestionText;
    [SerializeField] private Dropdown dropdownAnswersCount;
    [SerializeField] private Button buttonAddQuestion;
    [SerializeField] private Button buttonEditQuestion;
    [SerializeField] private Button buttonDeleteQuestion;
    [SerializeField] private Button buttonSaveFinalTest;
    [SerializeField] private Button buttonClearForm;
    [SerializeField] private Button buttonExit;
    [SerializeField] private InputField[] inputAnswers = new InputField[5];
    [SerializeField] private Toggle[] toggleCorrect = new Toggle[5];
    [SerializeField] private GameObject[] answerRows = new GameObject[5];

    private CoursesContainer _courses;
    private CourseModel _course;
    private FinalTestModel _draft = new FinalTestModel();
    private readonly List<GameObject> _spawnedQuestionRows = new List<GameObject>();
    private int _selectedQuestionIndex = -1;

    private void Awake()
    {
        TryAutoAssignRefs();
        BindButtons();
        BindAnswerCountDropdown();
    }

    private void OnEnable()
    {
        BindButtons();
        BindAnswerCountDropdown();
    }

    private void OnDisable()
    {
        if (buttonAddQuestion != null) buttonAddQuestion.onClick.RemoveListener(AddQuestionFromForm);
        if (buttonEditQuestion != null) buttonEditQuestion.onClick.RemoveListener(EditSelectedQuestionFromForm);
        if (buttonDeleteQuestion != null) buttonDeleteQuestion.onClick.RemoveListener(DeleteSelectedQuestion);
        if (buttonSaveFinalTest != null) buttonSaveFinalTest.onClick.RemoveListener(SaveFinalTest);
        if (buttonClearForm != null) buttonClearForm.onClick.RemoveListener(ClearQuestionForm);
        if (buttonExit != null) buttonExit.onClick.RemoveListener(CloseToTasksPanel);
        if (dropdownAnswersCount != null) dropdownAnswersCount.onValueChanged.RemoveListener(OnAnswerCountDropdownChanged);
    }

    public void OpenForCourse(int courseId)
    {
        TryAutoAssignRefs();
        _courses = DataManager.LoadCourses();
        _course = _courses?.courses?.FirstOrDefault(c => c != null && c.id == courseId);
        if (_course == null)
        {
            ShowValidationError("Ќе удалось открыть итоговый тест: курс не найден.");
            Debug.LogError($"[FinalTestEditor] Course not found: {courseId}");
            return;
        }

        DataManager.NormalizeFinalTestDefaults(_course.finalTest);
        _draft = CloneFinalTest(_course.finalTest);
        if (inputTestTitle != null) inputTestTitle.text = _draft.title ?? string.Empty;
        RebuildRequiredCorrectOptions();
        RefreshQuestionsList();
        ClearQuestionForm();
    }

    private void AddQuestionFromForm()
    {
        if (!TryReadQuestionForm(out var question)) return;
        _draft.questions.Add(question);
        _selectedQuestionIndex = _draft.questions.Count - 1;
        RebuildRequiredCorrectOptions();
        RefreshQuestionsList();
        ClearQuestionForm();
    }

    private void EditSelectedQuestionFromForm()
    {
        if (_selectedQuestionIndex < 0 || _selectedQuestionIndex >= _draft.questions.Count) return;
        if (!TryReadQuestionForm(out var question)) return;
        _draft.questions[_selectedQuestionIndex] = question;
        RefreshQuestionsList();
    }

    private void DeleteSelectedQuestion()
    {
        if (_selectedQuestionIndex < 0 || _selectedQuestionIndex >= _draft.questions.Count) return;
        _draft.questions.RemoveAt(_selectedQuestionIndex);
        _selectedQuestionIndex = -1;
        RebuildRequiredCorrectOptions();
        RefreshQuestionsList();
        ClearQuestionForm();
    }

    private void SaveFinalTest()
    {
        if (_course == null)
        {
            ShowValidationError("Ќе удалось сохранить итоговый тест: курс не найден.");
            return;
        }
        _draft.title = inputTestTitle != null ? inputTestTitle.text.Trim() : string.Empty;
        _draft.requiredCorrectAnswers = dropdownRequiredCorrect != null ? dropdownRequiredCorrect.value + 1 : Mathf.Min(1, _draft.questions.Count);
        DataManager.NormalizeFinalTestDefaults(_draft);
        _course.finalTest = CloneFinalTest(_draft);
        DataManager.SaveCourses(_courses);
        Debug.Log($"[FinalTestEditor] Saved final test for course {_course.id}: questions={_course.finalTest.questions.Count}, required={_course.finalTest.requiredCorrectAnswers}");
    }

    private bool TryReadQuestionForm(out FinalTestQuestionModel question)
    {
        question = null;
        int answerCount = GetSelectedAnswerCount();
        if (inputQuestionText == null || string.IsNullOrWhiteSpace(inputQuestionText.text))
        {
            ShowValidationError("¬ведите текст вопроса итогового теста.");
            return false;
        }

        var answers = new List<string>();
        var correct = new List<int>();
        for (int i = 0; i < answerCount; i++)
        {
            if (inputAnswers[i] == null || string.IsNullOrWhiteSpace(inputAnswers[i].text))
            {
                ShowValidationError($"«аполните ответ {i + 1}.");
                return false;
            }
            answers.Add(inputAnswers[i].text.Trim());
            if (toggleCorrect[i] != null && toggleCorrect[i].isOn) correct.Add(i);
        }
        if (correct.Count == 0)
        {
            ShowValidationError("¬ыберите хот€ бы один правильный ответ.");
            return false;
        }

        question = new FinalTestQuestionModel
        {
            questionText = inputQuestionText.text.Trim(),
            answerCount = answerCount,
            answers = answers,
            correctAnswerIndexes = correct
        };
        return true;
    }

    private void ShowValidationError(string message)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowError(UserErrorMessages.FromValidation(message));
            return;
        }

        ErrorPopupController.Show(UserErrorMessages.FromValidation(message));
    }

    private void SelectQuestion(int index)
    {
        if (index < 0 || index >= _draft.questions.Count) return;
        _selectedQuestionIndex = index;
        FillQuestionForm(_draft.questions[index]);
        RefreshQuestionsList();
    }

    private void FillQuestionForm(FinalTestQuestionModel question)
    {
        if (inputQuestionText != null) inputQuestionText.text = question.questionText ?? string.Empty;
        SetAnswerCount(question.answerCount);
        for (int i = 0; i < MaxAnswerCount; i++)
        {
            if (inputAnswers[i] != null) inputAnswers[i].text = question.answers != null && i < question.answers.Count ? question.answers[i] : string.Empty;
            if (toggleCorrect[i] != null) toggleCorrect[i].isOn = question.correctAnswerIndexes != null && question.correctAnswerIndexes.Contains(i);
        }
    }

    private void ClearQuestionForm()
    {
        if (inputQuestionText != null) inputQuestionText.text = string.Empty;
        SetAnswerCount(MinAnswerCount);
        foreach (var input in inputAnswers) if (input != null) input.text = string.Empty;
        foreach (var toggle in toggleCorrect) if (toggle != null) toggle.isOn = false;
    }

    private void RefreshQuestionsList()
    {
        foreach (var row in _spawnedQuestionRows) if (row != null) Destroy(row);
        _spawnedQuestionRows.Clear();
        if (questionsContent == null) return;

        for (int i = 0; i < _draft.questions.Count; i++)
        {
            var row = questionItemPrefab != null ? Instantiate(questionItemPrefab, questionsContent) : new GameObject("FinalTestQuestionItem", typeof(RectTransform), typeof(Button), typeof(Text));
            int index = i;
            SetQuestionRowText(row, i + 1, _draft.questions[i].questionText);
            var button = row.GetComponent<Button>() ?? row.GetComponentInChildren<Button>(true);
            if (button != null) button.onClick.AddListener(() => SelectQuestion(index));
            _spawnedQuestionRows.Add(row);
        }
    }

    private void SetQuestionRowText(GameObject row, int questionNumber, string questionText)
    {
        if (row == null) return;

        var orderText = FindTextInChildren(row, "Text_OrderNumber");
        var previewText = FindTextInChildren(row, "Text_QuestionPreview");

        if (orderText != null)
        {
            orderText.text = $"{questionNumber}.";
            orderText.horizontalOverflow = HorizontalWrapMode.Overflow;
            orderText.verticalOverflow = VerticalWrapMode.Truncate;
        }

        if (previewText != null)
        {
            previewText.text = TruncateQuestionPreview(questionText);
            previewText.horizontalOverflow = HorizontalWrapMode.Overflow;
            previewText.verticalOverflow = VerticalWrapMode.Truncate;
            return;
        }

        var fallbackText = row.GetComponentInChildren<Text>(true);
        if (fallbackText != null)
        {
            fallbackText.text = $"{questionNumber}. {TruncateQuestionPreview(questionText)}";
            fallbackText.verticalOverflow = VerticalWrapMode.Truncate;
        }
    }

    private Text FindTextInChildren(GameObject root, string childName)
    {
        foreach (var text in root.GetComponentsInChildren<Text>(true))
        {
            if (text != null && text.name == childName) return text;
        }
        return null;
    }

    private string TruncateQuestionPreview(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        value = value.Trim();
        if (value.Length <= MaxQuestionPreviewCharacters) return value;

        return value.Substring(0, MaxQuestionPreviewCharacters).TrimEnd() + "...";
    }

    private void RebuildRequiredCorrectOptions()
    {
        if (dropdownRequiredCorrect == null) return;
        int count = Mathf.Max(1, _draft.questions.Count);
        int old = dropdownRequiredCorrect.value;
        dropdownRequiredCorrect.ClearOptions();
        dropdownRequiredCorrect.AddOptions(Enumerable.Range(1, count).Select(x => x.ToString()).ToList());
        dropdownRequiredCorrect.SetValueWithoutNotify(Mathf.Clamp(_draft.requiredCorrectAnswers > 0 ? _draft.requiredCorrectAnswers - 1 : old, 0, count - 1));
        dropdownRequiredCorrect.RefreshShownValue();
    }

    private void BindButtons()
    {
        if (buttonAddQuestion != null) { buttonAddQuestion.onClick.RemoveListener(AddQuestionFromForm); buttonAddQuestion.onClick.AddListener(AddQuestionFromForm); }
        if (buttonEditQuestion != null) { buttonEditQuestion.onClick.RemoveListener(EditSelectedQuestionFromForm); buttonEditQuestion.onClick.AddListener(EditSelectedQuestionFromForm); }
        if (buttonDeleteQuestion != null) { buttonDeleteQuestion.onClick.RemoveListener(DeleteSelectedQuestion); buttonDeleteQuestion.onClick.AddListener(DeleteSelectedQuestion); }
        if (buttonSaveFinalTest != null) { buttonSaveFinalTest.onClick.RemoveListener(SaveFinalTest); buttonSaveFinalTest.onClick.AddListener(SaveFinalTest); }
        if (buttonClearForm != null) { buttonClearForm.onClick.RemoveListener(ClearQuestionForm); buttonClearForm.onClick.AddListener(ClearQuestionForm); }
        if (buttonExit != null) { buttonExit.onClick.RemoveListener(CloseToTasksPanel); buttonExit.onClick.AddListener(CloseToTasksPanel); }
    }

    private void BindAnswerCountDropdown()
    {
        if (dropdownAnswersCount == null) return;
        dropdownAnswersCount.ClearOptions();
        dropdownAnswersCount.AddOptions(new List<string> { "2", "3", "4", "5" });
        dropdownAnswersCount.onValueChanged.RemoveListener(OnAnswerCountDropdownChanged);
        dropdownAnswersCount.onValueChanged.AddListener(OnAnswerCountDropdownChanged);
    }

    private void OnAnswerCountDropdownChanged(int value)
    {
        ApplyAnswerCount(GetSelectedAnswerCount());
    }

    private int GetSelectedAnswerCount() => dropdownAnswersCount != null ? Mathf.Clamp(dropdownAnswersCount.value + MinAnswerCount, MinAnswerCount, MaxAnswerCount) : MinAnswerCount;
    private void SetAnswerCount(int count)
    {
        count = Mathf.Clamp(count, MinAnswerCount, MaxAnswerCount);
        if (dropdownAnswersCount != null) dropdownAnswersCount.SetValueWithoutNotify(count - MinAnswerCount);
        ApplyAnswerCount(count);
    }
    private void ApplyAnswerCount(int count)
    {
        for (int i = 0; i < answerRows.Length; i++) if (answerRows[i] != null) answerRows[i].SetActive(i < count);
    }

    private void CloseToTasksPanel()
    {
        if (UIManager.Instance != null && _course != null) UIManager.Instance.OpenTasksWindowForCourse(_course.id);
        else gameObject.SetActive(false);
    }

    private void TryAutoAssignRefs()
    {
        if (inputAnswers == null || inputAnswers.Length != MaxAnswerCount) inputAnswers = new InputField[MaxAnswerCount];
        if (toggleCorrect == null || toggleCorrect.Length != MaxAnswerCount) toggleCorrect = new Toggle[MaxAnswerCount];
        if (answerRows == null || answerRows.Length != MaxAnswerCount) answerRows = new GameObject[MaxAnswerCount];
        inputTestTitle = inputTestTitle != null ? inputTestTitle : FindComponent<InputField>("Input_TestTitle");
        dropdownRequiredCorrect = dropdownRequiredCorrect != null ? dropdownRequiredCorrect : FindComponent<Dropdown>("Dropdown_RequiredCorrect");
        inputQuestionText = inputQuestionText != null ? inputQuestionText : FindComponent<InputField>("Input_QuestionText");
        dropdownAnswersCount = dropdownAnswersCount != null ? dropdownAnswersCount : FindComponent<Dropdown>("Dropdown_AnswersCount");
        buttonAddQuestion = buttonAddQuestion != null ? buttonAddQuestion : FindComponent<Button>("ButtonAddQuestion");
        buttonEditQuestion = buttonEditQuestion != null ? buttonEditQuestion : FindComponent<Button>("ButtonEditQuestion");
        buttonDeleteQuestion = buttonDeleteQuestion != null ? buttonDeleteQuestion : FindComponent<Button>("ButtonDeleteQuestion");
        buttonSaveFinalTest = buttonSaveFinalTest != null ? buttonSaveFinalTest : FindComponent<Button>("ButtonSaveFinalTest");
        buttonClearForm = buttonClearForm != null ? buttonClearForm : FindComponent<Button>("ButtonClearForm");
        buttonExit = buttonExit != null ? buttonExit : FindComponent<Button>("BtnExit");
        if (questionsContent == null)
        {
            var scroll = FindComponent<ScrollRect>("QuestionsScrollView");
            if (scroll != null) questionsContent = scroll.content;
        }
        for (int i = 0; i < MaxAnswerCount; i++)
        {
            inputAnswers[i] = inputAnswers[i] != null ? inputAnswers[i] : FindComponent<InputField>($"Input_Answer{i + 1}");
            toggleCorrect[i] = toggleCorrect[i] != null ? toggleCorrect[i] : FindComponent<Toggle>($"Toggle_Correct{i + 1}");
            answerRows[i] = answerRows[i] != null ? answerRows[i] : FindChild($"Row_Answer{i + 1}");
        }
    }

    private T FindComponent<T>(string name) where T : Component
    {
        var child = FindChild(name);
        return child != null ? child.GetComponent<T>() : null;
    }

    private GameObject FindChild(string name)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true)) if (t != null && t.name == name) return t.gameObject;
        return null;
    }

    private static FinalTestModel CloneFinalTest(FinalTestModel source)
    {
        var clone = new FinalTestModel
        {
            title = source?.title ?? string.Empty,
            requiredCorrectAnswers = source?.requiredCorrectAnswers ?? 1,
            questions = new List<FinalTestQuestionModel>()
        };
        if (source?.questions != null)
        {
            foreach (var q in source.questions)
            {
                if (q == null) continue;
                clone.questions.Add(new FinalTestQuestionModel
                {
                    questionText = q.questionText,
                    answerCount = q.answerCount,
                    answers = q.answers != null ? new List<string>(q.answers) : new List<string>(),
                    correctAnswerIndexes = q.correctAnswerIndexes != null ? new List<int>(q.correctAnswerIndexes) : new List<int>()
                });
            }
        }
        return clone;
    }
}