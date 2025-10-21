using Player.Networking;

namespace Player.General_UI
{
    public class CanvasVisibility : PNetworkBehaviour
    {
        protected override void StartAnyOwner()
        {
            PCanvas.Instance?.TransitionCanvasVisible(true);
        }
        protected override void DisableAnyOwner()
        {
            PCanvas.Instance?.TransitionCanvasVisible(false);
        }
    }
}