using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    private Lobby currentLobby;
    private const float HeartbeatInterval = 15f;
    private float heartbeatTimer;
    private bool isReady = false;

    // Subscription callbacks
    private LobbyEventCallbacks lobbyEventCallbacks;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
    }

    public async void CreateLobbyWithRelay()
    {
        try
        {
            // Step 1: Create Relay Allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay Join Code: {joinCode}");

            // Step 2: Create Lobby with Relay Join Code in Metadata
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("TestLobby", 4, options);
            Debug.Log($"Lobby created: {currentLobby.Id}");

            // Step 3: Start Relay as Host
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.RelayServer.IpV4,
                                        (ushort)allocation.RelayServer.Port,
                                        allocation.AllocationIdBytes,
                                        allocation.Key,
                                        allocation.ConnectionData,
                                        allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();

            StartCoroutine(HeartbeatLobbyCoroutine());

            // Subscribe to lobby events
            await SubscribeToLobbyEvents();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void QuickJoinLobbyAndRelay()
    {
        try
        {
            // Step 1: Join Lobby
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log($"Joined lobby: {currentLobby.Id}");

            // Step 2: Subscribe to lobby events
            await SubscribeToLobbyEvents();

            // Step 3: Get Relay Join Code from Lobby
            if (currentLobby.Data.TryGetValue("joinCode", out var joinCodeData))
            {
                string joinCode = joinCodeData.Value;
                Debug.Log($"Found Relay Join Code: {joinCode}");

                // Step 4: Join Relay as Client
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(joinAllocation.RelayServer.IpV4,
                                            (ushort)joinAllocation.RelayServer.Port,
                                            joinAllocation.AllocationIdBytes,
                                            joinAllocation.Key,
                                            joinAllocation.ConnectionData,
                                            joinAllocation.HostConnectionData);

                NetworkManager.Singleton.StartClient();
            }
            else
            {
                Debug.LogError("Relay join code not found in Lobby data.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            CreateLobbyWithRelay();
        }
    }

    private IEnumerator HeartbeatLobbyCoroutine()
    {
        while (currentLobby != null)
        {
            heartbeatTimer += Time.deltaTime;

            if (heartbeatTimer >= HeartbeatInterval)
            {
                heartbeatTimer = 0f;
                SendHeartbeat();
            }

            yield return null;
        }
    }

    private async void SendHeartbeat()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task SetReadyStatus(bool isReady)
    {
        string readyValue = isReady ? "true" : "false";

        await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, readyValue) }
            }
        });
    }

    public void OnReadyToggleButtonClicked()
    {
        isReady = !isReady;
        _ = SetReadyStatus(isReady);

        Debug.Log($"Ready state set to: {isReady}");
    }

    public async Task StartGame()
    {
        var lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

        bool allReady = true;
        foreach (var player in lobby.Players)
        {
            if (!player.Data.TryGetValue("ready", out var readyData) || readyData.Value != "true")
            {
                allReady = false;
                break;
            }
        }

        if (!allReady)
        {
            Debug.Log("Not all players are ready!");
            return;
        }

        // Create Relay Allocation & Join Code
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Update lobby with join code and gameStarted flag
        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                { "gameStarted", new DataObject(DataObject.VisibilityOptions.Public, "true") }
            }
        });

        // Setup Relay Host transport and start host
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(allocation.RelayServer.IpV4,
                                    (ushort)allocation.RelayServer.Port,
                                    allocation.AllocationIdBytes,
                                    allocation.Key,
                                    allocation.ConnectionData,
                                    allocation.ConnectionData);

        NetworkManager.Singleton.StartHost();

        Debug.Log("Game started as host.");
    }

    // ----------------------- SUBSCRIBE TO LOBBY EVENTS ------------------------

    private async Task SubscribeToLobbyEvents()
    {
        if (lobbyEventCallbacks != null)
        {
            // Unsubscribe previous to avoid duplicates
            lobbyEventCallbacks.LobbyChanged -= OnLobbyChanged;
        }

        lobbyEventCallbacks = new LobbyEventCallbacks();
        lobbyEventCallbacks.LobbyChanged += OnLobbyChanged;

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(currentLobby.Id, lobbyEventCallbacks);

        Debug.Log("Subscribed to lobby events.");
    }

    private async void OnLobbyChanged(ILobbyChanges lobbyChanges)
    {
        Debug.Log("Lobby updated event received.");

        // Fetch latest lobby info
        var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

        if (updatedLobby.Data.TryGetValue("gameStarted", out var gameStarted) && gameStarted.Value == "true")
        {
            Debug.Log("Game started event received via updated lobby data.");
            _ = JoinGameFromLobbyAsync();
            return;
        }

        // Check if all players are ready
        bool allReady = true;

        foreach (var player in updatedLobby.Players)
        {
            if (player.Data == null ||
                !player.Data.TryGetValue("ready", out var readyData) ||
                readyData.Value != "true")
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            Debug.Log("All players ready (via updated lobby data). Starting game...");
            await StartGame();
        }
    }



    private async Task JoinGameFromLobbyAsync()
    {
        var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

        if (updatedLobby.Data.TryGetValue("joinCode", out var joinCodeData))
        {
            string joinCode = joinCodeData.Value;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.RelayServer.IpV4,
                                        (ushort)joinAllocation.RelayServer.Port,
                                        joinAllocation.AllocationIdBytes,
                                        joinAllocation.Key,
                                        joinAllocation.ConnectionData,
                                        joinAllocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();

            Debug.Log("Client started and loading game scene...");
        }
        else
        {
            Debug.LogError("Join code missing from lobby data.");
        }
    }

    private void OnDestroy()
    {
        if (lobbyEventCallbacks != null && currentLobby != null)
        {
            lobbyEventCallbacks.LobbyChanged -= OnLobbyChanged;
        }
    }

}
