using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    ISession activeSession;

    ISession ActiveSession
    {
        get => activeSession;
        set
        {
            activeSession = value;
            Debug.Log($"Active session: {activeSession}");
        }
    }

    NetworkManager networkManager;

    string sessionName = "MySession";

    const string playerNamePropertyKey = "playerName";

    void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (networkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{networkManager.LocalClientId} is the session owner!");
        }
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        if (networkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }
    }

    async void Start()
    {
        try
        {
            networkManager = GetComponent<NetworkManager>();
            networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;

            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = 4
            }.WithDistributedAuthorityNetwork();

            ActiveSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
      
    async Task<Dictionary<string,PlayerProperty>> GetPlayerProperties()
    {
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty> { { playerNamePropertyKey, playerNameProperty } };
    }
    async void StartSessionHost()
    {
        var playerProperies = await GetPlayerProperties();
        var options = new SessionOptions {MaxPlayers = 2, IsLocked = false, IsPrivate = false}.WithRelayNetwork();

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");
    }

    async Task JoinSessionById(string sessionId)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        Debug.Log($"Session {ActiveSession.Id} joined!");
    }

    async Task JoinSessionByCode(string sessionCode)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Session {ActiveSession.Id} joined!");
    }

    async Task KickPlayer(string playerId)
    {
        if (!ActiveSession.IsHost) return;
        await ActiveSession.AsHost().RemovePlayerAsync(playerId);
    }

    async Task<IList<ISessionInfo>> QuerySessions()
    {
        var sessionQueryOptions = new QuerySessionsOptions();
        var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }

    async Task LeaveSession()
    {
        if (ActiveSession != null)
        {
            try
            {
                await ActiveSession.LeaveAsync();
            }
            catch
            {

            }
            finally
            {
                ActiveSession = null;
            }
        }
    }
}
