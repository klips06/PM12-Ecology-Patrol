#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Создаёт настоящую редактируемую сцену Unity из исходных ресурсов.</summary>
public static class BuildEcologyScene
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("ПМ12/01 — Создать сцену «Экологический патруль»")]
    public static void CreateScene()
    {
        if (File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("Сцена уже существует",
                "Main.unity уже создана. Чтобы не затереть твою работу, повторное создание отключено. " +
                "При необходимости создай отдельный проект из копии исходного архива.", "Хорошо");
            return;
        }
        if (!EditorUtility.DisplayDialog("Создание сцены",
            "Сейчас будет создана новая сцена Main. Сохрани текущие несохранённые изменения перед продолжением.",
            "Создать", "Отмена")) return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EnsureSpriteImports();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateSpriteObject("Фон_экопарка", "Assets/Art/Background.png", Vector2.zero,
            new Vector2(16f, 9f), -10);
        CreateWall("Стена_слева", -7.86f, 0f, .26f, 6.65f);
        CreateWall("Стена_справа", 7.86f, 0f, .26f, 6.65f);
        CreateWall("Стена_сверху", 0f, 3.27f, 15.95f, .26f);
        CreateWall("Стена_снизу", 0f, -3.27f, 15.95f, .26f);
        CreateWall("Перегородка_А", -0.10f, 1.15f, 2.15f, .26f);
        CreateWall("Перегородка_Б", 3.10f, -0.85f, .25f, 1.40f);

        GameObject playerObject = CreateSpriteObject("Player_Робот", "Assets/Art/Robot.png",
            new Vector2(-6.20f, -1.85f), new Vector2(.80f, .80f), 3);
        Rigidbody2D playerBody = playerObject.AddComponent<Rigidbody2D>();
        playerBody.gravityScale = 0f;
        playerBody.freezeRotation = true;
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CircleCollider2D playerCollider = playerObject.AddComponent<CircleCollider2D>();
        playerCollider.radius = .35f;
        PlayerController controller = playerObject.AddComponent<PlayerController>();

        CreateItem("Упаковка_01", EcologyItem.ItemKind.RecyclablePackage, -5.30f, 1.45f);
        CreateItem("Упаковка_02", EcologyItem.ItemKind.RecyclablePackage, -2.50f, 2.10f);
        CreateItem("Упаковка_03", EcologyItem.ItemKind.RecyclablePackage, -.35f, -1.65f);
        CreateItem("Упаковка_04", EcologyItem.ItemKind.RecyclablePackage, 3.90f, 1.82f);
        CreateItem("Упаковка_05", EcologyItem.ItemKind.RecyclablePackage, 5.52f, -1.95f);
        CreateItem("Батарейка_01", EcologyItem.ItemKind.HazardousBattery, -3.76f, -.67f);
        CreateItem("Батарейка_02", EcologyItem.ItemKind.HazardousBattery, 1.70f, 2.00f);
        CreateItem("Батарейка_03", EcologyItem.ItemKind.HazardousBattery, 5.15f, .13f);

        GameObject exit = CreateSpriteObject("Выход_в_парк", "Assets/Art/Exit.png",
            new Vector2(6.70f, 2.05f), new Vector2(.83f, 1.13f), 1);
        BoxCollider2D exitCollider = exit.AddComponent<BoxCollider2D>();
        exitCollider.isTrigger = true;
        exitCollider.size = new Vector2(.95f, .8f);
        exit.AddComponent<ExitZone>();

        GameObject managerObj = new GameObject("GameManager_Правила");
        GameManager manager = managerObj.AddComponent<GameManager>();
        manager.player = controller;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Selection.activeObject = managerObj;
        Debug.Log("ПМ12: сцена Main создана. В Game установи 16:9 и нажми Play. " +
                  "Экранные кнопки и клавиши WASD/стрелки управляют игроком.");
    }

    [MenuItem("ПМ12/02 — Проверить состав сцены")]
    public static void ValidateScene()
    {
        int packages = 0, batteries = 0;
        foreach (EcologyItem item in Object.FindObjectsByType<EcologyItem>(FindObjectsSortMode.None))
        {
            if (item.kind == EcologyItem.ItemKind.RecyclablePackage) packages++;
            else batteries++;
        }
        bool ok = packages == 5 && batteries >= 3
            && Object.FindFirstObjectByType<PlayerController>() != null
            && Object.FindFirstObjectByType<ExitZone>() != null
            && Object.FindFirstObjectByType<GameManager>() != null;
        if (ok) Debug.Log("ПМ12: состав сцены верен: 5 упаковок, " + batteries + " батарейки, игрок, выход, менеджер.");
        else Debug.LogError("ПМ12: состав сцены неполный. Ожидается 5 упаковок, 3 батарейки, игрок, выход и менеджер.");
    }

    private static void EnsureSpriteImports()
    {
        foreach (string path in new[] { "Assets/Art/Background.png", "Assets/Art/Robot.png",
                     "Assets/Art/Package.png", "Assets/Art/Battery.png",
                     "Assets/Art/Exit.png", "Assets/Art/Wall.png" })
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogError("Не найден PNG: " + path); continue; }
            if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 100)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }
    }

    private static void CreateCamera()
    {
        GameObject obj = new GameObject("Main Camera");
        obj.tag = "MainCamera";
        Camera camera = obj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 4.5f;
        camera.backgroundColor = new Color(.06f, .14f, .16f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        obj.transform.position = new Vector3(0f, 0f, -10f);
        obj.AddComponent<AudioListener>();
    }

    private static GameObject CreateSpriteObject(string name, string path, Vector2 position,
        Vector2 desiredSize, int order)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = new Vector3(position.x, position.y, 0f);
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogError("Нет ресурса спрайта: " + path);
        else
        {
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            obj.transform.localScale = new Vector3(
                desiredSize.x / sprite.bounds.size.x,
                desiredSize.y / sprite.bounds.size.y, 1f);
        }
        return obj;
    }

    private static void CreateWall(string name, float x, float y, float width, float height)
    {
        GameObject wall = new GameObject(name);
        wall.transform.position = new Vector3(x, y, 0f);
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(width, height);
        SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Wall.png");
        renderer.sprite = sprite;
        renderer.color = new Color(.54f, .68f, .72f, .95f);
        renderer.sortingOrder = 0;
        if (sprite != null)
            wall.transform.localScale = new Vector3(width / sprite.bounds.size.x,
                height / sprite.bounds.size.y, 1f);
        // Коллайдер должен учитывать масштаб, так что переводим размер в локальные координаты.
        if (sprite != null) collider.size = sprite.bounds.size;
    }

    private static void CreateItem(string name, EcologyItem.ItemKind kind, float x, float y)
    {
        string path = kind == EcologyItem.ItemKind.RecyclablePackage
            ? "Assets/Art/Package.png" : "Assets/Art/Battery.png";
        Vector2 size = kind == EcologyItem.ItemKind.RecyclablePackage
            ? new Vector2(.71f, .71f) : new Vector2(.55f, .84f);
        GameObject obj = CreateSpriteObject(name, path, new Vector2(x, y), size, 2);
        BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer.sprite != null)
            collider.size = renderer.sprite.bounds.size * 0.76f;
        EcologyItem item = obj.AddComponent<EcologyItem>();
        item.kind = kind;
    }
}
#endif
