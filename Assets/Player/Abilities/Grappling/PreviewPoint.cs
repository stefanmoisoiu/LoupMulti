using Player.Networking;
using UnityEngine;

namespace Player.Abilities.Grappling
{
    public class PreviewPoint : PNetworkBehaviour
    {
        [SerializeField] private Grappling grappling;
        [SerializeField] private GameObject previewGameObject;
        private GameObject _previewGameObject;
        protected override void StartAnyOwner()
        {
            CreatePreviewPoint();
        }

        protected override void DisableAnyOwner()
        {
            Destroy(_previewGameObject);
        }

        private void CreatePreviewPoint()
        {
            _previewGameObject = Instantiate(previewGameObject, Vector3.zero, Quaternion.identity);
            _previewGameObject.SetActive(false);
        }

        protected override void UpdateAnyOwner()
        {
            if (_previewGameObject == null) CreatePreviewPoint();
            if (grappling.IsGrappling || !grappling.GrapplingRaycast(out RaycastHit hit))
            {
                if(_previewGameObject.activeSelf) _previewGameObject.SetActive(false);
                return;
            }
            if(!_previewGameObject.activeSelf) _previewGameObject.SetActive(true);
            _previewGameObject.transform.position = hit.point;
        }
    }
}