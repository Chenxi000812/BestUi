using System;
using HarmonyLib;
using Sons.Gameplay.GPS;
using Sons.Inventory;
using Sons.Items.Core;
using Sons.Weapon;
using TheForest.Items.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Types = Sons.Items.Core.Types;

namespace BestUi.Patch
{
    public class PlayerInventoryPatch
    {
        public static GameObject panel;
        public static TextMeshProUGUI TammoText;
        public static RawImage RammoIcon;
        public static PlayerInventory CurrentPlayerInventory;
        public static RangedWeapon CurrentRagedWeapon;
        public static ItemInstance CurrentEquidInstance;
        public static int preAmmoType = -1;
        [HarmonyPatch(typeof(PlayerInventory), "Start")]
        [HarmonyPostfix]
        public static void StartPostfix(ref PlayerInventory __instance)
        {
            CurrentPlayerInventory = __instance;
        }
        [HarmonyPatch(typeof(PlayerInventory), "OnDestroy")]
        [HarmonyPostfix]
        public static void OnDestroyPostfix(ref PlayerInventory __instance)
        {
            CurrentPlayerInventory = null;
        }
        
        
        [HarmonyPatch(typeof(RangedWeapon), "Awake")] //转装备时会调用
        [HarmonyPrefix]
        public static void RangedWeaponAwakePrefix(ref RangedWeapon __instance)
        {
            if (CurrentEquidInstance == null || __instance._weaponItemId != CurrentEquidInstance._itemID)return;
            panel.SetActive(true);
            CurrentRagedWeapon = __instance;
            refresh();
        }
        
        [HarmonyPatch(typeof(RangedWeapon), "Start")] //转装备时会调用
        [HarmonyPostfix]
        public static void RangedWeaponStartPostfix(ref RangedWeapon __instance)
        {
            refresh();
        }
        
        [HarmonyPatch(typeof(RangedWeapon), "OnDestroy")]
        [HarmonyPrefix]
        public static void RangedWeaponOnDestroyPostfix(ref RangedWeapon __instance)
        {
            if (CurrentRagedWeapon != null && CurrentRagedWeapon.GetInstanceID() == __instance.GetInstanceID())
            {
                panel.SetActive(false);
                CurrentRagedWeapon = null;
                // CurrentEquidInstance = null;
            }


        }
        
        [HarmonyPatch(typeof(RangedWeaponController), "Reload")]//换完子弹
        [HarmonyPostfix]
        public static void ReloadPostfix(ref RangedWeaponController __instance)
        {
            refresh();
        }
        
        [HarmonyPatch(typeof(LayoutItem),"OnMouseEnter")]
        [HarmonyPostfix]
        public static void OnMouseEnterPostfix(ref LayoutItem __instance)
        {
            Entrance.LOG.LogWarning("已选中物品"+__instance.ItemInstance.Data.Name+" 物品id:"+__instance.ItemInstance._itemID+" 类型:"+__instance.ItemInstance.Data._type);
            Entrance.LOG.LogWarning(__instance.ItemInstance.Data._rangedStyle);
        }
        
        //射击和切换弹药  换武器 都会触发
        [HarmonyPatch(typeof(RangedWeapon.Ammo),"SetItemType")]
        [HarmonyPostfix]
        public static void SetItemTypePostfix(ref int ammoType)
        {
            
            //当类型一样就不调用
            if (CurrentRagedWeapon==null||preAmmoType == ammoType) return;
            preAmmoType = ammoType;

            //不一样时说明 切换弹药了
            refresh();
            
        }
        
        [HarmonyPatch(typeof(RangedWeapon.Ammo),"AddAmmo")]
        [HarmonyPostfix]
        public static void AddAmmoPostfix()
        {
            refresh();
        }
        [HarmonyPatch(typeof(RangedWeapon.Ammo),"Remove")]
        [HarmonyPostfix]
        public static void RemovePostfix()
        {
            refresh();
        }
        
        [HarmonyPatch(typeof(PlayerInventory),"AddItem")]//捡起弓箭刷新
        [HarmonyPostfix]
        public static void AddItemfix()
        {
            refresh();
        }

        [HarmonyPatch(typeof(GPSTrackerSystem), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnablePostfix(ref GPSTrackerSystem __instance)
        {
            if (panel==null)init();
            
        }
        [HarmonyPatch(typeof(PlayerInventory),"TryEquip",new Type[]{typeof(ItemInstance),typeof(bool),typeof(bool)})]//捡起弓箭刷新
        [HarmonyPostfix]
        public static void TryEquipPostfix(ref ItemInstance itemInstance)
        {
            if (itemInstance.Data.HasType(Types.RangedWeapon))
            {
                CurrentEquidInstance = itemInstance;

            }

        }
            public static void refresh()
        {
            if (CurrentPlayerInventory == null|| panel == null|| CurrentRagedWeapon ==null) return;
            int remainingAmmoCount = CurrentRagedWeapon.GetAmmo().GetRemainingAmmo();  //弹夹内剩余的子弹
            
            int ammoType = CurrentRagedWeapon.GetAmmo()._type;
            if (ammoType == 389) ammoType = 388; //燃烧弹和手雷特殊处理
            if (ammoType == 382) ammoType = 381;
            bool throwAble = CurrentRagedWeapon._weaponItemId==ammoType; //武器和备弹id相同说明是投掷类
            ItemInstance itemInstance = CurrentPlayerInventory.GetItemInstanceOfType(ammoType);  //当前子弹实例为了获取图标
            int totalAmmoCount = CurrentPlayerInventory.AmountOf(ammoType,throwAble);  //背包里剩余的子弹
            
            TammoText.SetText(throwAble?totalAmmoCount.ToString():(remainingAmmoCount+"/"+totalAmmoCount));
            RammoIcon.texture = itemInstance.Data.UiData._icon;
        }

        public static void init()
        {
            GameObject canvas = GameObject.Find("ToggleableHUD");
            panel = new GameObject("AnmoUI");
            panel.transform.SetParent(canvas.transform);
            panel.transform.localPosition = new Vector3(0f ,-460f, 0);


            GameObject ammoIcon = new GameObject("amIcon");
            RammoIcon = ammoIcon.AddComponent<RawImage>();
            ammoIcon.transform.SetParent(panel.transform);
            RammoIcon.rectTransform.localPosition = new Vector3(-90f, 0f ,0f);

            RammoIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,100f);
            RammoIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,100f);

            
            GameObject ammoText = new GameObject("am1");
            TammoText= ammoText.gameObject.AddComponent<TextMeshProUGUI>();
            TammoText.SetText("25/100");
            TammoText.fontSize = 36;
            ammoText.transform.SetParent(panel.transform);
            TammoText.rectTransform.localPosition = new Vector3(90f, 0f ,0f);
            
            refresh();
            panel.SetActive(false);
            // t.rectTransform.pivot = new Vector2(0, 1);
        }
    }
}