using UnityEngine;

/// <summary>Победа, поражение, прогресс, жизни и интерфейс варианта 01.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerController player;

    public const int TargetPackages = 5;
    public const int InitialLives = 3;

    private EcologyItem[] items;
    private int packages;
    private int lives;
    private bool isPlaying;
    private bool didWin;
    private float lastDamageTime = -10f;
    private float hintExpiresAt;
    private string hint = "";
    private Vector2 mobileDirection;

    public int Packages => packages;
    public int Lives => lives;
    public bool IsPlaying => isPlaying;
    public Vector2 MobileDirection => mobileDirection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        items = FindObjectsByType<EcologyItem>(FindObjectsSortMode.None);
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (player == null || items.Length == 0)
        {
            Debug.LogError("ПМ12: сцена не настроена. Создайте её через меню ПМ12.");
            enabled = false;
            return;
        }
        RestartGame();
    }

    /// <summary>Ключевое индивидуальное правило: тип определяет награду или штраф.</summary>
    public void HandleItem(EcologyItem item)
    {
        if (!isPlaying || item == null) return;

        if (item.kind == EcologyItem.ItemKind.RecyclablePackage)
        {
            item.AcceptPackage();
            packages += 1;
            ShowHint("Упаковка сдана в переработку: +1");
        }
        else if (item.kind == EcologyItem.ItemKind.HazardousBattery)
        {
            // Касание батарейки не увеличивает счётчик упаковок.
            ApplyDamage();
        }
    }

    private void ApplyDamage()
    {
        if (!isPlaying || Time.time - lastDamageTime < 0.8f) return;
        lastDamageTime = Time.time;
        lives = Mathf.Max(0, lives - 1);
        ShowHint("Опасная батарейка: минус одна жизнь!");
        if (lives == 0) FinishGame(false);
    }

    public void TryExit()
    {
        if (!isPlaying) return;
        if (packages == TargetPackages)
            FinishGame(true);
        else
            ShowHint("Рано выходить! Осталось собрать: " + (TargetPackages - packages));
    }

    private void FinishGame(bool won)
    {
        isPlaying = false;
        didWin = won;
        mobileDirection = Vector2.zero;
        if (player != null) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    public void RestartGame()
    {
        packages = 0;
        lives = InitialLives;
        isPlaying = true;
        didWin = false;
        hint = "";
        hintExpiresAt = 0f;
        lastDamageTime = -10f;
        mobileDirection = Vector2.zero;
        if (items != null)
            foreach (EcologyItem item in items)
                if (item != null) item.ResetItem();
        if (player != null) player.ResetPlayer();
    }

    private void ShowHint(string message)
    {
        hint = message;
        hintExpiresAt = Time.time + 2.4f;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnGUI()
    {
        float scale = Mathf.Max(0.85f, Screen.width / 1280f);
        GUIStyle title = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(22 * scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        GUIStyle body = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(18 * scale),
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        GUIStyle center = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.RoundToInt(25 * scale), fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        float w = Screen.width;
        float h = Screen.height;
        float barHeight = 75f * scale;
        Color old = GUI.color;
        GUI.color = new Color(0.07f, 0.15f, 0.18f, 0.94f);
        GUI.Box(new Rect(0, 0, w, barHeight), GUIContent.none);
        GUI.color = old;
        GUI.Label(new Rect(16f * scale, 6f * scale, w * 0.5f, 28f * scale),
            "ЭКОЛОГИЧЕСКИЙ ПАТРУЛЬ · Вариант 01", title);
        GUI.Label(new Rect(16f * scale, 37f * scale, w * 0.67f, 29f * scale),
            "Упаковки: " + packages + "/" + TargetPackages + "    Жизни: " + lives + "/" + InitialLives, body);
        if (GUI.Button(new Rect(w - 154f * scale, 12f * scale, 138f * scale, 49f * scale),
                "Заново", buttonStyle)) RestartGame();

        if (isPlaying && Time.time < hintExpiresAt)
        {
            GUI.color = new Color(0.08f, 0.16f, 0.17f, 0.90f);
            GUI.Box(new Rect(w * 0.27f, barHeight + 7, w * 0.46f, 40f * scale), GUIContent.none);
            GUI.color = old;
            GUI.Label(new Rect(w * 0.27f, barHeight + 7, w * 0.46f, 40f * scale), hint, center);
        }

        // Большие экранные кнопки управляют роботом при удержании мыши или пальцем.
        if (isPlaying)
        {
            float s = Mathf.Clamp(h * 0.112f, 45f, 80f);
            float bottom = h - 2.1f * s - 16f;
            float left = 17f;
            bool up = GUI.RepeatButton(new Rect(left + s, bottom, s, s), "▲", buttonStyle);
            bool down = GUI.RepeatButton(new Rect(left + s, bottom + s, s, s), "▼", buttonStyle);
            bool l = GUI.RepeatButton(new Rect(left, bottom + s, s, s), "◀", buttonStyle);
            bool r = GUI.RepeatButton(new Rect(left + 2 * s, bottom + s, s, s), "▶", buttonStyle);
            // Зафиксировать ввод на фазе Repaint, чтобы физика использовала актуальное удержание.
            if (Event.current.type == EventType.Repaint)
                mobileDirection = new Vector2((r ? 1 : 0) - (l ? 1 : 0), (up ? 1 : 0) - (down ? 1 : 0));
            GUI.Label(new Rect(w * 0.50f, h - 64f * scale, w * 0.47f, 42f * scale),
                "WASD / стрелки · зелёные коробки ✓ · батарейки !", body);
        }
        else
        {
            float panelW = Mathf.Min(w * 0.68f, 630f * scale);
            float panelH = 230f * scale;
            Rect panel = new Rect((w - panelW) / 2, (h - panelH) / 2, panelW, panelH);
            GUI.color = new Color(0.05f, 0.17f, 0.17f, 0.95f);
            GUI.Box(panel, GUIContent.none);
            GUI.color = old;
            GUIStyle result = new GUIStyle(center)
            {
                fontSize = Mathf.RoundToInt(34 * scale), fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(panel.x + 10, panel.y + 20f * scale,
                panelW - 20, 55f * scale), didWin ? "ПОБЕДА!" : "ПОРАЖЕНИЕ", result);
            GUI.Label(new Rect(panel.x + 12, panel.y + 82f * scale,
                panelW - 24, 45f * scale),
                didWin ? "Все 5 упаковок собраны. Выход открыт." : "Жизни закончились. Попробуйте ещё раз.", center);
            if (GUI.Button(new Rect(panel.x + panelW * 0.26f, panel.y + 154f * scale,
                panelW * 0.48f, 55f * scale), "Повторить", buttonStyle)) RestartGame();
        }
    }
}
