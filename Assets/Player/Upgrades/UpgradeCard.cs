using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour
{
    public Image icon;
    public TMP_Text upgradeName;
    public TMP_Text description;
    public TMP_Text type;
    
    public void SetUpgrade(ScriptableUpgrade upgrade)
    {
        icon.sprite = upgrade.Icon;
        upgradeName.text = upgrade.UpgradeName;
        description.text = upgrade.UpgradeDescription;
        type.text = upgrade.Type.ToString();
    }
}