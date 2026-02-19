using UnityEngine;
using Mirror;
using MyAssets.Net;

namespace MyAssets.Client.UI
{
    public sealed class LobbyUI : MonoBehaviour
    {
        public void Host()
        {
            if (NetworkManager.singleton != null)
                NetworkManager.singleton.StartHost();
        }

        public void Client(string address)
        {
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.networkAddress = address;
                NetworkManager.singleton.StartClient();
            }
        }

        public void StartMatch(int seed)
        {
            if (NetworkClient.active)
                NetworkClient.Send(new StartMatchRequest { Seed = seed });
        }
    }
}
