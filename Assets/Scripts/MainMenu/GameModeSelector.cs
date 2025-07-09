using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameModeSelector : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image previewImage;


    public GameObject[] gameModeItems; // De knoppen of hun GameObjects
    private int currentIndex = 0;
    private string selectedGameMode = "";

    void Start()
    {
        // Zet eerste geselecteerde
        if (gameModeItems.Length > 0)
            SelectIndex(0);
    }

    void Update()
    {
        var gamepad = Gamepad.current;

        if (gamepad != null)
        {
            if (gamepad.dpad.up.wasPressedThisFrame)
                SelectIndex(currentIndex - 1);
            else if (gamepad.dpad.down.wasPressedThisFrame)
                SelectIndex(currentIndex + 1);
            else if (gamepad.buttonSouth.wasPressedThisFrame)
                TriggerCurrent();
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                SelectIndex(currentIndex - 1);
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                SelectIndex(currentIndex + 1);
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
                TriggerCurrent();
        }
    }

    void SelectIndex(int index)
    {
        if (gameModeItems.Length == 0) return;

        currentIndex = Mathf.Clamp(index, 0, gameModeItems.Length - 1);
        var go = gameModeItems[currentIndex];

        var button = go.GetComponent<Button>();
        if (button != null)
        {
            button.Select();  // focus voor controller
            button.OnSelect(null); // trigger info update
        }

        var item = go.GetComponent<GameModeItem>();
        if (item != null)
            item.Highlight();
    }

    void TriggerCurrent()
    {
        var go = gameModeItems[currentIndex];
        var button = go.GetComponent<Button>();
        button?.onClick.Invoke();
    }

    public void SelectGameMode(GameModeItem item)
    {
        Debug.Log($"Selected game mode: {item.modeName}");
        selectedGameMode = item.modeName;
    }

    public void StartGameMode(){
        SceneManager.LoadScene(selectedGameMode);
    }

    public void ShowInfo(string title, string description, Sprite preview)
{
    titleText.text = title;
    descriptionText.text = description;

    if (previewImage != null)
    {
        previewImage.sprite = preview;
        previewImage.enabled = (preview != null); // show or hide image
    }
}

}
