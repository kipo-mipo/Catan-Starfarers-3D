using System.Collections;
using UnityEngine;

namespace MyAssets.Client.Net
{
    public sealed class AutoConnectFlow : MonoBehaviour
    {
        [Header("Discovery")]
        [SerializeField] private StarfarersLanDiscovery discovery;
        [SerializeField] private float searchSeconds = 1.25f;
        [SerializeField] private float hostJitterMaxSeconds = 0.35f;
        [SerializeField] private int receiveTimeoutMs = 250;

        [Header("Timeouts")]
        [SerializeField] private float connectTimeoutSeconds = 4f;

        [Header("Lobby")]
        [SerializeField] private string defaultPlayerName = "";

        public event System.Action<string> OnStatus;

        private bool _started;
        private bool _waitConnectedResult;

        private void Awake()
        {
            if (discovery == null)
                discovery = GetComponent<StarfarersLanDiscovery>();
        }

        private void Start()
        {
            if (_started) return;
            _started = true;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            if (discovery == null)
            {
                Status("No StarfarersLanDiscovery on NetworkRoot.");
                yield break;
            }

            // Always fetch bridge via NetService (it can recover from destroyed duplicates)
            var bridge = NetService.Bridge;
            if (bridge == null)
            {
                Status("No INetBridge found. Ensure MirrorNetBridge is on persistent NetworkRoot.");
                yield break;
            }

            if (bridge.IsConnected)
            {
                Status($"Already connected as {bridge.Role}");
                yield break;
            }

            // Search for host
            Status("Searching for host...");
            string hostIp = null;

            float end = Time.unscaledTime + Mathf.Max(0.1f, searchSeconds);
            while (Time.unscaledTime < end)
            {
                discovery.BroadcastProbe();

                if (discovery.TryReceiveHostReply(receiveTimeoutMs, out hostIp))
                    break;

                yield return new WaitForSecondsRealtime(0.15f);
            }

            if (!string.IsNullOrWhiteSpace(hostIp))
            {
                bridge = NetService.Bridge;
                if (bridge == null) { Status("Bridge missing."); yield break; }

                Status($"Joining {hostIp}...");
                bridge.StartClient(hostIp);

                yield return StartCoroutine(WaitForConnected(connectTimeoutSeconds));
                if (!_waitConnectedResult) yield break;

                TryAutoJoinLobby();
                yield break;
            }

            // Jitter to reduce host collisions
            float jitter = Random.Range(0f, hostJitterMaxSeconds);
            yield return new WaitForSecondsRealtime(jitter);

            // One more probe before hosting
            discovery.BroadcastProbe();
            if (discovery.TryReceiveHostReply(receiveTimeoutMs, out hostIp) && !string.IsNullOrWhiteSpace(hostIp))
            {
                bridge = NetService.Bridge;
                if (bridge == null) { Status("Bridge missing."); yield break; }

                Status($"Joining {hostIp}...");
                bridge.StartClient(hostIp);

                yield return StartCoroutine(WaitForConnected(connectTimeoutSeconds));
                if (!_waitConnectedResult) yield break;

                TryAutoJoinLobby();
                yield break;
            }

            // Host: start responder immediately so other instance can discover even if Mirror startup is slow.
            Status("Hosting...");
            discovery.StartHostResponder();

            bridge = NetService.Bridge;
            if (bridge == null) { Status("Bridge missing."); yield break; }

            bridge.StartHost();

            yield return StartCoroutine(WaitForConnected(connectTimeoutSeconds));
            if (!_waitConnectedResult) yield break;

            TryAutoJoinLobby();
        }

        private IEnumerator WaitForConnected(float timeoutSeconds)
        {
            _waitConnectedResult = false;

            float end = Time.unscaledTime + Mathf.Max(0.1f, timeoutSeconds);
            while (Time.unscaledTime < end)
            {
                var bridge = NetService.Bridge;
                if (bridge == null)
                {
                    Status("Bridge destroyed / missing.");
                    yield break;
                }

                if (bridge.IsConnected)
                {
                    Status("Connected.");
                    _waitConnectedResult = true;
                    yield break;
                }
                yield return null;
            }

            Status("Connection failed. (Duplicate NetworkManager? Transport missing? Port in use?)");
            _waitConnectedResult = false;
        }

        private void TryAutoJoinLobby()
        {
            var bridge = NetService.Bridge;
            if (bridge == null) { Status("Bridge missing."); return; }

            string name = defaultPlayerName;
            if (string.IsNullOrWhiteSpace(name))
                name = $"Player {SystemInfo.deviceName}".Trim();

            if (!bridge.IsConnected)
            {
                Status("Not connected; can't join lobby.");
                return;
            }

            bridge.RequestJoinLobby(name);
            Status($"Joined lobby as {name}");
        }

        private void Status(string msg)
        {
            OnStatus?.Invoke(msg);
            Debug.Log($"[AutoConnectFlow] {msg}");
        }
    }
}
