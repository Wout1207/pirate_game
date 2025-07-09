using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameModeItem : MonoBehaviour, IPointerEnterHandler
{
    public string modeName;
    [TextArea] public string description;
    public Sprite modeImage;

    private Button button;
    private GameModeSelector selector;

    void Awake()
    {
        button = GetComponent<Button>();
        selector = FindFirstObjectByType<GameModeSelector>();

        button.onClick.AddListener(OnClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight();  // toon info bij hover
    }

    void OnClicked()
    {
        selector.SelectGameMode(this);  // informeer hoofdscript over selectie
    }

    public void Highlight()
    {
        selector.ShowInfo(modeName, description, modeImage);
    }
}
