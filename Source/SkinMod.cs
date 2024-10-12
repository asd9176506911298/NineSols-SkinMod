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

    private void Awake() {
        RCGLifeCycle.DontDestroyForever(gameObject);

        harmony = Harmony.CreateAndPatchAll(typeof(SkinMod).Assembly);

        //enableSkinConfig = Config.Bind("", "Enable Skin", false, "");
        enableSkinKeyboardShortcut = Config.Bind("", "Enable Skin Shortcut",
            new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift), "");

        KeybindManager.Add(this, ToggleSkin, () => enableSkinKeyboardShortcut.Value);

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        tree = AssemblyUtils.GetEmbeddedAssetBundle("SkinMod.Resources.tree");
        danceObject = tree.LoadAsset<GameObject>("danceRemoveObject");
    }

    private void ToggleSkin() {
        //ToastManager.Toast("ToggleSkin");
        Logger.LogInfo("ToggleSkin");

        if (Player.i == null) return;

        bool hasSkin = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/danceRemoveObject(Clone)");

        if (hasSkin) 
            {
            if (isEnableSkin)
            {
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/danceRemoveObject(Clone)").SetActive(false);
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("Player");
                isEnableSkin = false;
            } 
            else
            {
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/danceRemoveObject(Clone)").SetActive(true);
                GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("UI");
                isEnableSkin = true;
            }
        }
        else
        {
            Vector3 dancePos = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y + 35, Player.i.transform.position.z);
            GameObject skin = Instantiate(danceObject, dancePos, Quaternion.identity, GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder").transform);
            skin.transform.localPosition = new Vector3(0f, 11.2001f, 0f);
            skin.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("UI");
            isEnableSkin = true;
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