using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public MagnetEffect MagnetEffect { get; private set; }
    public StatsManager StatsManager { get; private set; }
    public ChooseGames ChooseGames { get; private set; }
    public TutorialManager TutorialManager { get; private set; }
    public MoneyManager MoneyManager { get; private set; }
    public SeatManager SeatManager { get; private set; }
    public TouchManager TouchManager { get; private set; }
    public HuggyGenerator HuggyGenerator { get; private set; }
    public SoundManager SoundManager { get; private set; }

    public bool SaveData { get; set; } = true;

    private void Awake()
    {
        // Turn on Save Data
        SaveData = true;

        // Set Max number of Tweens in LeanTween at given point of time
        LeanTween.init(1000);

        if (instance == null)
        {
            instance = this;

            MagnetEffect = GetComponent<MagnetEffect>();
            StatsManager = GetComponent<StatsManager>();
            ChooseGames = GetComponent<ChooseGames>();
            TutorialManager = GetComponent<TutorialManager>();
            MoneyManager = GetComponent<MoneyManager>();
            SeatManager = GetComponent<SeatManager>();
            TouchManager = GetComponent<TouchManager>();
            HuggyGenerator = GetComponent<HuggyGenerator>();
            SoundManager = GetComponent<SoundManager>();

            LoadData();
        }
    }

    private void LoadData()
    {
        PlayerData playerData = SaveSystem.LoadInfo();

        // Check if Save Data is null if not then continue, similarly for other Managers
        if (playerData == null) return;

        if (StatsManager != null)
            StatsManager.LoadData(loginTime: playerData.loginTime, logoutTime: playerData.logoutTime,
                dailyTaskCompleted: playerData.dailyTaskCompleted, dailyTaskProgress: playerData.dailyTaskProgress,
                achievementCompleted: playerData.achievementCompleted, achievementProgress: playerData.achievementProgress,
                achievementTier: playerData.achievementTier);

        if (ChooseGames != null)
            ChooseGames.LoadData(newPlayer: playerData.newPlayer);

        if (MoneyManager != null)
            MoneyManager.LoadData(coins: playerData.coins, diamonds: playerData.diamonds,
                stars: playerData.stars);

        if (SeatManager != null)
            SeatManager.LoadData(seatCount: playerData.seatCount, grid: playerData.grid,
                seatCost: playerData.seatCost, maxHuggyLevelUnlocked: playerData.maxHuggyLevelUnlocked,
                currentHuggyToAdd: playerData.currentHuggyToAdd);

        if (HuggyGenerator != null)
            HuggyGenerator.LoadData(secToMerge: playerData.secToMerge, 
                secToDoubleReward: playerData.secToDoubleReward, secToChestReset: playerData.secToChestReset,
                huggyGenLvl: playerData.huggyGenLvl);

        if (SoundManager != null)
            SoundManager.LoadData(music: playerData.music, sound: playerData.sound,
                haptic: playerData.haptic);
    }

    // Saving on possible scenarios
    private void OnDisable()
    {
        if (SaveData)
            SaveSystem.SaveInfo(instance);
    }

    private void OnApplicationPause(bool pause)
    {
        if (SaveData)
            SaveSystem.SaveInfo(instance);  
    }

    private void OnApplicationQuit()
    {
        if (SaveData)
            SaveSystem.SaveInfo(instance);
    }
}
