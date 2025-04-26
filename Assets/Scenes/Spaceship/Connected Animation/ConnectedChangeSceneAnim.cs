using System.Collections;
using Scenes.Scene_Load;
using UnityEngine;

public class ConnectedChangeSceneAnim : MonoBehaviour
{
    [SerializeField] private Animator anim;
    
    [SerializeField] private float waitDuration;
    [SerializeField] private string sceneName = "SoloLobby";
    
    
    public void ChangeScene()
    {
        // Start the coroutine to wait and then change the scene
        StartCoroutine(WaitAndChangeScene());
    }
    
    private IEnumerator WaitAndChangeScene()
    {
        // Play the animation
        anim.SetTrigger("Play");
        // Wait for the specified duration
        yield return new WaitForSeconds(waitDuration);
        
        // Change the scene
        NetcodeSceneChanger.Instance.LocalChangeScene(sceneName);
    }
}
