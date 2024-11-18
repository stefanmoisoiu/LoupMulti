using UnityEngine;

public class GlobalGameData : MonoBehaviour
{
    public static GlobalGameData Instance { get; private set; }
    public ScriptableUpgrade[] upgrades;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
}