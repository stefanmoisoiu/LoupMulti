using Player.Networking;

namespace Player.Model.Procedural_Anims
{
    public abstract class PAnimBehaviour : PNetworkBehaviour
    {
        public abstract AnimComponent[] GetAnimComponents();
    }
}