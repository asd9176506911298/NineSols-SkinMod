using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using InControl.NativeDeviceProfiles;
using NineSolsAPI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkinMod {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkinMod : BaseUnityPlugin {
        public static SkinMod Instance { get; private set; }

        private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut = null!;
        public ConfigEntry<bool> enableCool = null!;
        public ConfigEntry<float> time = null!;
        public bool xx = false;
        private Harmony harmony;

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);
            Log.Init(Logger);

            Instance = this;

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

            enableCool = Config.Bind("General.Something", "enable",false);
            time = Config.Bind("General.Something", "time", 1.0f);

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
            xx = !xx;
            Traverse.Create(SaveManager.Instance.allStatData.GetStat("RollCoolDown 閃避CD").Stat).Field("_value").SetValue(time.Value);

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
