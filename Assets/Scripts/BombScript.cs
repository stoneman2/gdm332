using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombScript : NetworkBehaviour
{
    public BombSpawnerScript bombSpawner;
    public GameObject effectPrefab;
    public ulong Owner { get; set; }
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.tag == "Player")
        { 
            ulong networkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
            SpawnEffect();
            bombSpawner.DestroyServerRpc(networkObjectId);
        }
    }
    
    void SpawnEffect()
    {
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        effect.GetComponent<NetworkObject>().Spawn();

        Destroy(effect, 2);
    }
}
