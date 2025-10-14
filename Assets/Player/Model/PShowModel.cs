using Player.Networking;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PShowModel : PNetworkBehaviour
    {
        [SerializeField] private GameObject[] showHideModels;
        [SerializeField] private string showToCamLayerName;
        [SerializeField] private string hiddenFromCamLayerName;
    
        [SerializeField] private bool alwaysShow;

        protected override void StartOnlineNotOwner()
        {
            foreach (var model in showHideModels)
            {
                SetModelState(true, model);
            }
        }

        protected override void StartAnyOwner()
        {
            foreach (var model in showHideModels)
            {
                SetModelState(alwaysShow, model);
            }
            
        }
        private void SetModelState(bool show, GameObject child)
        {
            child.layer = show ? LayerMask.NameToLayer(showToCamLayerName) : LayerMask.NameToLayer(hiddenFromCamLayerName);
            foreach (Transform childTransform in child.transform)
                SetModelState(show, childTransform.gameObject);
        }
    }
}
