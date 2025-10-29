using System;
using Game.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Perks.UI
{
    public class CarouselCardUI : MonoBehaviour
    {
        public Image icon;
        public TMP_Text itemName;
        public TMP_Text description;
        public Button button;

        private CarouselOption _option;
    
        public void SetOption(CarouselOption option, Action<ushort> callback, ushort choiceIndex)
        {
            _option = option;
            Item item = ItemRegistry.Instance.GetItem(option.ItemRegistryIndex);
            
            icon.sprite = item.Info.Icon;
            itemName.text = item.Info.Name;
            description.text = option.Type == CarouselOption.OptionType.UpgradeItem ? $"LVL [{option.CurrentLevel+1}] -> LVL [{option.CurrentLevel+2}]" : item.Info.Description;

            //bgMat.SetFloat(RarityShaderID, (int)perkData.Rarity);
        
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => callback(choiceIndex));
        }
    }
}