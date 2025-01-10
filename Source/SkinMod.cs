using BepInEx;
using BepInEx.Configuration;
using Com.LuisPedroFonseca.ProCamera2D;
using HarmonyLib;
using NineSolsAPI;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkinMod {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkinMod : BaseUnityPlugin {
        public static SkinMod Instance { get; private set; }

        private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut = null!;
        private ConfigEntry<KeyboardShortcut> CaptrueMapEnemyKey = null!;
        private ConfigEntry<KeyboardShortcut> CaptureCurrentPosition = null!;
        private ConfigEntry<KeyboardShortcut> DisableMapSkinKey = null!;
        private ConfigEntry<KeyboardShortcut> ShowEnemyKey = null!;
        private ConfigEntry<float> cameraZDistance = null!;
        private ConfigEntry<Color> CameraBackGroundColor = null!;

        private Harmony harmony;

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);
            Log.Init(Logger);

            Instance = this;

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);
            
            somethingKeyboardShortcut = Config.Bind("General.Some1thing", "Shortcut",
            new KeyboardShortcut(KeyCode.X, KeyCode.LeftControl), "Shortcut to execute");

            CaptrueMapEnemyKey = Config.Bind("General.So1mething", "CaptrueMapEnemyKey",
           new KeyboardShortcut(KeyCode.R, KeyCode.LeftControl), "CaptrueMapEnemyKey");

            CaptureCurrentPosition = Config.Bind("General.Someth1ing", "CaptureCurrentPosition",
          new KeyboardShortcut(KeyCode.D, KeyCode.LeftControl), "CaptureCurrentPosition");

            DisableMapSkinKey = Config.Bind("General.Somet1hing", "1",
          new KeyboardShortcut(KeyCode.W, KeyCode.LeftControl), "DisableMapSkinKey");

            ShowEnemyKey = Config.Bind("General.So1met1hing", "1",
          new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl), "ShowEnemyKey");

            cameraZDistance = Config.Bind("", "cameraZDistance", 150.0f,"");

            CameraBackGroundColor = Config.Bind("", "CameraBackGroundColor", new Color(0f,0f,0f,0f),"");

            //KeybindManager.Add(this, test, () => somethingKeyboardShortcut.Value);
            KeybindManager.Add(this, CaptrueMapEnemy, CaptrueMapEnemyKey.Value);
            KeybindManager.Add(this, Capture, () => CaptureCurrentPosition.Value);
            KeybindManager.Add(this, Skin, () => DisableMapSkinKey.Value);
            KeybindManager.Add(this, Show, () => ShowEnemyKey.Value);




            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        void Show() {
            foreach (var x in Resources.FindObjectsOfTypeAll<MonsterBase>()) {
                ToastManager.Toast(x.name);
                if (x != null) {
                    if (x.gameObject != null)
                        x.gameObject.SetActive(true);

                    if (x.monsterCore != null)
                        if (x.monsterCore.gameObject != null)
                            x.monsterCore.gameObject.SetActive(true);
                    x.transform.SetParent(null);

                    GetAllChildren(x.animator.transform);
                    x.animator.enabled = !x.animator.enabled;
                }
            }

            if (GameObject.Find("SceneCamera/AmplifyLightingSystem/FxCamera") != null)
                GameObject.Find("SceneCamera/AmplifyLightingSystem/FxCamera").SetActive(false);

            if (GameObject.Find("UE_Freecam") != null) {
                GameObject.Find("UE_Freecam").GetComponent<Camera>().cullingMask = (1 << 16) + (1 << 17);
                GameObject.Find("UE_Freecam").GetComponent<Camera>().farClipPlane = 20000f;
                GameObject.Find("UE_Freecam").GetComponent<Camera>().transform.position = new Vector3(Player.i.Center.x, Player.i.Center.y, -400f); // Adjust position as needed
                GameObject.Find("UE_Freecam").GetComponent<Camera>().transform.LookAt(Player.i.Center);         // Make the camera look at the player
            }
        }

        void GetAllChildren(Transform parent) {
            foreach (Transform child in parent) {
                if (child.name.Contains("Mask") || child.name.Contains("shadow") || child.name.Contains("Shadow") || child.name.Contains("Fx") || child.name.Contains("Light") || child.name.Contains("light") || child.name.Contains("Glow") || child.name.Contains("LM_"))
                    child.gameObject.SetActive(false);

                if (child.name == "SpriteHolder")
                    child.gameObject.SetActive(true);
                // Print the child's name
                //ToastManager.Toast(child);
                child.gameObject.layer = 16;
                // Recursive call to get this child's children
                GetAllChildren(child);
            }
        }

        void CaptrueMapEnemy() {
            try {
                // Log start of reload process
                Log.Info("Starting reload process...");

                // Validate and configure UE_Freecam
                GameObject freeCam = GameObject.Find("UE_Freecam");
                if (freeCam == null) {
                    Log.Error("UE_Freecam not found in the scene.");
                } else {
                    Camera freeCamCamera = freeCam.GetComponent<Camera>();
                    if (freeCamCamera == null) {
                        Log.Error("UE_Freecam does not have a Camera component.");
                    } else {
                        freeCamCamera.cullingMask = (1 << 16) + (1 << 17);
                        freeCamCamera.farClipPlane = 20000f;

                        if (Player.i == null) {
                            Log.Error("Player.i is null.");
                        } else if (Player.i.Center == null) {
                            Log.Error("Player.i.Center is null.");
                        } else {
                            freeCamCamera.transform.position = new Vector3(Player.i.Center.x, Player.i.Center.y, -300f);
                            freeCamCamera.transform.LookAt(Player.i.Center);
                        }
                    }
                }

                // Validate MonsterManager
                if (MonsterManager.Instance == null) {
                    Log.Error("MonsterManager.Instance is null.");
                    return;
                }

                if (MonsterManager.Instance.monsterDict == null) {
                    Log.Error("MonsterManager.Instance.monsterDict is null.");
                    return;
                }

                // Activate monsters and configure them
                foreach (var x in Resources.FindObjectsOfTypeAll<MonsterBase>()) {
                    if (x == null) {
                        Log.Error("A monster in monsterDict is null.");
                        continue;
                    }

                    if (x.gameObject != null) {
                        x.gameObject.SetActive(true);
                        x.transform.SetParent(null);
                    }
                    if (x.monsterCore != null)
                        if (x.monsterCore.gameObject != null)
                            x.monsterCore.gameObject.SetActive(true);

                    if (x.animator == null) {
                        Log.Error($"Animator is null for monster: {x.name ?? "Unknown Monster"}");
                    } else {
                        GetAllChildren(x.animator.transform);
                        x.animator.enabled = false;
                    }
                }

                // Capture each monster
                foreach (var x in Resources.FindObjectsOfTypeAll<MonsterBase>()) {
                    if (x == null) {
                        Log.Error("A monster in monsterDict is null.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(x.name)) {
                        Log.Error("Monster name is null or empty.");
                        continue;
                    }

                    Log.Info($"Capturing monster: {x.name}");
                    Capture(x);
                }

                // Log completion of reload process
                Log.Info("Reload process completed successfully.");
            } catch (NullReferenceException ex) {
                Log.Error($"NullReferenceException in reload: {ex.Message}");
                Log.Error("Check if all required components and objects are initialized properly.");
            } catch (Exception ex) {
                Log.Error($"An unexpected error occurred in reload: {ex.Message}");
            }
        }

        void Skin() {
            for (int i = 0; i < GameCore.Instance.gameLevel.transform.childCount; i++) {
                var child = GameCore.Instance.gameLevel.transform.GetChild(i).gameObject;
                if (child.name.Contains("_Skin")) {
                    child.SetActive(!child.activeSelf);
                }
            }

            GameObject.Find("SceneCamera/AmplifyLightingSystem/FxCamera").SetActive(false);
            Player.i.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        void Capture(MonsterBase m) {
            try {
                // Check if the MonsterBase object is null
                if (m == null) {
                    ToastManager.Toast("MonsterBase is null.");
                    return; // Early exit if 'm' is null
                }

                // Check if the Center property is null (if applicable, depending on your class structure)
                if (m.Center == null) {
                    ToastManager.Toast("MonsterBase Center is null.");
                    return; // Early exit if 'm.Center' is null
                }

                // Check if the name is null or empty
                if (string.IsNullOrEmpty(m.name)) {
                    ToastManager.Toast("MonsterBase name is null or empty.");
                    return; // Early exit if 'm.name' is null or empty
                }

                // Create a temporary camera
                GameObject tempCameraObj = new GameObject("TempCamera");
                Camera tempCamera = tempCameraObj.AddComponent<Camera>();
                tempCamera.transform.position = new Vector3(m.Center.x, m.Center.y, -cameraZDistance.Value); // Adjust position as needed
                tempCamera.transform.LookAt(m.Center); // Make the camera look at the monster's center

                // Set up the temporary camera for rendering
                rt = new RenderTexture(width, height, 24);
                screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tempCamera.targetTexture = rt;
                tempCamera.clearFlags = CameraClearFlags.SolidColor;
                //tempCamera.backgroundColor = new Color(0, 1, 0, 1); // Fully transparent
                tempCamera.backgroundColor = CameraBackGroundColor.Value;

                // Set the culling mask for the camera
                //tempCamera.cullingMask = 168279809;
                tempCamera.cullingMask = (1 << 16);

                // Render the object to the RenderTexture
                RenderTexture.active = rt;
                tempCamera.Render();

                // Read the pixels from the RenderTexture to the Texture2D
                screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenshot.Apply();

                string folderPath = Path.Combine(Paths.BepInExRootPath, "EnemyPic");

                // Ensure the directory exists
                if (!Directory.Exists(folderPath)) {
                    Directory.CreateDirectory(folderPath);
                }

                // Construct the full file path
                string path = Path.Combine(folderPath, $"{m.name}-{DateTime.Now:ssfff}.png");

                // Save the file
                File.WriteAllBytes(path, screenshot.EncodeToPNG());


                // Clean up
                tempCamera.targetTexture = null;
                RenderTexture.active = null;
                Destroy(rt);
                Destroy(tempCameraObj); // Destroy the temporary camera

            } catch (Exception e) {
                ToastManager.Toast($"Error in Capture: {e.Message}");
            }
        }


        void Capture() {
            // Create a temporary camera
            GameObject tempCameraObj = new GameObject("TempCamera");
            Camera tempCamera = tempCameraObj.AddComponent<Camera>();
            tempCamera.transform.position = new Vector3(Player.i.Center.x, Player.i.Center.y, cameraZDistance.Value); // Adjust position as needed
            tempCamera.transform.LookAt(Player.i.Center);         // Make the camera look at the player

            // Set up the temporary camera for rendering
            rt = new RenderTexture(width, height, 24);
            screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tempCamera.targetTexture = rt;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = CameraBackGroundColor.Value;

            // Disable skin objects and alert meter
            //DisableSkinObjects();

            // Set the culling mask for the camera
            //tempCamera.cullingMask = 168279809;
            tempCamera.cullingMask = (1 << 16);
            ToastManager.Toast(tempCamera.cullingMask);
            // Render the object to the RenderTexture
            RenderTexture.active = rt;
            tempCamera.Render();

            // Read the pixels from the RenderTexture to the Texture2D
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();

            string folderPath = Path.Combine(Paths.BepInExRootPath, "EnemyPic");

            // Ensure the directory exists
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            // Construct the full file path
            string path = Path.Combine(folderPath, $"{DateTime.Now:ssfff}.png");
            File.WriteAllBytes(path, screenshot.EncodeToPNG());

            // Clean up
            tempCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(tempCameraObj); // Destroy the temporary camera
        }

        void Update() {
            
        }


        void Start() {

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void test() {
            //GameObject.Find("P2_R22_Savepoint_GameLevel/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/ButterFly_BossFight_Logic/StealthGameMonster_Boss_ButterFly Variant").transform.SetParent(null);
            RCGLifeCycle.DontDestroyForever(GameObject.Find("P2_R22_Savepoint_GameLevel/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/ButterFly_BossFight_Logic/StealthGameMonster_Boss_ButterFly Variant (RCGLifeCycle)"));
            return;
            //ToastManager.Toast("123");

            CaptureScreenshot();

            //GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/[自然巡邏框架]/[MonsterBehaviorProvider] LevelDesign_CullingAndResetGroup/[MonsterBehaviorProvider] LevelDesign_Init_Scenario (看守的人)/StealthGameMonster_Spearman (1)/MonsterCore/Animator(Proxy)/Animator/StealthGameMonster_Spearman");

            //if (GameCore.Instance != null) {
            //    foreach (var x in GameCore.Instance.allScenes) {
            //        //ToastManager.Toast(x);
            //        SceneManager.LoadScene(x);
            //    }
            //}
            //SceneManager.LoadScene("A1_S2_ConnectionToElevator_Final");

        }

        Camera cameraToUse;
        RenderTexture rt;
        Texture2D screenshot;
        //int width = 1920;
        //int height = 1080;

        int width = 426;
        int height = 240;

        void CaptureScreenshot() {
            //foreach(var x in MonsterManager.Instance.monsterDict.Values){
            //    ToastManager.Toast(x.name);
            //}
            //return;
            // Find the camera dynamically if not set in inspector
            foreach (var camera in Camera.allCameras) {
                if (camera.name == "SceneCamera") {
                    cameraToUse = camera;
                    break;
                }
            }
            // Disable _Skin objects
            DisableSkinObjects();

            // Disable FxCamera and camera boundaries
            GameObject.Find("SceneCamera/AmplifyLightingSystem/FxCamera").SetActive(false);
            GameObject.Find("CameraCore").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;

            // Start the delayed actions
            StartCoroutine(DelayedActions());
        }

        // Disable all skin objects in the scene
        void DisableSkinObjects() {
            for (int i = 0; i < GameCore.Instance.gameLevel.transform.childCount; i++) {
                var child = GameCore.Instance.gameLevel.transform.GetChild(i).gameObject;
                if (child.name.Contains("_Skin")) {
                    child.SetActive(false);
                }
            }
        }

        // Coroutine for delayed actions
        IEnumerator DelayedActions() {
            // Find all MonsterBase objects once
            var monsters = MonsterManager.Instance.monsterDict.Values;

            // Set up common parameters for screenshot (done only once)
            rt = new RenderTexture(width, height, 24);
            screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
            cameraToUse.targetTexture = rt;
            cameraToUse.clearFlags = CameraClearFlags.SolidColor;
            cameraToUse.backgroundColor = new Color(0, 0, 0, 0); // Fully transparent

            // Loop through each monster and capture a screenshot
            foreach (var monster in monsters) {
                monster.LevelReset();
                // Wait for a short delay between capturing screenshots
                yield return new WaitForSeconds(0.2f);

                // Disable skin objects and alert meter
                DisableSkinObjects();
                monster.monsterCore.transform.Find("alert meter")?.gameObject.SetActive(false);
                monster.Show();

                // Move the player to the monster's position (assuming you want to focus on the monster)
                monster.transform.position = Player.i.transform.position;

                // Set the culling mask for the camera
                cameraToUse.cullingMask = 168279809;
                cameraToUse.cullingMask &= ~(1 << 17); // Disable the 17th layer

                // Render the object to the RenderTexture
                RenderTexture.active = rt;
                cameraToUse.Render();

                // Yield to wait for the next frame (to ensure the camera finishes rendering)
                yield return null;

                // Read the pixels from the RenderTexture to the Texture2D
                screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenshot.Apply();

                // Convert the texture to PNG and save it
                string path = Path.Combine("C:\\Users\\a0936\\AppData\\LocalLow\\RedCandleGames\\NineSols", $"{monster.gameObject.name}-{DateTime.Now:MMddHHmmss}.png");
                File.WriteAllBytes(path, screenshot.EncodeToPNG());

                // Clean up
                cameraToUse.targetTexture = null;
                RenderTexture.active = null;
                Destroy(rt);

                // Deactivate the monster after screenshot capture
                monster.Hide();

                Debug.Log($"Screenshot saved to: {path}");
            }
        }

        string GetGameObjectPath(GameObject obj) {
            string path = obj.name;
            Transform current = obj.transform;

            while (current.parent != null) {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            return;
            // Create a file path using the scene name
            string filePath = Path.Combine("E:\\Games\\note", $"{ scene.name}.txt");

            // Create or open the file for writing
            using (StreamWriter writer = new StreamWriter(filePath, false)) {
                // Write scene name to the file

                // Loop through all Note objects in the scene and write their details to the file
                foreach (var x in GameObject.FindObjectsOfType<Note>()) {
                    writer.WriteLine($"Note:{x.note}");
                    writer.WriteLine($"Path:{GetGameObjectPath(x.gameObject)}\n");
                }
            }

            // Optionally log the file saved location
            Log.Info($"Scene data saved to: {filePath}");
        }



        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            harmony?.UnpatchSelf();
        }
    }
}
