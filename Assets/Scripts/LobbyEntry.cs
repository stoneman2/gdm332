using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;

public class LobbyEntry : MonoBehaviour
{
    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI playerCountText;
    public TextMeshProUGUI gameModeText;
    private Lobby lobbyInfo;
    private float lastClickTime;
    private float doubleClickTime = 0.3f;
    public event Action OnDoubleClick;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        // Check for double click
        if (Time.time - lastClickTime < doubleClickTime)
        {
            // Double click detected
            OnDoubleClick?.Invoke();
        }
        
        lastClickTime = Time.time;
    }

    public void SetLobbyInfo(Lobby lobby)
    {
        lobbyInfo = lobby;

        if (lobbyNameText != null)
        {
            lobbyNameText.text = lobby.Name;
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }

        if (gameModeText != null && lobby.Data.ContainsKey("GameMode"))
        {
            gameModeText.text = lobby.Data["GameMode"].Value;
        }
    }
}
