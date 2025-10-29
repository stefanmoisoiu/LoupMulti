using Game.Common;
using Game.Data;
using Player.Networking;
using TMPro;
using UnityEngine;

namespace Player.General_UI.Resources
{
    public class DisplayResources : PNetworkBehaviour
    {
        [SerializeField] private string commonResourceTag = "CommonResource";
        private TMP_Text _commonResourceText;
        [SerializeField] private string rareResourceTag = "RareResource";
        private TMP_Text _rareResourceText;

        protected override void StartAnyOwner()
        {
            _commonResourceText = PCanvas.CanvasObjects[commonResourceTag].GetComponent<TMP_Text>();
            _rareResourceText = PCanvas.CanvasObjects[rareResourceTag].GetComponent<TMP_Text>();
            
            DataManager.OnEntryUpdatedOwner += OnDataUpdated;
        }

        private void OnDataUpdated(PlayerData previousData, PlayerData newData)
        {
            OwnedResourcesData resources = newData.inGameData.resources;
            int commonAmount = resources.GetResourceAmount(ResourceType.Common);
            int rareAmount = resources.GetResourceAmount(ResourceType.Rare);
            
            _commonResourceText.text = commonAmount.ToString();
            _rareResourceText.text = rareAmount.ToString();
        }

        protected override void DisableAnyOwner()
        {
            DataManager.OnEntryUpdatedOwner -= OnDataUpdated;
        }

        private void OnResourceCollected(ushort amountCollected, ushort totalAmount, ResourceData type)
        {
            Debug.Log("Collected " + amountCollected + " of " + type.ResourceType + ". Total: " + totalAmount);
            if (type.ResourceType == ResourceType.Common)
            {
                _commonResourceText.text = totalAmount.ToString();
            }
            else if (type.ResourceType == ResourceType.Rare)
            {
                _rareResourceText.text = totalAmount.ToString();
            }
        }
    }
}