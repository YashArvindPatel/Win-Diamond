using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Reward : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private float maxAmount;

    // Reward
    [SerializeField] private bool diamond = false;

    // Skin
    [SerializeField] private bool skin = false;

    private int parentIndex, selfIndex;

    private void OnEnable()
    {
        RefreshInfo();
    }

    private void Awake()
    {
        if (skin)
        {
            parentIndex = transform.parent.parent.GetSiblingIndex();
            selfIndex = transform.GetSiblingIndex();
        }
    }

    internal void RefreshInfo()
    {
        float amount = 0;

        if (skin)
        {
            amount = Mathf.Clamp(GameManager.instance.RewardManager.SkinPieceCount[parentIndex][selfIndex], 0, maxAmount);

            text.text = Utility.ConvertToGroupFormat(amount) + "/" + Utility.ConvertToGroupFormat(maxAmount);
        }
        else
        {
            amount = Mathf.Clamp(diamond ? GameManager.instance.MoneyManager.Diamonds : GameManager.instance.MoneyManager.Coins, 0, maxAmount);

            text.text = Utility.ConvertToGroupFormat(amount) + "/<color=grey>" + Utility.ConvertToGroupFormat(maxAmount);
        }

        image.fillAmount = amount / maxAmount;
    }

    public void CollectReward()
    {
        if (skin)
        {
            if (GameManager.instance.RewardManager.SkinPieceCount[parentIndex][selfIndex] >= maxAmount)
            {
                // Game Crash
                GameCrash();
            }
            else
            {
                GameManager.instance.RewardManager.OpenMorePiecesPanel(this, parentIndex, selfIndex);
            }
        }
        else
        {
            if (diamond)
            {
                if (GameManager.instance.MoneyManager.Diamonds >= maxAmount * .9999f)
                {
                    // Game Crash
                    GameCrash();
                }
                else
                {
                    MessagePopup.instance.DisplayMessage("Not enough Diamonds");
                }
            }
            else
            {
                if (GameManager.instance.MoneyManager.Coins >= maxAmount)
                {
                    // Game Crash
                    GameCrash();
                }
                else
                {
                    MessagePopup.instance.DisplayMessage("Not enough Coins");
                }
            }          
        }
    }

    private void GameCrash()
    {
        SaveSystem.ResetInfo();
        GameManager.instance.SaveData = false;
        Application.Quit();
    }
}
