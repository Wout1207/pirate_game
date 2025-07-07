using Unity.Netcode;
using UnityEngine;

public class SelectShip : NetworkBehaviour
{
    public void Select(GameObject shipPrefab)
    {
        GameObject ship = Instantiate(shipPrefab);
        ship.GetComponent<NetworkObject>().Spawn();
        Destroy(transform.parent.gameObject);
    }
}
