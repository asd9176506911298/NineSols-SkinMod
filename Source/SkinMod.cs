using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NineSolsAPI;
using NineSolsAPI.Utils;
using System.Linq.Expressions;
using UnityEngine;

namespace SkinMod;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class SkinMod : BaseUnityPlugin {
    private ConfigEntry<bool> enableSkinConfig = null;
    private ConfigEntry<KeyboardShortcut> enableSkinKeyboardShortcut = null!;

    private ConfigEntry<string> curSkin = null;
    private ConfigEntry<bool> danceYi = null;
    private ConfigEntry<bool> jieChuan = null;

    private Harmony harmony = null!;

    private string objectPath;
    private string objectName;
    private GameObject curSkinObject;

    private bool isEnableSkin = false;

    private AssetBundle tree;
    private GameObject danceYiObject;
    private GameObject jieChuanObject;

    private void Awake() {
        RCGLifeCycle.DontDestroyForever(gameObject);

        harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

        //enableSkinConfig = Config.Bind("", "Enable Skin", false, "");
        enableSkinKeyboardShortcut = Config.Bind("", "Enable Skin Shortcut",
            new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift), "");

        curSkin = Config.Bind<string>("", "currSkin", "", "");
        danceYi = Config.Bind<bool>("", "DanceYi", true, "");
        jieChuan = Config.Bind<bool>("", "JieChuan", false, "");

        danceYi.SettingChanged += DanceYi_SettingChanged;
        jieChuan.SettingChanged += JieChuan_SettingChanged;

        KeybindManager.Add(this, ToggleSkin, () => enableSkinKeyboardShortcut.Value);

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        tree = AssemblyUtils.GetEmbeddedAssetBundle("SkinMod.Resources.tree");
        danceYiObject = tree.LoadAsset<GameObject>("danceRemoveObject");
        jieChuanObject = tree.LoadAsset<GameObject>("JieChuan");
    }

    private void DanceYi_SettingChanged(object sender, System.EventArgs e) {
        curSkin.Value = "DanceYi";
        curSkinObject = danceYiObject;
        objectName = "danceRemoveObject";
        jieChuan.Value = false;
        danceYi.Value = false;
        ProcessVisible(true);
        isEnableSkin = false;
        changeObjectPath();
    }

    private void JieChuan_SettingChanged(object sender, System.EventArgs e) {
        curSkin.Value = "JieChuan";
        curSkinObject = jieChuanObject;
        objectName = "JieChuan";
        jieChuan.Value = false;
        danceYi.Value = false;
        ProcessVisible(true);
        isEnableSkin = false;
        changeObjectPath();
    }

    void EnableShadow(bool enable) {
        GameObject shadowRoot = GameObject.Find("CameraCore/DummyPlayer/ShadowRoot");

        if (shadowRoot != null) {
            shadowRoot.SetActive(enable);
        }
    }

    void changeObjectPath() {
        objectPath = $"GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/{objectName}(Clone)";
    }

    void ProcessVisible(bool enable) {
        DisableSkin(enable);
        HideYiSprite(!enable);
        EnableShadow(enable);
    }

    private void ToggleSkin() {
        ToastManager.Toast("ToggleSkin DDD");

        //ToastManager.Toast(GameObject.Find("CameraCore/DummyPlayer/ShadowRoot"));

        if (Player.i == null) return;

        changeObjectPath();

        bool hasSkin = GameObject.Find(objectPath);

        if (hasSkin) 
            {
            if (isEnableSkin)
            {
                ToastManager.Toast("hasSkin isEnableSkin");
                ProcessVisible(true);
                isEnableSkin = false;
            } 
            else
            {
                ToastManager.Toast("hasSkin no isEnableSkin");
                ProcessVisible(false);
                isEnableSkin = true;
            }
        }
        else
        {
            CreateSkin();
            HideYiSprite(true);
            isEnableSkin = true;
            EnableShadow(false);
        }
    }

    void DisableSkin(bool isDisable) {
        GameObject skin = GameObject.Find(objectPath);
        ToastManager.Toast($"DisableSkin {skin}");
        if (skin == null) return;

        if (isDisable)
            skin.SetActive(false);
        else
            skin.SetActive(true);
    }

    void HideYiSprite(bool isHide) {
        GameObject PlayerSprite = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite");

        ToastManager.Toast($"{PlayerSprite} {isHide}");

        if (PlayerSprite == null) return;

        if (isHide) {
            PlayerSprite.layer = LayerMask.NameToLayer("UI");
        } else {
            PlayerSprite.layer = LayerMask.NameToLayer("Player");
        }
    }

    void CreateSkin() {
        Vector3 skinPos = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y + 35, Player.i.transform.position.z);
        GameObject skinClone = Instantiate(curSkinObject, skinPos, Quaternion.identity, GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder").transform);

        if (curSkin.Value == "DanceYi") {
            // Yi Dance
            skinClone.transform.localPosition = new Vector3(0f, 11.2001f, 0f);
            skinClone.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        } else if (curSkin.Value == "JieChuan") {
            // JieChuan 
            skinClone.transform.localPosition = new Vector3(0.6006f, 11.6006f, 0f);
            skinClone.transform.localScale = new Vector3(5f, 5f, 5f);
        }
        
        GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("UI");
        isEnableSkin = true;
        EnableShadow(false);
    }

    private void OnDestroy() {  
        // Make sure to clean up resources here to support hot reloading

        if (tree != null) {
            tree.Unload(false);
        }

        harmony.UnpatchSelf();
    }
}