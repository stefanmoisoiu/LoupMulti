using Unity.Netcode;
using UnityEngine;

public class NetcodeLogger : NetworkBehaviour
{
    public static NetcodeLogger Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    
    [Rpc(SendTo.Everyone)]
    public void LogRpc(string message, ColorType type, AddedEffects[] effects = null)
    {
        string color = GetHexColor(type);
        string effect = "";
        if (effects != null)
            foreach (AddedEffects addedEffect in effects)
                switch (addedEffect)
                {
                    case AddedEffects.Bold: effect += "<b>"; break;
                    case AddedEffects.Italic: effect += "<i>"; break;
                }
        
        Debug.Log($"<{color}>{effect}{message}");
    }

    public enum AddedEffects
    {
        Bold,
        Italic,
    }
    public enum ColorType
    {
        Green,
        Blue,
        Purple,
        Orange,
        Yellow,
        Red,
    }
    public string GetHexColor(ColorType type)
    {
        string color = "#29b929";
        switch (type)
        {
            case ColorType.Green:
                color = "#29b929";
                break;
            case ColorType.Blue:
                color = "#80eeee";
                break;
            case ColorType.Purple:
                color = "#ff00ff";
                break;
            case ColorType.Orange:
                color = "#ff9900";
                break;
            case ColorType.Yellow:
                color = "#ffcc00";
                break;
            case ColorType.Red:
                color = "#ff0000";
                break;
        }
        return color;
    }
}