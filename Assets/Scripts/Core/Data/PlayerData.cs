using System;

[Serializable]
public class PlayerData
{
    // Stats Manager Data
    public long loginTime;
    public long logoutTime;
    public bool[] dailyTaskCompleted;
    public int[] dailyTaskProgress;
    public bool[] achievementCompleted;
    public int[] achievementProgress;
    public int[] achievementTier;

    // Reward Manager Data
    public int[][] skinPieceCount;

    // Choose Game Data
    public int newPlayer;
    public int[] selectedGames;

    // Money Manager Data
    public float coins;
    public float diamonds;
    public float stars;

    // Seat Manager Data
    public int seatCount;
    public int[,] grid;
    public float[] seatCost;
    public int maxHuggyLevelUnlocked;
    public int currentHuggyToAdd;

    // Huggy Generator Data
    public int secToMerge;
    public int secToDoubleReward;
    public int secToChestReset;
    public int huggyGenLvl;

    // Sound Manager Data 
    public bool music;
    public bool sound;
    public bool haptic;

    public PlayerData(GameManager manager)
    {
        // Stats Manager
        loginTime = manager.StatsManager.LoginTime.ToUnixTimeSeconds();
        logoutTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        dailyTaskCompleted = manager.StatsManager.DailyTaskCompleted;
        dailyTaskProgress = manager.StatsManager.DailyTaskProgress;
        achievementCompleted = manager.StatsManager.AchievementCompleted;
        achievementProgress = manager.StatsManager.AchievementProgress;
        achievementTier = manager.StatsManager.AchievementTier;

        // Reward Manager
        skinPieceCount = new int[3][];
        skinPieceCount = manager.RewardManager.SkinPieceCount;

        // Choose Games
        newPlayer = manager.ChooseGames.NewPlayer;
        selectedGames = new int[manager.ChooseGames.SelectedGames.Count]; 
        selectedGames = manager.ChooseGames.SelectedGames.ToArray();

        // Money Manager
        coins = manager.MoneyManager.Coins;
        diamonds = manager.MoneyManager.Diamonds;
        stars = manager.MoneyManager.Stars;

        // Seat Manager
        seatCount = manager.SeatManager.SeatCount;
        grid = new int[5, 5];
        grid = manager.SeatManager.Grid;
        seatCost = manager.SeatManager.SeatCost;
        maxHuggyLevelUnlocked = manager.SeatManager.MaxHuggyLevelUnlocked;
        currentHuggyToAdd = manager.SeatManager.CurrentHuggyToAdd;

        // Huggy Generator
        secToMerge = manager.HuggyGenerator.SecToMerge;
        secToDoubleReward = manager.HuggyGenerator.SecToDoubleReward;
        secToChestReset = manager.HuggyGenerator.SecToChestReset;
        huggyGenLvl = manager.HuggyGenerator.HuggyGenLvl;

        // Sound Manager
        music = manager.SoundManager.Music;
        sound = manager.SoundManager.Sound;
        haptic = manager.SoundManager.Haptic;
    }
}
