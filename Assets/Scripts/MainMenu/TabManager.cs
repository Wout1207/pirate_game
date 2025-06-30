using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;                  // De knop
        public GameObject content;             // De bijhorende content
        public Image indicator;                // Sprite indicator onder de knop
        public Sprite activeSprite;
        public Sprite inactiveSprite;
    }

    public Tab[] tabs;

    private int currentTabIndex = 0;
    private MainMenuTabInputActions inputActions;

    void Awake()
    {
        Debug.Log("TabManager Awake");

        inputActions = new MainMenuTabInputActions();
        
        inputActions.MainMenuTabs.PreviousTab.performed += ctx =>
        {
            Debug.Log("L1 pressed");
            SwitchTab(-1);
        };

        inputActions.MainMenuTabs.NextTab.performed += ctx =>
        {
            Debug.Log("R1 pressed");
            SwitchTab(1);
        };
    }

    void OnEnable()
    {
        inputActions.MainMenuTabs.Enable();
    }

    void OnDisable()
    {
        inputActions.MainMenuTabs.Disable();
    }

    void Start()
    {
        // Voeg click listeners toe
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // nodig vanwege closure
            tabs[i].button.onClick.AddListener(() => ActivateTab(index));
        }

        ActivateTab(0); // standaard naar eerste tab
    }

    void SwitchTab(int direction)
    {
        int newIndex = (currentTabIndex + direction + tabs.Length) % tabs.Length;
        ActivateTab(newIndex);
    }

    public void ActivateTab(int index)
    {
        currentTabIndex = index;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (i == index);
            tabs[i].content.SetActive(isActive);
            tabs[i].indicator.sprite = isActive ? tabs[i].activeSprite : tabs[i].inactiveSprite;
        }
    }
}
