using NineSolsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkinMod {
    public class bullet :MonoBehaviour
    {
        void Start() {
            //ToastManager.Toast("Start");
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

        void OnTriggerEnter2D (Collider2D collision) {
            if (collision.tag == "Enemy" && collision.GetComponentInParent<MonsterBase>()) {
                //ToastManager.Toast("Enter");
                MonsterBase monsterbase = collision.GetComponentInParent<MonsterBase>();
                monsterbase.DecreasePostureByDOT(500);
                Destroy(gameObject);
            }
        }

        void OnTriggerStay2D(Collider2D collision) {
            
            if(collision.tag == "Enemy" && collision.GetComponentInParent<MonsterBase>()) {
                //ToastManager.Toast($"{collision.GetComponentInParent<MonsterBase>().gameObject}");
                //MonsterBase monsterbase = collision.GetComponentInParent<MonsterBase>();
                //ToastManager.Toast(monsterbase.postureSystem.CurrentHealthValue);
                //monsterbase.postureSystem.CurrentHealthValue -= 1;
                //monsterbase.DecreasePostureByDOT(1);
            }
        }

    }
}
