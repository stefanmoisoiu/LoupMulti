using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Base_Scripts
{
    public class NetcodeLogger : NetworkBehaviour
    {
        public static NetcodeLogger Instance;
    
        private Dictionary<LogType, string> _logColors = new()
        {
            {LogType.Netcode, "#00FF00"},
            {LogType.Data, "#0000FF"},
            {LogType.ItemSelection, "#FF00FF"},
            {LogType.GameLoop, "#FFFF00"},
            {LogType.TickLoop, "#FF0000"},
            {LogType.Map, "#00FFFF"},
        };
    
        private void Awake()
        {
            Instance = this;
        }
    
        [Rpc(SendTo.Everyone)]
        public void LogRpc(string message, LogType type, AddedEffects[] effects = null)
        {
            string color = _logColors[type];
            string effect = "";
            if (effects != null)
                foreach (AddedEffects addedEffect in effects)
                    switch (addedEffect)
                    {
                        case AddedEffects.Bold: effect += "<b>"; break;
                        case AddedEffects.Italic: effect += "<i>"; break;
                    }
        
            Debug.Log($"<{color}>{effect}{message}");
        }

        public enum AddedEffects
        {
            Bold,
            Italic,
        }
        public enum LogType
        {
            Netcode,
            Data,
            ItemSelection,
            GameLoop,
            TickLoop,
            Map
        }
    }
}