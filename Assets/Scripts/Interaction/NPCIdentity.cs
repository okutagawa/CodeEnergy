using UnityEngine;
using System;

[DisallowMultipleComponent]
public class NPCIdentity : MonoBehaviour
{
    [Tooltip("Стабильный уникальный идентификатор NPC. Генерируется автоматически в редакторе.")]
    [SerializeField] private string guid = "";

    [Tooltip("Человекочитаемое имя NPC для UI")]
    public string displayName;

    public string Guid => guid;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? gameObject.name : displayName;

    private void Awake()
    {
        // runtime: ничего не генерируем, GUID должен быть уже установлен в префабе/сцене
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // В редакторе: если guid пустой — генерируем и помечаем объект как изменённый
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
            try
            {
                UnityEditor.EditorUtility.SetDirty(this);
                if (!Application.isPlaying)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
            }
            catch { }
        }

        // Поддержим автоматическое заполнение displayName из имени объекта, если пусто
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = gameObject.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
