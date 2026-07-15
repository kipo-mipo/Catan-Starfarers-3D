using System.Collections;
using UnityEngine;
using MyAssets.GameCore;
using MyAssets.GameCore.Net;

namespace MyAssets.Client.Net
{
    public sealed class ClientEventReceiver : MonoBehaviour
    {
        private INetBridge _bridge;
        private Coroutine _hookRoutine;

        private void OnEnable()
        {
            // NetService.Bridge often comes online a few frames after scene load.
            // If we only check once, we miss every event forever.
            _hookRoutine = StartCoroutine(HookWhenReady());
        }

        private void OnDisable()
        {
            if (_hookRoutine != null)
            {
                StopCoroutine(_hookRoutine);
                _hookRoutine = null;
            }

            Unhook();
        }

        private IEnumerator HookWhenReady()
        {
            // Wait until NetService.Bridge exists, then subscribe exactly once.
            while (true)
            {
                var b = NetService.Bridge;
                if (b != null)
                {
                    _bridge = b;
                    _bridge.OnGameEvent += HandleGameEvent;
                    _bridge.OnNetStatus += HandleStatus;
                    Debug.Log("[ClientEventReceiver] Hooked NetService.Bridge.");
                    yield break;
                }

                yield return null;
            }
        }

        private void Unhook()
        {
            if (_bridge == null) return;

            _bridge.OnGameEvent -= HandleGameEvent;
            _bridge.OnNetStatus -= HandleStatus;
            _bridge = null;
        }

        private void HandleStatus(string msg) => Debug.Log($"[Net] {msg}");

        private void HandleGameEvent(IGameEvent e)
        {
            switch (e)
            {
                case MatchStartedEvent ms:
                    Debug.Log($"Match started seed={ms.Seed}");
                    break;
                case SetupStepChangedEvent step:
                    Debug.Log($"Setup step: round={step.Round}, currentPlayer={step.CurrentPlayer.Value}");
                    break;
                case ColonyPlacedEvent cp:
                    Debug.Log($"Setup colony placed p={cp.Player.Value} node={cp.Node.Value}");
                    break;
                case SpaceportPlacedEvent spc:
                    Debug.Log($"Setup spaceport placed p={spc.Player.Value} node={spc.Node.Value}");
                    break;
                case SetupUpgradeGrantedEvent up:
                    Debug.Log($"Setup upgrade p={up.Player.Value} upgrade={up.Upgrade}");
                    break;
                case StartingResourcesGrantedEvent res:
                    Debug.Log($"Starting resources p={res.Player.Value} count={res.Count}");
                    break;
                case FameMedalGrantedEvent fm:
                    Debug.Log($"Fame medal p={fm.Player.Value}");
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
