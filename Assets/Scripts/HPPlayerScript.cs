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
    public List<MonoBehaviour> scriptsRagdoll;
    private bool isRagdolled = false;
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

        if (!IsLocalPlayer) return;

        // Pressing R toggles the ragdoll state
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isRagdolled)
            {
                UnragdollPlayer();
            }
            else
            {
                RagdollPlayer();
            }
        }
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


    [ServerRpc(RequireOwnership = false)]
    public void RagdollServerRpc()
    {
        Debug.Log("Ragdolling player");
        RagdollClientRpc();
    }
    [ClientRpc]
    public void RagdollClientRpc()
    {
        Debug.Log("Ragdolling player");
        BeginStateRagdoll(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnragdollServerRpc()
    {
        Debug.Log("Unragdolling player");
        UnragdollClientRpc();
    }

    [ClientRpc]
    public void UnragdollClientRpc()
    {
        Debug.Log("Unragdolling player");
        BeginStateRagdoll(false);
    }
    
    public void RagdollPlayer()
    {
        // When the player is ragdolled, spawn 4 network prefabs of the bomb around the player
        for (int i = 0; i < 4; i++)
        {
            GetComponent<BombSpawnerScript>().SpawnBombServerRpc(OwnerClientId);
        }
        RagdollServerRpc();
    }

    public void UnragdollPlayer()
    {
        UnragdollServerRpc();
    }

    public IEnumerator DestroyBomb(GameObject bomb)
    {
        yield return new WaitForSeconds(5);
    }

    public void BeginStateRagdoll(bool state)
    {
        isRagdolled = state;

        // Turn on / scripts for players
        foreach (MonoBehaviour script in scriptsRagdoll)
        {
            script.enabled = !state;
        }

        // Turn on / off freeze rotation on the rigid body
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = state ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeRotation;

        if (state)
        {
            // Let's make the player fall over
            rb.AddForce(Vector3.down * 10, ForceMode.Impulse);
            rb.AddForce(transform.forward * 5, ForceMode.Impulse);
        }

        // Reset the rotation of the player
        if (!state)
        {
            transform.rotation = Quaternion.identity;
        }
    }
}
