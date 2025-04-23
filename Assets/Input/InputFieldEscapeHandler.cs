using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldEscapeHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private TMP_InputField _inputField;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        // ① Ne plus restaurer l’ancien texte automatiquement sur ESC
        _inputField.restoreOriginalTextOnEscape = false;
        // ② S’abonner à la fin de l’édition
        _inputField.onEndEdit.AddListener(OnEndEdit);
        _inputField.onSubmit.AddListener(OnEndEdit);
    }

    private void OnDestroy()
    {
        _inputField.onEndEdit.RemoveListener(OnEndEdit);
        _inputField.onSubmit.RemoveListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        InputManager.Instance.EnableUI();
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        InputManager.Instance.EnableGameplay();
    }
}