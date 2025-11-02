using Game.Common;
using Game.Data;
using Player.Networking;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PShowModel : PNetworkBehaviour
    {
        [SerializeField] private GameObject[] otherModels;
        [SerializeField] private GameObject[] ownerHiddenModels;
        [SerializeField] private string showToCamLayerName;
        [SerializeField] private string hiddenFromCamLayerName;
    
        [SerializeField] private bool alwaysShow;

        protected override void StartOnlineNotOwner()
        {
            DataManager.OnEntryUpdatedClient += OnEntryUpdated;
            UpdateModels();
        }

        protected override void StartOnlineOwner()
        {
            DataManager.OnEntryUpdatedClient += OnEntryUpdated;
            UpdateModels();
        }

        private void OnDisable()
        {
            DataManager.OnEntryUpdatedClient -= OnEntryUpdated;
        }
        
        protected override void StartOffline()
        {
            SetOwnerHiddenModels(alwaysShow);
        }
        
        private void OnEntryUpdated(PlayerData _, PlayerData newData)
        {
            if (newData.clientId != OwnerClientId) return;
            UpdateModels();
        }

        private bool ShouldShowOtherModels()
        {
            if (DataManager.Instance == null) return true;
            if (!DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData ownerData)) return true;
            return ownerData.outerData.playingState != OuterData.PlayingState.SpectatingGame;
        }
        private bool ShouldShowOwnerHiddenModel()
        {
            if (!IsOnline || IsOwner)
            {
                Debug.Log("ShouldShowOwnerHiddenModel");
                return alwaysShow;
            }
            if (DataManager.Instance == null) return true;
            if (!DataManager.Instance.TryGetValue(OwnerClientId, out PlayerData ownerData)) return true;
            return ownerData.outerData.playingState != OuterData.PlayingState.SpectatingGame;
        }

        private void UpdateModels()
        {
            SetOtherModels(ShouldShowOtherModels());
            SetOwnerHiddenModels(ShouldShowOwnerHiddenModel());
        }
        private void SetOtherModels(bool show)
        {
            foreach (GameObject model in otherModels) SetModelState(show, model);
        }
        private void SetOwnerHiddenModels(bool show)
        {
            foreach (GameObject model in ownerHiddenModels) SetModelState(show, model);
        }

        private void SetModelState(bool show, GameObject child)
        {
            child.layer = show ? LayerMask.NameToLayer(showToCamLayerName) : LayerMask.NameToLayer(hiddenFromCamLayerName);
            foreach (Transform childTransform in child.transform)
                SetModelState(show, childTransform.gameObject);
        }
    }
}
