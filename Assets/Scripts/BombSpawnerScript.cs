using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombSpawnerScript : NetworkBehaviour
{
    public GameObject bombPrefab;
    private List<GameObject> spawnedBomb = new List<GameObject>();
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OwnerNetworkAnimationScript networkAnimationScript = GetComponent<OwnerNetworkAnimationScript>();
            networkAnimationScript.SetTrigger("PuttingDown");
            SpawnBombServerRpc(OwnerClientId);
        }
    }
    
    [ServerRpc]
    public void SpawnBombServerRpc(ulong clientId)
    {
        Vector3 spawnPos = transform.position + (transform.forward * -1.5f) + (transform.up * 0.8f);
        Quaternion spawnRot = transform.rotation;
        GameObject bomb = Instantiate(bombPrefab, spawnPos, spawnRot);
        spawnedBomb.Add(bomb);
        bomb.GetComponent<BombScript>().bombSpawner = this;
        bomb.GetComponent<NetworkObject>().Spawn(true);
        bomb.GetComponent<BombScript>().Owner = clientId;
    }
    
    [ServerRpc (RequireOwnership = false)]
    public void DestroyServerRpc(ulong networkObjectId)
    {
        GameObject toDestory = findBombFromNetworkId(networkObjectId);
        if (toDestory == null) return;

        toDestory.GetComponent<NetworkObject>().Despawn();
        spawnedBomb.Remove(toDestory);
        Destroy(toDestory);
    }

    private GameObject findBombFromNetworkId(ulong networkObjectId)
    {
        foreach (GameObject bomb in spawnedBomb)
        {
            ulong bombId = bomb.GetComponent<NetworkObject>().NetworkObjectId;
            Debug.Log("bombId  " + bombId);
            if (bombId == networkObjectId)
            {
                return bomb;
            }
        }
        return null;
    }
}
