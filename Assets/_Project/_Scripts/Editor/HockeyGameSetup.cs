#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;

namespace HockeyGame.Editor
{
    public class HockeyGameSetup : EditorWindow
    {
        [MenuItem("Hockey Game/Setup Project", false, 1)]
        public static void SetupProject()
        {
            if (EditorUtility.DisplayDialog("Hockey Game Setup",
                "This will create:\n" +
                "- Boot, MainMenu, and GameplaySandbox scenes\n" +
                "- Default ScriptableObject assets\n" +
                "- Materials\n\n" +
                "Continue?", "Yes", "Cancel"))
            {
                CreateScenes();
                CreateAssets();
                UpdateBuildSettings();
                EditorUtility.DisplayDialog("Setup Complete",
                    "Hockey Game project setup complete!\n\n" +
                    "Open the Boot scene and press Play to test.",
                    "OK");
            }
        }

        [MenuItem("Hockey Game/Create Scenes Only", false, 2)]
        public static void CreateScenesOnly()
        {
            CreateScenes();
            UpdateBuildSettings();
        }

        [MenuItem("Hockey Game/Create Assets Only", false, 3)]
        public static void CreateAssetsOnly()
        {
            CreateAssets();
        }

        private static void CreateScenes()
        {
            string scenesPath = "Assets/_Project/_Scenes";

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(scenesPath))
            {
                Directory.CreateDirectory(Application.dataPath + "/_Project/_Scenes");
                AssetDatabase.Refresh();
            }

            CreateBootScene(scenesPath);
            CreateMainMenuScene(scenesPath);
            CreateGameplaySandboxScene(scenesPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[HockeyGameSetup] All scenes created successfully!");
        }

        private static void CreateBootScene(string basePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create Bootstrapper GameObject
            var bootstrapper = new GameObject("Bootstrapper");
            bootstrapper.AddComponent<HockeyGame.Core.Bootstrapper>();

            EditorSceneManager.SaveScene(scene, $"{basePath}/Boot.unity");
            Debug.Log("[HockeyGameSetup] Boot scene created");
        }

        private static void CreateMainMenuScene(string basePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Setup Camera
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
                mainCamera.orthographic = false;
            }

            // Create Canvas
            var canvas = CreateCanvas("MainMenuCanvas");

            // Create Main Panel
            var mainPanel = CreatePanel(canvas.transform, "MainPanel");
            mainPanel.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            mainPanel.GetComponent<RectTransform>().anchorMax = Vector2.one;
            mainPanel.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            // Title Text
            var titleObj = CreateText(mainPanel.transform, "Title", "ARCADE HOCKEY", 72);
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.9f);
            titleRect.sizeDelta = new Vector2(600, 100);
            titleRect.anchoredPosition = Vector2.zero;

            // Start Button
            var startButton = CreateButton(mainPanel.transform, "StartButton", "START GAME", new Vector2(0, 50));

            // Settings Button
            var settingsButton = CreateButton(mainPanel.transform, "SettingsButton", "SETTINGS", new Vector2(0, -30));

            // Quit Button
            var quitButton = CreateButton(mainPanel.transform, "QuitButton", "QUIT", new Vector2(0, -110));

            // Settings Panel (hidden by default)
            var settingsPanel = CreatePanel(canvas.transform, "SettingsPanel");
            settingsPanel.SetActive(false);
            var settingsRect = settingsPanel.GetComponent<RectTransform>();
            settingsRect.anchorMin = new Vector2(0.2f, 0.2f);
            settingsRect.anchorMax = new Vector2(0.8f, 0.8f);
            settingsRect.sizeDelta = Vector2.zero;

            // Back Button in Settings
            var backButton = CreateButton(settingsPanel.transform, "BackButton", "BACK", new Vector2(0, -150));

            // Add MainMenuController
            var menuController = canvas.gameObject.AddComponent<HockeyGame.UI.MainMenuController>();

