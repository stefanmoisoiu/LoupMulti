using System;
using Game.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.UI.Perks
{
    public class PerkCard : MonoBehaviour
    {
        public Image icon;
        public TMP_Text perkName;
        public TMP_Text description;
        public Button button;
        
        private PerkData _perkData;
    
        public void SetPerk(PerkData perkData, Action<ushort> callback, ushort perkIndex)
        {
            _perkData = perkData;
            
            icon.sprite = perkData.PerkIcon;
            perkName.text = perkData.PerkName;
            description.text = perkData.PerkDescription;

            //bgMat.SetFloat(RarityShaderID, (int)perkData.Rarity);
        
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => callback(perkIndex));
        }
    }
}