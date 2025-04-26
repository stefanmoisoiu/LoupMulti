using System;
using Game.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.UI.Upgrades
{
    public class Card : MonoBehaviour
    {
        public Image icon;
        public TMP_Text upgradeName;
        public TMP_Text description;
        public TMP_Text type;
        public Button button;
    
        public void SetUpgrade(ScriptableUpgrade upgrade, Action<ushort> callback, ushort upgradeIndex)
        {
            icon.sprite = upgrade.Icon;
            upgradeName.text = upgrade.UpgradeName;
            description.text = upgrade.UpgradeDescription;
            type.text = upgrade.Type.ToString();
        
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => callback(upgradeIndex));
        }
    }
}