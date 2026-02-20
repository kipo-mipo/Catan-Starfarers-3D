using UnityEngine;
using MyAssets.GameCore;

namespace MyAssets.Client.Net
{
    public sealed class ClientEventReceiver : MonoBehaviour
    {
        private void OnEnable()
        {
            var bridge = NetService.Bridge;
            if (bridge == null) return;

            bridge.OnGameEvent += HandleGameEvent;
            bridge.OnNetStatus += HandleStatus;
        }

        private void OnDisable()
        {
            var bridge = NetService.Bridge;
            if (bridge == null) return;

            bridge.OnGameEvent -= HandleGameEvent;
            bridge.OnNetStatus -= HandleStatus;
        }

        private void HandleStatus(string msg) => Debug.Log($"[Net] {msg}");

        private void HandleGameEvent(IGameEvent e)
        {
            switch (e)
            {
                case MatchStartedEvent ms:
                    Debug.Log($"Match started seed={ms.Seed}");
                    break;
                case ShipMovedEvent sm:
                    Debug.Log($"Ship moved p={sm.Player.Value} ship={sm.Ship.Value} {sm.From.Value}->{sm.To.Value}");
                    break;
                case SectorRevealedEvent sr:
                    Debug.Log($"Sector revealed slot={sr.Slot.Value} piece={sr.Piece.Value} rot={sr.Rotation}");
                    break;
                case ActionRejectedEvent r:
                    Debug.LogWarning($"Action rejected: {r.Reason}");
                    break;
                default:
                    Debug.Log($"Event: {e.GetType().Name}");
                    break;
            }
        }
    }
}
