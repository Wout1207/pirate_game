using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelectShip : NetworkBehaviour
{
    [SerializeField] private GameObject selectionUI;  // Reference to just the UI GameObject
    [SerializeField] private List<GameObject> shipPrefabs;

    private void Start()
    {
        selectionUI = transform.parent.gameObject;
        if (!IsOwner)
        {
            selectionUI.SetActive(false);  // Hide UI for non-owner
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Select(int prefabIndex)
    {
        if (!IsOwner) return;
        if (prefabIndex < 0 || prefabIndex >= shipPrefabs.Count) return;

        SelectShipServerRpc(prefabIndex);
        selectionUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void SelectShipServerRpc(int prefabIndex, ServerRpcParams rpcParams = default)
    {
        if (prefabIndex < 0 || prefabIndex >= shipPrefabs.Count) return;

        GameObject prefab = shipPrefabs[prefabIndex];
        GameObject ship = Instantiate(prefab);

        ship.GetComponent<NetworkObject>().SpawnWithOwnership(rpcParams.Receive.SenderClientId);
    }

}
