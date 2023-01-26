using UnityEngine;
using UnityEngine.Advertisements;
using System;
using System.Collections;

public class AdManager : MonoBehaviour, IUnityAdsListener
{
    public static AdManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    [SerializeField] private bool testMode = true, showAds = true;
    [SerializeField] private string gameId = "4616979";
    private string rewarded_placement = "Rewarded_Android";
    private string interstitial_placement = "Interstitial_Android";

    public Action WatchedVideo;

    // Interstitial Ad Logic
    [Header("Interstitial Ad Logic")]
    [SerializeField] private bool showAdOnStartOfGame = false;
    [SerializeField] private int lowerHuggyMergeCount = 6, upperHuggyMergeCount = 8;

    private int adCountOnHuggyMerge;

    private void OnDisable()
    {
        GameManager.instance.SeatManager.MergedHuggy -= OnHuggyMerge;
    }

    private void Start()
    {
        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);

        if (Advertisement.isInitialized)
        {
            Advertisement.Load(rewarded_placement);
            Advertisement.Load(interstitial_placement);
        }

        if (showAdOnStartOfGame && GameManager.instance.ChooseGames.NewPlayer == 0)
        {
            StartCoroutine(TryShowingInterstitialAd());
        }

        // Subscribe to Events here 

        GameManager.instance.SeatManager.MergedHuggy += OnHuggyMerge;
    }

    // Interstitial Ad Logic
    IEnumerator TryShowingInterstitialAd()
    {
        while (true)
        {
            if (Advertisement.IsReady(interstitial_placement))
            {
                ShowInterstitialAd();
                break;
            }

            yield return null;
        }
    }

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

    // Ad Logic
    public void ShowInterstitialAd()
    {
        if (showAds && Advertisement.IsReady(interstitial_placement))
        {
            //Debug.Log("Playing Interstitial Ad");

            Advertisement.Show(interstitial_placement);
        }
        else
        {
            //Debug.Log("Loading Interstitial Ad");

            Advertisement.Load(interstitial_placement);
        }
    }

    public void ShowRewardedVideo()
    {
        if (showAds && Advertisement.IsReady(rewarded_placement))
        {
            //Debug.Log("Playing Rewarded Video");

            Advertisement.Show(rewarded_placement);
        }
        else
        {
            //Debug.Log("Loading Rewarded Ad");

            Advertisement.Load(rewarded_placement);
        }
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        //Debug.Log(placementId);

        if (showResult == ShowResult.Finished)
        {
            // Invoke Watched Video Event
            WatchedVideo?.Invoke();
        }
        else if (showResult == ShowResult.Skipped)
        {
            
        }

        Advertisement.Load(rewarded_placement);
    }

    public void OnUnityAdsReady(string placementId)
    {
    }

    public void OnUnityAdsDidError(string message)
    {
    }

    public void OnUnityAdsDidStart(string placementId)
    {
    }
}
