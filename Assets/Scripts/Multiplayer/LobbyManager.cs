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

public class LobbyManager : MonoBehaviour
{
    private Lobby currentLobby;
    private const float HeartbeatInterval = 15f;
    private float heartbeatTimer;

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
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("TestLobby", 4, options);
            Debug.Log($"Lobby created: {currentLobby.Id}");

            // Step 3: Start Relay as Host
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.ConnectionData);


            NetworkManager.Singleton.StartHost();

            StartCoroutine(HeartbeatLobbyCoroutine());
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

            // Step 2: Get Relay Join Code from Lobby
            if (currentLobby.Data.TryGetValue("joinCode", out var joinCodeData))
            {
                string joinCode = joinCodeData.Value;
                Debug.Log($"Found Relay Join Code: {joinCode}");

                // Step 3: Join Relay as Client
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData
);

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
}