using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    // Face the canvas towards the camera
    private Camera _camera;
    private void LateUpdate()
    {
        if (_camera != Camera.main)
            _camera = Camera.main;

        if (_camera == null) return;
        transform.LookAt(transform.position - _camera.transform.rotation * Vector3.forward,
            _camera.transform.rotation * Vector3.up);
    }
}
