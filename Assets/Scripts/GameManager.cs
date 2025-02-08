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
    public GameObject hostIDInput;
    public GameObject hostUsernameInput;
    public GameObject hostMenuButton;

    // Join Menu
    public GameObject joinServerIDInput;
    public GameObject joinUsernameInput;
    public GameObject joinMenuButton;

    // Leave
    public GameObject leaveButton;

    public GameObject oldCam;
    public GameObject redJoin;
    public GameObject blueJoin;
    public GameObject yellowJoin;
    public GameObject redHost;
    public GameObject blueHost;
    public GameObject yellowHost;
    public int colorPicked = 0;
 
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
    public void OnClickHostMenuButton()
    {
        string serverID = hostIDInput.GetComponent<TMPro.TMP_InputField>().text;
        string username = hostUsernameInput.GetComponent<TMPro.TMP_InputField>().text;
        if (string.IsNullOrEmpty(serverID) || string.IsNullOrEmpty(username))
        {
            Debug.Log("Empty fields!");
            return;
        }

        LoginManager.Instance.StartHost(serverID, username, colorPicked);
    }

    // Join Menu
    public void OnClickJoinMenuButton()
    {
        string serverID = joinServerIDInput.GetComponent<TMPro.TMP_InputField>().text;
        string username = joinUsernameInput.GetComponent<TMPro.TMP_InputField>().text;

        if (string.IsNullOrEmpty(serverID) || string.IsNullOrEmpty(username))
        {
            Debug.Log("Empty fields!");
            return;
        }

        LoginManager.Instance.ClientJoin(serverID, username, colorPicked);
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
