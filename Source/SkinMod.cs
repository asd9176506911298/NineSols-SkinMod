using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NineSolsAPI;
using NineSolsAPI.Utils;
using UnityEngine;

namespace SkinMod;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class SkinMod : BaseUnityPlugin {
    private ConfigEntry<bool> enableSkinConfig = null!;
    private ConfigEntry<KeyboardShortcut> enableSkinKeyboardShortcut = null!;

    private Harmony harmony = null!;

    private bool isEnableSkin = false;

    private AssetBundle tree;
    private GameObject danceObject;
    private GameObject jieChuanObject;

    private void Awake() {
        RCGLifeCycle.DontDestroyForever(gameObject);

        harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

        //enableSkinConfig = Config.Bind("", "Enable Skin", false, "");
        enableSkinKeyboardShortcut = Config.Bind("", "Enable Skin Shortcut",
            new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift), "");

        KeybindManager.Add(this, ToggleSkin, () => enableSkinKeyboardShortcut.Value);

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        tree = AssemblyUtils.GetEmbeddedAssetBundle("SkinMod.Resources.tree");
        danceObject = tree.LoadAsset<GameObject>("danceRemoveObject");
        jieChuanObject = tree.LoadAsset<GameObject>("JieChuan");
    }

    void EnableShadow(bool enable) {
        GameObject shadowRoot = GameObject.Find("CameraCore/DummyPlayer/ShadowRoot");

        if (shadowRoot != null) {
            shadowRoot.SetActive(enable);
        }
    }

    private void ToggleSkin() {
        ToastManager.Toast("ToggleSkin DDD");
        Logger.LogInfo("ToggleSkin");

        ToastManager.Toast(GameObject.Find("CameraCore/DummyPlayer/ShadowRoot"));

        if (Player.i == null) return;

        bool hasSkin = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/JieChuan(Clone)");

        if (hasSkin) 
            {
            if (isEnableSkin)
            {
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/JieChuan(Clone)").SetActive(false);
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("Player");
                isEnableSkin = false;
                EnableShadow(true);
            } 
            else
            {
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/JieChuan(Clone)").SetActive(true);
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("UI");
                isEnableSkin = true;
                EnableShadow(false);
            }
        }
        else
        {
            Vector3 dancePos = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y + 35, Player.i.transform.position.z);
            GameObject skin = Instantiate(jieChuanObject, dancePos, Quaternion.identity, GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder").transform);
            // Yi Dance
            //skin.transform.localPosition = new Vector3(0f, 11.2001f, 0f);
            //skin.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            // JieChuan 
            skin.transform.localPosition = new Vector3(0.6006f, 11.6006f, 0f);
            skin.transform.localScale = new Vector3(5f, 5f, 5f);

            GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("UI");
            isEnableSkin = true;
            EnableShadow(false);
        }
    }

    private void OnDestroy() {  
        // Make sure to clean up resources here to support hot reloading

        if (tree != null) {
            tree.Unload(false);
        }

        harmony.UnpatchSelf();
    }
}