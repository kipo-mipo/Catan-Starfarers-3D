using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace MyAssets.Client.Net
{
    /// <summary>
    /// LAN discovery via UDP broadcast with NO port-binding conflicts:
    /// - Host responder binds to discoveryPort (well-known).
    /// - Client probe uses an ephemeral local port and listens for replies on that SAME socket.
    /// This allows: (1) host + client in the same process, and (2) multiple clients on one machine.
    /// </summary>
    public sealed class StarfarersLanDiscovery : MonoBehaviour
    {
        [Header("Ports")]
        [SerializeField] private int discoveryPort = 47777;

        [Header("Protocol")]
        [SerializeField] private string probe = "STARFARERS_DISCOVER";
        [SerializeField] private string reply = "STARFARERS_HOST";

        private UdpClient _hostListener;     // bound to discoveryPort (host only)
        private bool _isHostingResponder;

        private UdpClient _clientSocket;     // bound to ephemeral port (client probe + reply receive)

        public void StartHostResponder()
        {
            if (_isHostingResponder) return;

            try
            {
                // Allow quick restart on some platforms
                _hostListener = new UdpClient();
                _hostListener.EnableBroadcast = true;
                _hostListener.ExclusiveAddressUse = false;
                _hostListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _hostListener.Client.Bind(new IPEndPoint(IPAddress.Any, discoveryPort));

                _isHostingResponder = true;
                BeginReceiveHost();
                Debug.Log($"[StarfarersLanDiscovery] Host responder listening on UDP {discoveryPort}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[StarfarersLanDiscovery] Failed to start host responder: {e}");
                StopHostResponder();
            }
        }

        public void StopHostResponder()
        {
            _isHostingResponder = false;
            try { _hostListener?.Close(); } catch { }
            _hostListener = null;
        }

        private void OnDestroy()
        {
            StopHostResponder();
            try { _clientSocket?.Close(); } catch { }
            _clientSocket = null;
        }

        /// <summary>
        /// Sends a broadcast probe FROM an ephemeral port.
        /// Host replies to that ephemeral port, and we read the reply from the same socket.
        /// </summary>
        public void BroadcastProbe()
        {
            try
            {
                EnsureClientSocket();

                var data = Encoding.UTF8.GetBytes(probe);
                var ep = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
                _clientSocket.Send(data, data.Length, ep);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[StarfarersLanDiscovery] BroadcastProbe failed: {e.Message}");
            }
        }

        /// <summary>
        /// Waits for a host reply on the client socket.
        /// </summary>
        public bool TryReceiveHostReply(int timeoutMs, out string hostIp)
        {
            hostIp = null;

            try
            {
                EnsureClientSocket();
                _clientSocket.Client.ReceiveTimeout = Mathf.Max(1, timeoutMs);

                IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
                var bytes = _clientSocket.Receive(ref from);
                var msg = Encoding.UTF8.GetString(bytes);

                if (msg.StartsWith(reply, StringComparison.Ordinal))
                {
                    hostIp = from.Address.ToString();
                    return true;
                }
            }
            catch (SocketException)
            {
                // timeout: normal
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[StarfarersLanDiscovery] TryReceiveHostReply failed: {e.Message}");
            }

            return false;
        }

        private void EnsureClientSocket()
        {
            if (_clientSocket != null) return;

            // Bind to an ephemeral local port so multiple instances/clients can coexist.
            _clientSocket = new UdpClient(0);
            _clientSocket.EnableBroadcast = true;
        }

        private void BeginReceiveHost()
        {
            if (!_isHostingResponder || _hostListener == null) return;
            try { _hostListener.BeginReceive(OnReceiveHost, null); } catch { }
        }

        private void OnReceiveHost(IAsyncResult ar)
        {
            if (!_isHostingResponder || _hostListener == null) return;

            IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
            byte[] bytes = null;

            try { bytes = _hostListener.EndReceive(ar, ref from); } catch { }
            BeginReceiveHost();

            if (bytes == null || bytes.Length == 0) return;

            var msg = Encoding.UTF8.GetString(bytes);
            if (!msg.StartsWith(probe, StringComparison.Ordinal)) return;

            try
            {
                var payload = Encoding.UTF8.GetBytes(reply);
                _hostListener.Send(payload, payload.Length, from);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[StarfarersLanDiscovery] Reply send failed: {e.Message}");
            }
        }
    }
}
