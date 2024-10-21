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
        public static SkinMod Instance { get; private set; }

        private ConfigEntry<KeyboardShortcut> enableSkinKeyboardShortcut;
        private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut = null!;
        public ConfigEntry<string> curSkin;
        private ConfigEntry<bool> danceYi;
        private ConfigEntry<bool> jieChuan;
        private ConfigEntry<bool> usagi;
        private ConfigEntry<bool> jee;
        private ConfigEntry<bool> heng;
        private ConfigEntry<bool> goblin;

        private Harmony harmony;

        public string objectName;
        private GameObject curSkinObject;

        public bool isEnableSkin = false;

        private AssetBundle tree;
        private GameObject danceYiObject;
        private GameObject jieChuanObject;
        private GameObject usagiObject;
        private GameObject jeeObject;
        private GameObject hengObject;
        private GameObject goblinObject;

        private const string SkinHolderPath = "GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder";

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);

            Instance = this;

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);


            curSkin = Config.Bind<string>("", "currSkin", "",
            new ConfigDescription("", null,
            new ConfigurationManagerAttributes { Order = 5 }));

            danceYi = Config.Bind<bool>("", "DanceYi", true,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 4 }));

            jieChuan = Config.Bind<bool>("", "JieChuan", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 3 }));

            usagi = Config.Bind<bool>("", "Usagi", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 2 }));

            jee = Config.Bind<bool>("", "Jee", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 2 }));

            heng = Config.Bind<bool>("", "Heng", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 2 }));

            goblin = Config.Bind<bool>("", "Goblin", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 2 }));

            enableSkinKeyboardShortcut = Config.Bind("", "Enable Skin Shortcut",
                        new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift),
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 1 }));

            somethingKeyboardShortcut = Config.Bind("General.Something", "Shortcut",
            new KeyboardShortcut(KeyCode.Q, KeyCode.LeftControl), "Shortcut to execute");
            KeybindManager.Add(this, TestMethod, () => somethingKeyboardShortcut.Value);

            danceYi.SettingChanged += (s, e) => OnSkinChanged("DanceYi", danceYiObject, "danceRemoveObject");
            jieChuan.SettingChanged += (s, e) => OnSkinChanged("JieChuan", jieChuanObject, "JieChuan");
            usagi.SettingChanged += (s, e) => OnSkinChanged("Usagi", usagiObject, "Usagi");
            jee.SettingChanged += (s, e) => OnSkinChanged("Jee", jeeObject, "Jee");
            heng.SettingChanged += (s, e) => OnSkinChanged("Heng", hengObject, "Heng");
            goblin.SettingChanged += (s, e) => OnSkinChanged("Goblin", goblinObject, "Goblin");

            KeybindManager.Add(this, ToggleSkin, () => enableSkinKeyboardShortcut.Value);

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            tree = AssemblyUtils.GetEmbeddedAssetBundle("SkinMod.Resources.tree");
            danceYiObject = tree.LoadAsset<GameObject>("danceRemoveObject");
            jieChuanObject = tree.LoadAsset<GameObject>("JieChuan");
            usagiObject = tree.LoadAsset<GameObject>("Usagi");
            jeeObject = tree.LoadAsset<GameObject>("Jee");
            hengObject = tree.LoadAsset<GameObject>("Heng");
            goblinObject = tree.LoadAsset<GameObject>("Goblin");
        }

        void TestMethod() {
            ToastManager.Toast("Toast");
            GameObject atkObject = GameObject.Find($"{SkinHolderPath}/Attack(Clone)");
            if (atkObject != null) {
                Destroy(atkObject);
            }

            GameObject parryEffectObject = GameObject.Find($"{SkinHolderPath}/PlayerSprite/Effect_TAICHIParry/Effect_ParryCounterAttack0");
            if (parryEffectObject != null) {
                parryEffectObject.SetActive(false);
            }

            GameObject.Find($"{SkinHolderPath}/Effect_Attack").gameObject.transform.localPosition = new Vector3(0f, -8f, 8000f);
            Vector3 skinPos = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y + 25, Player.i.transform.position.z);
            GameObject skinClone = Instantiate(tree.LoadAsset<GameObject>("Attack"), skinPos, Quaternion.identity, GameObject.Find(SkinHolderPath).transform);
            
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
                ProcessVisible(isEnableSkin);
                isEnableSkin = !isEnableSkin;
            } else {
                CreateSkin();
                ProcessVisible(false);
                isEnableSkin = true;
            }
        }

        private void ProcessVisible(bool enable) {
            SetSkinActive(!enable);
            SetPlayerSpriteLayer(enable ? "Player" : "UI");
            activeHeal(enable);
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
                skinClone.transform.localPosition = new Vector3(-0.399f, 28.3011f, 0f);
                skinClone.transform.localScale = new Vector3(5f, 5f, 5f);
            } else if (curSkin.Value == "Usagi") {
                skinClone.transform.localPosition = new Vector3(2.601f, 14.7012f, 0f);
                skinClone.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            } else if (curSkin.Value == "Jee") {
                skinClone.transform.localPosition = new Vector3(-0.699f, 17.7012f, 0f);
                skinClone.transform.localScale = new Vector3(-13f, -13f, 13f);
                skinClone.transform.Find("Hand Right").gameObject.transform.localScale = new Vector3(0.26f, -0.26f, 0.26f);
                skinClone.transform.Find("Hand Left").gameObject.transform.localScale = new Vector3(0.26f, -0.26f, 0.26f);
                skinClone.transform.Find("Hand Right").gameObject.transform.eulerAngles = new Vector3(0f, 0f, 350f);
            } else if (curSkin.Value == "Heng") {
                skinClone.transform.localPosition = new Vector3(-3.499f, 17.7012f, 0f);
                skinClone.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            } else if (curSkin.Value == "Goblin") {
                skinClone.transform.localPosition = new Vector3(-1.499f, 12.7012f, 0f);
                skinClone.transform.localScale = new Vector3(7f, 7f, 7f);   
            }

            SetPlayerSpriteLayer("UI");
            isEnableSkin = true;
            EnableShadow(false);
        }

        private void activeHeal(bool active) {
            GameObject HealSmoke = GameObject.Find($"{SkinHolderPath}/PlayerSprite/HealSmoke/GameObject3");
            if (HealSmoke != null)
                HealSmoke.SetActive(active);
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
