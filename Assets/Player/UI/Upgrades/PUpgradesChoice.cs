using System.Collections.Generic;
using UnityEngine;

public class PUpgradesChoice : PNetworkBehaviour
{
    [SerializeField] private PCanvas canvas;
    private GameObject _upgradesListLayout;
    private const string UpgradesListLayoutTag = "UpgradesList";
    private Transform _upgradesList;
    [SerializeField] private GameObject upgradeCardPrefab;
    private List<UpgradeCard> _upgradeCards = new();

    protected override void StartOnlineOwner()
    {
        _upgradesList = GameObject.FindGameObjectWithTag(UpgradesListLayoutTag).transform;
        UpgradesManager.OnUpgradeChoicesAvailable += DisplayUpgrades;
        GameLoop.OnRoundStateChanged += TryHideUpgrades;
    }

    protected override void DisableOnlineOwner()
    {
        if (GameManager.Instance == null) return;
        UpgradesManager.OnUpgradeChoicesAvailable -= DisplayUpgrades;
        GameLoop.OnRoundStateChanged -= TryHideUpgrades;
    }
    private void TryHideUpgrades(GameLoop.RoundState state, float serverTime)
    {
        if (state == GameLoop.RoundState.ChoosingUpgrade) return; // if still upgrading, return :)
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
                _upgradeCards.Add(upgradeCard.GetComponent<UpgradeCard>());
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
        foreach (UpgradeCard upgradeCard in _upgradeCards)
            upgradeCard.gameObject.SetActive(true);
    }
    public void HideUpgrades()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        foreach (UpgradeCard upgradeCard in _upgradeCards)
            upgradeCard.gameObject.SetActive(false);
    }
    
    private void SetUpgradeCardInfo(UpgradeCard upgradeCard, ScriptableUpgrade upgrade, ushort upgradeIndex)
    {
        upgradeCard.SetUpgrade(upgrade,UpgradeChosen, upgradeIndex);
    }

    private void UpgradeChosen(ushort index)
    {
        HideUpgrades();
        GameManager.Instance.UpgradesManager.ChooseUpgradeClient(index);
    }
}