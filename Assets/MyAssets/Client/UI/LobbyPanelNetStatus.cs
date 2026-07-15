using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MyAssets.Client.Net;
using MyAssets.GameCore.Net;

namespace MyAssets.Client.UI
{
    public sealed class LobbyPanelNetStatus : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button startButton;
        [SerializeField] private int seed = 12345;

        [Header("Optional")]
        [SerializeField] private AutoConnectFlow autoConnectFlow;

        private string _last = "";
        private float _nextRefresh;

        private LobbyStateInfo _lobby;
        private bool _hasLobby;

        private void Awake()
        {
            if (autoConnectFlow == null)
                autoConnectFlow = FindAnyObjectByType<AutoConnectFlow>();
        }

        private void OnEnable()
        {
            if (autoConnectFlow != null)
                autoConnectFlow.OnStatus += OnAutoStatus;

            var bridge = NetService.Bridge;
            if (bridge != null)
                bridge.OnLobbyState += OnLobbyState;

            _nextRefresh = 0f;
            RefreshUI(true);
        }

        private void OnDisable()
        {
            if (autoConnectFlow != null)
                autoConnectFlow.OnStatus -= OnAutoStatus;

            var bridge = NetService.Bridge;
            if (bridge != null)
                bridge.OnLobbyState -= OnLobbyState;
        }

        private void Update()
        {
            RefreshUI(false);
        }

        private void OnAutoStatus(string msg)
        {
            _last = msg ?? "";
            _nextRefresh = 0f;
            RefreshUI(true);
        }

        private void OnLobbyState(LobbyStateInfo s)
        {
            _lobby = s;
            _hasLobby = true;
            _nextRefresh = 0f;
        }

        private void RefreshUI(bool force)
        {
            if (!force && Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.2f;

            var bridge = NetService.Bridge;
            var role = bridge != null ? bridge.Role : NetRole.None;
            var connected = bridge != null && bridge.IsConnected;

            int players = _hasLobby ? (_lobby.Players?.Length ?? 0) : 0;
            bool canStart = _hasLobby && _lobby.CanStart;

            if (statusText != null)
            {
                statusText.text =
                    $"Role: {role}\n" +
                    $"Connected: {connected}\n" +
                    $"Players: {players}\n" +
                    $"CanStart: {canStart}\n" +
                    $"Last: {_last}";
            }

            if (startButton != null)
            {
                bool show = connected && role == NetRole.Host;
                startButton.gameObject.SetActive(show);

                // Require >= 2 players (server enforces too).
                startButton.interactable = show && canStart;
            }
        }

        public void OnClickStart()
        {
            var bridge = NetService.Bridge;
            if (bridge == null) return;

            bridge.RequestStartMatch(seed);
            _last = "Start requested.";
            _nextRefresh = 0f;
            Debug.Log("Start requested.");
        }
    }
}
