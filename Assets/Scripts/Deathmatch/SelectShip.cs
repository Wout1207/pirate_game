using Unity.Netcode;
using UnityEngine;

public class SelectShip : NetworkBehaviour
{
    private void Start()
    {
        if(!IsOwner)
        {
            Destroy(transform.parent.gameObject);
        }
    }
    public void Select(GameObject shipPrefab)
    {
        GameObject ship = Instantiate(shipPrefab);
        ship.GetComponent<NetworkObject>().Spawn();
        Destroy(transform.parent.gameObject);
    }
}
