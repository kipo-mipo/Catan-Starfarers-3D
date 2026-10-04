using System;
using UnityEngine;
using MyAssets.GameCore.Net;

namespace MyAssets.Client.Net
{
    /// <summary>
    /// Simple service-locator for the active INetBridge.
    /// Robust against Unity-destroyed objects and duplicate scene loads.
    /// </summary>
    public static class NetService
    {
        private static INetBridge _bridge;

        public static INetBridge Bridge
        {
            get
            {
                // UnityEngine.Object null check works only for Unity objects.
                if (_bridge is UnityEngine.Object uo && uo == null)
                    _bridge = null;

                if (_bridge == null)
                    _bridge = FindBridgeInScene();

                return _bridge;
            }
            set
            {
                _bridge = value;
            }
        }

        private static INetBridge FindBridgeInScene()
        {
            // Find first enabled MonoBehaviour that implements INetBridge.
            // Works even if the concrete type lives in a different asmdef.
            var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var b in behaviours)
            {
                if (!b.isActiveAndEnabled) continue;
                if (b is INetBridge bridge) return bridge;
            }
            return null;
        }
    }
}
