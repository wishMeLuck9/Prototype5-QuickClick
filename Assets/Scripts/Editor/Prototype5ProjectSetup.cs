using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Prototype5ProjectSetup
{
    private const string ScenePath = "Assets/Scenes/Prototype5.unity";

    public static void CreateProject()
    {
        EnsureFolders();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Prototype5";

        CreateMaterials();
        CreateCamera();
        CreateSensor();
        var explosion = CreateExplosionPrefab();
        var prefabs = new List<GameObject>
        {
            CreateTargetPrefab("GoodApple", PrimitiveType.Sphere, "AppleRed", 10, false, explosion),
            CreateTargetPrefab("GoodCheese", PrimitiveType.Cube, "CheeseYellow", 15, false, explosion),
            CreateTargetPrefab("GoodMelon", PrimitiveType.Cylinder, "MelonGreen", 5, false, explosion),
            CreateTargetPrefab("BadSkull", PrimitiveType.Sphere, "SkullDark", 0, true, explosion)
        };

        var gameManager = new GameObject("Game Manager").AddComponent<GameManager>();
        var canvas = CreateCanvas();
        var scoreText = CreateText(canvas.transform, "Score Text", "Score: 0", new Vector2(24f, -24f), TextAnchor.UpperLeft, 28, new Vector2(300f, 50f));
        var gameOverText = CreateText(canvas.transform, "Game Over Text", "GAME OVER", new Vector2(0f, 80f), TextAnchor.MiddleCenter, 46, new Vector2(520f, 80f));
        gameOverText.color = new Color(0.85f, 0.1f, 0.08f);

        var titleScreen = new GameObject("Title Screen", typeof(RectTransform));
        titleScreen.transform.SetParent(canvas.transform, false);
        Stretch(titleScreen.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        var title = CreateText(titleScreen.transform, "Title", "Quick Click", new Vector2(0f, 150f), TextAnchor.MiddleCenter, 48, new Vector2(520f, 80f));
        title.color = new Color(0.1f, 0.16f, 0.22f);
        CreateDifficultyButton(titleScreen.transform, "Easy Button", "Easy", new Vector2(-170f, 20f), 1);
        CreateDifficultyButton(titleScreen.transform, "Medium Button", "Medium", new Vector2(0f, 20f), 2);
        CreateDifficultyButton(titleScreen.transform, "Hard Button", "Hard", new Vector2(170f, 20f), 3);

        var restartButton = CreateButton(canvas.transform, "Restart Button", "Restart", new Vector2(0f, -90f), new Vector2(180f, 54f));
        UnityEventTools.AddPersistentListener(restartButton.onClick, gameManager.RestartGame);

        var serialized = new SerializedObject(gameManager);
        serialized.FindProperty("scoreText").objectReferenceValue = scoreText;
        serialized.FindProperty("gameOverText").objectReferenceValue = gameOverText;
        serialized.FindProperty("titleScreen").objectReferenceValue = titleScreen;
        serialized.FindProperty("restartButton").objectReferenceValue = restartButton;
        var list = serialized.FindProperty("targets");
        list.arraySize = prefabs.Count;
        for (int i = 0; i < prefabs.Count; i++)
        {
            list.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();

        CreateEventSystem();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.93f, 0.96f, 0.98f);

        var light = new GameObject("Directional Light");
        light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        light.AddComponent<Light>().type = LightType.Directional;
    }

    private static void CreateSensor()
    {
        var sensor = new GameObject("Sensor");
        sensor.transform.position = new Vector3(0f, -6.5f, 0f);
        var collider = sensor.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(14f, 0.5f, 2f);
    }

    private static GameObject CreateTargetPrefab(string name, PrimitiveType primitive, string materialName, int points, bool bad, ParticleSystem explosion)
    {
        var root = GameObject.CreatePrimitive(primitive);
        root.name = name;
        root.transform.localScale = Vector3.one * (bad ? 0.85f : 0.75f);
        root.GetComponent<Renderer>().sharedMaterial = LoadMaterial(materialName);
        var rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.useGravity = true;
        rigidbody.mass = 1f;

        var target = root.AddComponent<Target>();
        var serialized = new SerializedObject(target);
        serialized.FindProperty("pointValue").intValue = points;
        serialized.FindProperty("badTarget").boolValue = bad;
        serialized.FindProperty("explosionParticle").objectReferenceValue = explosion;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        string path = "Assets/Prefabs/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static ParticleSystem CreateExplosionPrefab()
    {
        var root = new GameObject("ClickExplosion");
        var particles = root.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.35f;
        main.startSpeed = 3f;
        main.startSize = 0.18f;
        main.maxParticles = 40;
        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });
        root.AddComponent<DestroyAfterDelay>();

        string path = "Assets/Prefabs/ClickExplosion.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<ParticleSystem>();
    }

    private static Canvas CreateCanvas()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        return canvas;
    }

    private static Text CreateText(Transform parent, string name, string value, Vector2 position, TextAnchor anchor, int size, Vector2 dimensions)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        var rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = dimensions;
        rect.anchoredPosition = position;
        SetAnchor(rect, anchor);

        var text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = BuiltInFont();
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.black;
        return text;
    }

    private static Button CreateDifficultyButton(Transform parent, string name, string label, Vector2 position, int difficulty)
    {
        var button = CreateButton(parent, name, label, position, new Vector2(150f, 52f));
        var difficultyButton = button.gameObject.AddComponent<DifficultyButton>();
        var serialized = new SerializedObject(difficultyButton);
        serialized.FindProperty("difficulty").intValue = difficulty;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return button;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        SetAnchor(rect, TextAnchor.MiddleCenter);
        buttonObject.GetComponent<Image>().color = new Color(0.1f, 0.36f, 0.62f);

        var text = CreateText(buttonObject.transform, "Text", label, Vector2.zero, TextAnchor.MiddleCenter, 23, size);
        text.color = Color.white;
        Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
        return buttonObject.GetComponent<Button>();
    }

    private static void CreateMaterials()
    {
        CreateMaterial("AppleRed", new Color(0.9f, 0.1f, 0.08f));
        CreateMaterial("CheeseYellow", new Color(1f, 0.75f, 0.18f));
        CreateMaterial("MelonGreen", new Color(0.1f, 0.62f, 0.28f));
        CreateMaterial("SkullDark", new Color(0.08f, 0.08f, 0.1f));
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            return existing;
        }

        var material = new Material(Shader.Find("Standard"));
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Material LoadMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/" + name + ".mat");
    }

    private static void CreateEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private static void EnsureFolders()
    {
        CreateFolder("Assets", "Scenes");
        CreateFolder("Assets", "Prefabs");
        CreateFolder("Assets", "Materials");
    }

    private static void CreateFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Font BuiltInFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void SetAnchor(RectTransform rect, TextAnchor anchor)
    {
        Vector2 anchorPoint = anchor switch
        {
            TextAnchor.UpperLeft => new Vector2(0f, 1f),
            TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
            _ => new Vector2(0.5f, 0.5f)
        };

        rect.anchorMin = anchorPoint;
        rect.anchorMax = anchorPoint;
        rect.pivot = anchorPoint;
    }
}
