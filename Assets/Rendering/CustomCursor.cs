namespace Rendering
{
    using UnityEngine;

    public class CustomCursor : MonoBehaviour
    {
        [Tooltip("Votre texture de curseur 64×64 importée en Texture Type = Cursor")]
        public Texture2D cursorTexture;

        [Tooltip("Position du point actif (hotspot) à l’intérieur de l’image (ex: coin supérieur gauche = (0,63))")]
        public Vector2 hotspot;

        [Tooltip("Choisissez Auto pour laisser Unity adapter le mode selon la plateforme")]
        public CursorMode cursorMode = CursorMode.Auto;

        private void Awake()
        {
            if (cursorTexture != null)
            {
                Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
            }
            else
            {
                Debug.LogWarning("CustomCursor : pas de texture assignée !");
            }
        }
    }

}