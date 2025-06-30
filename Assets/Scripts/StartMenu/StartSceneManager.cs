using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    public TextMeshProUGUI promptText;

    private string keyboardText = "Press SPACE to plunder the seas";
    private string gamepadText = "Press X to plunder the seas";

    private bool inputReceived = false;
    private bool usingGamepad = false;

    void Start()
    {
        promptText.text = keyboardText;
    }

    void Update()
    {
        // Detecteer laatste gebruikte device
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            if (!usingGamepad)
            {
                usingGamepad = true;
                promptText.text = gamepadText;
            }
        }
        else if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
        {
            if (usingGamepad)
            {
                usingGamepad = false;
                promptText.text = keyboardText;
            }
        }

        // Detecteer input om te starten
        if (!inputReceived)
        {
            if (usingGamepad && Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                inputReceived = true;
                LoadLobby();
            }
            else if (!usingGamepad && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                inputReceived = true;
                LoadLobby();
            }
        }
    }

    void LoadLobby()
    {
        SceneManager.LoadScene("LobbyMenu");
    }
}
