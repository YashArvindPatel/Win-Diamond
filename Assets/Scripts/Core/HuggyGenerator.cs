using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HuggyGenerator : MonoBehaviour
{
    // Auto Generate Huggy
    [Header("Auto Generate Huggy")]

    [SerializeField] private Image huggyGenImg;
    [SerializeField] private RectTransform huggyGenRT;
    [SerializeField] private Image fillImg;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerGO; 
    [SerializeField] private GameObject seatFULLGO;
    [SerializeField] private int setSecToGen = 10;
    [SerializeField] private Sprite giftSprite;

    private int secToGen = 10;
    private bool readyToGen = false;
    private bool generateGift = false;

    private SeatManager seatManager;

    private int tweenId;
    private float fillAmt;

    public Action<int> GeneratedHuggy;

    public int HuggyGenLvl { get; private set; } = 1;

    // Auto Merge Huggy
    [Header("Auto Merge Huggy")]

    [SerializeField] private int setSecToMerge = 180;
    public int SecToMerge { get; private set; } = 180;

    [SerializeField][Range(1,5)] private int ticksPerSec = 2;
    [SerializeField] private GameObject mergeText;
    [SerializeField] private GameObject mergeTimerTextHolder;
    [SerializeField] private TextMeshProUGUI mergeTimerText;
    [SerializeField] private GameObject autoMergePanelGO;
    [SerializeField] private GameObject mergeRainbowGO;
    [SerializeField] private RectTransform mergeRainbowRT;

    public static bool AUTO_MERGE = false;
    private bool autoMergePaused = false;

    // Reward x2
    [Header("Reward x2")]

    [SerializeField] private int setSecToDoubleReward = 7200;

    public int SecToDoubleReward { get; private set; } = 7200;
    public static bool DOUBLE_REWARD = false;

    [SerializeField] private GameObject doubleRewardText;
    [SerializeField] private GameObject doubleRewardTimerTextHolder;
    [SerializeField] private TextMeshProUGUI doubleRewardTimerText;
    [SerializeField] private GameObject doubleRewardPanelGO;
    [SerializeField] private GameObject doubleRewardRainbowGO;
    [SerializeField] private RectTransform doubleRewardRainbowRT;

    [Header("RAINBOW TIME")]

    [SerializeField] private float rainbowTime = 1f;
    [SerializeField] private float rainbowFrom = 106.066f;
    [SerializeField] private float rainbowTo = -58.2f;

    // Chest Reward
    [Header("Chest Reward")]

    [SerializeField] private GameObject chestPanel;
    [SerializeField] private GameObject coinRewardGO;
    [SerializeField] private GameObject diamondRewardGO;
    [SerializeField] private GameObject chestOpenTextGO;
    [SerializeField] private GameObject chestGlowGO;
    [SerializeField] private TextMeshProUGUI chestCooldownText;
    [SerializeField] private int setSecToChestReset = 180;
    [SerializeField] private Animator chestAnimator;

    public int SecToChestReset { get; private set; } = 180;
    private bool chestOnCooldown = false;

    private bool rewardCoins = true;

    private void OnEnable()
    {
        Timer.TickEvent += GenerateHuggyOnTimer;
        Utility.PanelOpened += PauseAutoMerge;
        Utility.PanelClosed += UnpauseAutoMerge;
    }

    private void OnDisable()
    {
        Timer.TickEvent -= GenerateHuggyOnTimer;
        Utility.PanelOpened -= PauseAutoMerge;
        Utility.PanelClosed -= UnpauseAutoMerge;
    }

    private void Start()
    {
        if (seatManager == null) seatManager = GameManager.instance.SeatManager;

        UpdateHuggyGenIcon();
    }

    internal void IsNewPlayer()
    {
        secToGen = 1;
        fillImg.fillAmount = .9f;
        timerText.text = $"{secToGen}";
        seatManager = GameManager.instance.SeatManager;
        this.enabled = false;
    }

    public void LoadData(int secToMerge, int secToDoubleReward, int secToChestReset, int huggyGenLvl)
    {
        seatManager = GameManager.instance.SeatManager;

        SecToMerge = secToMerge;

        if (SecToMerge < setSecToMerge)
        {
            AutoMergeHuggy();
        }

        SecToDoubleReward = secToDoubleReward;

        if (SecToDoubleReward < setSecToDoubleReward)
        {
            DoubleRewardStars();
        }

        SecToChestReset = secToChestReset;

        if (SecToChestReset < setSecToChestReset)
        {
            StartChestRewardCooldown();
        }

        HuggyGenLvl = huggyGenLvl;
    }

    public void UpgradeHuggyGenerator()
    {
        IncreaseHuggyGenLevel();
    }

    internal void IncreaseHuggyGenLevel()
    {
        HuggyGenLvl = Mathf.Clamp(HuggyGenLvl + 1, 1, 30);
    }

    private void UpdateHuggyGenIcon()
    {
        huggyGenImg.sprite = fillImg.sprite = seatManager.huggySprites[HuggyGenLvl - 1];

        huggyGenRT.anchoredPosition = new Vector2(0, 12f);
    }

    private void UpdateGiftGenIcon()
    {
        huggyGenImg.sprite = fillImg.sprite = giftSprite;

        huggyGenRT.anchoredPosition = new Vector2(0, -12f);
    }

    // Chest Reward

    public void OpenChestRewardPanel()
    {
        if (!chestOnCooldown)
        {
            Utility.OpenGO(chestPanel);

            // Swap Rewards depending on Random chance or if above certain threshold
            rewardCoins = GameManager.instance.MoneyManager.IsDiamondCountAboveThreshold() ? true : !rewardCoins;
            coinRewardGO.SetActive(rewardCoins);
            diamondRewardGO.SetActive(!rewardCoins);

            SoundManager.instance.PlayAudioClip((int)AudioEffect.RewardCashIn);
        }
        else
        {
            // Inform User via Message
            MessagePopup.instance.DisplayMessage("Chest on cooldown");

            //Debug.Log("Chest Reward on cooldown");
        }
    }

    public void CollectChestReward()
    {
        // Reward Player 
        if (rewardCoins)
            GameManager.instance.MoneyManager.AddCoins(5000, 15);
        else
            GameManager.instance.MoneyManager.AddDiamonds(20, 10);

        Utility.CloseGO(chestPanel);

        StartChestRewardCooldown();
    }

    private void StartChestRewardCooldown()
    {
        Timer.TickEvent += ChestRewardCooldown;
        chestOnCooldown = true;

        chestOpenTextGO.SetActive(false);
        chestCooldownText.gameObject.SetActive(true);
        chestGlowGO.SetActive(false);
        chestAnimator.SetBool("CD", true);
    }

    private void ChestRewardCooldown()
    {
        SecToChestReset -= 1;

        chestCooldownText.text = Utility.ConvertToMMSS(SecToChestReset);

        if (SecToChestReset <= 0)
        {
            SecToChestReset = setSecToChestReset;

            chestOpenTextGO.SetActive(true);
            chestCooldownText.gameObject.SetActive(false);
            chestGlowGO.SetActive(true);
            chestAnimator.SetBool("CD", false);

            chestOnCooldown = false;
            Timer.TickEvent -= ChestRewardCooldown;
        }
    }

    // Auto Generate Huggy

    public void ClickGenerateHuggy()
    {
        if (readyToGen && seatManager.FULL)
        {
            if (seatManager.SeatCount < 25)
            {
                seatManager.OpenMoreSeatsPanel();
            }
            else
            {
                //Debug.Log("No more Seats");

                MessagePopup.instance.DisplayMessage("Not enough Seats");
            }
        }
        else
        {
            GenerateHuggyOnTimer();

            SoundManager.instance.PlayAudioClip((int)AudioEffect.CloseButtonClicked);
        }
    }

    private void GenerateHuggyOnTimer()
    {
        if (!readyToGen)
        {
            LeanTween.cancel(tweenId);
            fillImg.fillAmount = fillAmt;

            timerText.text = $"{secToGen}";

            secToGen -= 1;

            fillAmt = (float)(setSecToGen - secToGen) / setSecToGen;

            tweenId = LeanTween.value(fillImg.fillAmount, fillAmt, 1f).setOnUpdate(x => fillImg.fillAmount = x).uniqueId;

            if (secToGen <= 0)
            {
                if (seatManager.FULL)
                {
                    timerGO.SetActive(false);
                    seatFULLGO.SetActive(true);

                    readyToGen = true;
                }
                else
                {
                    GenerateHuggyNow();
                }
            }
        }
    }

    private void Update()
    {
        // Auto Generate Huggy

        if (readyToGen && !seatManager.FULL)
        {
            GenerateHuggyNow();
        }
    }

    private void GenerateHuggyNow()
    {
        if (generateGift)
        {
            seatManager.AddGiftInEmptySeat();
        }
        else
        {
            seatManager.AddHuggyInEmptySeat(HuggyGenLvl, spawnIntro: 1);
            GeneratedHuggy?.Invoke(HuggyGenLvl);
        }
 
        if (seatManager.MaxHuggyLevelUnlocked >= 5 && UnityEngine.Random.Range(0,2) == 0)
        {
            generateGift = true;
            UpdateGiftGenIcon();
        }
        else
        {
            generateGift = false;
            UpdateHuggyGenIcon();
        }

        // Play Huggy Generated Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.HuggyGeneratedSound);

        // Reset Info
        LeanTween.cancel(tweenId);
        fillAmt = fillImg.fillAmount = 0f;

        seatFULLGO.SetActive(false);
        timerGO.SetActive(true);

        secToGen = setSecToGen;
        readyToGen = false;
    }

    // Auto Merge Huggy

    public void OpenAutoMergePanel()
    {
        if (seatManager.MaxHuggyLevelUnlocked >= 5)
        {
            if (!AUTO_MERGE)
            {
                Utility.OpenGO(autoMergePanelGO);

                SoundManager.instance.PlayAudioClip((int)AudioEffect.AutoMergeClicked);
            }
        }
        else
        {
            MessagePopup.instance.DisplayMessage("Lv.5 Unlock");

            SoundManager.instance.PlayAudioClip((int)AudioEffect.CoinDoubleClicked);
        }
    }

    public void AutoMergeHuggyClicked()
    {
        // Play Ad
        AdManager.instance.ShowRewardedVideo();

        AutoMergeHuggy();
    }

    private void AutoMergeHuggy()
    {
        Timer.TickEvent += MergeHuggyOnTimer;
        AUTO_MERGE = true;
        mergeTimerTextHolder.SetActive(true);

        mergeRainbowGO.SetActive(true);
        mergeRainbowRT.LeanMoveLocalY(rainbowTo, rainbowTime).setFrom(rainbowFrom).setLoopClamp();

        mergeText.SetActive(false);

        // Stop Helper Hand
        seatManager.StopHelperHand();
    }

    private void MergeHuggyOnTimer()
    {
        if (autoMergePaused) return;

        SecToMerge -= 1;

        mergeTimerText.text = Utility.ConvertToMMSS(SecToMerge);

        StartCoroutine(Merger(ticksPerSec));

        if (SecToMerge <= 0)
        {        
            SecToMerge = setSecToMerge;

            mergeTimerTextHolder.SetActive(false);

            mergeRainbowGO.SetActive(false);
            LeanTween.cancel(mergeRainbowRT);

            mergeText.SetActive(true);

            AUTO_MERGE = false;
            Timer.TickEvent -= MergeHuggyOnTimer;
        }
    }

    IEnumerator Merger(int ticks)
    {
        float timer = (float)1 / ticks;

        while (ticks > 0)
        {
            seatManager.MergeCommonHuggies();

            ticks -= 1;

            yield return new WaitForSeconds(timer);
        }
    }

    private void PauseAutoMerge()
    {
        autoMergePaused = true;
    }

    private void UnpauseAutoMerge()
    {
        autoMergePaused = false;
    }

    // Reward x2 Stars

    public void OpenDoubleRewardPanel()
    {
        if (seatManager.MaxHuggyLevelUnlocked >= 5)
        {
            if (!DOUBLE_REWARD)
            {
                Utility.OpenGO(doubleRewardPanelGO);

                SoundManager.instance.PlayAudioClip((int)AudioEffect.CoinDoubleClicked);
            }
        }
        else
        {
            MessagePopup.instance.DisplayMessage("Lv.5 Unlock");

            SoundManager.instance.PlayAudioClip((int)AudioEffect.CoinDoubleClicked);
        }
    }

    public void DoubleRewardStarsClicked()
    {
        // Play Ad
        AdManager.instance.ShowRewardedVideo();

        DoubleRewardStars();
    }

    private void DoubleRewardStars()
    {
        Timer.TickEvent += DoubleRewardTimer;
        DOUBLE_REWARD = true;
        doubleRewardTimerTextHolder.SetActive(true);

        doubleRewardRainbowGO.SetActive(true);
        doubleRewardRainbowRT.LeanMoveLocalY(rainbowTo, rainbowTime).setFrom(rainbowFrom).setLoopClamp();

        doubleRewardText.SetActive(false);   
    }

    private void DoubleRewardTimer()
    {
        SecToDoubleReward -= 1;

        doubleRewardTimerText.text = Utility.ConvertToHHMMSS(SecToDoubleReward);

        if (SecToDoubleReward <= 0)
        {
            SecToDoubleReward = setSecToDoubleReward;

            doubleRewardTimerTextHolder.SetActive(false);

            doubleRewardRainbowGO.SetActive(false);
            LeanTween.cancel(doubleRewardRainbowRT);

            doubleRewardText.SetActive(true);
            DOUBLE_REWARD = false;
            Timer.TickEvent -= DoubleRewardTimer;
        }
    }
}
