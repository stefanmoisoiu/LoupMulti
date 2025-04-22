using UnityEngine;

public class PHotPotatoUI : PNetworkBehaviour
{
    [SerializeField] private string HotPoatatoTextTag = "HotPotatoText";
    private CanvasGroup hotPotato;
    protected override void StartOnlineOwner()
    {
        if(GameManager.Instance == null)
            GameManager.OnCreated += m => m.GameLoop.HotPotatoManager.target.OnValueChanged += UpdateHotPotatoUI;
        else
            GameManager.Instance.GameLoop.HotPotatoManager.target.OnValueChanged += UpdateHotPotatoUI;
    }

    protected override void DisableAnyOwner()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameLoop.HotPotatoManager.target.OnValueChanged -= UpdateHotPotatoUI;
        SetHotPotatoUI(false);
    }
    
    private void UpdateHotPotatoUI(ulong previousTarget, ulong newTarget)
    {
        SetHotPotatoUI(newTarget == NetworkManager.LocalClientId);
    }
    
    private void SetHotPotatoUI(bool isActive)
    {
        if (hotPotato == null)
        {
            try
            {
                hotPotato = GameObject.FindGameObjectWithTag(HotPoatatoTextTag).GetComponent<CanvasGroup>();
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