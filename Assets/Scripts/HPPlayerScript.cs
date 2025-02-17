using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class HPPlayerScript : NetworkBehaviour
{
    TMP_Text hpText;
    MainPlayerScript mainScript;
    public bool isDead = false;
    public NetworkVariable<int> plyHP = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // Start is called before the first frame update
    void Start()
    {
        // Check if we're the host or not
        if (OwnerClientId == 0)
        {
            hpText = GameManager.Instance.scorePanelText1.GetComponent<TMP_Text>();
        }
        else
        {
            hpText = GameManager.Instance.scorePanelText2.GetComponent<TMP_Text>();
        }

        mainScript = gameObject.GetComponent<MainPlayerScript>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            plyHP.Value = 5;
        }
    }

    // Update is called once per frame
    void Update()
    {
        hpText.text = $"{mainScript.playerName.Value}:\n{plyHP.Value}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsLocalPlayer) return;
        if (isDead) return;

        if (other.gameObject.tag == "Deathzone")
        {
            isDead = true;
            plyHP.Value--;
            GetComponent<PlayerDeath>().Respawn();
        }
    }
}
