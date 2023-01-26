using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsManager : MonoBehaviour
{
    // Shop
    [Header("Shop")]

    [SerializeField] private GameObject shopPanelGO;
    [SerializeField] private Image[] shopImages;
    [SerializeField] private GameObject[] lockedGO;
    [SerializeField] private GameObject[] buyCostGO;
    [SerializeField] private TextMeshProUGUI[] buyCostText;

    // Daily Task
    [Header("Daily Task")]

    [SerializeField] private GameObject dailyTaskReadyToCollect;
    [SerializeField] private CanvasGroup[] dailyTaskCG;
    [SerializeField] private Image[] dailyTaskFillImage;
    [SerializeField] private TextMeshProUGUI[] dailyTaskFillText;
    [SerializeField] private GameObject[] dailyTaskCollectedGO;
    [SerializeField] private Button[] dailyTaskClaimAmountGO;
    [SerializeField] private TextMeshProUGUI[] dailyTaskClaimAmountText;

    public bool[] DailyTaskCompleted { get; private set; } = new bool[6];
    public int[] DailyTaskProgress { get; private set; } = new int[6];

    private readonly int[] dailyTaskMultipliers = new int[6]
    {
        1,5,10,15,20,30
    };

    private readonly int[] dailyTaskLimit = new int[6]
    {
        1,5,20,30,10,2
    };

    private float[] dailyTaskData;

    public DateTimeOffset LoginTime { get; private set; } = DateTimeOffset.MinValue;

    // Achievement
    [Header("Achievement")]

    [SerializeField] private GameObject achievementReadyToCollect;
    [SerializeField] private CanvasGroup[] achievementCG;
    [SerializeField] private Image[] achievementFillImage;
    [SerializeField] private TextMeshProUGUI[] achievementFillText;
    [SerializeField] private GameObject[] achievementCollectedGO;
    [SerializeField] private Button[] achievementClaimAmountGO;
    [SerializeField] private TextMeshProUGUI[] achievementClaimAmountText;
    [SerializeField] private TextMeshProUGUI[] achievementDescText;

    public bool[] AchievementCompleted { get; private set; } = new bool[15];
    public int[] AchievementProgress { get; private set; } = new int[15];
    public int[] AchievementTier { get; private set; } = new int[15];

    private readonly int[,] achievementMultipliers = new int[3, 14]
    {
        { 5,10,60,30,1,2,3,4,5,6,6,6,6,6 },
        { 20,30,90,60,15,30,45,60,75,90,90,90,90,90 },
        { 50,60,150,90,30,60,90,120,150,180,180,180,180,180 }
    };

    private readonly int[,] achievementLimit = new int[3, 14]
    {
        { 5,50,1200,40,1,1,1,1,1,1,1,1,1,1},
        { 10,100,3000,80,20,20,20,20,20,20,20,20,20,20 },
        { 20,200,5000,150,50,50,50,50,50,50,50,50,50,50 }
    };

    private readonly int achievementAdLimit = 5;

    private readonly string[] leftSideText = new string[15]
    {
        "unlock ","buy ","merge ","open ","collect ","collect ","collect ","collect ","collect ","collect ",
        "collect ","collect ","collect ","collect ","watch "
    };

    private readonly string[] rightSideText = new string[15]
    {
        " animals"," animals"," animals", " boxes"," Lv.3 animal"," Lv.6 animal"," Lv.9 animal"," Lv.12 animal",
        " Lv.15 animal"," Lv.18 animal"," Lv.21 animal"," Lv.24 animal"," Lv.27 animal"," Lv.30 animal"," videos"
    };

    // Grey Scale 
    [Header("Grey Scale")]

    [SerializeField] private GameObject shopGreyScale;
    [SerializeField] private GameObject autoMergeGreyScale, autoMergeNormal;
    [SerializeField] private GameObject rewardDoubleGreyScale, rewardDoubleNormal;

    // Welcome Back
    [Header("Welcome Back")]

    [SerializeField] private GameObject welcomeBackPanel;
    [SerializeField] private GameObject welcomeBackClaimGO, welcomeBackDoubleClaimGO;
    [SerializeField] private TextMeshProUGUI welcomeBackAmountText;
    [SerializeField] private float doubleClaimThreshold = 100000f;

    public DateTimeOffset LogoutTime { get; private set; } = DateTimeOffset.MinValue;
    private float welcomeBackAmount;

    // Extra Info
    private float Amount;

    private Transform buttonPos;

    private SeatManager seatManager;

    private void OnDisable()
    {
        // Unsubscribe to Events here

        if (seatManager == null || GameManager.instance.HuggyGenerator == null || AdManager.instance == null)
            return;

        // Shop
        seatManager.MaxHuggyLevelIncreased -= DisplayHuggyInfo;

        // Daily Task
        seatManager.BuyHuggy -= DailyTaskBuyHuggy;

        seatManager.MergedHuggy -= DailyTaskMergeHuggy;

        seatManager.BuyHuggy -= DailyTaskClaimHuggy;
        seatManager.AddedHuggy -= DailyTaskClaimHuggy;
        seatManager.MergedHuggy -= DailyTaskClaimHuggy;
        GameManager.instance.HuggyGenerator.GeneratedHuggy -= DailyTaskClaimHuggy;

        seatManager.OpenedGift -= DailyTaskOpenBoxes;

        AdManager.instance.WatchedVideo -= DailyTaskWatchVideo;

        // Achievement
        seatManager.MaxHuggyLevelIncreased -= AchievementUnlockHuggy;

        seatManager.BuyHuggy -= AchievementBuyHuggy;

        seatManager.MergedHuggy -= AchievementMergeHuggy;

        seatManager.BuyHuggy -= AchievementCollectHuggy;
        seatManager.AddedHuggy -= AchievementCollectHuggy;
        seatManager.MergedHuggy -= AchievementCollectHuggy;
        GameManager.instance.HuggyGenerator.GeneratedHuggy -= AchievementCollectHuggy;

        seatManager.OpenedGift -= AchievementOpenBoxes;

        AdManager.instance.WatchedVideo -= AchievementWatchVideo;

        // Grey Scale
        seatManager.MaxHuggyLevelIncreased -= UnlockAtLvl5;
    }

    private void Start()
    {
        seatManager = GameManager.instance.SeatManager;

        // Load in DailyTaskData values if null

        if (dailyTaskData == null)
        {
            dailyTaskData = new float[31];

            dailyTaskData[0] = 0f;
            dailyTaskData[1] = 120f;

            for (int i = 2; i < dailyTaskData.Length; i++)
            {
                dailyTaskData[i] = 300f * Mathf.Pow(2f, i - 2);
            }
        }

        // Subscribe to Events here

        // Shop
        seatManager.MaxHuggyLevelIncreased += DisplayHuggyInfo;

        // Daily Task
        seatManager.BuyHuggy += DailyTaskBuyHuggy;

        seatManager.MergedHuggy += DailyTaskMergeHuggy;

        seatManager.BuyHuggy += DailyTaskClaimHuggy;
        seatManager.AddedHuggy += DailyTaskClaimHuggy;
        seatManager.MergedHuggy += DailyTaskClaimHuggy;
        GameManager.instance.HuggyGenerator.GeneratedHuggy += DailyTaskClaimHuggy;

        seatManager.OpenedGift += DailyTaskOpenBoxes;

        AdManager.instance.WatchedVideo += DailyTaskWatchVideo;

        // Achievement
        seatManager.MaxHuggyLevelIncreased += AchievementUnlockHuggy;
        seatManager.BuyHuggy += AchievementBuyHuggy;
        seatManager.MergedHuggy += AchievementMergeHuggy;

        seatManager.BuyHuggy += AchievementCollectHuggy;
        seatManager.AddedHuggy += AchievementCollectHuggy;
        seatManager.MergedHuggy += AchievementCollectHuggy;
        GameManager.instance.HuggyGenerator.GeneratedHuggy += AchievementCollectHuggy;

        seatManager.OpenedGift += AchievementOpenBoxes;

        AdManager.instance.WatchedVideo += AchievementWatchVideo;

        // Grey Scale
        seatManager.MaxHuggyLevelIncreased += UnlockAtLvl5;

        // Initialization

        InitializeShop();
        InitializeDailyTask();
        InitializeAchievement();
        UnlockAtLvl5(seatManager.MaxHuggyLevelUnlocked);
        WelcomeBackStars();
    }

    public void LoadData(long loginTime, long logoutTime, bool[] dailyTaskCompleted, int[] dailyTaskProgress,
        bool[] achievementCompleted, int[] achievementProgress, int[] achievementTier)
    {
        LoginTime = DateTimeOffset.FromUnixTimeSeconds(loginTime);
        LogoutTime = DateTimeOffset.FromUnixTimeSeconds(logoutTime);

        DailyTaskCompleted = dailyTaskCompleted;
        DailyTaskProgress = dailyTaskProgress;

        AchievementCompleted = achievementCompleted;
        AchievementProgress = achievementProgress;
        AchievementTier = achievementTier;
    }

    // Welcome Back

    private void WelcomeBackStars()
    {
        if (LogoutTime == DateTimeOffset.MinValue  || GameManager.instance.ChooseGames.NewPlayer == 1) return;

        welcomeBackAmount = (int)(DateTimeOffset.UtcNow - LogoutTime).TotalSeconds * GameManager.instance.MoneyManager.StarsPerSec;

        if (welcomeBackAmount > 9999)
        {
            welcomeBackAmountText.text = Utility.ConvertToKMB(welcomeBackAmount);

            if (welcomeBackAmount >= doubleClaimThreshold)
            {
                welcomeBackClaimGO.SetActive(false);
                welcomeBackDoubleClaimGO.SetActive(true);
            }
            else
            {
                welcomeBackClaimGO.SetActive(true);
                welcomeBackDoubleClaimGO.SetActive(false);
            }

            Utility.OpenGO(welcomeBackPanel);

            SoundManager.instance.PlayAudioClip((int)AudioEffect.RewardCashIn);
        }
        else
        {
            GameManager.instance.MoneyManager.AddStars(welcomeBackAmount);
        }
    }

    public void ClaimWelcomeBackStars(bool claimDouble = false)
    {
        //Debug.Log(welcomeBackAmount);

        if (claimDouble)
        {
            // Show Rewarded Ad and then give double claims Stars
            AdManager.instance.ShowRewardedVideo();

            GameManager.instance.MoneyManager.AddStars(welcomeBackAmount * 2, 40, true);
        }
        else
        {
            GameManager.instance.MoneyManager.AddStars(welcomeBackAmount, 20, true);
        }

        Utility.CloseGO(welcomeBackPanel);
    }

    // Grey Scale

    private void UnlockAtLvl5(int index, float amount = 0f)
    {
        if (index >= 5)
        {
            shopGreyScale.SetActive(false);
            autoMergeGreyScale.SetActive(false);
            rewardDoubleGreyScale.SetActive(false);

            autoMergeNormal.SetActive(true);
            rewardDoubleNormal.SetActive(true);

            shopGreyScale.GetComponentInParent<Button>().interactable =
             autoMergeGreyScale.GetComponentInParent<Button>().interactable =
             rewardDoubleGreyScale.GetComponentInParent<Button>().interactable = true;

            seatManager.MaxHuggyLevelIncreased -= UnlockAtLvl5;
        }
    }

    // Shop 

    public void OpenShop()
    {
        if (seatManager.MaxHuggyLevelUnlocked >= 5)
        {
            Utility.OpenGO(shopPanelGO);        
        }
        else
        {
            MessagePopup.instance.DisplayMessage("Lv.5 Unlock");
        }
    }

    private void InitializeShop()
    {
        for (int i = 0; i < seatManager.MaxHuggyLevelUnlocked; i++)
        {
            DisplayHuggyInfo(i + 1, seatManager.SeatCost[i]);
        }
    }

    internal void DisplayHuggyInfo(int huggyLevel, float amount)
    {
        shopImages[huggyLevel - 1].color = Color.white;

        lockedGO[huggyLevel - 1].SetActive(false);
        buyCostGO[huggyLevel - 1].SetActive(true);

        UpdateHuggyCostInShop(huggyLevel - 1, amount);
    }

    internal void UpdateHuggyCostInShop(int index, float amount)
    {
        buyCostText[index].text = Utility.ConvertToKMB(amount);
    }

    // Daily Task

    private void InitializeDailyTask()
    {
        // Check if its over a day since last Login, if so reset Login Time and Daily Tasks
        if ((int)(DateTimeOffset.UtcNow - LoginTime).TotalDays >= 1)
        {
            LoginTime = DateTimeOffset.UtcNow;

            for (int i = 0; i < DailyTaskCompleted.Length; i++)
            {
                DailyTaskCompleted[i] = false;
                DailyTaskProgress[i] = 0;
            }
        }

        for (int i = 0; i < DailyTaskCompleted.Length; i++)
        {
            if (DailyTaskCompleted[i])
            {
                dailyTaskCG[i].alpha = 0.5f;
                dailyTaskCollectedGO[i].SetActive(true);
                dailyTaskClaimAmountGO[i].gameObject.SetActive(false);
            }
            else if (DailyTaskProgress[i] >= dailyTaskLimit[i])
            {
                dailyTaskClaimAmountGO[i].interactable = true;

                if (!dailyTaskReadyToCollect.activeSelf)
                    dailyTaskReadyToCollect.SetActive(true);
            }
        }

        DailyTaskLogin();
        RefreshDailyTask();
    }

    public void RefreshDailyTask()
    {
        CalculateAmount();

        for (int i = 0; i < dailyTaskMultipliers.Length; i++)
        {
            if (!DailyTaskCompleted[i])
            {
                dailyTaskClaimAmountText[i].text = Utility.ConvertToKMB(Amount * dailyTaskMultipliers[i]);
            }

            dailyTaskFillImage[i].fillAmount = (float)DailyTaskProgress[i] / dailyTaskLimit[i];
            dailyTaskFillText[i].text = $"{DailyTaskProgress[i]}/{dailyTaskLimit[i]}";

        }
    }

    private void DailyTaskLogin()
    {
        if (!DailyTaskCompleted[0])
        {
            DailyTaskProgress[0] = Mathf.Clamp(DailyTaskProgress[0] + 1, 0, dailyTaskLimit[0]);

            if (DailyTaskProgress[0] >= dailyTaskLimit[0])
            {
                dailyTaskClaimAmountGO[0].interactable = true;

                if (!dailyTaskReadyToCollect.activeSelf)
                    dailyTaskReadyToCollect.SetActive(true);
            }
        }
    }

    private void DailyTaskBuyHuggy(int huggyLevel)
    {
        if (!DailyTaskCompleted[1])
        {
            DailyTaskProgress[1] = Mathf.Clamp(DailyTaskProgress[1] + 1, 0, dailyTaskLimit[1]);

            if (DailyTaskProgress[1] >= dailyTaskLimit[1])
            {
                dailyTaskClaimAmountGO[1].interactable = true;

                if (!dailyTaskReadyToCollect.activeSelf)
                    dailyTaskReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.BuyHuggy -= DailyTaskBuyHuggy;
        }
    }

    private void DailyTaskMergeHuggy(int huggyLevel)
    {
        if (!DailyTaskCompleted[2])
        {
            DailyTaskProgress[2] = Mathf.Clamp(DailyTaskProgress[2] + 1, 0, dailyTaskLimit[2]);

            if (DailyTaskProgress[2] >= dailyTaskLimit[2])
            {
                dailyTaskClaimAmountGO[2].interactable = true;

                if (!dailyTaskReadyToCollect.activeSelf)
                    dailyTaskReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.MergedHuggy -= DailyTaskMergeHuggy;
        }
    }

    private void DailyTaskClaimHuggy(int huggyLevel)
    {
        if (!DailyTaskCompleted[3])
        {
            DailyTaskProgress[3] = Mathf.Clamp(DailyTaskProgress[3] + 1, 0, dailyTaskLimit[3]);

            if (DailyTaskProgress[3] >= dailyTaskLimit[3])
            {
                dailyTaskClaimAmountGO[3].interactable = true;

                if (!dailyTaskReadyToCollect.activeSelf)
                    dailyTaskReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.BuyHuggy -= DailyTaskClaimHuggy;
            seatManager.AddedHuggy -= DailyTaskClaimHuggy;
            seatManager.MergedHuggy -= DailyTaskClaimHuggy;
            GameManager.instance.HuggyGenerator.GeneratedHuggy -= DailyTaskClaimHuggy;
        }
    }

    private void DailyTaskOpenBoxes()
    {
        if (!DailyTaskCompleted[4])
        {
            DailyTaskProgress[4] = Mathf.Clamp(DailyTaskProgress[4] + 1, 0, dailyTaskLimit[4]);

            if (DailyTaskProgress[4] >= dailyTaskLimit[4])
            {
                dailyTaskClaimAmountGO[4].interactable = true;

                if (!dailyTaskReadyToCollect.activeSelf)
                    dailyTaskReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.OpenedGift -= DailyTaskOpenBoxes;
        }
    }

    private void DailyTaskWatchVideo()
    {
        if (!DailyTaskCompleted[5])
        {
            DailyTaskProgress[5] = Mathf.Clamp(DailyTaskProgress[5] + 1, 0, dailyTaskLimit[5]);

            if (DailyTaskProgress[5] >= dailyTaskLimit[5])
            {
                dailyTaskClaimAmountGO[5].interactable = true;

                if (!dailyTaskReadyToCollect.activeSelf)
                    dailyTaskReadyToCollect.SetActive(true);
            }
        }
        else
        {
            AdManager.instance.WatchedVideo -= DailyTaskWatchVideo;
        }
    }

    public void ProvideButtonPos(Transform pos)
    {
        buttonPos = pos;
    }

    public void DailyTaskClaim(int index)
    {
        // Provide the claim reward
        GameManager.instance.MoneyManager.AddStars(Amount * dailyTaskMultipliers[index], 15, true, buttonPos.position);

        // Turn off ReadyToCollect
        if (dailyTaskReadyToCollect.activeSelf)
            dailyTaskReadyToCollect.SetActive(false);

        // Set the task as Completed
        DailyTaskCompleted[index] = true;

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.ShopInnerButtonClicked);

        // Re-initialize the Daily Task
        InitializeDailyTask();
    }

    // Achievements

    private void InitializeAchievement()
    {
        for (int i = 0; i < AchievementCompleted.Length; i++)
        {
            if (AchievementCompleted[i])
            {
                achievementCG[i].alpha = 0.5f;
                achievementCollectedGO[i].SetActive(true);
                achievementClaimAmountGO[i].gameObject.SetActive(false);
            }
            else
            {
                if (i == AchievementCompleted.Length - 1)
                {
                    if (AchievementProgress[i] >= achievementAdLimit * (AchievementTier[14] + 1))
                    {
                        achievementClaimAmountGO[i].interactable = true;

                        if (!achievementReadyToCollect.activeSelf)
                            achievementReadyToCollect.SetActive(true);
                    }
                }
                else if (AchievementProgress[i] >= achievementLimit[AchievementTier[i], i])
                {
                    achievementClaimAmountGO[i].interactable = true;

                    if (!achievementReadyToCollect.activeSelf)
                        achievementReadyToCollect.SetActive(true);
                }
            }
        }

        RefreshAchievement();
    }

    public void RefreshAchievement()
    {
        CalculateAmount();

        for (int i = 0; i < AchievementTier.Length - 1; i++)
        {
            if (!AchievementCompleted[i])
            {
                achievementClaimAmountText[i].text = Utility.ConvertToKMB(Amount * achievementMultipliers[AchievementTier[i], i]);
            }

            achievementFillImage[i].fillAmount = (float)AchievementProgress[i] / achievementLimit[AchievementTier[i], i];
            achievementFillText[i].text = $"{AchievementProgress[i]}/{achievementLimit[AchievementTier[i], i]}";
            achievementDescText[i].text = leftSideText[i] + achievementLimit[AchievementTier[i], i] + rightSideText[i];
        }

        // Refresh Watch Ad Separately if not Completed
        if (!AchievementCompleted[14])
        {
            achievementClaimAmountText[14].text = $"Lv.{AchievementTier[14] + 2} Huggy";
        }

        achievementFillImage[14].fillAmount = (float)AchievementProgress[14] / (achievementAdLimit * (AchievementTier[14] + 1));
        achievementFillText[14].text = $"{AchievementProgress[14]}/{achievementAdLimit * (AchievementTier[14] + 1)}";
        achievementDescText[14].text = leftSideText[14] + achievementAdLimit * (AchievementTier[14] + 1) + rightSideText[14];
    }

    private void AchievementUnlockHuggy(int huggyLevel, float amount)
    {
        if (!AchievementCompleted[0])
        {
            AchievementProgress[0] = Mathf.Clamp(AchievementProgress[0] + 1, 0, achievementLimit[AchievementTier[0], 0]);

            if (AchievementProgress[0] >= achievementLimit[AchievementTier[0], 0])
            {
                achievementClaimAmountGO[0].interactable = true;

                if (!achievementReadyToCollect.activeSelf)
                    achievementReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.MaxHuggyLevelIncreased -= AchievementUnlockHuggy;
        }
    }

    private void AchievementBuyHuggy(int huggyLevel)
    {
        if (!AchievementCompleted[1])
        {
            AchievementProgress[1] = Mathf.Clamp(AchievementProgress[1] + 1, 0, achievementLimit[AchievementTier[1], 1]);

            if (AchievementProgress[1] >= achievementLimit[AchievementTier[1], 1])
            {
                achievementClaimAmountGO[1].interactable = true;

                if (!achievementReadyToCollect.activeSelf)
                    achievementReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.BuyHuggy -= AchievementBuyHuggy;
        }
    }

    private void AchievementMergeHuggy(int huggyLevel)
    {
        if (!AchievementCompleted[2])
        {
            AchievementProgress[2] = Mathf.Clamp(AchievementProgress[2] + 1, 0, achievementLimit[AchievementTier[2], 2]);

            if (AchievementProgress[2] >= achievementLimit[AchievementTier[2], 2])
            {
                achievementClaimAmountGO[2].interactable = true;

                if (!achievementReadyToCollect.activeSelf)
                    achievementReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.MergedHuggy -= AchievementMergeHuggy;
        }
    }

    private void AchievementOpenBoxes()
    {
        if (!AchievementCompleted[3])
        {
            AchievementProgress[3] = Mathf.Clamp(AchievementProgress[3] + 1, 0, achievementLimit[AchievementTier[3], 3]);

            if (AchievementProgress[3] >= achievementLimit[AchievementTier[3], 3])
            {
                achievementClaimAmountGO[3].interactable = true;

                if (!achievementReadyToCollect.activeSelf)
                    achievementReadyToCollect.SetActive(true);
            }
        }
        else
        {
            seatManager.OpenedGift -= AchievementOpenBoxes;
        }
    }

    private void AchievementCollectHuggy(int huggyLevel)
    {
        if (huggyLevel % 3 != 0) return;

        int index = 3 + huggyLevel / 3;

        if (!AchievementCompleted[index])
        {
            AchievementProgress[index] = Mathf.Clamp(AchievementProgress[index] + 1, 0, achievementLimit[AchievementTier[index], index]);

            if (AchievementProgress[index] >= achievementLimit[AchievementTier[index], index])
            {
                achievementClaimAmountGO[index].interactable = true;

                if (!achievementReadyToCollect.activeSelf)
                    achievementReadyToCollect.SetActive(true);
            }
        }
    }

    private void AchievementWatchVideo()
    {
        if (!AchievementCompleted[14])
        {
            AchievementProgress[14] = Mathf.Clamp(AchievementProgress[14] + 1, 0, achievementAdLimit * (AchievementTier[14] + 1));

            if (AchievementProgress[14] >= achievementAdLimit * (AchievementTier[14] + 1))
            {
                achievementClaimAmountGO[14].interactable = true;

                if (!achievementReadyToCollect.activeSelf)
                    achievementReadyToCollect.SetActive(true);
            }
        }
        else
        {
            AdManager.instance.WatchedVideo -= AchievementWatchVideo;
        }
    }

    public void AchievementClaim(int index)
    {
        // Provide the claim reward
        GameManager.instance.MoneyManager.AddStars(Amount * achievementMultipliers[AchievementTier[index], index], 15, true, buttonPos.position);

        // Turn off ReadyToCollect
        if (achievementReadyToCollect.activeSelf)
            achievementReadyToCollect.SetActive(false);

        // Increase the tier, if max tier then set the achievement as Completed
        AchievementTier[index] = Mathf.Clamp(AchievementTier[index] + 1, 0, 3);

        if (AchievementTier[index] > 2)
        {
            AchievementCompleted[index] = true;
        }

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.ShopInnerButtonClicked);

        // Reset claim button
        achievementClaimAmountGO[index].interactable = false;

        // Re-initialize the Daily Task
        InitializeAchievement();
    }

    public void AchievementClaimForAds()
    {
        // Increase HuggyGenLevel 
        GameManager.instance.HuggyGenerator.IncreaseHuggyGenLevel();

        // Increase the tier, if max tier then set the achievement as Completed
        AchievementTier[14] = Mathf.Clamp(AchievementTier[14] + 1, 0, 29);

        if (AchievementTier[14] > 28)
        {
            AchievementCompleted[14] = true;
        }

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.ShopInnerButtonClicked);

        // Reset claim button
        achievementClaimAmountGO[14].interactable = false;

        // Re-initialize the Daily Task
        InitializeAchievement();
    }

    private void CalculateAmount()
    {
        Amount = 0;

        foreach (int level in seatManager.GridInfo())
        {
            if (level == 99 || level == 100) continue;
            Amount += dailyTaskData[level];
        }
    }

}
