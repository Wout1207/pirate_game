using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject content;
        public Image indicator;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        public Transform cameraTarget;
        [HideInInspector] public CanvasGroup canvasGroup;
    }

    public Tab[] tabs;

    private int currentTabIndex = 0;
    private MainMenuTabInputActions inputActions;
    public CameraMover cameraMover;


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
            tabs[i].canvasGroup = tabs[i].content.GetComponent<CanvasGroup>();
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
            tabs[i].indicator.sprite = (i == index) ? tabs[i].activeSprite : tabs[i].inactiveSprite;

            if (tabs[i].canvasGroup != null)
            {
                StopAllCoroutines(); // stop vorige fades
                tabs[i].canvasGroup.alpha = 0;
                tabs[i].canvasGroup.interactable = false;
                tabs[i].canvasGroup.blocksRaycasts = false;
            }
        }

        cameraMover.SetTarget(tabs[index].cameraTarget);
        StartCoroutine(FadeInAfterDelay(tabs[index].canvasGroup, 1f, 1f));
    }

    private System.Collections.IEnumerator ShowContentAfterDelay(GameObject content, float delay)
    {
        yield return new WaitForSeconds(delay);
        content.SetActive(true);
    }

    private System.Collections.IEnumerator FadeInAfterDelay(CanvasGroup group, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }
}
