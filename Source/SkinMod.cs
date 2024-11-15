using BepInEx;
using BepInEx.Configuration;

using BepInEx.Logging;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using NineSolsAPI;
using NineSolsAPI.Utils;
using RCGFSM.PlayerAbility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace SkinMod {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkinMod : BaseUnityPlugin {
        public static SkinMod Instance { get; private set; }

        private ConfigEntry<KeyboardShortcut> enableSkinKeyboardShortcut;
        private ConfigEntry<KeyboardShortcut> customObjectShortcut = null!;
        public ConfigEntry<string> curSkin;
        private ConfigEntry<bool> danceYi;
        private ConfigEntry<bool> jieChuan;
        private ConfigEntry<bool> usagi;
        private ConfigEntry<bool> jee;
        private ConfigEntry<bool> heng;
        private ConfigEntry<bool> goblin;
        private ConfigEntry<bool> niko;
        private ConfigEntry<bool> attackEffect;
        private ConfigEntry<string> path;
        private ConfigEntry<Vector3> pos;
        private ConfigEntry<Vector3> scale;
        private ConfigEntry<Vector3> rotate;
        private ConfigEntry<int> orderLayer;
        private ConfigEntry<float> gifSpeed;
        private ConfigEntry<bool> disableYi;
        private ConfigEntry<bool> hideCustomObject;
        private ConfigEntry<Vector3> cc;

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
        private GameObject nikoObject;
        private GameObject customObject;

        private testGif testgif;

        public delegate void HandlerNoParam();
        public HandlerNoParam onUpdate;

        private const string SkinHolderPath = "GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder";

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);

            Instance = this;

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

            // Skin settings (higher order numbers appear first)
            danceYi = Config.Bind<bool>("Skin", "DanceYi", true,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 24 }));

            niko = Config.Bind<bool>("Skin", "Niko", true,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 23 }));

            jieChuan = Config.Bind<bool>("Skin", "JieChuan", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 22 }));

            usagi = Config.Bind<bool>("Skin", "Usagi", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 21 }));

            jee = Config.Bind<bool>("Skin", "Jee", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 20 }));

            heng = Config.Bind<bool>("Skin", "Heng", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 19 }));

            goblin = Config.Bind<bool>("Skin", "Goblin", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 18 }));

            attackEffect = Config.Bind<bool>("Skin", "AttackEffect", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 17 }));

            // Custom Pic Gif settings
            path = Config.Bind<string>("Custom Pic Gif", "path", "",
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 16 }));

            pos = Config.Bind<Vector3>("Custom Pic Gif", "Position", Vector3.zero,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 15 }));

            scale = Config.Bind<Vector3>("Custom Pic Gif", "Scale", new Vector3(10f, 10f, 10f),
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 14 }));

            rotate = Config.Bind<Vector3>("Custom Pic Gif", "Rotate", Vector3.zero,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 13 }));

            orderLayer = Config.Bind<int>("Custom Pic Gif", "orderLayer", 101,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 12 }));

            gifSpeed = Config.Bind<float>("Custom Pic Gif", "GifSpeed", 1f,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 11 }));

            disableYi = Config.Bind<bool>("Custom Pic Gif", "DisableYi", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 10 }));

            hideCustomObject = Config.Bind<bool>("Custom Pic Gif", "HideCustomPic", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 9 }));

            // General settings
            enableSkinKeyboardShortcut = Config.Bind("", "Enable Skin Shortcut",
                        new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift),
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 8 }));

            customObjectShortcut = Config.Bind("", "Create Custom Picture Gif Shortcut",
                        new KeyboardShortcut(KeyCode.Q, KeyCode.LeftControl),
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 7 }));

            curSkin = Config.Bind<string>("", "currSkin", "",
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 6 }));


            KeybindManager.Add(this, ToggleSkin, () => enableSkinKeyboardShortcut.Value);
            KeybindManager.Add(this, CustomObject, () => customObjectShortcut.Value);

            disableYi.Value = false;

            danceYi.SettingChanged += (s, e) => OnSkinChanged("DanceYi", danceYiObject, "danceRemoveObject");
            jieChuan.SettingChanged += (s, e) => OnSkinChanged("JieChuan", jieChuanObject, "JieChuan");
            usagi.SettingChanged += (s, e) => OnSkinChanged("Usagi", usagiObject, "Usagi");
            jee.SettingChanged += (s, e) => OnSkinChanged("Jee", jeeObject, "Jee");
            heng.SettingChanged += (s, e) => OnSkinChanged("Heng", hengObject, "Heng");
            goblin.SettingChanged += (s, e) => OnSkinChanged("Goblin", goblinObject, "Goblin");
            niko.SettingChanged += (s, e) => OnSkinChanged("Niko", nikoObject, "Niko");
            attackEffect.SettingChanged += (s, e) => AttackEffect(attackEffect.Value);
            pos.SettingChanged += (s, e) => UpdateCustom();
            scale.SettingChanged += (s, e) => UpdateCustom();
            rotate.SettingChanged += (s, e) => UpdateCustom();
            orderLayer.SettingChanged += (s, e) => UpdateCustom();
            gifSpeed.SettingChanged += (s, e) => testgif.setSpeed(gifSpeed.Value);
            disableYi.SettingChanged += (s, e) => ActiveYi(!disableYi.Value);
            hideCustomObject.SettingChanged += (s, e) => hideCustomObjcet(hideCustomObject.Value);



            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            tree = AssemblyUtils.GetEmbeddedAssetBundle("SkinMod.Resources.tree");
            danceYiObject = tree.LoadAsset<GameObject>("danceRemoveObject");
            jieChuanObject = tree.LoadAsset<GameObject>("JieChuan");
            usagiObject = tree.LoadAsset<GameObject>("Usagi");
            jeeObject = tree.LoadAsset<GameObject>("Jee");
            hengObject = tree.LoadAsset<GameObject>("Heng");
            goblinObject = tree.LoadAsset<GameObject>("Goblin");
            nikoObject = tree.LoadAsset<GameObject>("Niko");

            testgif = new testGif();
            testgif.testHook();
        }

        void hideCustomObjcet(bool enable) {    
            if (GameObject.Find($"{SkinHolderPath}/customObject")) {
                customObject = GameObject.Find($"{SkinHolderPath}/customObject");
                customObject.SetActive(!enable);
            }
        }



        void ActiveYi(bool enable) {
            SetPlayerSpriteLayer(enable ? "Player" : "UI");
            EnableShadow(enable);
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

        void Update() {
            onUpdate?.Invoke();
        }

        void UpdateCustom() {
            //ToastManager.Toast("test");
            
            customObject.transform.localPosition = pos.Value;
            customObject.transform.localScale = scale.Value;
            customObject.transform.eulerAngles = rotate.Value;
            customObject.GetComponent<SpriteRenderer>().sortingOrder = orderLayer.Value;
        }

        void CustomObject() {
            
            //ToastManager.Toast("Test");

            testgif.clear();

            hideCustomObject.Value = false;

            if (path.Value == "")
                return;

            try {
                if (!GameObject.Find($"{SkinHolderPath}/customObject")) {
                    customObject = new GameObject("customObject");
                    customObject.transform.position = Player.i.transform.position;
                    customObject.transform.localScale = new Vector3(10f, 10f, 10f);
                    customObject.transform.localPosition = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y, Player.i.transform.position.z);
                    customObject.transform.SetParent(GameObject.Find($"{SkinHolderPath}").transform);
                    customObject.AddComponent<SpriteRenderer>();
                    
                    customObject.GetComponent<SpriteRenderer>().sprite = testGif.LoadSprite(path.Value);
                    //Destroy(GameObject.Find($"{SkinHolderPath}/test"));
                } else {
                    customObject = GameObject.Find($"{SkinHolderPath}/customObject");
                    GameObject.Find($"{SkinHolderPath}/customObject").GetComponent<SpriteRenderer>().sprite = testGif.LoadSprite(path.Value);
                }

                pos.Value = customObject.transform.localPosition;
                scale.Value = customObject.transform.localScale;
                rotate.Value = customObject.transform.localEulerAngles;

            } catch (Exception e) {
                //ToastManager.Toast(e);
            }
        }

        void Start() {
            curSkin.Value = "";
            danceYi.Value = false;
            jieChuan.Value = false;
            usagi.Value = false;
            jee.Value = false;
            heng.Value = false;
            goblin.Value = false;
            niko.Value = false;
            attackEffect.Value = false;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void AttackEffect(bool isEnable) {
            //ToastManager.Toast(isEnable);

            if (Player.i == null)
                return;

            // Cache GameObject references
            GameObject atkObject = GameObject.Find($"{SkinHolderPath}/Attack(Clone)");
            GameObject effectAttackObject = GameObject.Find($"{SkinHolderPath}/Effect_Attack")?.gameObject;
            GameObject parryEffectObject = GameObject.Find($"{SkinHolderPath}/PlayerSprite/Effect_TAICHIParry/Effect_ParryCounterAttack0");

            // Destroy the existing attack object if it exists
            if (atkObject != null) {
                Destroy(atkObject);
            }

            // Modify Effect_Attack's local position
            if (effectAttackObject != null) {
                effectAttackObject.transform.localPosition = isEnable ? new Vector3(0f, -8f, 8000f) : new Vector3(0f, -8f, 0f);
            }

            // Handle enabling or disabling parry effect
            if (parryEffectObject != null) {
                parryEffectObject.SetActive(!isEnable);
            }

            // Instantiate the attack effect if enabled
            if (isEnable) {
                Vector3 skinPos = Player.i.transform.position + new Vector3(0, 25, 0);
                Instantiate(tree.LoadAsset<GameObject>("Attack"), skinPos, Quaternion.identity, GameObject.Find(SkinHolderPath)?.transform);
            }
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
                skinClone.transform.Find("Animator/Hand Right").gameObject.transform.localScale = new Vector3(0.26f, -0.26f, 0.26f);
                skinClone.transform.Find("Animator/Hand Left").gameObject.transform.localScale = new Vector3(0.26f, -0.26f, 0.26f);
                skinClone.transform.Find("Animator/Hand Right").gameObject.transform.eulerAngles = new Vector3(0f, 0f, 350f);
            } else if (curSkin.Value == "Heng") {
                skinClone.transform.localPosition = new Vector3(-3.499f, 17.7012f, 0f);
                skinClone.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            } else if (curSkin.Value == "Goblin") {
                skinClone.transform.localPosition = new Vector3(-1.499f, 12.7012f, 0f);
                skinClone.transform.localScale = new Vector3(7f, 7f, 7f);
            } else if (curSkin.Value == "Niko") {
                skinClone.transform.localPosition = new Vector3(-1.499f, 15.7012f, 0f);
                skinClone.transform.localScale = new Vector3(6f, 6f, 6f);
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

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            //ToastManager.Toast(attackEffect.Value);
            if (attackEffect.Value) {
                AttackEffect(true);
            }
        }

        private void ResetSkins() {
            //danceYi.Value = false;
            //jieChuan.Value = false;
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (tree != null)
                tree.Unload(false);

            harmony?.UnpatchSelf();
        }
    }
}
