using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using TMPro;

public class LobbyMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public LobbyManagerScript lobbyManagerScript;
    public Transform lobbyListContent;
    public GameObject lobbyEntry;
    public float refreshInterval = 5f;
    public List<GameObject> lobbyEntries = new List<GameObject>();
    private float refreshTimer;
    public Button joinRandomButton;
    public Button joinIDButton;
    public TMP_InputField joinIDInputField;
    // Start is called before the first frame update
    void Start()
    { 
        refreshTimer = refreshInterval;
        RefreshLobbies();

        joinRandomButton.onClick.AddListener(JoinRandomLobby);
        joinIDButton.onClick.AddListener(() => JoinLobbyByCode());
    }

    // Update is called once per frame
    void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0)
        {
            refreshTimer = refreshInterval;
            RefreshLobbies();
        }
    }

    private void ClearLobbyEntries()
    {
        foreach (GameObject entry in lobbyEntries)
        {
            Destroy(entry);
        }
        lobbyEntries.Clear();
    }

    public async void RefreshLobbies()
    {
        // Clear existing entries
        ClearLobbyEntries();
        
        // Get new lobbies
        List<Lobby> lobbies = await lobbyManagerScript.ListLobbies();
        
        // Create new entries
        foreach (Lobby lobby in lobbies)
        {
            GameObject entryGO = Instantiate(lobbyEntry, lobbyListContent);
            LobbyEntry entryUI = entryGO.GetComponent<LobbyEntry>();
            
            if (entryUI != null)
            {
                entryUI.SetLobbyInfo(lobby);
                entryUI.OnDoubleClick += () => JoinLobby(lobby);
            }
            
            lobbyEntries.Add(entryGO);
        }
    }

    private async void JoinLobby(Lobby lobby)
    {
        try
        {
            // Assuming you have a join method in your lobby manager
            await lobbyManagerScript.JoinLobby(lobby.Id);
            
            // After successful join, you might want to:
            // - Close this menu
            // - Show a loading screen
            // - Connect to the game server
            // Debug.Log($"Joined lobby: {lobby.Name}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error joining lobby: {e.Message}");
        }
    }

    private async void JoinLobbyByCode()
    {
        try
        {
            string lobbyID = joinIDInputField.text;
            if (string.IsNullOrEmpty(lobbyID))
            {
                Debug.Log("Lobby ID is empty!");
                return;
            }

            // Assuming you have a method to join a lobby by ID in your lobby manager
            await lobbyManagerScript.JoinLobbyByCode(lobbyID);
            
            // After successful join, you might want to:
            // - Close this menu
            // - Show a loading screen
            // - Connect to the game server
            // Debug.Log($"Joined lobby by ID: {lobbyID}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error joining lobby by ID: {e.Message}");
        }
    }

    private async void JoinRandomLobby()
    {
        try
        {
            // Assuming you have a method to join a random lobby in your lobby manager
            await lobbyManagerScript.QuickJoinLobby();
            
            // After successful join, you might want to:
            // - Close this menu
            // - Show a loading screen
            // - Connect to the game server
            // Debug.Log("Joined random lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error joining random lobby: {e.Message}");
        }
    }

    public void OnDestroy()
    {
        ClearLobbyEntries();
    }
}
