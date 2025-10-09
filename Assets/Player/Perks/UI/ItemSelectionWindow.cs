using System;
using System.Collections.Generic;
using Game.Common;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Game.Upgrade.Perks;
using Player.General_UI;
using Player.Networking;
using UnityEngine;

namespace Player.Perks.UI
{
    public class ItemSelectionWindow : PNetworkBehaviour
    {
        [SerializeField] private PCanvas canvas;
        private const string ItemSelectionLayoutTag = "ItemSelectionLayout";
        private const string CardGroupTag = "CardDisplayGroup";
        private Transform _layout;
        private CanvasGroup _cardGroup;
        [SerializeField] private GameObject cardPrefab;
        private List<ItemCard> _cards = new();
        
        public static event Action <ushort> OnItemChosen;

        protected override void StartOnlineOwner()
        {
            _layout = PCanvas.CanvasObjects[ItemSelectionLayoutTag].transform;
            _cardGroup = PCanvas.CanvasObjects[CardGroupTag].GetComponent<CanvasGroup>();
            ItemSelectionManager.OnStartChooseItem += CreateAndShowItemCards;
            GameLoopEvents.OnRoundStateChangedAll += OnRoundStateChanged;
        }

        protected override void DisableOnlineOwner()
        {
            if (GameManager.Instance == null) return;
            ItemSelectionManager.OnStartChooseItem -= CreateAndShowItemCards;
            GameLoopEvents.OnRoundStateChangedAll -= OnRoundStateChanged;
        }
        private void OnRoundStateChanged(GameRoundState state, float serverTime)
        {
            if (state == GameRoundState.Upgrade) return; // if still upgrading, return :)    pourquoi t'es si heureux fdp
            HideCards();
        }
    
        public void CreateAndShowItemCards(ushort[] itemsInd)
        {
            if (itemsInd == null)
            {
                Debug.Log("No perks to choose from. Skipping item choice.");

                GameManager.Instance.ItemSelectionManager.ChooseItemClient(0);
                OnItemChosen?.Invoke(0);
                
                return;
            }
            
            Item[] items = new Item[itemsInd.Length];
            for (int i = 0; i < itemsInd.Length; i++)
                items[i] = ItemRegistry.Instance.GetItem(itemsInd[i]);
            CreateAndShowPerkCards(items);
        }
    
        public void CreateAndShowPerkCards(Item[] items)
        {
            if (_cards.Count < items.Length)
            {
                for (int i = _cards.Count; i < items.Length; i++)
                {
                    GameObject perkCard = Instantiate(cardPrefab, _layout);
                    _cards.Add(perkCard.GetComponent<ItemCard>());
                }
            }
            else if (_cards.Count > items.Length)
            {
                for (int i = _cards.Count - 1; i >= items.Length; i--)
                {
                    Destroy(_cards[i].gameObject);
                    _cards.RemoveAt(i);
                }
            }
        
            for (int i = 0; i < items.Length; i++)
            {
                SetCardInfo(_cards[i], items[i], (ushort)i);
            }
        
            ShowCards();
        }
        public void ShowCards()
        {
            foreach (ItemCard card in _cards)
                card.gameObject.SetActive(true);
            
            _cardGroup.alpha = 1;
            _cardGroup.interactable = true;
            _cardGroup.blocksRaycasts = true;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public void HideCards()
        {
            foreach (ItemCard card in _cards)
                card.gameObject.SetActive(false);
            
            _cardGroup.alpha = 0;
            _cardGroup.interactable = false;
            _cardGroup.blocksRaycasts = false;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    
        private void SetCardInfo(ItemCard itemCard, Item item, ushort perkIndex)
        {
            itemCard.SetItem(item,PerkChosen, perkIndex);
        }

        private void PerkChosen(ushort index)
        {
            HideCards();
            GameManager.Instance.ItemSelectionManager.ChooseItemClient(index);
            OnItemChosen?.Invoke(index);
        }
    }
}