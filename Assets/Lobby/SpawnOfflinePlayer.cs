using System;
using Unity.Netcode;
using UnityEngine;

public class SpawnOfflinePlayer : NetworkBehaviour
{
        [SerializeField] private GameObject player;
        
        private void OnEnable()
        {
                if (!NetcodeManager.InGame) Spawn();
                NetcodeManager.OnLeaveGame += Spawn;
        }

        private void OnDisable()
        {
                NetcodeManager.OnLeaveGame -= Spawn;
        }

        private void Spawn()
        {
                Instantiate(player, transform.position, Quaternion.identity);
        }
}