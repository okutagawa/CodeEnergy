using System;
using UnityEngine;

public static class UserErrorMessages
{
    public const string Generic = "Произошла ошибка. Попробуйте повторить действие.";

    public static string FromLog(string condition, string stackTrace, LogType type)
    {
        var source = condition ?? string.Empty;

        if (Contains(source, "courseSelectPanel") || Contains(source, "CourseSelectPanelController"))
            return "Не удалось открыть выбор курса. Проверьте, что панель выбора курса настроена в сцене.";

        if (Contains(source, "settingsPanel") || Contains(source, "SettingsController"))
            return "Не удалось открыть настройки. Панель настроек не найдена.";

        if (Contains(source, "tasksPanel") || Contains(source, "TasksListManager"))
            return "Не удалось открыть список заданий. Панель заданий не настроена.";

        if (Contains(source, "FinalTestEditor"))
            return "Не удалось открыть или сохранить итоговый тест. Проверьте данные курса.";

        if (Contains(source, "TaskEditor"))
            return "Не удалось сохранить задание. Проверьте заполнение формы.";

        if (Contains(source, "Save failed") || Contains(source, "Load failed") || Contains(source, "Delete failed"))
            return "Не удалось обработать сохранение. Проверьте доступ к файлам игры.";

        if (Contains(source, "Import failed"))
            return "Не удалось импортировать файл. Проверьте формат JSON и попробуйте снова.";

        if (Contains(source, "Export failed"))
            return "Не удалось экспортировать файл. Проверьте доступ к папке данных.";

        if (Contains(source, "Restore failed"))
            return "Не удалось восстановить резервную копию. Проверьте наличие файлов бэкапа.";

        if (type == LogType.Exception)
            return "Произошла непредвиденная ошибка. Попробуйте повторить действие.";

        return Generic;
    }

    public static string FromValidation(string message)
    {
        return string.IsNullOrWhiteSpace(message) ? Generic : message.Trim();
    }

    private static bool Contains(string source, string value)
    {
        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}