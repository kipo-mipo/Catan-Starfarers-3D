using Mirror;
using UnityEngine;

namespace MyAssets.Net
{
    public sealed class CustomNetworkManager : NetworkManager
    {
        [Header("Server prefabs")]
        [SerializeField] private GameObject matchServerPrefab;

        private GameObject _matchServerInstance;

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (matchServerPrefab == null)
            {
                Debug.LogError("CustomNetworkManager: matchServerPrefab not assigned.");
                return;
            }

            // Spawn exactly once.
            if (_matchServerInstance == null)
            {
                _matchServerInstance = Instantiate(matchServerPrefab);
                NetworkServer.Spawn(_matchServerInstance);
                Debug.Log("[Net] MatchServer spawned.");
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            _matchServerInstance = null;
        }
    }
}