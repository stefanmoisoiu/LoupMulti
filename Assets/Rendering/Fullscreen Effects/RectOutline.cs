using Sirenix.OdinInspector;
using UnityEngine;

public class RectOutline : MonoBehaviour
{
    [SerializeField] private Renderer renderer;
    
    [Button]
    public void AddOutline()
    {
        if (renderer == null) return;
        if (RectOutlineEffect.Instance == null) return;
        
        RectOutlineEffect.Instance.AddRect(renderer);
    }
    
    [Button]
    public void RemoveOutline()
    {
        if (renderer == null) return;
        if (RectOutlineEffect.Instance == null) return;
        
        RectOutlineEffect.Instance.RemoveRect(renderer);
    }
}
