using UnityEngine;
using UnityEngine.UI;

public class PCanvas : PNetworkBehaviour
{
    [SerializeField] private GameObject canvas;
    private static GraphicRaycaster _graphicRaycaster;
    public static GameObject Canvas { get; private set; }
    
    protected override void StartAnyOwner()
    {
        if (Canvas != null) return;
        Canvas = Instantiate(canvas);
        _graphicRaycaster = Canvas.GetComponent<GraphicRaycaster>();
        DontDestroyOnLoad(Canvas);
    }

    protected override void UpdateAnyOwner()
    {
        _graphicRaycaster.enabled = Cursor.visible;
    }
}