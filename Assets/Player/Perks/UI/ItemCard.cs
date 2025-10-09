using System;
using Game.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Perks.UI
{
    public class ItemCard : MonoBehaviour
    {
        public Image icon;
        public TMP_Text itemName;
        public TMP_Text description;
        public Button button;
        
        private Item _item;
    
        public void SetItem(Item itemData, Action<ushort> callback, ushort itemIndex)
        {
            _item = itemData;
            
            icon.sprite = _item.Info.Icon;
            itemName.text = _item.Info.Name;
            description.text = _item.Info.Description;

            //bgMat.SetFloat(RarityShaderID, (int)perkData.Rarity);
        
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => callback(itemIndex));
        }
    }
}