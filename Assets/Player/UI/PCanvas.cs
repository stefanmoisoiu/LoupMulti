using UnityEngine;

public class PCanvas : PNetworkBehaviour
{
    [SerializeField] private GameObject canvas;
    public static GameObject Canvas { get; private set; }
    
    protected override void StartAnyOwner()
    {
        if (Canvas != null) return;
        Canvas = Instantiate(canvas);
        DontDestroyOnLoad(Canvas);
    }
}