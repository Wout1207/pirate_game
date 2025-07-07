using UnityEngine;
using Unity.Netcode;

public class CameraManager : NetworkBehaviour
{
    public Camera ship { get; private set; }
    public Camera UICamera { get; private set; }

    private void Start()
    {
        if (!IsOwner) return;

        ship = GameObject.Find("ShipCamera").GetComponent<Camera>();
        ship.gameObject.SetActive(false);

        GameObject.Find("UICamera").GetComponent<Camera>();
        UICamera.gameObject.SetActive(true);
    }

    public void SetMainCamera()
    {
        UICamera.gameObject.SetActive(false);
        ship.gameObject.SetActive(true);
    }

    public void SetUICamera()
    {
        ship.gameObject.SetActive(false);
        UICamera.gameObject.SetActive(true);
    }
}
