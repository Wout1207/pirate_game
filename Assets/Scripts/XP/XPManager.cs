using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPManager : MonoBehaviour
{
    [Header("Level en XP")]
    public int currentLevel = 1;
    public float currentXP = 0f;
    public float baseXPRequired = 1000f;
    public float xpIncreaseFactor = 1.10f;

    [Header("UI")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;

    void Start()
    {
        UpdateUI();
    }

    void Update()
{
    if (Input.GetKeyDown(KeyCode.X))
    {
        AddXP(250f); // test: voeg 250 XP toe
    }
}

    public void AddXP(float amount)
    {
        currentXP += amount;

        while (currentXP >= GetXPNeededForLevel(currentLevel))
        {
            currentXP -= GetXPNeededForLevel(currentLevel);
            currentLevel++;
        }

        UpdateUI();
    }

    float GetXPNeededForLevel(int level)
    {
        return baseXPRequired * Mathf.Pow(xpIncreaseFactor, level - 1);
    }

    void UpdateUI()
    {
        float requiredXP = GetXPNeededForLevel(currentLevel);
        xpSlider.value = currentXP / requiredXP;
        levelText.text = currentLevel.ToString();
    }
}
