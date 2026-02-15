using UnityEngine;
using Mirror;

public class UIRouter : MonoBehaviour
{
    public GameObject connectionPanel;
    public GameObject lobbyPanel;
    public GameObject gamePanel;

    void Update()
    {
        bool online = NetworkClient.active;

        var gm = GameManager.Instance;
        bool gmExists = gm != null;

        if (connectionPanel) connectionPanel.SetActive(!online);

        bool inLobby = online && (!gmExists || gm.phase == MatchPhase.Lobby);
        if (lobbyPanel) lobbyPanel.SetActive(inLobby);

        bool inGame = online && gmExists && gm.phase != MatchPhase.Lobby;
        if (gamePanel) gamePanel.SetActive(inGame);
    }
}
