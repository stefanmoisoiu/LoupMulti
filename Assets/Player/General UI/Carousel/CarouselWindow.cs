using System;
using System.Collections.Generic;
using Game.Common;
using Game.Data; // Ajouté pour DataManager et ResourceType
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Game.Upgrade.Carousel;
using Player.General_UI;
using Player.Networking;
using TMPro;
using Unity.Netcode; // Ajouté pour NetworkManager
using UnityEngine;
using UnityEngine.UI;

namespace Player.Perks.UI
{
    public class CarouselWindow : PNetworkBehaviour
    {
        private const string CarouselLayoutTag = "CarouselLayout";
        private const string CarouselTag = "Carousel";
        private const string UseRerollTag = "UseReroll";
        private const string RemainingRerollsTag = "RemainingRerolls";
        private const string BuyRerollTag = "BuyReroll";
        private const string RerollPriceTag = "RerollPrice";
        
        private Transform _layout;
        private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject cardPrefab;
        private List<CarouselCardUI> _cards = new();

        private Button _useRerollButton; // Renommé
        private TMP_Text _rerollCountText;
        private Button _buyRerollButton; // Renommé
        private TMP_Text _rerollPriceText;

        private int _currentRerollsAvailable = 0; // Pour gérer l'interactabilité

        protected override void StartOnlineOwner()
        {
            _layout = PCanvas.CanvasObjects[CarouselLayoutTag].transform;
            _canvasGroup = PCanvas.CanvasObjects[CarouselTag].GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _useRerollButton = PCanvas.CanvasObjects[UseRerollTag].GetComponent<Button>();
            _rerollCountText = PCanvas.CanvasObjects[RemainingRerollsTag].GetComponent<TMP_Text>();
            _buyRerollButton = PCanvas.CanvasObjects[BuyRerollTag].GetComponent<Button>();
            _rerollPriceText = PCanvas.CanvasObjects[RerollPriceTag].GetComponent<TMP_Text>();
            _rerollPriceText.text = GameSettings.Instance.RareResourceRerollCost.ToString();

            CarouselManager.OnShowCarouselOptionsOwner += CreateAndShowItemCards;
            GameLoopEvents.OnRoundStateChangedAll += OnRoundStateChanged;

            _useRerollButton.onClick.AddListener(RequestUseReroll); // Mis à jour
            _buyRerollButton.onClick.AddListener(RequestBuyAndUseReroll); // Mis à jour

            HideCards();
        }

        protected override void DisableOnlineOwner()
        {
            CarouselManager.OnShowCarouselOptionsOwner -= CreateAndShowItemCards;
            GameLoopEvents.OnRoundStateChangedAll -= OnRoundStateChanged;

            _useRerollButton.onClick.RemoveListener(RequestUseReroll); // Mis à jour
            _buyRerollButton.onClick.RemoveListener(RequestBuyAndUseReroll); // Mis à jour
            
            CursorManager.Instance.ReleaseCursorUnlock(this);
        }

        private void OnRoundStateChanged(GameRoundState state, float serverTime)
        {
            if (state != GameRoundState.Upgrade)
            {
                 HideCards();
            }
        }

        public void CreateAndShowItemCards(CarouselOption[] options, int currentRerolls)
        {
            _currentRerollsAvailable = currentRerolls; // Stocke la valeur reçue

            if (options == null || options.Length == 0)
            {
                 if (GameManager.Instance != null && GameManager.Instance.CarouselManager != null)
                     GameManager.Instance.CarouselManager.ClientChoseItemServerRpc(0);
                 HideCards();
                 return;
            }

            while (_cards.Count < options.Length)
            {
                GameObject perkCard = Instantiate(cardPrefab, _layout);
                _cards.Add(perkCard.GetComponent<CarouselCardUI>());
            }
            while (_cards.Count > options.Length)
            {
                int lastIndex = _cards.Count - 1;
                Destroy(_cards[lastIndex].gameObject);
                _cards.RemoveAt(lastIndex);
            }

            for (ushort i = 0; i < options.Length; i++)
            {
                SetCardInfo(_cards[i], options[i], i);
            }

            _rerollCountText.text = $"Rerolls: {currentRerolls}";
            // Met à jour l'interactabilité immédiatement
            SetRerollButtonsInteractable(true);

            ShowCards();
        }

        public void ShowCards()
        {
            foreach (CarouselCardUI card in _cards)
                card.gameObject.SetActive(true);

            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            CursorManager.Instance.RequestCursorUnlock(this);
            SetRerollButtonsInteractable(true);
        }

        public void HideCards()
        {
            foreach (CarouselCardUI card in _cards)
                if(card != null && card.gameObject != null) card.gameObject.SetActive(false); // Ajout de vérifs null

            if (_canvasGroup == null) return;
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            CursorManager.Instance.ReleaseCursorUnlock(this);
        }

        private void SetCardInfo(CarouselCardUI carouselCardUI, CarouselOption option, ushort choiceIndex)
        {
            carouselCardUI.SetOption(option, CardChosenCallback, choiceIndex);
        }

        private void CardChosenCallback(ushort choiceIndex)
        {
            HideCards();
             if (GameManager.Instance != null && GameManager.Instance.CarouselManager != null)
                 GameManager.Instance.CarouselManager.ClientChoseItemServerRpc(choiceIndex);
        }

        private void RequestUseReroll()
        {
             SetRerollButtonsInteractable(false);
             if (GameManager.Instance != null && GameManager.Instance.CarouselManager != null)
                 GameManager.Instance.CarouselManager.RequestRerollServerRpc(false);
        }

        private void RequestBuyAndUseReroll()
        {
             SetRerollButtonsInteractable(false);
             if (GameManager.Instance != null && GameManager.Instance.CarouselManager != null)
                  GameManager.Instance.CarouselManager.RequestRerollServerRpc(true);
        }

        private void SetRerollButtonsInteractable(bool interactable)
        {
             if (interactable)
             {
                  _useRerollButton.interactable = _currentRerollsAvailable > 0;
                  bool canAfford = false;
                  // Vérifie si DataManager et le joueur local existent
                  if (DataManager.Instance != null && NetworkManager.Singleton != null && DataManager.Instance.TryGetValue(NetworkManager.Singleton.LocalClientId, out PlayerData data))
                  {
                       canAfford = data.inGameData.resources.HasEnough(ResourceType.Rare, GameSettings.Instance.RareResourceRerollCost);
                  }
                  _buyRerollButton.interactable = canAfford;
             } else {
                 _useRerollButton.interactable = false;
                 _buyRerollButton.interactable = false;
             }
        }
    }
}