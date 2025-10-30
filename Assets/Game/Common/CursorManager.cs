using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        private readonly HashSet<object> _lockRequesters = new HashSet<object>();

        public bool IsCursorLocked { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            LockCursor();
        }

        public void RequestCursorUnlock(object requester)
        {
            if (requester == null) return;
            
            if (!_lockRequesters.Add(requester)) throw new System.Exception("Cursor is already locked");

            if (_lockRequesters.Count > 0)
            {
                UnlockCursor();
            }
        }

        public void ReleaseCursorUnlock(object requester)
        {
            if (requester == null) return;

            _lockRequesters.Remove(requester);

            if (_lockRequesters.Count == 0)
            {
                LockCursor();
            }
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            IsCursorLocked = false;
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            IsCursorLocked = true;
        }
    }
}