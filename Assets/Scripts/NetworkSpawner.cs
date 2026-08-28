using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private float despawnDelay = 5.0f;

    private void Update()
    {
        // Only allow the Server / Host to trigger spawns
        if (!IsServer) return;

        // Press 'S' to spawn a networked object
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnNetworkObject();
        }
    }

    private void SpawnNetworkObject()
    {
        // 1. Instantiate locally on the server
        Vector3 randomPosition = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        GameObject instance = Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);

        // 2. Obtain the NetworkObject component
        NetworkObject netObj = instance.GetComponent<NetworkObject>();

        // 3. Spawn across the network so all Clients replicate it
        netObj.Spawn();

        // 4. Automatically despawn and destroy after a delay
        StartCoroutine(DespawnRoutine(netObj));
    }

    private IEnumerator DespawnRoutine(NetworkObject netObj)
    {
        yield return new WaitForSeconds(despawnDelay);

        if (netObj != null && netObj.IsSpawned)
        {
            // Despawn unbinds it from the network and destroys the GameObject across all clients
            netObj.Despawn(true);
        }
    }
}