using System.Collections.Generic;
using UnityEngine;

public class PUpgradesChoice : PNetworkBehaviour
{
    [SerializeField] private PCanvas canvas;
    [SerializeField] private GameObject upgradesListLayout;
    private GameObject _upgradesList;
    [SerializeField] private GameObject upgradeCardPrefab;
    private List<UpgradeCard> _upgradeCards = new();

    protected override void StartOnlineOwner()
    {
        SetupUpgradesList();
        GameManager.Instance.upgradesManager.OnUpgradeChoices += DisplayUpgrades;
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameManager.GameState state, GameManager.GameStateCallbackType type)
    {
        if (state != GameManager.GameState.ChoosingUpgrade) return;
        if (type != GameManager.GameStateCallbackType.StateEnded) return;
        HideUpgrades();
    }

    private void SetupUpgradesList()
    {
        _upgradesList = Instantiate(upgradesListLayout, canvas.transform);
    }
    
    public void DisplayUpgrades(ushort[] upgradesIndex)
    {
        ScriptableUpgrade[] upgrades = new ScriptableUpgrade[upgradesIndex.Length];
        for (int i = 0; i < upgradesIndex.Length; i++)
            upgrades[i] = GameManager.Instance.upgradesManager.GetUpgrade(upgradesIndex[i]);
        DisplayUpgrades(upgrades);
    }
    
    public void DisplayUpgrades(ScriptableUpgrade[] upgrades)
    {
        if (_upgradeCards.Count < upgrades.Length)
        {
            for (int i = _upgradeCards.Count; i < upgrades.Length; i++)
            {
                GameObject upgradeCard = Instantiate(upgradeCardPrefab, _upgradesList.transform);
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
            SetUpgradeCardInfo(_upgradeCards[i], upgrades[i]);
        
        ShowUpgrades();
    }
    public void ShowUpgrades()
    {
        foreach (UpgradeCard upgradeCard in _upgradeCards)
            upgradeCard.gameObject.SetActive(true);
    }
    public void HideUpgrades()
    {
        foreach (UpgradeCard upgradeCard in _upgradeCards)
            upgradeCard.gameObject.SetActive(false);
    }
    
    private void SetUpgradeCardInfo(UpgradeCard upgradeCard, ScriptableUpgrade upgrade)
    {
        upgradeCard.SetUpgrade(upgrade);
    }
}