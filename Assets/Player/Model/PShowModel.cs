using UnityEngine;

public class PShowModel : PNetworkBehaviour
{
    [SerializeField] private GameObject modelParent;
    [SerializeField] private string showToCamLayerName;
    [SerializeField] private string hiddenFromCamLayerName;
    
    [SerializeField] private bool alwaysShow;

    protected override void StartOnlineNotOwner()
    {
        SetModelState(true);
    }

    protected override void StartAnyOwner()
    {
        SetModelState(alwaysShow);
    }
    private void SetModelState(bool show, GameObject child = null)
    {
        child ??= modelParent;
        child.layer = show ? LayerMask.NameToLayer(showToCamLayerName) : LayerMask.NameToLayer(hiddenFromCamLayerName);
        foreach (Transform childTransform in child.transform)
            SetModelState(show, childTransform.gameObject);
    }
}
