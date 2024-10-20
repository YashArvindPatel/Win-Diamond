using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    private readonly string _appKey = "1f77bcc8d";

    public Action WatchedVideo;

    // Interstitial Ad Logic
    [Header("Interstitial Ad Logic")]
    [SerializeField] private int lowerHuggyMergeCount = 6, upperHuggyMergeCount = 8;

    private int adCountOnHuggyMerge;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            InitializeAds();
        }
    }

    private void OnEnable()
    {
        // Init Event
        IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

        // Rewarded Ad
        IronSourceRewardedVideoEvents.onAdLoadFailedEvent += RewardedOnAdLoadFailed;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;

        // Interstitial Ad
        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;
    }

    private void OnDisable()
    {
        // Init Event
        IronSourceEvents.onSdkInitializationCompletedEvent -= SdkInitializationCompletedEvent;

        // Rewarded Ad
        IronSourceRewardedVideoEvents.onAdLoadFailedEvent -= RewardedOnAdLoadFailed;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;

        // Interstitial Ad
        IronSourceInterstitialEvents.onAdLoadFailedEvent -= InterstitialOnAdLoadFailed;
        IronSourceInterstitialEvents.onAdShowFailedEvent -= InterstitialOnAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent -= InterstitialOnAdClosedEvent;

        GameManager.instance.SeatManager.MergedHuggy -= OnHuggyMerge;
    }

    private void Start()
    {
        // Subscribe to Events here 

        GameManager.instance.SeatManager.MergedHuggy += OnHuggyMerge;
    }
    private void OnApplicationPause(bool pause)
    {
        IronSource.Agent.onApplicationPause(pause);
    }

    private void InitializeAds()
    {
        // Validate Integration
        IronSource.Agent.validateIntegration();

        // SDK init
        IronSource.Agent.init(_appKey);
    }

    private void SdkInitializationCompletedEvent()
    {
        // Rewarded Ad
        LoadRewardedVideo();

        // Interstitial Ad
        LoadInterstitialAd();
    }

    // Interstitial Ad Logic

    private void OnHuggyMerge(int huggyLevel)
    {
        if (GameManager.instance.SeatManager.MaxHuggyLevelUnlocked >= 3)
            adCountOnHuggyMerge += 1;

        if (adCountOnHuggyMerge >= upperHuggyMergeCount || (adCountOnHuggyMerge >= lowerHuggyMergeCount && UnityEngine.Random.Range(0, 2) == 0))
        {
            adCountOnHuggyMerge = 0;

            ShowInterstitialAd();
        }
    }

    // Ad Methods

    private void LoadRewardedVideo()
    {
        IronSource.Agent.loadRewardedVideo();
    }

    private void LoadInterstitialAd()
    {
        IronSource.Agent.loadInterstitial();
    }

    public void ShowInterstitialAd()
    {
        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
        }
        else
        {
            LoadInterstitialAd();
        }
    }

    public void ShowRewardedVideo()
    {
        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo();
        }
        else
        {
            LoadRewardedVideo();
        }
    }

    void RewardedOnAdLoadFailed(IronSourceError ironSourceError)
    {
        LoadRewardedVideo();
    }

    void RewardedOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
    {
        LoadRewardedVideo();
    }

    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        LoadRewardedVideo();

        WatchedVideo?.Invoke();
    }

    // Interstitial Ad Callback

    void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
    {
        LoadInterstitialAd();
    }

    void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
    {
        LoadInterstitialAd();
    }

    void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        LoadInterstitialAd();
    }
}
