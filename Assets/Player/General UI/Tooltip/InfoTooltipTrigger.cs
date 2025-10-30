using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.General_UI.Tooltips
{
    [RequireComponent(typeof(IInfoTooltipDataProvider))]
    public class InfoTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private IInfoTooltipDataProvider _dataProvider;

        private void Awake()
        {
            _dataProvider = GetComponent<IInfoTooltipDataProvider>();
            if (_dataProvider == null)
            {
                Debug.LogError($"InfoTooltipTrigger sur {gameObject.name} n'a pas trouvé de IInfoTooltipDataProvider.", this);
                enabled = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            InfoTooltipManager.Instance.EnteredTarget(_dataProvider, transform.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            InfoTooltipManager.Instance.ExitedTarget(_dataProvider);
        }
    }
}