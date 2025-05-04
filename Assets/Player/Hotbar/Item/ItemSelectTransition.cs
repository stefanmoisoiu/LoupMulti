using System.Collections;
using Player.Networking;
using UnityEngine;

namespace Player.Hotbar.Item
{
    public class ItemSelectTransition : PNetworkBehaviour
    {
        [SerializeField] private ItemModels itemModels;
        [SerializeField] private ItemList itemList;


        public IEnumerator Select(Item previousItem, Item newItem)
        {
            if (previousItem != null) yield return HideAnim(previousItem);
            if (newItem != null) yield return ShowAnim(newItem);
        }

        private IEnumerator HideAnim(Item item)
        {
            
            // remplacer par une animation de transition
            yield return new WaitForSeconds(0.5f);
            itemModels.ShowItemModel(null);
        }
        
        private IEnumerator ShowAnim(Item item)
        {
            // remplacer par une animation de transition
            itemModels.ShowItemModel(item);
            yield return new WaitForSeconds(0.5f);
        }
    }
}