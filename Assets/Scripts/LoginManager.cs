using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance;
    public string player1;
    public string player2;
    public string hostServerID;
    public List<uint> AlternatePlayerPrefabs = new List<uint>();
    public string myUsername;
    public string ipAddress = "127.0.0.1";
    public UnityTransport transport;
    public string currentJoinCode;
    public TMPro.TextMeshProUGUI joinCodeText; 

    public class ConnectionPayload
    {
        public string username;
        public int colorPicked;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void HandleServerStarted()
    {
        Debug.Log("Server started");
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        SetInGameUIVisible(true);
    }

    public void SetInGameUIVisible(bool visible)
    {
        if (visible)
        {
            GameManager.Instance.mainMenu.SetActive(false);
            GameManager.Instance.joinMenu.SetActive(false);
            GameManager.Instance.hostMenu.SetActive(false);
            GameManager.Instance.leaveButton.SetActive(true);
            GameManager.Instance.oldCam.SetActive(false);
            GameManager.Instance.scorePanel.SetActive(true);

            if (joinCodeText != null && !string.IsNullOrEmpty(currentJoinCode))
            {
                joinCodeText.text = "Join Code: " + currentJoinCode;
                joinCodeText.gameObject.SetActive(true);
            }
        }
        else
        {
            GameManager.Instance.mainMenu.SetActive(true);
            GameManager.Instance.joinMenu.SetActive(false);
            GameManager.Instance.hostMenu.SetActive(false);
            GameManager.Instance.leaveButton.SetActive(false);
            GameManager.Instance.oldCam.SetActive(true);
            GameManager.Instance.scorePanel.SetActive(false);

            if (joinCodeText != null)
            {
                joinCodeText.gameObject.SetActive(false);
            }
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
        SetInGameUIVisible(false);
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public string GetLocalIPV4()
    {
        return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
    }

    public void SetIPAddress(string ip)
    {
        ipAddress = ip;
        transport.ConnectionData.Address = ip;
        Debug.Log($"Set IP Address to {ip}");
    }
    public void StartHost(string username, int colorPicked)
    {
        var payload = new ConnectionPayload
        {
            username = username,
            colorPicked = colorPicked
        };

        myUsername = username;
        // SetIPAddress(ipAddress);
        string payloadString = JsonUtility.ToJson(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadString);
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }
    public void DisconnectFromServer()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        NetworkManager.Singleton.Shutdown();
        GameManager.Instance.mainMenu.SetActive(true);
        GameManager.Instance.leaveButton.SetActive(false);
        GameManager.Instance.oldCam.SetActive(true);
    }

    public async Task ClientJoin(string joinCode, string username, int colorPicked)
    {
        var payload = new ConnectionPayload
        {
            username = username,
            colorPicked = colorPicked
        };

        myUsername = username;
        currentJoinCode = joinCode;

        string payloadString = JsonUtility.ToJson(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadString);

        await RelayManager.Instance.JoinRelay(joinCode);

        NetworkManager.Singleton.StartClient();
    }

    private void SetSpawnLocation(ulong clientId, NetworkManager.ConnectionApprovalResponse response)
    {
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        
        // Set the spawn location to the host's location
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            spawnPos = new Vector3(2, 25, 0);
            spawnRot = Quaternion.Euler(0, 135f, 0);
        }

        // Randomize the spawn location for the client
        else
        {
            spawnPos = new Vector3(Random.Range(-5, 5), 7, Random.Range(-5, 5));
            spawnRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }

        response.Position = spawnPos;
        response.Rotation = spawnRot;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
            bool isApproved = true;
            // Validation
            var payload = JsonUtility.FromJson<ConnectionPayload>(System.Text.Encoding.ASCII.GetString(request.Payload));

            if (payload != null)
            {
                // Debug.Log($"Payload: {payload.username} {payload.joinCode}");
                // if (string.IsNullOrEmpty(payload.username) || string.IsNullOrEmpty(payload.joinCode))
                // {
                //     Debug.Log("Invalid payload");
                //     response.Reason = "Invalid payload";
                //     isApproved = false;
                // }

                // // If the host's username is already in use, reject the connection
                // if (player1 == payload.username || player2 == payload.username)
                // {
                //     Debug.Log("Username already in use");
                //     response.Reason = "Username already in use";
                //     isApproved = false;
                // }

                // Debug.Log(NetworkManager.Singleton.IsHost);

                // if (hostServerID != payload.joinCode && request.ClientNetworkId != 0)
                // {
                //     Debug.Log("Invalid join code");
                //     response.Reason = "Invalid join code";
                //     isApproved = false;
                // }
            }

            // The client identifier to be authenticated
            var clientId = request.ClientNetworkId;

            // Additional connection data defined by user code
            var connectionData = request.Payload;

            // Your approval logic determines the following values
            response.Approved = isApproved;
            response.CreatePlayerObject = true;

            // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
            response.PlayerPrefabHash = AlternatePlayerPrefabs[payload.colorPicked];

            // Position to spawn the player object (if null it uses default of Vector3.zero)
            response.Position = Vector3.zero;

            // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
            response.Rotation = Quaternion.identity;

            SetSpawnLocation(clientId, response);

            // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
            // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
            response.Reason = "Some reason for not approving the client";

            Debug.Log($"Connection request from {clientId} approved: {isApproved}");
            if (isApproved && payload != null)
            {
                Debug.Log($"Player {clientId} approved with username {payload.username}");
                if (player1 == null)
                {
                    player1 = payload.username;
                }
                else
                {
                    player2 = payload.username;
                }
            }

            // If additional approval steps are needed, set this to true until the additional steps are complete
            // once it transitions from true to false the connection approval response will be processed.
            response.Pending = false;
    }
}
