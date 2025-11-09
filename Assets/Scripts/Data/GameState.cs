using UnityEngine;

// Глобальное хранилище текущих флагов состояния игры (например, режим админа, текущий профиль и т.п.)
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    public bool IsAdminMode = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
