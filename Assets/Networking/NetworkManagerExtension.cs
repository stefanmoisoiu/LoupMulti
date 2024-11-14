using Unity.Netcode;

public static class NetworkManagerExtension
{
        public static int GetCurrentPlayerCount(this NetworkManager networkManager)
        {
                return GetCurrentPlayerCountServerRpc();
        }
        [Rpc(SendTo.Server)]
        private static int GetCurrentPlayerCountServerRpc() => NetworkManager.Singleton.ConnectedClients.Count;
}