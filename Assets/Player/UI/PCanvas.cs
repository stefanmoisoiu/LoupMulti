using UnityEngine;
using UnityEngine.UI;

public class PCanvas : PNetworkBehaviour
{
    [SerializeField] private GameObject canvas;
    private GraphicRaycaster _graphicRaycaster;
    public GameObject Canvas { get; private set; }
    
    protected override void StartAnyOwner()
    {
        Canvas = Instantiate(canvas);
        _graphicRaycaster = Canvas.GetComponent<GraphicRaycaster>();
        DontDestroyOnLoad(Canvas);
    }

    protected override void DisableAnyOwner()
    {
        Destroy(Canvas);
    }

    protected override void UpdateAnyOwner()
    {
        _graphicRaycaster.enabled = Cursor.visible;
    }
}