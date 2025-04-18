using Unity.Netcode;

public static class CachedOwnerClientData
{
    public static ScriptableUpgrade[] upgrades { get; private set; }
    public static ushort score { get; private set; }

    public static void UpdateCachedScore()
    {
        if (GameManager.Instance == null) return;
        ulong id = NetworkManager.Singleton.LocalClientId;
        PlayerData data = GameManager.Instance.gameData.playerGameData.Value.GetDataOrDefault(id);
        if (data.ClientId == ulong.MaxValue) return;
        score = data.InGameData.score;
    }
    
    public static void UpdateCachedUpgrades()
    {
        if (GameManager.Instance == null) return;
        ulong id = NetworkManager.Singleton.LocalClientId;
        PlayerData data = GameManager.Instance.gameData.playerGameData.Value.GetDataOrDefault(id);
        if (data.ClientId == ulong.MaxValue) return;
        upgrades = data.InGameData.GetUpgrades();
    }

    public static void SetUpgrades(ScriptableUpgrade[] newCachedUpgrades)
    {
        upgrades = newCachedUpgrades;
    }
    public static void SetScore(ushort newCachedScore)
    {
        score = newCachedScore;
    }
}