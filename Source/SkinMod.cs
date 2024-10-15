using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NineSolsAPI;
using NineSolsAPI.Utils;
using UnityEngine;

namespace SkinMod {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkinMod : BaseUnityPlugin {
        private ConfigEntry<bool> enableSkinConfig;
        private ConfigEntry<KeyboardShortcut> enableSkinKeyboardShortcut;
        private ConfigEntry<string> curSkin;
        private ConfigEntry<bool> danceYi;
        private ConfigEntry<bool> jieChuan;

        private Harmony harmony;

        private string objectName;
        private GameObject curSkinObject;

        private bool isEnableSkin = false;

        private AssetBundle tree;
        private GameObject danceYiObject;
        private GameObject jieChuanObject;

        private const string SkinHolderPath = "GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder";

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

            enableSkinKeyboardShortcut = Config.Bind("", "Enable Skin Shortcut", new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift), "");
            curSkin = Config.Bind<string>("", "currSkin", "", "");
            danceYi = Config.Bind<bool>("", "DanceYi", true, "");
            jieChuan = Config.Bind<bool>("", "JieChuan", false, "");

            danceYi.SettingChanged += (s, e) => OnSkinChanged("DanceYi", danceYiObject, "danceRemoveObject");
            jieChuan.SettingChanged += (s, e) => OnSkinChanged("JieChuan", jieChuanObject, "JieChuan");

            KeybindManager.Add(this, ToggleSkin, () => enableSkinKeyboardShortcut.Value);

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            tree = AssemblyUtils.GetEmbeddedAssetBundle("SkinMod.Resources.tree");
            danceYiObject = tree.LoadAsset<GameObject>("danceRemoveObject");
            jieChuanObject = tree.LoadAsset<GameObject>("JieChuan");
        }

        private void OnSkinChanged(string skinName, GameObject skinObject, string objName) {
            ProcessVisible(true);
            isEnableSkin = false;
            curSkin.Value = skinName;
            curSkinObject = skinObject;
            objectName = objName;
            ResetSkins();
            ToggleSkin();
        }

        private void ToggleSkin() {
            if (Player.i == null) return;

            string objectPath = $"{SkinHolderPath}/{objectName}(Clone)";
            GameObject skin = GameObject.Find(objectPath);

            if (skin) {
                ToastManager.Toast(isEnableSkin);
                ProcessVisible(isEnableSkin);
                isEnableSkin = !isEnableSkin;
            } else {
                ToastManager.Toast(isEnableSkin);
                CreateSkin();
                ProcessVisible(false);
                isEnableSkin = true;
            }
        }

        private void ProcessVisible(bool enable) {
            SetSkinActive(!enable);
            SetPlayerSpriteLayer(enable ? "Player" : "UI");
            EnableShadow(enable);
        }

        private void SetSkinActive(bool isActive) {
            string objectPath = $"{SkinHolderPath}/{objectName}(Clone)";
            GameObject skin = GameObject.Find(objectPath);
            if (skin != null)
                skin.SetActive(isActive);
        }

        private void SetPlayerSpriteLayer(string layerName) {
            GameObject playerSprite = GameObject.Find($"{SkinHolderPath}/PlayerSprite");
            if (playerSprite != null)
                playerSprite.layer = LayerMask.NameToLayer(layerName);
        }

        private void EnableShadow(bool enable) {
            GameObject shadowRoot = GameObject.Find("CameraCore/DummyPlayer/ShadowRoot");
            if (shadowRoot != null)
                shadowRoot.SetActive(enable);
        }

        private void CreateSkin() {
            Vector3 skinPos = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y + 35, Player.i.transform.position.z);
            GameObject skinClone = Instantiate(curSkinObject, skinPos, Quaternion.identity, GameObject.Find(SkinHolderPath).transform);

            if (curSkin.Value == "DanceYi") {
                skinClone.transform.localPosition = new Vector3(0f, 11.2001f, 0f);
                skinClone.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            } else if (curSkin.Value == "JieChuan") {
                skinClone.transform.localPosition = new Vector3(0.6006f, 11.6006f, 0f);
                skinClone.transform.localScale = new Vector3(5f, 5f, 5f);
            }

            SetPlayerSpriteLayer("UI");
            isEnableSkin = true;
            EnableShadow(false);
        }

        private void ResetSkins() {
            //danceYi.Value = false;
            //jieChuan.Value = false;
        }

        private void OnDestroy() {
            if (tree != null)
                tree.Unload(false);

            harmony?.UnpatchSelf();
        }
    }
}
