using Mirror;
using UnityEngine;

public enum SetupRound
{
    Colony1,
    Colony2,
    SpaceportAndShip,
    FreeUpgrade,
    Done
}

public partial class GameManager
{
    [SyncVar] public SetupRound setupRound = SetupRound.Colony1;
    [SyncVar] public int setupIndex = 0;

    // -1 = not placed
    public readonly SyncList<int> colony1NodeBySeat = new SyncList<int>();
    public readonly SyncList<int> colony2NodeBySeat = new SyncList<int>();

    [Server]
    void ServerBeginSetup()
    {
        setupRound = SetupRound.Colony1;
        setupIndex = 0;

        EnsureColonyListsSized();
        EnsureShipListSized(); // exists in Ships partial

        Debug.Log($"[Server] Setup begin. Round={setupRound} CurrentSetup={CurrentSetupPlayerNetId}");
    }

    public uint CurrentSetupPlayerNetId
    {
        get
        {
            int n = seatNetIds.Count;
            if (n == 0) return 0;

            // Colony1 + Spaceport go clockwise, Colony2 + FreeUpgrade go counterclockwise (for now)
            bool clockwise = (setupRound == SetupRound.Colony1 || setupRound == SetupRound.SpaceportAndShip);

            int idx = setupIndex % n;

            if (clockwise)
                return seatNetIds[idx];

            // counterclockwise
            int ccw = (n - 1 - idx + n) % n;
            return seatNetIds[ccw];
        }
    }

    [Server]
    public void ConfirmSetupAction(uint requesterNetId)
    {
        Debug.Log($"[Server] ConfirmSetupAction: phase={phase} round={setupRound} setupIndex={setupIndex} requester={requesterNetId} current={CurrentSetupPlayerNetId}");

        if (phase != MatchPhase.Setup)
        {
            Debug.Log("[Server] Reject: not in Setup");
            return;
        }

        if (requesterNetId != CurrentSetupPlayerNetId)
        {
            Debug.Log("[Server] Reject: not your setup turn");
            return;
        }

        EnsureColonyListsSized();
        EnsureShipListSized();

        int seat = SeatIndexOf(requesterNetId); // MUST exist exactly once in project
        if (seat < 0)
        {
            Debug.Log("[Server] Reject: requester not in seats");
            return;
        }

        // 1) Validate required action for CURRENT round BEFORE advancing anything
        switch (setupRound)
        {
            case SetupRound.Colony1:
            {
                if (colony1NodeBySeat[seat] < 0)
                {
                    Debug.Log("[Server] Reject: must place Colony1 before confirming");
                    return;
                }
                break;
            }
            case SetupRound.Colony2:
            {
                if (colony2NodeBySeat[seat] < 0)
                {
                    Debug.Log("[Server] Reject: must place Colony2 before confirming");
                    return;
                }
                break;
            }
            case SetupRound.SpaceportAndShip:
            {
                if (shipNetIdsBySeat.Count <= seat || shipNetIdsBySeat[seat] == 0u)
                {
                    Debug.Log("[Server] Reject: must place ship before confirming");
                    return;
                }
                break;
            }
            case SetupRound.FreeUpgrade:
            {
                // you can add validation later; for now allow confirm
                break;
            }
            case SetupRound.Done:
                // already done; ignore
                return;
        }

        // 2) Advance turn order inside the current setup round
        int n = seatNetIds.Count;
        setupIndex++;

        // if we’ve completed a full pass for this round, move to next round and reset index
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

        if (setupRound == SetupRound.Done)
        {
            phase = MatchPhase.Turn;
            turnIndex = 0;
            Debug.Log($"[Server] Setup complete -> Phase=Turn CurrentPlayer={CurrentPlayerNetId}");
        }
        else
        {
            Debug.Log($"[Server] Setup advanced -> Round={setupRound} setupIndex={setupIndex} CurrentSetup={CurrentSetupPlayerNetId}");
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
        Debug.Log($"[Server] RequestPlaceColony from={requesterNetId} node={nodeId} phase={phase} round={setupRound} currentSetup={CurrentSetupPlayerNetId}");

        if (phase != MatchPhase.Setup) return;
        if (requesterNetId != CurrentSetupPlayerNetId) return;
        if (setupRound != SetupRound.Colony1 && setupRound != SetupRound.Colony2) return;

        if (BoardRegistry.Instance == null)
        {
            Debug.LogError("[Server] No BoardRegistry in scene");
            return;
        }

        if (!BoardRegistry.Instance.Nodes.ContainsKey(nodeId))
        {
            Debug.Log("[Server] Reject: invalid node id");
            return;
        }

        EnsureColonyListsSized();

        int seat = SeatIndexOf(requesterNetId); // MUST exist exactly once in project
        if (seat < 0) return;

        // Do NOT allow placing multiple times for the same round.
        if (setupRound == SetupRound.Colony1 && colony1NodeBySeat[seat] >= 0)
        {
            Debug.Log("[Server] Reject: Colony1 already placed for this seat");
            return;
        }
        if (setupRound == SetupRound.Colony2 && colony2NodeBySeat[seat] >= 0)
        {
            Debug.Log("[Server] Reject: Colony2 already placed for this seat");
            return;
        }

        // Prevent any colony occupying the same node
        for (int i = 0; i < seatNetIds.Count; i++)
        {
            if (colony1NodeBySeat[i] == nodeId || colony2NodeBySeat[i] == nodeId)
            {
                Debug.Log("[Server] Reject: node already occupied by a colony");
                return;
            }
        }

        if (setupRound == SetupRound.Colony1)
            colony1NodeBySeat[seat] = nodeId;
        else
            colony2NodeBySeat[seat] = nodeId;

        Debug.Log($"[Server] Colony placed. seat={seat} node={nodeId} round={setupRound}");
    }
}
