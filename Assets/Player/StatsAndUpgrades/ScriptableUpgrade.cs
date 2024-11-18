using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrade")]
public class ScriptableUpgrade : ScriptableObject
{
    public StatData BaseStatData;
    
    public virtual StatData GetStatData()
    {
        return BaseStatData;
    }
}