            // Wire up references via SerializedObject
            var so = new SerializedObject(menuController);
            so.FindProperty("_startButton").objectReferenceValue = startButton.GetComponent<Button>();
            so.FindProperty("_settingsButton").objectReferenceValue = settingsButton.GetComponent<Button>();
            so.FindProperty("_quitButton").objectReferenceValue = quitButton.GetComponent<Button>();
            so.FindProperty("_mainPanel").objectReferenceValue = mainPanel;
            so.FindProperty("_settingsPanel").objectReferenceValue = settingsPanel;
            so.ApplyModifiedProperties();

            // Create EventSystem
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, $"{basePath}/MainMenu.unity");
            Debug.Log("[HockeyGameSetup] MainMenu scene created");
        }

        private static void CreateGameplaySandboxScene(string basePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Setup Main Camera with GameCamera component
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0, 15, -8.66f);
                mainCamera.transform.rotation = Quaternion.Euler(60, 0, 0);
                mainCamera.orthographic = false;
                mainCamera.fieldOfView = 60;
                mainCamera.gameObject.AddComponent<HockeyGame.Core.GameCamera>();
            }

            // Create Rink Parent
            var rinkParent = new GameObject("Rink_Parent");

            // Create Floor (NHL: 60m x 26m)
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Ice";
            floor.transform.SetParent(rinkParent.transform);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2.6f, 1f, 6f);
            var floorRenderer = floor.GetComponent<Renderer>();
            floorRenderer.material.color = new Color(0.9f, 0.95f, 1f);

            // Create Walls
            CreateWall(rinkParent.transform, "NorthWall", new Vector3(0, 0.5f, 30), new Vector3(26, 1, 0.5f));
            CreateWall(rinkParent.transform, "SouthWall", new Vector3(0, 0.5f, -30), new Vector3(26, 1, 0.5f));
            CreateWall(rinkParent.transform, "EastWall", new Vector3(13, 0.5f, 0), new Vector3(0.5f, 1, 60));
            CreateWall(rinkParent.transform, "WestWall", new Vector3(-13, 0.5f, 0), new Vector3(0.5f, 1, 60));

            // Create Goal Zones
            CreateGoalZone(rinkParent.transform, "GoalZone_TeamA", new Vector3(0, 0.5f, -29));
            CreateGoalZone(rinkParent.transform, "GoalZone_TeamB", new Vector3(0, 0.5f, 29));

            // Create Center Line
            var centerLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerLine.name = "CenterLine";
            centerLine.transform.SetParent(rinkParent.transform);
            centerLine.transform.position = new Vector3(0, 0.01f, 0);
            centerLine.transform.localScale = new Vector3(26, 0.01f, 0.1f);
            centerLine.GetComponent<Renderer>().material.color = Color.red;
            Object.DestroyImmediate(centerLine.GetComponent<Collider>());

            // Create Players Parent
            var playersParent = new GameObject("Players_Parent");
            var teamAParent = new GameObject("TeamA");
            var teamBParent = new GameObject("TeamB");
            teamAParent.transform.SetParent(playersParent.transform);
            teamBParent.transform.SetParent(playersParent.transform);

            // Create GameInitializer
            var gameManager = new GameObject("GameManager");
            var initializer = gameManager.AddComponent<HockeyGame.Core.GameInitializer>();

            // Wire up references
            var so = new SerializedObject(initializer);
            so.FindProperty("_playersParent").objectReferenceValue = playersParent.transform;
            so.FindProperty("_teamAParent").objectReferenceValue = teamAParent.transform;
            so.FindProperty("_teamBParent").objectReferenceValue = teamBParent.transform;
            if (mainCamera != null)
            {
                so.FindProperty("_gameCamera").objectReferenceValue = mainCamera.GetComponent<HockeyGame.Core.GameCamera>();
            }
            so.ApplyModifiedProperties();

            // Create UI Canvas for HUD
            var hudCanvas = CreateCanvas("HUDCanvas");

            // Create Mobile Controls Panel
            var mobilePanel = CreatePanel(hudCanvas.transform, "MobileControlsPanel");
            var mobileRect = mobilePanel.GetComponent<RectTransform>();
            mobileRect.anchorMin = Vector2.zero;
            mobileRect.anchorMax = Vector2.one;
            mobileRect.sizeDelta = Vector2.zero;

            // Create Virtual Joystick
            var joystickBg = new GameObject("JoystickBackground");
            joystickBg.transform.SetParent(mobilePanel.transform);
            var joystickBgImage = joystickBg.AddComponent<Image>();
            joystickBgImage.color = new Color(1, 1, 1, 0.3f);
            var joystickBgRect = joystickBg.GetComponent<RectTransform>();
            joystickBgRect.anchorMin = new Vector2(0, 0);
            joystickBgRect.anchorMax = new Vector2(0, 0);
            joystickBgRect.pivot = new Vector2(0, 0);
            joystickBgRect.sizeDelta = new Vector2(200, 200);
            joystickBgRect.anchoredPosition = new Vector2(50, 50);

            var joystickHandle = new GameObject("JoystickHandle");
            joystickHandle.transform.SetParent(joystickBg.transform);
            var handleImage = joystickHandle.AddComponent<Image>();
            handleImage.color = new Color(1, 1, 1, 0.6f);
            var handleRect = joystickHandle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(80, 80);
            handleRect.anchoredPosition = Vector2.zero;

            var virtualJoystick = joystickBg.AddComponent<HockeyGame.Input.VirtualJoystick>();
            var vjSo = new SerializedObject(virtualJoystick);
            vjSo.FindProperty("_background").objectReferenceValue = joystickBgRect;
            vjSo.FindProperty("_handle").objectReferenceValue = handleRect;
            vjSo.ApplyModifiedProperties();

            // Create Shoot Button
            var shootButton = new GameObject("ShootButton");
            shootButton.transform.SetParent(mobilePanel.transform);
            var shootImage = shootButton.AddComponent<Image>();
            shootImage.color = new Color(1, 0.3f, 0.3f, 0.5f);
            var shootRect = shootButton.GetComponent<RectTransform>();
            shootRect.anchorMin = new Vector2(1, 0);
            shootRect.anchorMax = new Vector2(1, 0);
            shootRect.pivot = new Vector2(1, 0);
            shootRect.sizeDelta = new Vector2(150, 150);
            shootRect.anchoredPosition = new Vector2(-50, 50);
            shootButton.AddComponent<HockeyGame.Input.VirtualButton>();

            // Create Pass Button
            var passButton = new GameObject("PassButton");
            passButton.transform.SetParent(mobilePanel.transform);
            var passImage = passButton.AddComponent<Image>();
            passImage.color = new Color(0.3f, 0.3f, 1, 0.5f);
            var passRect = passButton.GetComponent<RectTransform>();
            passRect.anchorMin = new Vector2(1, 0);
            passRect.anchorMax = new Vector2(1, 0);
            passRect.pivot = new Vector2(1, 0);
            passRect.sizeDelta = new Vector2(100, 100);
            passRect.anchoredPosition = new Vector2(-220, 50);
            passButton.AddComponent<HockeyGame.Input.VirtualButton>();

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, $"{basePath}/GameplaySandbox.unity");
            Debug.Log("[HockeyGameSetup] GameplaySandbox scene created");
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObj = new GameObject(name);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent);
            var image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.5f);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            return panel;
        }

        private static GameObject CreateText(Transform parent, string name, string textContent, int fontSize)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            // Use legacy Text component for compatibility
            var text = textObj.AddComponent<Text>();
            text.text = textContent;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            }

            return textObj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);

            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);

            var button = buttonObj.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.25f);
            button.colors = colors;

            var rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300, 60);
            rect.anchoredPosition = position;

            // Button Text using legacy Text
            var textObj = CreateText(buttonObj.transform, "Text", text, 28);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            return buttonObj;
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            // Wall tag assignment skipped - using name matching instead
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.4f);
        }

        private static void CreateGoalZone(Transform parent, string name, Vector3 position)
        {
            var goal = new GameObject(name);
            goal.transform.SetParent(parent);
            goal.transform.position = position;

            var collider = goal.AddComponent<BoxCollider>();
            collider.size = new Vector3(3, 2, 1);
            collider.isTrigger = true;

            // Visual indicator
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "GoalVisual";
            visual.transform.SetParent(goal.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(3, 2, 0.1f);
            visual.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.3f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private static void CreateAssets()
        {
            string dataPath = "Assets/_Project/_Data";
            string materialsPath = "Assets/_Project/_Art/Materials";

            EnsureDirectory(dataPath);
            EnsureDirectory(materialsPath);

            CreateInputReaderAsset(dataPath);
            CreatePlayerConfigAsset(dataPath, "DefaultPlayerConfig", 8f, 25f, 1f);
            CreateTeamConfigAsset(dataPath, "TeamAConfig", "Red Team", "RED", Color.red);
            CreateTeamConfigAsset(dataPath, "TeamBConfig", "Blue Team", "BLU", Color.blue);
            CreateGameplaySettingsAsset(dataPath);
            CreatePixelArtMaterial(materialsPath, "PixelArt_Red", Color.red);
            CreatePixelArtMaterial(materialsPath, "PixelArt_Blue", Color.blue);
            CreatePixelArtMaterial(materialsPath, "PixelArt_White", Color.white);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[HockeyGameSetup] All assets created successfully!");
        }

        private static void EnsureDirectory(string path)
        {
            string fullPath = Application.dataPath.Replace("Assets", "") + path;
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private static void CreateInputReaderAsset(string path)
        {
            var asset = ScriptableObject.CreateInstance<HockeyGame.Input.InputReader>();
            AssetDatabase.CreateAsset(asset, $"{path}/InputReader.asset");
            Debug.Log("[HockeyGameSetup] Created InputReader asset");
        }

        private static void CreatePlayerConfigAsset(string path, string name, float speed, float shotPower, float mass)
        {
            var asset = ScriptableObject.CreateInstance<HockeyGame.Gameplay.Player.PlayerConfig>();
            asset.moveSpeed = speed;
            asset.shotPower = shotPower;
            asset.mass = mass;
            asset.playerName = name;
            AssetDatabase.CreateAsset(asset, $"{path}/{name}.asset");
            Debug.Log($"[HockeyGameSetup] Created {name} asset");
        }

        private static void CreateTeamConfigAsset(string path, string name, string teamName, string abbrev, Color color)
        {
            var asset = ScriptableObject.CreateInstance<HockeyGame.Data.TeamConfig>();
            asset.teamName = teamName;
            asset.abbreviation = abbrev;
            asset.primaryColor = color;
            AssetDatabase.CreateAsset(asset, $"{path}/{name}.asset");
            Debug.Log($"[HockeyGameSetup] Created {name} asset");
        }

        private static void CreateGameplaySettingsAsset(string path)
        {
            var asset = ScriptableObject.CreateInstance<HockeyGame.Data.GameplaySettings>();
            asset.periodCount = 3;
            asset.periodLengthMinutes = 5f;
            asset.rinkLength = 60f;
            asset.rinkWidth = 26f;
            AssetDatabase.CreateAsset(asset, $"{path}/GameplaySettings.asset");
            Debug.Log("[HockeyGameSetup] Created GameplaySettings asset");
        }

        private static void CreatePixelArtMaterial(string path, string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            var material = new Material(shader);
            material.color = color;
            AssetDatabase.CreateAsset(material, $"{path}/{name}.mat");
            Debug.Log($"[HockeyGameSetup] Created {name} material");
        }

        private static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/_Project/_Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/_Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/_Scenes/GameplaySandbox.unity", true)
            };

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[HockeyGameSetup] Build settings updated with scene order: Boot (0), MainMenu (1), GameplaySandbox (2)");
        }
    }
}
#endif
