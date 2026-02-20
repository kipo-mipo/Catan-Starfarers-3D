using UnityEngine;
using MyAssets.GameCore.Net;

namespace MyAssets.Client.Net
{
    public static class NetService
    {
        private static INetBridge _bridge;

        public static INetBridge Bridge
        {
            get
            {
                if (_bridge != null) return _bridge;

                foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (mb is INetBridge b)
                    {
                        _bridge = b;
                        return _bridge;
                    }
                }

                Debug.LogError("INetBridge not found. Add MirrorNetBridge to a bootstrap prefab in the scene.");
                return null;
            }
        }
    }
}
