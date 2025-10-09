using UnityEngine;

namespace Base_Scripts
{
    public class OpenCloseDebugConsole : MonoBehaviour
    {
        private bool visible;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                visible = !visible;
                Debug.developerConsoleVisible = visible;
            }
        }

        private void OnDisable()
        {
            if (visible)
            {
                visible = false;
                Debug.developerConsoleVisible = false;
            }
        }
    }
}
