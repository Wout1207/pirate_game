using UnityEngine;
using Unity.Netcode;

public class CameraManager : NetworkBehaviour
{
    public Camera ship { get; private set; }
    public Camera UICamera { get; private set; }
    public GameObject LoadingScreen;
}
