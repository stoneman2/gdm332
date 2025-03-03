using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerDeath : NetworkBehaviour
{
    private Rigidbody rb;
    public List<MonoBehaviour> scriptsDeath;
    public List<SkinnedMeshRenderer> renderersDeath;
    private ClientNetworkTransform networkTransform;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkTransform = GetComponent<ClientNetworkTransform>();
    }
    public void Respawn()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-5, 5), 7, Random.Range(-5, 5));
        Quaternion spawnRot = Quaternion.Euler(0, Random.Range(0, 360), 0);

        RespawnServerRpc(spawnPos, spawnRot);
    }

    [ServerRpc]
    private void RespawnServerRpc(Vector3 spawnPos, Quaternion spawnRot)
    {
        RespawnClientRpc(spawnPos, spawnRot);
    }
    
    [ClientRpc]
    private void RespawnClientRpc(Vector3 spawnPos, Quaternion spawnRot)
    {
        if (!IsLocalPlayer) return;
        StartCoroutine(RespawnTimer(spawnPos, spawnRot));
    }

    IEnumerator RespawnTimer(Vector3 spawnPos, Quaternion spawnRot)
    {
        SetPlayerState(false);
        yield return new WaitForSeconds(3);
        networkTransform.Teleport(spawnPos, spawnRot, Vector3.one);
        GetComponent<HPPlayerScript>().isDead = false;
        SetPlayerState(true);
    }

    private void SetPlayerState(bool state)
    {
        foreach (var script in scriptsDeath) { script.enabled = state; }
        foreach (var renderer in renderersDeath) { renderer.enabled = state; }
        GameManager.Instance.deathCam.SetActive(!state);
    }
}
