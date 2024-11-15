using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkinMod {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkinMod : BaseUnityPlugin {
        public static SkinMod Instance { get; private set; }

        private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut = null!;

        private Harmony harmony;

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);
            Log.Init(Logger);

            Instance = this;

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

            somethingKeyboardShortcut = Config.Bind("General.Something", "Shortcut",
            new KeyboardShortcut(KeyCode.X, KeyCode.LeftControl), "Shortcut to execute");

            KeybindManager.Add(this, test, () => somethingKeyboardShortcut.Value);
            



            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        void Update() {
            
        }


        void Start() {

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void test() {
            ToastManager.Toast("123");
            if (GameCore.Instance != null) {
                foreach (var x in GameCore.Instance.allScenes) {
                    //ToastManager.Toast(x);
                    SceneManager.LoadScene(x);
                }
            }
            //SceneManager.LoadScene("A1_S2_ConnectionToElevator_Final");

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
