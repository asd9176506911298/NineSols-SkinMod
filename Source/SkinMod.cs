using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using NineSolsAPI;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace SkinMod {
    [BepInDependency(NineSolsAPICore.PluginGUID)]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SkinMod : BaseUnityPlugin, INetEventListener {
        public static SkinMod Instance { get; private set; }

        private ConfigEntry<KeyboardShortcut> hostShortcut;
        private ConfigEntry<KeyboardShortcut> joinShortcut;

        private Harmony harmony;
        private NetManager netManager;
        private Dictionary<int, GameObject> otherPlayers = new Dictionary<int, GameObject>();
        private NetPacketProcessor packetProcessor = new NetPacketProcessor();
        private bool isServer = false;

        private void Awake() {
            RCGLifeCycle.DontDestroyForever(gameObject);
            ToastManager.Toast("Plugin is initializing...");

            Instance = this;

            harmony = Harmony.CreateAndPatchAll(typeof(Patches).Assembly);

            hostShortcut = Config.Bind("General", "Host Shortcut",
                new KeyboardShortcut(KeyCode.H), "Shortcut to host a server");
            joinShortcut = Config.Bind("General", "Join Shortcut",
                new KeyboardShortcut(KeyCode.J), "Shortcut to join a server");

            KeybindManager.Add(this, HostServer, () => hostShortcut.Value);
            KeybindManager.Add(this, JoinServer, () => joinShortcut.Value);

            ToastManager.Toast($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update() {
            netManager?.PollEvents();

            if (netManager != null && netManager.IsRunning) {
                SendPlayerPosition();
            }
        }

        private void OnDestroy() {
            harmony?.UnpatchSelf();
            netManager?.Stop();
            ToastManager.Toast("Plugin is shutting down...");
        }

        private void HostServer() {
            if (netManager != null) return;

            isServer = true;
            netManager = new NetManager(this);
            netManager.Start(9050);
            ToastManager.Toast("Hosting server...");
        }

        private void JoinServer() {
            if (netManager != null) return;

            isServer = false;
            netManager = new NetManager(this);
            netManager.Start();
            netManager.Connect("127.0.0.1", 9050, "NineSolsMP");
            ToastManager.Toast("Joining server...");
        }

        private void SendPlayerPosition() {
            GameObject player = Player.i.gameObject;
            Vector3 position = player.transform.position;

            var writer = new NetDataWriter();
            writer.Put("Position");
            writer.Put(position.x);
            writer.Put(position.y);
            writer.Put(position.z);

            netManager.SendToAll(writer, DeliveryMethod.Unreliable);
        }

        public void OnPeerConnected(NetPeer peer) {
            ToastManager.Toast($"Player connected: ");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
            ToastManager.Toast($"Player disconnected: ");
            if (otherPlayers.ContainsKey(peer.Id)) {
                Destroy(otherPlayers[peer.Id]);
                otherPlayers.Remove(peer.Id);
            }
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod) {
            string type = reader.GetString();
            if (type == "Position") {
                float x = reader.GetFloat();
                float y = reader.GetFloat();
                float z = reader.GetFloat();

                if (!otherPlayers.ContainsKey(peer.Id)) {
                    GameObject newPlayer = new GameObject($"Player_{peer.Id}");
                    otherPlayers[peer.Id] = newPlayer;
                }

                otherPlayers[peer.Id].transform.position = new Vector3(x, y, z);
            }
        }

        public void OnNetworkError(IPEndPoint endPoint, System.Net.Sockets.SocketError socketError) {
            ToastManager.Toast($"Network error: {endPoint}, Error: {socketError}");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) {
            ToastManager.Toast($"Latency update for: {latency}ms");
        }

        public void OnConnectionRequest(ConnectionRequest request) {
            if (isServer) {
                request.AcceptIfKey("NineSolsMP");
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {
            ToastManager.Toast($"Received unconnected message from {remoteEndPoint}: {messageType}");
        }
    }
}
