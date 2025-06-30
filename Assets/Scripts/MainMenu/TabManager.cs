using UnityEngine;
using UnityEngine.UI;

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
