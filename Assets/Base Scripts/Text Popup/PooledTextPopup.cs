using Base_Scripts;
using Base_Scripts.Text_Popup;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PooledTextPopup : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject popupPrefab;

    [Header("Settings")]
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float positionVariation = 0.5f;
    

    private GameObject[] popupPool;
    private TextPopupInstance[] popupTextPool;
    private int currentIndex = 0;

    private void OnEnable()
    {
        Setup();
    }

    private void Setup()
    {
        if (popupPool != null)
        {
            for (int i = 0; i < popupPool.Length; i++)
            {
                if (popupPool[i] != null)
                    Destroy(popupPool[i].transform.parent.gameObject);
            }
        }

        popupPool = new GameObject[poolSize];
        popupTextPool = new TextPopupInstance[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            popupPool[i] = Instantiate(popupPrefab, transform);
            popupTextPool[i] = popupPool[i].GetComponent<TextPopupInstance>();
            popupPool[i].SetActive(false);
        }
    }

    public void ShowPopupToAll(Vector3 position, string value, NetworkColor color, bool showToOwner = true)
    {
        ShowPopupServerRpc(position, value, color, showToOwner);
        ShowPopup(position, value, color, showToOwner);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ShowPopupServerRpc(Vector3 position, string value, NetworkColor color, bool showToOwner) => ShowPopupClientRpc(position, value, color, showToOwner);
    [Rpc(SendTo.NotOwner)]
    private void ShowPopupClientRpc(Vector3 position, string value, NetworkColor color, bool showToOwner) => ShowPopup(position, value, color, showToOwner);
    
    
    public void ShowPopup(Vector3 position, string value, Color color, bool showToOwner = true)
    {
        if (IsOwner && !showToOwner) return;
        PlayPooledPopup(position, value, color);
    }
    
    private void PlayPooledPopup(Vector3 position, string value, Color color)
    {
        GameObject popupInstance = popupPool[currentIndex];

        TextPopupInstance popupText = popupTextPool[currentIndex];
        Vector3 randomPosition = position + Random.insideUnitSphere * positionVariation;
        
        popupInstance.transform.position = randomPosition;
        popupText.SetData(value, color);
        
        popupInstance.SetActive(true);

        // Passe au prochain popup dans le pool
        currentIndex++;
        if (currentIndex >= poolSize)
        {
            currentIndex = 0;
        }
    }
}
