using UnityEngine;
using Game.Common; // Pour ResourceType

namespace Player.General_UI.Tooltips
{
    public interface IInfoTooltipDataProvider
    {
        string GetHeaderText();
        string GetDescriptionText();
        
        // --- Optionnels ---
        
        Sprite GetMainIcon();
        
        bool ShouldShowPrice(out int price, out ResourceType resourceType);
        
        Color GetHeaderColor();
    }
}