using UnityEngine;
using UnityEngine.UI;
using MyAssets.GameCore;
using MyAssets.GameCore.Net;
using MyAssets.Client.Net;

namespace MyAssets.Client.UI
{
    public sealed class UIStateRouter : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject gamePanel;

        [Header("Host-only")]
        [SerializeField] private Button startButton;
        [SerializeField] private int seed = 12345;

        private bool _inGame;

        private void OnEnable()
        {
            var bridge = NetService.Bridge;
            if (bridge == null) return;

            bridge.OnLobbyState += OnLobbyState;   // if you have it
            bridge.OnGameEvent += OnGameEvent;     // if you have it
            bridge.OnNetStatus += OnNetStatus;     // if you have it

            ApplyPanels(false);
            RefreshHostUI();
        }

        private void OnDisable()
        {
            var bridge = NetService.Bridge;
            if (bridge == null) return;

            bridge.OnLobbyState -= OnLobbyState;
            bridge.OnGameEvent -= OnGameEvent;
            bridge.OnNetStatus -= OnNetStatus;
        }

        private void OnNetStatus(string _)
        {
            RefreshHostUI();
        }

        private void OnLobbyState(LobbyStateInfo _)
        {
            // still in lobby until match starts
            if (!_inGame) RefreshHostUI();
        }

        private void OnGameEvent(IGameEvent e)
        {
            if (e is MatchStartedEvent)
            {
                _inGame = true;
                ApplyPanels(true);
                RefreshHostUI();
            }
        }

        private void ApplyPanels(bool inGame)
        {
            if (lobbyPanel) lobbyPanel.SetActive(!inGame);
            if (gamePanel) gamePanel.SetActive(inGame);
        }

        private void RefreshHostUI()
        {
            var bridge = NetService.Bridge;
            if (bridge == null || startButton == null) return;

            bool show = bridge.IsConnected && bridge.Role == NetRole.Host && !_inGame;
            startButton.gameObject.SetActive(show);
            startButton.interactable = show;
        }

        public void OnClickStart()
        {
            var bridge = NetService.Bridge;
            if (bridge == null) return;

            bridge.RequestStartMatch(seed);
        }
    }
}