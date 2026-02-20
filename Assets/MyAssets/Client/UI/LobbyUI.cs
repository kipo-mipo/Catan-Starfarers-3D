using UnityEngine;
using MyAssets.Client.Net;

namespace MyAssets.Client.UI
{
    public sealed class LobbyUI : MonoBehaviour
    {
        public void Host() => NetService.Bridge?.StartHost();

        public void Client(string address) => NetService.Bridge?.StartClient(address);

        public void Disconnect() => NetService.Bridge?.Disconnect();

        public void StartMatch(int seed) => NetService.Bridge?.RequestStartMatch(seed);
    }
}
