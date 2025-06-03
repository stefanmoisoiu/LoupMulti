using Game.Game_Loop.Round.Tag.Hot_Potato;
using Player.Networking;
using UnityEngine;

namespace Player.UI
{
    public class HotPotato : PNetworkBehaviour
    {
        private const string HotPoatatoTextTag = "HotPotatoText";
        private CanvasGroup hotPotato;
        protected override void StartOnlineOwner()
        {
            HotPotatoTarget.OnTargetChanged += UpdateHotPotatoUI;
        }

        protected override void DisableAnyOwner()
        {
            HotPotatoTarget.OnTargetChanged -= UpdateHotPotatoUI;
            SetHotPotatoUI(false);
        }
    
        private void UpdateHotPotatoUI(ulong newTarget)
        {
            SetHotPotatoUI(newTarget == NetworkManager.LocalClientId);
        }
    
        private void SetHotPotatoUI(bool isActive)
        {
            if (hotPotato == null)
            {
                try
                {
                    hotPotato = PCanvas.CanvasObjects[HotPoatatoTextTag].GetComponent<CanvasGroup>();
                }
                catch
                {
                    // ignored
                }
            }
        
            if (hotPotato != null)
                hotPotato.alpha = isActive ? 1 : 0;
        }
    }
}