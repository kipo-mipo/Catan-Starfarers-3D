using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

public class LobbyUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject connectionPanel;
    public GameObject lobbyPanel;

    [Header("Connection UI")]
    public TMP_InputField addressInput;
    public Button hostButton;
    public Button joinButton;
    public Button stopButton;
    public TMP_Text statusText;

    [Header("Lobby UI")]
    public Button readyButton;
    public TMP_Text readyButtonText;
    public Button startButton;

    CustomNetworkManager nm;
    LobbyPlayer localLobbyPlayer;

    void Awake()
    {
        nm = (CustomNetworkManager)NetworkManager.singleton;

        hostButton.onClick.AddListener(OnHost);
        joinButton.onClick.AddListener(OnJoin);
        stopButton.onClick.AddListener(OnStop);
        readyButton.onClick.AddListener(OnToggleReady);
        startButton.onClick.AddListener(OnStartGame);
    }

    void Start()
    {
        if (addressInput != null && string.IsNullOrWhiteSpace(addressInput.text))
            addressInput.text = "localhost";
    }

    void Update()
    {
        // Grab local player when it exists
        if (localLobbyPlayer == null && NetworkClient.localPlayer != null)
            localLobbyPlayer = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();

        bool active = NetworkClient.active;
        bool isServer = NetworkServer.active;

        // Determine phase safely
        var gm = GameManager.Instance != null ? GameManager.Instance : FindFirstObjectByType<GameManager>();
        bool inLobbyPhase = (gm == null) || gm.phase == MatchPhase.Lobby;

        // Panels
        if (connectionPanel != null) connectionPanel.SetActive(!active); // show only when offline
        if (lobbyPanel != null) lobbyPanel.SetActive(active && inLobbyPhase);

        // Buttons
        hostButton.interactable = !active;
        joinButton.interactable = !active;
        stopButton.interactable = active;

        // Ready
        bool canReady = active && inLobbyPhase && localLobbyPlayer != null;
        readyButton.interactable = canReady;

        if (localLobbyPlayer != null)
            readyButtonText.text = localLobbyPlayer.isReady ? "Unready" : "Ready";
        else
            readyButtonText.text = "Ready";

        // Start (host only + all ready)
        startButton.gameObject.SetActive(isServer && inLobbyPhase);
        startButton.interactable = isServer && inLobbyPhase && nm != null && nm.AreAllPlayersReady();

        // Status
        if (statusText != null)
        {
            if (!active) statusText.text = "Offline";
            else if (isServer && NetworkClient.isConnected) statusText.text = "Host";
            else statusText.text = "Client";
        }

        if (nm == null) nm = NetworkManager.singleton as CustomNetworkManager;

    }

    void OnHost() => NetworkManager.singleton.StartHost();

    void OnJoin()
    {
        string addr = addressInput != null ? addressInput.text.Trim() : "localhost";
        if (string.IsNullOrEmpty(addr)) addr = "localhost";
        NetworkManager.singleton.networkAddress = addr;
        NetworkManager.singleton.StartClient();
    }

    void OnStop()
    {
        if (NetworkServer.active && NetworkClient.isConnected) NetworkManager.singleton.StopHost();
        else if (NetworkClient.isConnected) NetworkManager.singleton.StopClient();
        else if (NetworkServer.active) NetworkManager.singleton.StopServer();

        localLobbyPlayer = null;

        // Clear UI focus so keyboard works again
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void OnToggleReady()
    {
        if (localLobbyPlayer == null) return;
        localLobbyPlayer.CmdSetReady(!localLobbyPlayer.isReady);
    }

    void OnStartGame()
    {
        if (!NetworkServer.active) return;
        nm.ServerStartGameIfReady();
    }
}
