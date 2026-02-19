using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// LobbyUI is a dumb controller for lobby widgets ONLY.
/// It does NOT route screens/panels. UIRouter handles that.
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [Header("Connection Controls")]
    [SerializeField] TMP_InputField addressInput;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button stopButton;

    [Header("Lobby Controls")]
    [SerializeField] Button readyButton;
    [SerializeField] TMP_Text readyButtonLabel;
    [SerializeField] Button startButton;

    [Header("Status Text")]
    [SerializeField] TMP_Text statusLabel;

    CustomNetworkManager nm;
    LobbyParticipant localLobby;

    void Awake()
    {
        nm = NetworkManager.singleton as CustomNetworkManager;

        if (hostButton) hostButton.onClick.AddListener(OnHost);
        if (joinButton) joinButton.onClick.AddListener(OnJoin);
        if (stopButton) stopButton.onClick.AddListener(OnStop);
        if (readyButton) readyButton.onClick.AddListener(OnToggleReady);
        if (startButton) startButton.onClick.AddListener(OnStartMatch);
    }

    void OnDestroy()
    {
        if (hostButton) hostButton.onClick.RemoveListener(OnHost);
        if (joinButton) joinButton.onClick.RemoveListener(OnJoin);
        if (stopButton) stopButton.onClick.RemoveListener(OnStop);
        if (readyButton) readyButton.onClick.RemoveListener(OnToggleReady);
        if (startButton) startButton.onClick.RemoveListener(OnStartMatch);
    }

    void Start()
    {
        if (addressInput && string.IsNullOrWhiteSpace(addressInput.text))
            addressInput.text = "localhost";
    }

    void Update()
    {
        if (nm == null) nm = NetworkManager.singleton as CustomNetworkManager;

        // Cache local lobby participant once local player exists
        if (localLobby == null && NetworkClient.localPlayer != null)
            localLobby = NetworkClient.localPlayer.GetComponent<LobbyParticipant>();

        bool clientActive = NetworkClient.active;
        bool connected = NetworkClient.isConnected;
        bool serverActive = NetworkServer.active;

        // "In lobby" definition: connected, and match hasn't started yet.
        // If you still use matchState/phase, use that. Otherwise fall back to "MatchServer/MatchHost exists and started".
        bool inLobby = IsInLobby();

        // Connection buttons: only make sense when not already connected/hosting
        if (hostButton) hostButton.interactable = !clientActive;
        if (joinButton) joinButton.interactable = !clientActive;

        // Stop is allowed any time you're active
        if (stopButton) stopButton.interactable = clientActive;

        // Ready only makes sense when connected AND still in lobby
        bool canReady = connected && inLobby && localLobby != null;
        if (readyButton) readyButton.interactable = canReady;

        if (readyButtonLabel)
        {
            if (localLobby == null) readyButtonLabel.text = "Ready";
            else readyButtonLabel.text = localLobby.isReady ? "Unready" : "Ready";
        }

        // Start button: host/server only, lobby only, and only if everyone is ready.
        if (startButton)
        {
            startButton.gameObject.SetActive(serverActive && inLobby);

            bool allReady = false;
            if (serverActive && inLobby && nm != null)
                allReady = nm.AreAllPlayersReady(); // server-only safe check because serverActive is true

            startButton.interactable = serverActive && inLobby && allReady;
        }

        // Status text: keep it informative, not routing logic
        if (statusLabel)
        {
            if (!clientActive) statusLabel.text = "Offline";
            else if (serverActive && connected) statusLabel.text = "Host";
            else statusLabel.text = "Client";
        }
    }

    bool IsInLobby()
    {
        // Try MatchServer first (from your refactor). If you kept MatchHost, add an alternative here.
        // The goal: LobbyUI should NOT decide panels, but it *can* decide whether Ready/Start is valid.

        // If your MatchServer exposes something like:
        //   public static MatchServer Instance
        //   public MatchState matchState
        // then use it:
        var matchServerType = System.Type.GetType("MatchServer");
        if (matchServerType != null)
        {
            var instProp = matchServerType.GetProperty("Instance");
            var inst = instProp?.GetValue(null);
            if (inst != null)
            {
                var stateField = matchServerType.GetField("matchState");
                if (stateField != null)
                {
                    var stateVal = stateField.GetValue(inst)?.ToString();
                    // Treat any state named "Lobby" as lobby. Anything else means match started.
                    return stateVal == "Lobby";
                }
            }
        }

        // Fallback: if we can't detect match state, assume you're in lobby while connected.
        // UIRouter will still control panels.
        return NetworkClient.isConnected;
    }

    void OnHost()
    {
        NetworkManager.singleton.StartHost();
    }

    void OnJoin()
    {
        string addr = addressInput ? addressInput.text.Trim() : "localhost";
        if (string.IsNullOrEmpty(addr)) addr = "localhost";
        NetworkManager.singleton.networkAddress = addr;
        NetworkManager.singleton.StartClient();
    }

    void OnStop()
    {
        var nmgr = NetworkManager.singleton;

        if (NetworkServer.active && NetworkClient.isConnected) nmgr.StopHost();
        else if (NetworkClient.isConnected) nmgr.StopClient();
        else if (NetworkServer.active) nmgr.StopServer();

        localLobby = null;

        // Clear selection so keyboard focus doesn't get stuck on dead buttons.
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
    }

    void OnToggleReady()
    {
        if (localLobby == null) return;
        localLobby.CmdSetReady(!localLobby.isReady);
    }

    void OnStartMatch()
    {
        if (!NetworkServer.active) return;
        if (nm == null) nm = NetworkManager.singleton as CustomNetworkManager;
        nm?.ServerStartGameIfReady();
    }
}
