using BepInEx;
using BepInEx.Configuration;

using BepInEx.Logging;
using Com.LuisPedroFonseca.ProCamera2D.TopDownShooter;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using NineSolsAPI;
using NineSolsAPI.Utils;
using RCGFSM.PlayerAbility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

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
        private ConfigEntry<bool> attackEffect;
        private ConfigEntry<string> path;
        private ConfigEntry<float> x;
        private ConfigEntry<float> y;
        private ConfigEntry<float> z;
        private ConfigEntry<float> scaleX;
        private ConfigEntry<float> scaleY;
        private ConfigEntry<float> scaleZ;
        private ConfigEntry<float> rotateX;
        private ConfigEntry<float> rotateY;
        private ConfigEntry<float> rotateZ;
        private ConfigEntry<int> orderLayer;
        private ConfigEntry<float> gifSpeed;
        private ConfigEntry<bool> disableYi;
        private ConfigEntry<bool> hideCustomObject;

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
        private GameObject customObject;

        GameObject RotateProxy = null;

        private testGif testgif;

        public delegate void HandlerNoParam();
        public HandlerNoParam onUpdate;

        private const string SkinHolderPath = "GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder";

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);

            Instance = this;

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

            curSkin = Config.Bind<string>("", "currSkin", "",
            new ConfigDescription("", null,
            new ConfigurationManagerAttributes { Order = 24 }));

            danceYi = Config.Bind<bool>("", "DanceYi", true,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 23 }));

            jieChuan = Config.Bind<bool>("", "JieChuan", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 22 }));

            usagi = Config.Bind<bool>("", "Usagi", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 21 }));

            jee = Config.Bind<bool>("", "Jee", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 20 }));

            heng = Config.Bind<bool>("", "Heng", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 19 }));

            goblin = Config.Bind<bool>("", "Goblin", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 18 }));

            attackEffect = Config.Bind<bool>("", "AttackEffect", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 17 }));

            path = Config.Bind<string>("", "path", "",
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 16 }));

            x = Config.Bind<float>("", "Pos x", 0,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 15 }));

            y = Config.Bind<float>("", "Pos y", 0,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 14 }));

            z = Config.Bind<float>("", "Pos z", 0,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 13 }));

            scaleX = Config.Bind<float>("", "scaleX", 10,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 12 }));

            scaleY = Config.Bind<float>("", "scaleY", 10,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 11 }));

            scaleZ = Config.Bind<float>("", "scaleZ", 10,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 10 }));

            rotateX = Config.Bind<float>("", "RotateX", 0,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 9 }));

            rotateY = Config.Bind<float>("", "RotateY", 0,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 8 }));

            rotateZ = Config.Bind<float>("", "RotateZ", 0,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 7 }));

            orderLayer = Config.Bind<int>("", "orderLayer", 101,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 6 }));

            gifSpeed = Config.Bind<float>("", "GifSpeed", 1f,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 5 }));

            disableYi = Config.Bind<bool>("", "DisableYi", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 4 }));

            hideCustomObject = Config.Bind<bool>("", "HideCustomPic", false,
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 3 }));

            enableSkinKeyboardShortcut = Config.Bind("", "Enable Skin Shortcut",
                        new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift),
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 2 }));

            customObjectShortcut = Config.Bind("", "Create Custom Picture Gif Shortcut",
                        new KeyboardShortcut(KeyCode.Q, KeyCode.LeftControl),
                        new ConfigDescription("", null,
                        new ConfigurationManagerAttributes { Order = 1 }));
            KeybindManager.Add(this, test, KeyCode.X);

            KeybindManager.Add(this, p1, KeyCode.Keypad1);
            //KeybindManager.Add(this, p2, KeyCode.Keypad2);
            //KeybindManager.Add(this, p3, KeyCode.Keypad3);
            //KeybindManager.Add(this, p4, KeyCode.Keypad4);
            //KeybindManager.Add(this, reset, KeyCode.C);

            KeybindManager.Add(this, ToggleSkin, () => enableSkinKeyboardShortcut.Value);
            KeybindManager.Add(this, CustomObject, () => customObjectShortcut.Value);



            disableYi.Value = false;

            danceYi.SettingChanged += (s, e) => OnSkinChanged("DanceYi", danceYiObject, "danceRemoveObject");
            jieChuan.SettingChanged += (s, e) => OnSkinChanged("JieChuan", jieChuanObject, "JieChuan");
            usagi.SettingChanged += (s, e) => OnSkinChanged("Usagi", usagiObject, "Usagi");
            jee.SettingChanged += (s, e) => OnSkinChanged("Jee", jeeObject, "Jee");
            heng.SettingChanged += (s, e) => OnSkinChanged("Heng", hengObject, "Heng");
            goblin.SettingChanged += (s, e) => OnSkinChanged("Goblin", goblinObject, "Goblin");
            attackEffect.SettingChanged += (s, e) => AttackEffect(attackEffect.Value);
            x.SettingChanged += (s, e) => UpdateCustom();
            y.SettingChanged += (s, e) => UpdateCustom();
            z.SettingChanged += (s, e) => UpdateCustom();
            scaleX.SettingChanged += (s, e) => UpdateCustom();
            scaleY.SettingChanged += (s, e) => UpdateCustom();
            scaleZ.SettingChanged += (s, e) => UpdateCustom();
            rotateX.SettingChanged += (s, e) => UpdateCustom();
            rotateY.SettingChanged += (s, e) => UpdateCustom();
            rotateZ.SettingChanged += (s, e) => UpdateCustom();
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

            testgif = new testGif();
            testgif.testHook();

            _lineTexture = new Texture2D(1, 1); 
            _lineTexture.SetPixel(0, 0, Color.white);
            _lineTexture.Apply();
        }

        void hideCustomObjcet(bool enable) {    
            if (GameObject.Find($"{SkinHolderPath}/customObject")) {
                customObject = GameObject.Find($"{SkinHolderPath}/customObject");
                customObject.SetActive(!enable);
            }
        }

        bool e = false;

        void reset() {
            GameCore.Instance.DiscardUnsavedFlagsAndReset();
        }

        void p1() {
            //var tri = GameObject.Find("GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/States/Attacks/[13] Tripple Poke 三連");

            //foreach (var x in tri.transform.Find("weight").GetComponent<LinkNextMoveStateWeight>().stateWeightList) {
            //    ToastManager.Toast($"Name:{x.State} weight:{x.weight}");
            //}

            //foreach (var x in tri.transform.Find("weight (1)").GetComponent<LinkNextMoveStateWeight>().stateWeightList) {
            //    ToastManager.Toast($"Name:{x.State} weight:{x.weight}");
            //}

            //這個 和下面那個都要
            //var attacksPath = "A5_S1/Room/FlashKill Binding/werw/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_JieChuan/States/Attacks";
            //var attacksParent = GameObject.Find(attacksPath);

            //if (attacksParent != null) {
            //    for (int i = 0; i < attacksParent.transform.childCount; i++) {
            //        var attackChild = attacksParent.transform.GetChild(i);

            //        // Loop through each child under attackChild
            //        for (int z = 0; z < attackChild.transform.childCount; z++) {
            //            var weights = attackChild.transform.GetChild(z);
            //            var linkNextMoveStateWeight = weights.GetComponent<LinkNextMoveStateWeight>();

            //            // Check if the component exists to avoid null reference exceptions
            //            if (linkNextMoveStateWeight != null) {
            //                // Retrieve the immediate parent names for the path
            //                string childName = attackChild.name;
            //                string parentName = attackChild.parent != null ? attackChild.parent.name : "No Parent";

            //                foreach (var x in linkNextMoveStateWeight.stateWeightList) {
            //                    ToastManager.Toast($"Parent Path: {parentName}/{childName}/{weights.name}, Name: {x.State}, Weight: {x.weight}");
            //                }
            //            }
            //        }
            //    }
            //} else {
            //    ToastManager.Toast("Attacks parent not found.");
            //}

            //這個 也要
            //[6] double attack
            var attacksPath = "A5_S1/Room/FlashKill Binding/werw/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_JieChuan/States";
            var attacksParent = GameObject.Find(attacksPath);

            if (attacksParent != null) {
                for (int i = 0; i < attacksParent.transform.childCount; i++) {
                    var attackChild = attacksParent.transform.GetChild(i);

                    // Loop through each child under attackChild
                    for (int z = 0; z < attackChild.transform.childCount; z++) {
                        var weights = attackChild.transform.GetChild(z);
                        var linkNextMoveStateWeight = weights.GetComponent<LinkNextMoveStateWeight>();

                        // Check if the component exists to avoid null reference exceptions
                        if (linkNextMoveStateWeight != null) {
                            // Retrieve names of the immediate parent and its parent
                            string firstParentName = attackChild.name;
                            string secondParentName = attackChild.parent != null ? attackChild.parent.name : "No Parent";

                            // Loop through mustUseStates and prepare the "mustUseInStart" part
                            string mustUseInStart = string.Join(", ", linkNextMoveStateWeight.mustUseStates.Select(c => $"mustUseInStart:{{{c}}}"));

                            foreach (var x in linkNextMoveStateWeight.stateWeightList) {
                                ToastManager.Toast($"{firstParentName}/{secondParentName}/{weights.name}, Name: {x.State}, Weight: {x.weight}, {mustUseInStart}");
                            }
                        }
                    }
                }
            } else {
                ToastManager.Toast("Attacks parent not found.");
            }



            //var attacksPath = "GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung/States";
            //var attacksParent = GameObject.Find(attacksPath);

            //if (attacksParent != null) {
            //    for (int i = 0; i < attacksParent.transform.childCount; i++) {
            //        var attackChild = attacksParent.transform.GetChild(i);

            //        // Loop through each child under attackChild
            //        for (int z = 0; z < attackChild.transform.childCount; z++) {
            //            var weights = attackChild.transform.GetChild(z);
            //            var linkNextMoveStateWeight = weights.GetComponent<LinkNextMoveStateWeight>();

            //            // Check if the component exists to avoid null reference exceptions
            //            if (linkNextMoveStateWeight != null) {
            //                foreach (var x in linkNextMoveStateWeight.stateWeightList) {
            //                    ToastManager.Toast($"Attack: {attackChild.name}, Name: {x.State}, Weight: {x.weight}");
            //                }
            //            }
            //        }
            //    }
            //} else {
            //    ToastManager.Toast("Attacks parent not found.");
            //}



            //foreach(var x in GameObject.FindObjectsOfType<MonsterBase>()) {
            //    x.ChangeStateIfValid(MonsterBase.States.Attack13);
            //}
        }


        void p2() {
            foreach (var x in MonsterManager.Instance.monsterDict) {
                if (x.Value.tag == "Boss") {
                    GotoPhase(x.Value, 1);
                    ToastManager.Toast($"Goto Phase2 Name:{x.Value}");
                }
            }
        }

        void p3() {
            foreach (var x in MonsterManager.Instance.monsterDict) {
                if (x.Value.tag == "Boss") {
                    GotoPhase(x.Value, 2);
                    ToastManager.Toast($"Goto Phase3 Name:{x.Value}");
                }
            }
        }


        void p4() {
            foreach (var x in MonsterManager.Instance.monsterDict) {
                if (x.Value.tag == "Boss") {
                    var monsterStatField = typeof(MonsterBase).GetField("monsterStat")
                                       ?? typeof(MonsterBase).GetField("_monsterStat");
                    if (monsterStatField != null) {
                        var monsterStat = monsterStatField.GetValue(x.Value) as MonsterStat; // Assuming MonsterStat is the type of the field
                        monsterStat.IsLockPostureInvincible = !monsterStat.IsLockPostureInvincible;
                        ToastManager.Toast($"Invincible:{monsterStat.IsLockPostureInvincible} Name:{x.Value}");
                    }
                }
            }
        }

        void GotoPhase(MonsterBase m, int index) {
            m.PhaseIndex = index;
            m.animator.SetInteger(Animator.StringToHash("PhaseIndex"), m.PhaseIndex);
            m.postureSystem.RestorePosture();
            m.monsterCore.EnablePushAway();
            if (SingletonBehaviour<GameCore>.Instance.monsterHpUI.CurrentBossHP != null) {
                SingletonBehaviour<GameCore>.Instance.monsterHpUI.CurrentBossHP.TempShow();
            }
            m.monsterCore.DisablePushAway();
            for (int i = 0; i < m.attackSensors.Count; i++) {
                if (m.attackSensors[i] != null) {
                    m.attackSensors[i].ClearQueue();
                }
            }
            m.VelX = 0f;
        }

        void test() {

            AnimationMotionRebind[] rootObjects = GameObject.FindObjectsOfType<AnimationMotionRebind>();

            // Iterate through all found AnimationMotionRebind components
            foreach (AnimationMotionRebind rootObject in rootObjects) {
                // 1. Check if the rootObject is the one with the Animator
                if (rootObject.name == "Animator") {
                    // Get the Animator component and disable it
                    Animator animator = rootObject.GetComponent<Animator>();
                    if (animator != null) {
                        animator.enabled = false; // Disable the Animator component
                        Debug.Log("Disabled Animator on: " + rootObject.name);
                    }

                    // 2. Find AbstractRoot once
                    Transform abstractRootTransform = rootObject.transform.Find("AbstractRoot");

                    // If AbstractRoot exists, set it and its children active
                    if (abstractRootTransform != null) {
                        // Enable AbstractRoot and all its children
                        abstractRootTransform.gameObject.SetActive(true);
                        Debug.Log("Enabled AbstractRoot: " + abstractRootTransform.name);

                        foreach (Transform child in abstractRootTransform) {
                            child.gameObject.SetActive(true); // Set each child active
                            if(child.name == "Body")
                                child.GetComponent<SpriteRenderer>().sortingOrder = 1;
                            Debug.Log("Enabled child of AbstractRoot: " + child.name);
                        }
                    } else {
                        Debug.LogWarning("AbstractRoot not found in " + rootObject.name);
                    }
                }
            }

            return;
            //ToastManager.Toast("test");
            RotateProxy = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy");
            RotateProxy.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            //ToastManager.Toast(RotateProxy);
            e = !e;
            return;
            GameObject t = new GameObject("bullet");
            t.transform.localScale = new Vector3(10, 10, 10);
            t.layer = LayerMask.NameToLayer("EffectDealer");
            Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            SpriteRenderer sr = t.AddComponent<SpriteRenderer>();
            BoxCollider2D bc = t.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(2, 2);
            bc.isTrigger = true;
            sr.sprite = testGif.LoadSprite(path.Value);
            //sr.sortingOrder = 101;
            t.transform.position = Player.i.Center;

            if (Player.i.Facing == Facings.Right)
                rb.AddForce(new Vector2(200, 0), ForceMode2D.Impulse);
            else {
                sr.flipX = true;
                rb.AddForce(new Vector2(-200, 0), ForceMode2D.Impulse);
            }

            t.AddComponent<bullet>();


            Destroy(t, 10f);
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
            return;
            GameObject[] rootObjects = GameObject.FindObjectsOfType<GameObject>();

            // Iterate through all found GameObjects
            foreach (GameObject rootObject in rootObjects) {
                if (rootObject.name == "Animator") {
                    // Disable the Animator component
                    Animator animator = rootObject.GetComponent<Animator>();
                    if (animator != null) {
                        animator.enabled = false;
                    }
                }

                // Check if the GameObject name is "AbstractRoot"
                if (rootObject.name == "AbstractRoot") {
                    // Set the "AbstractRoot" GameObject active
                    rootObject.SetActive(true);

                    // Iterate over all children of the "AbstractRoot" GameObject
                    foreach (Transform child in rootObject.transform) {
                        // Set each child GameObject active
                        child.gameObject.SetActive(true);

                        // Optionally, you can also do something else with each child here
                        Debug.Log("Setting child active: " + child.name);
                    }
                }

                // If you need to disable the Animator component of another object named "Animator"
                
            }
    }
        private Texture2D _lineTexture;

        
        private void OnGUI() {
            //ToastManager.Toast($"Ongui {e}");
            //Camera sceneCamera = GameObject.Find("CameraCore/DockObj/OffsetObj/ShakeObj/SceneCamera").GetComponent<Camera>();
            //Vector2 screenPointB = sceneCamera.WorldToScreenPoint(monster.transform.position);

            //// Calculate the top center of the screen
            //Vector2 screenPointA = new Vector2(Screen.width / 2, Screen.height);

            //// Flip the Y-coordinate for the `OnGUI()` system
            //screenPointB.y = Screen.height - screenPointB.y;

            //DrawLine(screenPointA, screenPointB, Color.red, 2f);

            return;
            if (RotateProxy == null)
                RotateProxy = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy");

            if (!e) {
                RotateProxy.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                return;
            }
            // Define the Camera from the GameObject path
            Camera sceneCamera = GameObject.Find("CameraCore/DockObj/OffsetObj/ShakeObj/SceneCamera").GetComponent<Camera>();

            // Increment the player's rotation around the x-axis by 1 degree
            

            float randomX = UnityEngine.Random.Range(0.0f, 360.0f);
            float randomY = UnityEngine.Random.Range(0.0f, 360.0f);
            float randomZ = UnityEngine.Random.Range(0.0f, 360.0f);

            // Create a quaternion for rotation around y and z only
            Quaternion randomRotation = Quaternion.Euler(randomX, randomY, randomZ);

            // Apply the random rotation to the RotateProxy's transform
            RotateProxy.transform.rotation = randomRotation;

            var monsterDict = MonsterManager.Instance.monsterDict;

            foreach (KeyValuePair<string, MonsterBase> kvp in monsterDict) {
                string monsterName = kvp.Key; // The key (monster name)
                MonsterBase monster = kvp.Value; // The value (MonsterBase object)

                var dis = Vector3.Distance(SingletonBehaviour<GameCore>.Instance.player.transform.position, monster.transform.position);

                if (!monster.IsActive) continue;
                if (dis > 900) continue; // Skip if too far
                
                //if (!MonsterManager.Instance.CheckMonsterTransformsInCamera(monster)) continue;
                if (monster.postureSystem.CurrentHealthValue <= 0) // Check for dead monsters
                {
                    continue; // Skip processing dead monsters
                }   

                if (monster.tag == "Trap" || monster.name.Contains("BlindSwordMan")) continue; // Skip traps

                // Check if this monster has already shot


                Vector2 screenPointB = sceneCamera.WorldToScreenPoint(monster.transform.position);

                // Calculate the top center of the screen
                Vector2 screenPointA = new Vector2(Screen.width / 2, Screen.height);

                // Flip the Y-coordinate for the `OnGUI()` system
                screenPointB.y = Screen.height - screenPointB.y;

                DrawLine(screenPointA, screenPointB, Color.red, 2f);

                // Create the bullet
                GameObject bullet = new GameObject("bullet");
                bullet.transform.localScale = new Vector3(10, 10, 10);
                bullet.layer = LayerMask.NameToLayer("EffectDealer");
                Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
                BoxCollider2D bc = bullet.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(2, 2);
                bc.isTrigger = true;
                sr.sprite = testGif.LoadSprite(path.Value);

                // Set the bullet's position to the player's position
                bullet.transform.position = Player.i.Center;

                // Calculate direction vector from player to monster
                Vector2 direction = (monster.transform.position - Player.i.Center).normalized;

                // Apply force in the direction of the monster
                rb.AddForce(direction * 999999, ForceMode2D.Impulse);
                //bullet.transform.position = monster.transform.position;
                // If you want to flip the sprite based on direction
                if (direction.x < 0) {
                    sr.flipX = true; // Flip sprite if the monster is to the left of the player
                }

                // Add bullet component if necessary    
                bullet.AddComponent<bullet>();

                // Destroy the bullet after 0.1 seconds
                Destroy(bullet, 0.5f);

                // Mark this monster as having shot

            }
        }



        private void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float thickness) {
            Vector2 delta = pointB - pointA;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            float length = delta.magnitude;

            // Set line color
            GUI.color = color;

            // Rotate and draw line texture
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pointA);
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, length, thickness), _lineTexture);
            GUI.matrix = matrixBackup;

            // Reset GUI color
            GUI.color = Color.white;
        }

        void UpdateCustom() {
            //ToastManager.Toast("test");
            
            customObject.transform.localPosition = new Vector3(x.Value, y.Value, z.Value);
            customObject.transform.localScale = new Vector3(scaleX.Value, scaleY.Value, scaleZ.Value);
            customObject.transform.eulerAngles = new Vector3(rotateX.Value, rotateY.Value, rotateZ.Value);
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

                x.Value = customObject.transform.localPosition.x;
                y.Value = customObject.transform.localPosition.y;
                z.Value = customObject.transform.localPosition.z;
                scaleX.Value = customObject.transform.localScale.x;
                scaleY.Value = customObject.transform.localScale.y;
                scaleZ.Value = customObject.transform.localScale.z;
                rotateX.Value = customObject.transform.localEulerAngles.x;
                rotateY.Value = customObject.transform.localEulerAngles.y;
                rotateZ.Value = customObject.transform.localEulerAngles.z;


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
