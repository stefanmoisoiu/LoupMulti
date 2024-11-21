using System;
using Unity.Mathematics;
using UnityEngine;

public class PGrapplingPreviewPoint : PNetworkBehaviour
{
    [SerializeField] private PGrappling grappling;
    [SerializeField] private GameObject previewGameObject;
    private GameObject _previewGameObject;
    protected override void StartAnyOwner()
    {
        CreatePreviewPoint();
    }
    private void CreatePreviewPoint()
    {
        _previewGameObject = Instantiate(previewGameObject, Vector3.zero, Quaternion.identity);
        _previewGameObject.SetActive(false);
    }

    protected override void UpdateAnyOwner()
    {
        if (_previewGameObject == null) CreatePreviewPoint();
        if (grappling.Grappling || !grappling.GrapplingRaycast(out RaycastHit hit))
        {
            if(_previewGameObject.activeSelf) _previewGameObject.SetActive(false);
            return;
        }
        if(!_previewGameObject.activeSelf) _previewGameObject.SetActive(true);
        _previewGameObject.transform.position = hit.point;
    }
}