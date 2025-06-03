using AYellowpaper.SerializedCollections;
using Base_Scripts;
using Player.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Player.UI
{
    public class PCanvas : PNetworkBehaviour
    {
        [SerializeField] private GameObject canvas;
        private static GraphicRaycaster _graphicRaycaster;
        public static GameObject Canvas { get; private set; }
        private static CanvasReferences _canvasReferences;
        public static SerializedDictionary<string, GameObject> CanvasObjects => _canvasReferences?.References;
        protected override void StartAnyOwner()
        {
            Canvas = Instantiate(canvas);
            _canvasReferences = Canvas.GetComponent<CanvasReferences>();
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
}