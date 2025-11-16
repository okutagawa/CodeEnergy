using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MyGame.Models;

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

    // Runtime
    private int rowIndex = -1;
    private InputFieldExpander expander;

    // Если строка соответствует существующей задаче, хранит её id, иначе -1
    public int existingTaskId = -1;

    // ВАЖНО: параллельные списки GUID для пунктов дропдауна
    // Индекс dropdown.value соответствует элементу из этих списков
    [HideInInspector] public List<string> giverOptionGuids = new List<string>();
    [HideInInspector] public List<string> receiverOptionGuids = new List<string>();

    public void Initialize(int index)
    {
        rowIndex = index;
        if (textId != null)
            textId.text = index.ToString();
    }

    public void SetExistingTaskId(int id)
    {
        existingTaskId = id;
        if (textId != null)
            textId.text = id > 0 ? id.ToString() : (rowIndex > 0 ? rowIndex.ToString() : "");
    }

    // Заполняем поля из модели (без установки dropdown.value — это сделает контроллер, когда заполнит списки GUID)
    public void FillFromModel(TaskModel model)
    {
        if (model == null) return;
        existingTaskId = model.id;
        if (textId != null) textId.text = model.id.ToString();

        if (inputTitle != null) inputTitle.text = model.title ?? "";

        if (inputTextGiver != null) inputTextGiver.text = model.textForGiver ?? "";
        if (inputTextReceiver != null) inputTextReceiver.text = model.textForReceiver ?? "";

        if (inputAnswers != null) inputAnswers.text = model.answers != null ? string.Join(";", model.answers) : "";
        if (inputCorrect != null) inputCorrect.text = model.correctAnswerIndexes != null
            ? string.Join(",", model.correctAnswerIndexes.Select(i => correctIndexesAreOneBased ? (i + 1).ToString() : i.ToString()))
            : "";

        if (toggleHasStars != null) toggleHasStars.isOn = model.hasStars;
    }

    // Вызывается контроллером после PopulateNpcDropdown, чтобы выставить нужные выбранные значения по GUID из модели
    public void ApplyModelSelection(TaskModel model)
    {
        if (model == null) return;

        if (dropdownGiver != null && giverOptionGuids != null && giverOptionGuids.Count > 0)
        {
            var idx = IndexOfGuid(giverOptionGuids, model.giverNpcGuid);
            dropdownGiver.value = idx >= 0 ? idx : 0;
        }

        if (dropdownReceiver != null && receiverOptionGuids != null && receiverOptionGuids.Count > 0)
        {
            var idx = IndexOfGuid(receiverOptionGuids, model.receiverNpcGuid);
            dropdownReceiver.value = idx >= 0 ? idx : 0;
        }

        // Обновим full labels после установки value
        SetupDropdownLabels();
    }

    private int IndexOfGuid(List<string> list, string guid)
    {
        if (string.IsNullOrEmpty(guid) || list == null) return -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == guid) return i;
        }
        return -1;
    }

    // Expander для больших вводов
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

    // Синхронизация полных лейблов с текущим выбором дропдауна
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

        // Проверим, что выбранные GUID существуют
        var giverGuid = GetSelectedGiverGuid();
        var receiverGuid = GetSelectedReceiverGuid();

        if (string.IsNullOrEmpty(giverGuid))
        {
            validationMessage = "Giver NPC is required";
            return false;
        }

        if (string.IsNullOrEmpty(receiverGuid))
        {
            validationMessage = "Receiver NPC is required";
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

    // Преобразование UI ? данные для сохранения (GUID вместо индексов)
    public RowData ToRowData()
    {
        return new RowData
        {
            title = inputTitle?.text ?? "",
            giverNpcGuid = GetSelectedGiverGuid(),
            receiverNpcGuid = GetSelectedReceiverGuid(),
            textForGiver = inputTextGiver?.text ?? "",
            textForReceiver = inputTextReceiver?.text ?? "",
            answers = GetAnswers(),
            correctAnswerIndexes = GetCorrectIndices(),
            hasStars = toggleHasStars != null && toggleHasStars.isOn
        };
    }

    private string GetSelectedGiverGuid()
    {
        if (dropdownGiver == null || giverOptionGuids == null || giverOptionGuids.Count == 0) return "";
        var i = dropdownGiver.value;
        if (i < 0 || i >= giverOptionGuids.Count) return "";
        return giverOptionGuids[i];
    }

    private string GetSelectedReceiverGuid()
    {
        if (dropdownReceiver == null || receiverOptionGuids == null || receiverOptionGuids.Count == 0) return "";
        var i = dropdownReceiver.value;
        if (i < 0 || i >= receiverOptionGuids.Count) return "";
        return receiverOptionGuids[i];
    }

    public class RowData
    {
        public string title;
        public string giverNpcGuid;
        public string receiverNpcGuid;
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
