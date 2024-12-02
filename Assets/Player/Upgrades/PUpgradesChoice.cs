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
    }

    private void SetupUpgradesList()
    {
        _upgradesList = Instantiate(upgradesListLayout, canvas.transform);
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
    }
    
    private void SetUpgradeCardInfo(UpgradeCard upgradeCard, ScriptableUpgrade upgrade)
    {
        upgradeCard.SetUpgrade(upgrade);
    }
}