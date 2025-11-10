using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TaskRow : MonoBehaviour
{
    [Header("UI widgets (assign in prefab)")]
    public Text textId;
    public InputField inputTitle;
    public Dropdown dropdownGiver;
    public Dropdown dropdownReceiver;
    public InputField inputTextGiver;
    public InputField inputTextReceiver;
    public InputField inputAnswers;
    public InputField inputCorrect;
    public Toggle toggleHasStars;

    [Header("Optional labels")]
    public Text fullGiverLabel;
    public Text fullReceiverLabel;

    [Header("Settings")]
    public bool correctIndexesAreOneBased = true;

    // runtime
    private int rowIndex = -1;
    private InputFieldExpander expander;
    public int existingTaskId = -1; // -1 = новая строка, >=0 = соответствует TaskModel.id

    public void Initialize(int index)
    {
        rowIndex = index;
        if (textId != null)
            textId.text = index.ToString();
    }

    // отметить существующий идентификатор (в визуальном идентификаторе будет отображаться идентификатор задачи, если он >0)
    public void SetExistingTaskId(int id)
    {
        existingTaskId = id;
        if (textId != null)
            textId.text = id > 0 ? id.ToString() : (rowIndex > 0 ? rowIndex.ToString() : "");
    }

    // заполнение полей пользовательского интерфейса из существующей TaskModel
    public void FillFromModel(MyGame.Models.TaskModel model)
    {
        if (model == null) return;
        existingTaskId = model.id;
        if (textId != null) textId.text = model.id.ToString();
        if (inputTitle != null) inputTitle.text = model.title;
        if (inputTextGiver != null) inputTextGiver.text = model.textForGiver;
        if (inputTextReceiver != null) inputTextReceiver.text = model.textForReceiver;
        if (inputAnswers != null) inputAnswers.text = model.answers != null ? string.Join(";", model.answers) : "";
        if (inputCorrect != null) inputCorrect.text = model.correctAnswerIndexes != null
            ? string.Join(",", model.correctAnswerIndexes.Select(i => correctIndexesAreOneBased ? (i + 1).ToString() : i.ToString()))
            : "";
        if (toggleHasStars != null) toggleHasStars.isOn = model.hasStars;
    }

    // Настройка экспандера: добавление записей EventTrigger в небольшие поля ввода
    public void SetupExpander(InputFieldExpander exp)
    {
        expander = exp;
        if (expander == null) return;

        void AddTrigger(InputField field)
        {
            if (field == null) return;
            var go = field.gameObject;
            var trigger = go.GetComponent<EventTrigger>();
            if (trigger == null) trigger = go.AddComponent<EventTrigger>();

            var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            clickEntry.callback.AddListener((data) => { expander.Expand(field); });
            trigger.triggers.Add(clickEntry);

            var selectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            selectEntry.callback.AddListener((data) => { expander.Expand(field); });
            trigger.triggers.Add(selectEntry);
        }

        AddTrigger(inputTitle);
        AddTrigger(inputTextGiver);
        AddTrigger(inputTextReceiver);
        AddTrigger(inputAnswers);
        AddTrigger(inputCorrect);
    }

    // синхронизация полных текстов с метками с выпадающим списком
    public void SetupDropdownLabels()
    {
        if (dropdownGiver != null)
        {
            dropdownGiver.onValueChanged.RemoveAllListeners();
            dropdownGiver.onValueChanged.AddListener(i =>
            {
                if (fullGiverLabel != null)
                    fullGiverLabel.text = (dropdownGiver.options.Count > i) ? dropdownGiver.options[i].text : "";
            });

            if (fullGiverLabel != null && dropdownGiver.options.Count > 0)
                fullGiverLabel.text = dropdownGiver.options[dropdownGiver.value].text;
        }

        if (dropdownReceiver != null)
        {
            dropdownReceiver.onValueChanged.RemoveAllListeners();
            dropdownReceiver.onValueChanged.AddListener(i =>
            {
                if (fullReceiverLabel != null)
                    fullReceiverLabel.text = (dropdownReceiver.options.Count > i) ? dropdownReceiver.options[i].text : "";
            });

            if (fullReceiverLabel != null && dropdownReceiver.options.Count > 0)
                fullReceiverLabel.text = dropdownReceiver.options[dropdownReceiver.value].text;
        }
    }

    // Валидация
    public bool IsValid(out string validationMessage)
    {
        validationMessage = null;
        if (inputTitle == null || string.IsNullOrWhiteSpace(inputTitle.text))
        {
            validationMessage = "Title is required";
            return false;
        }

        var answers = GetAnswers();
        if (answers == null || answers.Length < 1)
        {
            validationMessage = "At least one answer required";
            return false;
        }

        var correct = GetCorrectIndices();
        if (correct == null || correct.Length == 0)
        {
            validationMessage = "At least one correct answer required";
            return false;
        }

        if (correct.Any(i => i < 0 || i >= answers.Length))
        {
            validationMessage = "Correct answer index out of range";
            return false;
        }

        return true;
    }

    public string[] GetAnswers()
    {
        if (inputAnswers == null) return new string[0];
        var raw = inputAnswers.text ?? "";
        var parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => s.Length > 0)
                       .ToArray();
        return parts;
    }

    public int[] GetCorrectIndices()
    {
        if (inputCorrect == null) return new int[0];
        var raw = inputCorrect.text ?? "";
        var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => s.Length > 0)
                       .ToArray();
        if (parts.Length == 0) return new int[0];

        try
        {
            var idxs = parts.Select(p =>
            {
                var val = int.Parse(p);
                return correctIndexesAreOneBased ? val - 1 : val;
            }).ToArray();
            return idxs;
        }
        catch
        {
            return new int[0];
        }
    }

    // Преобразование пользовательского интерфейса в промежуточные данные
    public RowData ToRowData()
    {
        return new RowData
        {
            title = inputTitle?.text ?? "",
            giverIndex = dropdownGiver != null ? dropdownGiver.value : -1,
            receiverIndex = dropdownReceiver != null ? dropdownReceiver.value : -1,
            textForGiver = inputTextGiver?.text ?? "",
            textForReceiver = inputTextReceiver?.text ?? "",
            answers = GetAnswers(),
            correctAnswerIndexes = GetCorrectIndices(),
            hasStars = toggleHasStars != null && toggleHasStars.isOn
        };
    }

    public class RowData
    {
        public string title;
        public int giverIndex;
        public int receiverIndex;
        public string textForGiver;
        public string textForReceiver;
        public string[] answers;
        public int[] correctAnswerIndexes;
        public bool hasStars;
    }

    private void OnDestroy()
    {
        void RemoveTriggers(InputField field)
        {
            if (field == null) return;
            var trigger = field.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) return;
            trigger.triggers.Clear();
            Destroy(trigger);
        }

        RemoveTriggers(inputTitle);
        RemoveTriggers(inputTextGiver);
        RemoveTriggers(inputTextReceiver);
        RemoveTriggers(inputAnswers);
        RemoveTriggers(inputCorrect);

        if (dropdownGiver != null) dropdownGiver.onValueChanged.RemoveAllListeners();
        if (dropdownReceiver != null) dropdownReceiver.onValueChanged.RemoveAllListeners();
    }
}
