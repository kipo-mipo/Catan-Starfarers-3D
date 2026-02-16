using Mirror;
using UnityEngine;

public enum SetupRound { Colony1, Colony2, SpaceportAndShip, FreeUpgrade, Done }

public partial class GameManager
{
    [SyncVar] public SetupRound setupRound = SetupRound.Colony1;
    [SyncVar] public int setupIndex = 0;
    public readonly SyncList<int> colony1NodeBySeat = new(); // -1 = not placed
    public readonly SyncList<int> colony2NodeBySeat = new();

    [Server]
    void ServerBeginSetup()
    {
        setupRound = SetupRound.Colony1;
        setupIndex = 0;
    }

    public uint CurrentSetupPlayerNetId
    {
        get
        {
            int n = seatNetIds.Count;
            if (n == 0) return 0;

            bool clockwise = (setupRound == SetupRound.Colony1 || setupRound == SetupRound.SpaceportAndShip);
            int idx = setupIndex % n;

            if (clockwise) return seatNetIds[idx];

            // counterclockwise
            int ccw = (n - 1 - idx + n) % n;
            return seatNetIds[ccw];
        }
    }

    [Server]
    public void ConfirmSetupAction(uint requesterNetId)
    {

        Debug.Log($"[Server] ConfirmSetupAction called. phase={phase} round={setupRound} setupIndex={setupIndex} requester={requesterNetId} currentSetup={CurrentSetupPlayerNetId}");

        if (phase != MatchPhase.Setup) { Debug.Log("[Server] Reject: not in Setup"); return; }
        if (requesterNetId != CurrentSetupPlayerNetId) { Debug.Log("[Server] Reject: not your setup turn"); return; }

        int n = seatNetIds.Count;

        if (setupRound == SetupRound.Colony1 || setupRound == SetupRound.Colony2)
        {
            EnsureColonyListsSized();
            int seat = SeatIndexOf(requesterNetId);
            if (seat < 0) return;

            int placed = (setupRound == SetupRound.Colony1) ? colony1NodeBySeat[seat] : colony2NodeBySeat[seat];
            if (placed < 0)
            {
                Debug.Log("[Server] Reject: must place colony before confirming");
                return;
            }
        }

        setupIndex++;
        if (setupIndex >= n)
        {
            setupIndex = 0;
            setupRound = setupRound switch
            {
                SetupRound.Colony1 => SetupRound.Colony2,
                SetupRound.Colony2 => SetupRound.SpaceportAndShip,
                SetupRound.SpaceportAndShip => SetupRound.FreeUpgrade,
                SetupRound.FreeUpgrade => SetupRound.Done,
                _ => setupRound
            };
        }

        if (setupRound == SetupRound.SpaceportAndShip)
        {
            EnsureShipListSized();
            int seat = SeatIndexOf(requesterNetId);
            if (seat < 0 || shipNetIdsBySeat[seat] == 0u)
            {
                Debug.Log("[Server] Reject: must place ship before confirming");
                return;
            }
        }

        if (setupRound == SetupRound.Done)
        {
            phase = MatchPhase.Turn;
            turnIndex = 0;
            Debug.Log($"[Server] Setup complete. Phase=Turn Current={CurrentPlayerNetId}");
        }
        else
        {
            Debug.Log($"[Server] Setup advanced. Round={setupRound} CurrentSetup={CurrentSetupPlayerNetId}");
        }
    }

    [Server]
    void EnsureColonyListsSized()
    {
        while (colony1NodeBySeat.Count < seatNetIds.Count) colony1NodeBySeat.Add(-1);
        while (colony2NodeBySeat.Count < seatNetIds.Count) colony2NodeBySeat.Add(-1);
    }

    [Server]
    public void RequestPlaceColony(uint requesterNetId, int nodeId)
    {
        if (phase != MatchPhase.Setup) return;
        if (requesterNetId != CurrentSetupPlayerNetId) return;
        if (setupRound != SetupRound.Colony1 && setupRound != SetupRound.Colony2) return;

        if (BoardRegistry.Instance == null) { Debug.LogError("No BoardRegistry"); return; }
        if (!BoardRegistry.Instance.Nodes.ContainsKey(nodeId)) return;

        EnsureColonyListsSized();
        int seat = SeatIndexOf(requesterNetId);
        if (seat < 0) return;

        // prevent duplicate occupation
        for (int i = 0; i < seatNetIds.Count; i++)
        {
            if (colony1NodeBySeat[i] == nodeId || colony2NodeBySeat[i] == nodeId)
                return;
        }

        if (setupRound == SetupRound.Colony1)
            colony1NodeBySeat[seat] = nodeId;
        else
            colony2NodeBySeat[seat] = nodeId;

        Debug.Log($"[Server] Colony placed. seat={seat} node={nodeId} round={setupRound}");
    }


}
