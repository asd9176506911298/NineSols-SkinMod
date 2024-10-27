using HarmonyLib;
using Newtonsoft.Json;
using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SkinMod {
    public class testGif {
        private float timeSinceLastUpdate = 0f;
        private float baseInterval = 0.1f; // Base interval in seconds (e.g., 1 second)
        private float updateInterval = 0.1f;

        readonly static Dictionary<string, Sprite> cacheSprite = new Dictionary<string, Sprite>();
        readonly static Dictionary<string, Gif> mapGif = new Dictionary<string, Gif>();

        public class Gif {
            public List<Sprite> frames = new List<Sprite>();
            public List<float> delay = new List<float>();
            private float time = 0.0f;
            private int frame = 0;

            public Sprite Current => frames[frame];

            public void Reset() {
                time = 0.0f;
                frame = 0;
            }

            public void Update(Traverse t) {
                time += Time.deltaTime;
                if (time >= delay[frame]) {
                    frame = (frame + 1) % frames.Count;
                    time = 0.0f;
                    t.SetValue(Current);
                }
            }
        }

        public class SpriteDesc {
            public Vector2 pivot = new Vector2(0, 0);
            public float pixelsPerUnit = 100.0f;
            public uint extrude = 0;
            public SpriteMeshType spriteType = SpriteMeshType.Tight;

            public static SpriteDesc _default = new SpriteDesc();
        }

        public static Sprite LoadSprite(string filePath, string specificName = null) {
            if (cacheSprite.ContainsKey(filePath))
                return cacheSprite[filePath];
            if (!File.Exists(filePath))
                return null;

            var name = string.IsNullOrEmpty(specificName) ? Path.GetFileNameWithoutExtension(filePath) : specificName;

            if (mapGif.TryGetValue(name, out Gif gifFound)) {
                gifFound.Reset();
                return gifFound.Current;
            }

            var ext = Path.GetExtension(filePath).ToLower();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg") {
                byte[] data = File.ReadAllBytes(filePath);
                Texture2D tex2D = new Texture2D(2, 2);
                if (tex2D.LoadImage(data)) {
                    var spriteJsonPath = filePath.Replace(ext, ".json");
                    SpriteDesc desc = SpriteDesc._default;
                    if (File.Exists(spriteJsonPath))
                        desc = JsonConvert.DeserializeObject<SpriteDesc>(File.ReadAllText(spriteJsonPath));

                    // Set pivot to the center (0.5, 0.5)
                    Sprite sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f), desc.pixelsPerUnit, desc.extrude, desc.spriteType);
                    sprite.name = name;
                    cacheSprite.Add(filePath, sprite);
                    return sprite;
                }
            } else if (ext == ".gif") {
                byte[] data = File.ReadAllBytes(filePath);
                using (var decoder = new MG.GIF.Decoder(data)) {
                    Texture2D tex2D;
                    var img = decoder.NextImage();
                    if (img == null)
                        return null;

                    var gif = new Gif();
                    Debug.Log($"Add gif {name}");
                    mapGif.Add(name, gif);
                    while (img != null) {
                        tex2D = img.CreateTexture();
                        tex2D.name = name;

                        // Set pivot to the center (0.5, 0.5)
                        SpriteDesc desc = SpriteDesc._default;
                        Sprite sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f), desc.pixelsPerUnit, desc.extrude, desc.spriteType);
                        sprite.name = tex2D.name;
                        gif.frames.Add(sprite);
                        gif.delay.Add(img.Delay * 0.001f);
                        img = decoder.NextImage();
                    }
                    cacheSprite.Add(filePath, gif.frames[0]);
                    return gif.frames[0];
                }
            }
            return null;
        }


        static void GifUpdate(Type t) {
            UnityEngine.Object[] renderers = UnityEngine.Object.FindObjectsOfType(t);
            foreach (var renderer in renderers) {
                var traverse = Traverse.Create(renderer).Property("sprite");
                if (traverse == null)
                    continue;
                var sprite = traverse.GetValue<Sprite>();
                if (sprite == null || string.IsNullOrEmpty(sprite.name))
                    continue;
                if (mapGif.TryGetValue(sprite.name, out Gif gif))
                    gif.Update(traverse);
            }
        }

        public void testHook() {
            SkinMod.Instance.onUpdate += OnUpdate;
        }

        public void OnUpdate() {

            if (mapGif.Count > 0) {
                timeSinceLastUpdate += Time.deltaTime;

                if (timeSinceLastUpdate >= updateInterval) {
                    testGif.GifUpdate(typeof(SpriteRenderer));
                    timeSinceLastUpdate = 0f; // Reset timer
                }
            }

            //if (mapGif.Count > 0) {
            //    testGif.GifUpdate(typeof(SpriteRenderer));
            //    //foreach (KeyValuePair<string, Gif> kvp in mapGif) {
            //    //    ToastManager.Toast(kvp); // Use kvp.Value to access the Gif
            //    //}
            //}
        }

        public void clear() {
            cacheSprite.Clear();
            mapGif.Clear();
        }

        public void setSpeed(float speedMultiplier) {
            updateInterval = baseInterval / Mathf.Abs(speedMultiplier);

        }
    }
}