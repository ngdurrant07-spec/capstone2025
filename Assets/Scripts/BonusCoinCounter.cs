using TMPro;
using UnityEngine;

public class BonusCoinCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private string label = "Bonus Coins: ";
    [SerializeField] private bool persistAcrossScenes = false;

    private static BonusCoinCounter instance;
    private static int totalCoins;

    void Awake()
    {
        if (persistAcrossScenes)
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            instance = this;
        }

        UpdateText();
    }

    public static void AddCoin(int amount = 1)
    {
        totalCoins += Mathf.Max(0, amount);

        if (instance != null)
            instance.UpdateText();
    }

    public static int GetTotalCoins()
    {
        return totalCoins;
    }

    private void UpdateText()
    {
        if (counterText == null)
            return;

        counterText.SetText($"{label}{totalCoins}");
    }
}
