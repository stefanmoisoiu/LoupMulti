using System;
using System.Collections.Generic;
using Game.Common;
using Game.Common.List;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Game.Upgrade.Perks;
using Player.Networking;
using UnityEngine;

namespace Player.UI.Perks
{
    public class PerkChoiceWindow : PNetworkBehaviour
    {
        [SerializeField] private PCanvas canvas;
        private const string PerkDisplayLayoutTag = "PerkDisplayLayout";
        private const string PerkDisplayGroupTag = "PerkDisplayGroup";
        private Transform _perksLayout;
        private CanvasGroup _perksListGroup;
        [SerializeField] private GameObject perkCardPrefab;
        private List<PerkCard> _perkCards = new();
        
        public static event Action <ushort> OnPerkChosen;

        protected override void StartOnlineOwner()
        {
            _perksLayout = PCanvas.CanvasObjects[PerkDisplayLayoutTag].transform;
            _perksListGroup = PCanvas.CanvasObjects[PerkDisplayGroupTag].GetComponent<CanvasGroup>();
            PerkSelectionManager.OnStartChoosePerks += DisplayPerks;
            GameLoopEvents.OnRoundStateChangedAll += TryHidePerks;
        }

        protected override void DisableOnlineOwner()
        {
            if (GameManager.Instance == null) return;
            PerkSelectionManager.OnStartChoosePerks -= DisplayPerks;
            GameLoopEvents.OnRoundStateChangedAll -= TryHidePerks;
        }
        private void TryHidePerks(GameRoundState state, float serverTime)
        {
            if (state == GameRoundState.Upgrade) return; // if still upgrading, return :)
            HidePerks();
        }
    
        public void DisplayPerks(ushort[] perksIndex)
        {
            PerkData[] perks = new PerkData[perksIndex.Length];
            for (int i = 0; i < perksIndex.Length; i++)
                perks[i] = PerkList.Instance.GetPerk(perksIndex[i]);
            DisplayPerks(perks);
        }
    
        public void DisplayPerks(PerkData[] perks)
        {
            if (_perkCards.Count < perks.Length)
            {
                for (int i = _perkCards.Count; i < perks.Length; i++)
                {
                    GameObject perkCard = Instantiate(perkCardPrefab, _perksLayout);
                    _perkCards.Add(perkCard.GetComponent<PerkCard>());
                }
            }
            else if (_perkCards.Count > perks.Length)
            {
                for (int i = _perkCards.Count - 1; i >= perks.Length; i--)
                {
                    Destroy(_perkCards[i].gameObject);
                    _perkCards.RemoveAt(i);
                }
            }
        
            for (int i = 0; i < perks.Length; i++)
            {
                SetPerkCardInfo(_perkCards[i], perks[i], (ushort)i);
            }
        
            ShowPerks();
        }
        public void ShowPerks()
        {
            foreach (PerkCard card in _perkCards)
                card.gameObject.SetActive(true);
            
            _perksListGroup.alpha = 1;
            _perksListGroup.interactable = true;
            _perksListGroup.blocksRaycasts = true;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public void HidePerks()
        {
            foreach (PerkCard card in _perkCards)
                card.gameObject.SetActive(false);
            
            _perksListGroup.alpha = 0;
            _perksListGroup.interactable = false;
            _perksListGroup.blocksRaycasts = false;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    
        private void SetPerkCardInfo(PerkCard perkCard, PerkData perkData, ushort perkIndex)
        {
            perkCard.SetPerk(perkData,PerkChosen, perkIndex);
        }

        private void PerkChosen(ushort index)
        {
            HidePerks();
            GameManager.Instance.PerkSelectionManager.ChoosePerkClient(index);
            OnPerkChosen?.Invoke(index);
        }
    }
}