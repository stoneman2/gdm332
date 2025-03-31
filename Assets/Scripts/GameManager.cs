using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // Main Menu
    public GameObject mainMenu;
    public GameObject hostMenu;
    public GameObject joinMenu;

    // Host Menu
    public GameObject hostServerIPInput;
    public GameObject hostUsernameInput;
    public GameObject hostMenuButton;

    // Join Menu
    public GameObject joinServerCodeInput;
    public GameObject joinUsernameInput;
    public GameObject joinMenuButton;

    // Leave
    public GameObject leaveButton;

    public GameObject oldCam;
    public GameObject redJoin;
    public GameObject blueJoin;
    public GameObject yellowJoin;
    public GameObject scorePanel;
    public GameObject scorePanelText1;
    public GameObject scorePanelText2;
    public GameObject deathCam;
    public int colorPicked = 0;
    public GameObject bombPrefab;
    public LobbyManagerScript lobbyManagerScript;
 
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
    // Host Menu
    public async void OnClickHostMenuButton()
    {
        // string serverIP = hostServerIPInput.GetComponent<TMPro.TMP_InputField>().text;
        string username = hostUsernameInput.GetComponent<TMPro.TMP_InputField>().text;
        if (/*string.IsNullOrEmpty(serverIP) ||*/ string.IsNullOrEmpty(username))
        {
            Debug.Log("Empty fields!");
            return;
        }

        // if (RelayManager.Instance.IsRelayEnabled)
        // {
        //     await RelayManager.Instance.CreateRelay();
        // }

        await lobbyManagerScript.CreateLobby();
        // LoginManager.Instance.StartHost(username, colorPicked);

    }

    // Join Menu
    public async void OnClickJoinMenuButton()
    {
        Debug.Log(joinServerCodeInput.GetComponent<TMPro.TMP_InputField>().text);
        string joinCode = joinServerCodeInput.GetComponent<TMPro.TMP_InputField>().text;
        string username = joinUsernameInput.GetComponent<TMPro.TMP_InputField>().text;

        if (string.IsNullOrEmpty(joinCode) || string.IsNullOrEmpty(username))
        {
            Debug.Log("Empty fields!");
            return;
        }

        await LoginManager.Instance.ClientJoin(joinCode, username, colorPicked);
    }

    // Menus!
    public void OnClickMainMenuHost()
    {
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);
    }

    public void OnClickMainMenuJoin()
    {
        mainMenu.SetActive(false);
        joinMenu.SetActive(true);
    }

    public void OnClickBack()
    {
        joinMenu.SetActive(false);
        hostMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OnClickColor(int color)
    {
        switch(color)
        {
        case 0:
            colorPicked = 0;
            break;
        case 1:
            colorPicked = 1;
            break;
        case 2:
            colorPicked = 2;
            break;
        }
    }

    public void OnClickLeave()
    {
        LoginManager.Instance.DisconnectFromServer();
    }
}
