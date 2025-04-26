using System.Collections.Generic;
using Game.Game_Loop;
using Game.Game_Loop.Round;
using Game.Manager;
using Game.Upgrades;
using Player.Networking;
using UnityEngine;

namespace Player.UI.Upgrades
{
    public class MainWindow : PNetworkBehaviour
    {
        [SerializeField] private PCanvas canvas;
        private GameObject _upgradesListLayout;
        private const string UpgradesListLayoutTag = "UpgradesList";
        private Transform _upgradesList;
        [SerializeField] private GameObject upgradeCardPrefab;
        private List<Card> _upgradeCards = new();

        protected override void StartOnlineOwner()
        {
            _upgradesList = GameObject.FindGameObjectWithTag(UpgradesListLayoutTag).transform;
            UpgradesManager.OnUpgradeChoicesAvailable += DisplayUpgrades;
            GameLoopEvents.OnRoundStateChangedAll += TryHideUpgrades;
        }

        protected override void DisableOnlineOwner()
        {
            if (GameManager.Instance == null) return;
            UpgradesManager.OnUpgradeChoicesAvailable -= DisplayUpgrades;
            GameLoopEvents.OnRoundStateChangedAll -= TryHideUpgrades;
        }
        private void TryHideUpgrades(GameRoundState state, float serverTime)
        {
            if (state == GameRoundState.ChoosingUpgrade) return; // if still upgrading, return :)
            HideUpgrades();
        }
    
        public void DisplayUpgrades(ushort[] upgradesIndex)
        {
            ScriptableUpgrade[] upgrades = new ScriptableUpgrade[upgradesIndex.Length];
            for (int i = 0; i < upgradesIndex.Length; i++)
                upgrades[i] = GameManager.Instance.UpgradesManager.GetUpgrade(upgradesIndex[i]);
            DisplayUpgrades(upgrades);
        }
    
        public void DisplayUpgrades(ScriptableUpgrade[] upgrades)
        {
            if (_upgradeCards.Count < upgrades.Length)
            {
                for (int i = _upgradeCards.Count; i < upgrades.Length; i++)
                {
                    GameObject upgradeCard = Instantiate(upgradeCardPrefab, _upgradesList);
                    _upgradeCards.Add(upgradeCard.GetComponent<Card>());
                }
            }
            else if (_upgradeCards.Count > upgrades.Length)
            {
                for (int i = _upgradeCards.Count - 1; i >= upgrades.Length; i--)
                {
                    Destroy(_upgradeCards[i].gameObject);
                    _upgradeCards.RemoveAt(i);
                }
            }
        
            for (int i = 0; i < upgrades.Length; i++)
                SetUpgradeCardInfo(_upgradeCards[i], upgrades[i],(ushort)i);
        
            ShowUpgrades();
        }
        public void ShowUpgrades()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            foreach (Card upgradeCard in _upgradeCards)
                upgradeCard.gameObject.SetActive(true);
        }
        public void HideUpgrades()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            foreach (Card upgradeCard in _upgradeCards)
                upgradeCard.gameObject.SetActive(false);
        }
    
        private void SetUpgradeCardInfo(Card card, ScriptableUpgrade upgrade, ushort upgradeIndex)
        {
            card.SetUpgrade(upgrade,UpgradeChosen, upgradeIndex);
        }

        private void UpgradeChosen(ushort index)
        {
            HideUpgrades();
            GameManager.Instance.UpgradesManager.ChooseUpgradeClient(index);
        }
    }
}