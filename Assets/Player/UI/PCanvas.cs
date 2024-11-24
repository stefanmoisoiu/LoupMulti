using UnityEngine;

public class PCanvas : PNetworkBehaviour
{
    [SerializeField] private GameObject canvas;
    private static GameObject _canvas;
    
    protected override void StartAnyOwner()
    {
        if (_canvas != null) return;
        _canvas = Instantiate(canvas);
        DontDestroyOnLoad(_canvas);
    }
}