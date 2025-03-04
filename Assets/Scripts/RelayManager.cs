using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using QFSW.QC;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class RelayManager : Singleton<RelayManager>
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public UnityTransport Transport =>
        NetworkManager.Singleton.GetComponent<UnityTransport>();

    public bool IsRelayEnabled =>
        Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    [Command]
    public async Task CreateRelay()
    {
        try
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log("Join code: " + joinCode);
            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    [Command]
    public async Task JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAlloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
}
