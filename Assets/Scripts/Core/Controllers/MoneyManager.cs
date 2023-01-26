using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinCount;
    [SerializeField] private TextMeshProUGUI diamondCount;
    [SerializeField] private TextMeshProUGUI starCount;
    [SerializeField] private TextMeshProUGUI starsPerSec;

    [SerializeField] private TextMeshProUGUI rewardCoinCount;
    [SerializeField] private TextMeshProUGUI rewardDiamondCount;

    public float Coins { get; private set; } = 0;
    public float Diamonds { get; private set; } = 0;
    public float Stars { get; private set; } = 10000;
    public float StarsPerSec { get; private set; } = 0;

    private float DIAMOND_CAP = 10000f;

    // Merge Diamond Reward

    [SerializeField] private int mergeCountThreshold = 4;
    [SerializeField] private float diamondAmountDivider = 5f;

    private float diamondMergeThreshold;
    private int mergeCount = 0;

    [SerializeField] private TextMeshProUGUI mergeDiamondAmount;
    [SerializeField] private GameObject mergeDiamondRewardPanel;
    [SerializeField] private GameObject diamondMergePanel;
    [SerializeField] private GameObject coinMergePanel;

    // Max Huggy Level Increased

    [SerializeField] private GameObject maxHuggyLevelPanel;
    [SerializeField] private Image maxHuggyLevelImage;
    [SerializeField] private TextMeshProUGUI maxHuggyLevelText;
    [SerializeField] private GameObject maxHuggyDiamondPanel;
    [SerializeField] private TextMeshProUGUI maxHuggyDiamondAmount;
    [SerializeField] private GameObject freeSubmitButton;
    [SerializeField] private GameObject adSubmitButton;

    private float maxHuggyAmount;

    private SeatManager seatManager;

    private void OnDisable()
    {
        if (seatManager == null) return;

        seatManager.MaxHuggyLevelIncreased -= RewardPlayerOnMaxHuggyLevelIncrease;
    }

    private void Start()
    {
        seatManager = GameManager.instance.SeatManager;

        // Calculate the Merge Threshold after which you provide less than 50 Diamonds on rewards
        diamondMergeThreshold = DIAMOND_CAP - 100 * diamondAmountDivider;

        coinCount.text = Utility.ConvertToGroupFormat(Coins);
        diamondCount.text = IsDiamondCountAboveThreshold() ?
            Diamonds.ToString("N2") : Utility.ConvertToGroupFormat(Mathf.RoundToInt(Diamonds));
        starCount.text = Utility.ConvertToKMB(Stars);

        // Subscribe to Events here

        seatManager.MaxHuggyLevelIncreased += RewardPlayerOnMaxHuggyLevelIncrease;
    }

    public void LoadData(float coins, float diamonds, float stars)
    {
        Coins = coins;
        Diamonds = Mathf.Clamp(diamonds, 0, DIAMOND_CAP);
        Stars = stars;
    }

    internal void AddCoins(float amount, int spawnCount = 10, Vector3 spawnPos = default)
    {
        amount = Coins + amount;
        LeanTween.value(Coins, amount, 1f).setOnUpdate(x => coinCount.text = Utility.ConvertToGroupFormat(x));
        Coins = amount;

        GameManager.instance.MagnetEffect.GenerateCurrencyEffect(spawnCount, 0, spawnPos);
    }

    internal void AddDiamonds(float amount, int spawnCount = 10, Vector3 spawnPos = default)
    {
        amount = Diamonds + amount;

        if (amount >= diamondMergeThreshold)
        {
            LeanTween.value(Diamonds, amount, 1f).setOnUpdate(x =>
            diamondCount.text = ((float)x).ToString("N2"));
        }
        else
        {
            LeanTween.value(Diamonds, amount, 1f).setOnUpdate(x =>
            diamondCount.text = Utility.ConvertToGroupFormat(Mathf.RoundToInt(x)));
        }
        
        Diamonds = amount;

        GameManager.instance.MagnetEffect.GenerateCurrencyEffect(spawnCount, 1, spawnPos);
    }

    internal void AddStars(float amount, int spawnCount = 10, bool effect = false, Vector3 spawnPos = default)
    {
        Stars += amount;

        starCount.text = Utility.ConvertToKMB(Stars);

        seatManager.CheckForSeatAmountThreshold(Stars);

        if (effect)
            GameManager.instance.MagnetEffect.GenerateCurrencyEffect(spawnCount, 2, spawnPos);
    }

    internal void IncrementStarsPerSec(float amount)
    {
        StarsPerSec += amount;

        starsPerSec.text = Utility.ConvertToKMB(StarsPerSec) + "/s";
    }

    internal void RefreshRewardCoinsAndDiamonds()
    {
        rewardCoinCount.text = coinCount.text;
        rewardDiamondCount.text = diamondCount.text;
    }

    internal void RewardPlayerOnMerge(int huggyLevel, Vector3 spawnPos)
    {
        // Reward for merging Huggies

        // Reward Coins
        AddCoins(huggyLevel * 100, 5, spawnPos);

        // Occasionally Reward Coins/Diamonds 
        mergeCount++;

        if (mergeCount >= mergeCountThreshold)
        {
            mergeCount = 0;

            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                if (!(huggyLevel > seatManager.MaxHuggyLevelUnlocked))
                {
                    PanelAnimation(coinMergePanel, true, 10000);
                    diamondMergePanel.SetActive(false);
                }
            }
            else
            {
                float amount = IsDiamondCountAboveThreshold() ?
                    (DIAMOND_CAP - Diamonds) / diamondAmountDivider : 50;

                if (!(huggyLevel > seatManager.MaxHuggyLevelUnlocked))
                {
                    mergeDiamondAmount.text = $"Reward        {Mathf.Clamp((float)Math.Round(amount, 2), .001f, DIAMOND_CAP)}";
                    PanelAnimation(diamondMergePanel, false, amount);
                    coinMergePanel.SetActive(false);
                }
            }

            void PanelAnimation(GameObject panel, bool coins, float amount)
            {
                SoundManager.instance.PlayAudioClip((int)AudioEffect.RewardCashIn);

                mergeDiamondRewardPanel.SetActive(true);
                panel.SetActive(true);
                panel.GetComponent<CanvasGroup>().LeanAlpha(1f, .3f).setFrom(0f);
                panel.LeanScale(Vector3.one, .3f).setFrom(Vector3.one * 1.5f).setOnComplete(() =>
                    panel.LeanDelayedCall(.5f, () =>
                    panel.GetComponent<CanvasGroup>().LeanAlpha(0f, .3f).setOnComplete(() =>
                        {
                            panel.SetActive(false);
                            mergeDiamondRewardPanel.SetActive(false);
                            if (coins)
                                AddCoins(amount, 20);
                            else
                                AddDiamonds(amount, 10);
                        })
                    )
                );
            }
        }
    }

    private void RewardPlayerOnMaxHuggyLevelIncrease(int huggyLevel, float seatCost)
    {
        // Display Icon
        maxHuggyLevelImage.sprite = seatManager.huggySprites[huggyLevel - 1];

        maxHuggyLevelText.text = $"Lv.{huggyLevel}";
        Utility.OpenGO(maxHuggyLevelPanel);

        // Reward Diamonds
        maxHuggyAmount = (DIAMOND_CAP - Diamonds) / diamondAmountDivider;

        if (huggyLevel > 3)
        {
            maxHuggyDiamondAmount.text = $"Reward        {Mathf.Clamp((float)Math.Round(maxHuggyAmount, IsDiamondCountAboveThreshold() ? 2 : 0), .001f, DIAMOND_CAP)}";
            adSubmitButton.SetActive(true);
            freeSubmitButton.SetActive(false);
        }
        else
        {
            freeSubmitButton.SetActive(true);
            adSubmitButton.SetActive(false);
        }

        LeanTween.delayedCall(1f, () =>
        {
            Utility.CloseGO(maxHuggyLevelPanel);
            LeanTween.delayedCall(.5f, () => Utility.OpenGO(maxHuggyDiamondPanel));
        });
    }

    public void CollectMaxHuggyLevelIncreaseReward()
    {
        if (seatManager.MaxHuggyLevelUnlocked > 3)
        {
            // Play Ad
            AdManager.instance.ShowRewardedVideo();
        }

        if (TutorialManager.TutorialOn)
        {
            GameManager.instance.TutorialManager.ProgressFocusArea();
        }

        AddDiamonds(maxHuggyAmount, 20);

        Utility.CloseGO(maxHuggyDiamondPanel);
    }

    internal bool IsDiamondCountAboveThreshold()
    {
        bool isCountAbove = Diamonds >= diamondMergeThreshold;

        if (isCountAbove) diamondAmountDivider = 10f;

        return isCountAbove;
    }
}
