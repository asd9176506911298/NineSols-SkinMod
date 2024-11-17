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
            ToastManager.Toast("test");


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
            ToastManager.Toast(scene.name);
        }



        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            harmony?.UnpatchSelf();
        }
    }
}
