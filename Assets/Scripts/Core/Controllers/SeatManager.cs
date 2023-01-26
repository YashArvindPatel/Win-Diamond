using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System;

public class SeatManager : MonoBehaviour
{
    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 spacing;
    [SerializeField] private Transform holderT;
    [SerializeField] private GameObject bushPrefab;
    [SerializeField] private GameObject addSeatPrefab;
    [SerializeField] private Vector3 huggySpawnPos;

    internal GameObject[] huggyPrefabs = new GameObject[30];
    internal Sprite[] huggySprites = new Sprite[30];

    private int rows;
    private int currentRows;
    private int limit;

    public int SeatCount { get; private set; } = 8;
    public int[,] Grid { get; private set; }

    public bool FULL = false;

    private int seatFilled = 0;

    // Add Huggy/Seat Parameters
    [Header("Add Huggy/Seat Parameters")]

    [SerializeField] internal GameObject moreSeatsGO;
    [SerializeField] private GameObject freeSubmitButton, adSubmitButton;
    [SerializeField] private Image seatCostImage;
    [SerializeField] private TextMeshProUGUI seatCostText;
    [SerializeField] private GameObject seatCostStarIcon, adIcon;
    [SerializeField] private float seatCostMultiplier = 1.22f;

    public float[] SeatCost { get; private set; }

    public int CurrentHuggyToAdd { get; private set; } = 1;

    public Action<int> AddedHuggy;

    // Remove Huggy Parameters
    [Header("Remove Huggy Parameters")]

    [SerializeField] private Transform sellStarTransform;
    [SerializeField] private TextMeshProUGUI sellInfoText;
    [SerializeField] private float sellAmountMultiplier = 2.8f;

    // Merge Huggy Parameters
    public Action<int> MergedHuggy;

    // Buy Huggy Parameters
    public Action<int> BuyHuggy;

    // Add Gift
    [Header("Add Gift Parameters")]
    [SerializeField] private GameObject freeUpgradePanel;
    [SerializeField] private Image huggyBefore, huggyAfter;
    [SerializeField] private TextMeshProUGUI freeUpgradeText, huggyBeforeText, huggyAfterText;
    [SerializeField] private GameObject giftPrefab;
    [SerializeField] private int openGiftCountThreshold = 3;

    private GameObject currentGiftGO;
    private int openGiftCount;

    public Action OpenedGift;

    // Add Puzzle Piece 
    [Header("Add Puzzle Piece Parameters")]
    [SerializeField] private GameObject puzzlePiecePrefab;
    [SerializeField] private int lowerSpawnChance = 2, upperSpawnChance = 5;

    private int mergeCount;

    // Merge Common Huggies
    private int index1, index2, lvl;

    // Helper Hand
    [Header("Helper Hand")]
    [SerializeField] private GameObject huggyHand;
    [SerializeField] private SpriteRenderer huggyHandSR;
    [SerializeField] private int recurringHandTimer = 5;
    [SerializeField] internal bool helperHandEnabled = true;

    private int currentHandTimer = 0;
    private bool handsEngaged = false;
    private Color whiteColor = Color.white;
    private WaitForSeconds huggyHandWait = new WaitForSeconds(1.75f);

    //AdManager Interstitial
    [Header("AdManager Interstitial")]
    [SerializeField] private int setAdCountOnFreeUpgradeClosed = 2;

    private int adCountOnFreeUpgradeClosed;

    // General Data 
    public int MaxHuggyLevelUnlocked { get; private set; } = 1;

    public Action<int, float> MaxHuggyLevelIncreased;

    private readonly float[] starsPerSecData = new float[30]
    {
        2,5,10,20,40,80,160,320,640,1280,2560,5120,10240,20480,40960,81920,163840,327680,655360,1310720,
        2621440,5242880,10485760,20971520,41943040,83886080,167772160,335544320,671088640,1342177280
    };

    private readonly float[] starRefundData; // Populate if not calculating at Runtime

    private void OnEnable()
    {
        Timer.TickEvent += DisplayHelperHandOnTimer;
    }

    private void OnDisable()
    {
        Timer.TickEvent -= DisplayHelperHandOnTimer;
    }

    private void Start()
    {
        LoadPrefabs();
        Initialize();
    }

    internal void IsNewPlayer()
    {
        currentHandTimer = recurringHandTimer;
        helperHandEnabled = false;
    }

    public void LoadData(int seatCount, int[,] grid, float[] seatCost, int maxHuggyLevelUnlocked, int currentHuggyToAdd)
    {
        SeatCount = seatCount;
        Grid = grid;
        SeatCost = seatCost;
        MaxHuggyLevelUnlocked = maxHuggyLevelUnlocked;
        CurrentHuggyToAdd = currentHuggyToAdd;
    }

    private void LoadPrefabs()
    {
        for (int i = 0; i < huggyPrefabs.Length; i++)
        {
            huggyPrefabs[i] = Resources.Load<GameObject>($"Prefabs/{i}");
            huggySprites[i] = Resources.Load<Sprite>($"Huggy/{i}");
        }
    }

    private void Initialize()
    {
        // Check if Grid is null, if so then initialize it 
        if (Grid == null)
        {
            Grid = new int[5, 5];
            Grid[0, 0] = 1;
            Grid[0, 1] = 1;
        }

        Arrange();

        // Calculate required info

        currentRows = SeatCount / rows + 1;
        limit = SeatCount % rows;

        //Debug.Log($"Current Rows: {currentRows}");

        GameObject bushGO;

        for (int i = 0; i < currentRows; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (i == currentRows - 1 && j >= limit) break;

                bushGO = AddSeat(j, i);

                int huggyLevel = this.Grid[i, j];

                if (huggyLevel == 99)
                {
                    AddGift(parent: bushGO.transform);
                }
                else if (huggyLevel == 100)
                {
                    AddPuzzlePiece(parent: bushGO.transform);
                }
                else if (huggyLevel != 0)
                {
                    AddHuggy(huggyLevel: huggyLevel, parent: bushGO.transform);
                }
            }
        }

        //Add bush with "More Seats" sign if Seats are not full

        if (SeatCount != 25)
        {
            bushGO = AddSeat(limit, currentRows - 1);

            Instantiate(addSeatPrefab, bushGO.transform);
        }

        // Load in SeatCost Base Values if null

        if (SeatCost == null)
        {
            SeatCost = new float[30];

            for (int i = 0; i < SeatCost.Length; i++)
            {
                SeatCost[i] = Mathf.Round(600 * Mathf.Pow(sellAmountMultiplier, i));
            }
        }

        UpdateAddHuggyIcon();
    }

    private void UpdateAddHuggyIcon()
    {
        seatCostImage.sprite = huggySprites[CurrentHuggyToAdd - 1];

        seatCostText.text = Utility.ConvertToKMB(SeatCost[CurrentHuggyToAdd - 1]);
    }

    private void Arrange(bool rearrangeGrid = false)
    {
        //Debug.Log($"SeatCount: {SeatCount}");

        if (SeatCount < 9)
        {
            rows = 3;
            holderT.localScale = Vector2.one * 1f;
        }
        else if (SeatCount < 16)
        {
            rows = 4;
            holderT.localScale = Vector2.one * .735f;

            if (rearrangeGrid)
                RearrangeGrid(3, 4);
        }
        else
        {
            rows = 5;
            holderT.localScale = Vector2.one * .575f;

            if (rearrangeGrid)
                RearrangeGrid(4, 5);
        }

        for (int i = 0; i < holderT.childCount; i++)
        {
            holderT.GetChild(i).localPosition = startPos + new Vector2(spacing.x * (i % rows), spacing.y * (i / rows));
        }
    }

    public void MoreSeats(bool freeSeat = false)
    {
        if (SeatCount == 25) return;

        if (!freeSeat)
        {
            // Play Ad and give player the seat
            AdManager.instance.ShowRewardedVideo();
        }

        SeatCount += 1;

        if (SeatCount == 9 || SeatCount == 16)
            Arrange(rearrangeGrid: true);

        AddSeat(SeatCount % rows, SeatCount / rows);

        // Move Add Seat to end

        holderT.GetChild(SeatCount - 1).GetChild(0).SetParent(holderT.GetChild(SeatCount), false);

        if (SeatCount == 25)
        {
            DestroyImmediate(holderT.GetChild(25).gameObject);
        }

        // Calculate required info

        currentRows = SeatCount / rows + 1;
        limit = SeatCount % rows;

        FULL = false;
    }

    internal void CheckForSeatAmountThreshold(float stars)
    {
        if (FULL || (stars >= SeatCost[CurrentHuggyToAdd - 1]))
        {
            seatCostStarIcon.SetActive(true);
            adIcon.SetActive(false);
        }
        else
        {
            seatCostStarIcon.SetActive(false);
            adIcon.SetActive(true);
        }
    }

    public void AddHuggyOrMoreSeats()
    {
        OpenMoreSeatsOption(CurrentHuggyToAdd, true);
    }

    public void AddHuggyFromShop(int huggyLevel)
    {
        SoundManager.instance.PlayAudioClip((int)AudioEffect.ShopInnerButtonClicked);

        OpenMoreSeatsOption(huggyLevel);
    }

    private void OpenMoreSeatsOption(int huggyLevel, bool updateCurrentHuggyToAdd = false)
    {
        if (FULL)
        {
            if (SeatCount < 25)
            {
                OpenMoreSeatsPanel();
            }
            else
            {
                //Debug.Log("No more Seats");

                MessagePopup.instance.DisplayMessage("Not enough Seats");
            }
        }
        else
        {
            float cost = SeatCost[huggyLevel - 1];

            if (GameManager.instance.MoneyManager.Stars >= cost)
            {
                // Deduct the Seat Cost
                GameManager.instance.MoneyManager.AddStars(-cost);

                // Add the Huggy
                AddHuggyInEmptySeat(huggyLevel: huggyLevel, spawnIntro: 2, spawnPos: huggySpawnPos);

                // Update Seat Cost
                SeatCost[huggyLevel - 1] = Mathf.Round(cost * seatCostMultiplier);

                // Update Shop Cost Info
                GameManager.instance.StatsManager.UpdateHuggyCostInShop(huggyLevel - 1, SeatCost[huggyLevel - 1]);

                if (updateCurrentHuggyToAdd)
                {
                    // Rotate Huggy Icon
                    if (MaxHuggyLevelUnlocked > 3)
                    {
                        CurrentHuggyToAdd = CurrentHuggyToAdd == MaxHuggyLevelUnlocked - 2 ? 1 : CurrentHuggyToAdd + 1;
                    }

                    UpdateAddHuggyIcon();

                    SoundManager.instance.PlayAudioClip((int)AudioEffect.CloseButtonClicked);
                }

                BuyHuggy?.Invoke(huggyLevel);
            }
            else if (updateCurrentHuggyToAdd)
            {
                // Show unskippable ad & then give Huggy
                AdManager.instance.ShowRewardedVideo();

                AddHuggyInEmptySeat(huggyLevel: CurrentHuggyToAdd, spawnIntro: 2, spawnPos: huggySpawnPos);

                // Raise Added Huggy Event
                AddedHuggy?.Invoke(huggyLevel);
            }
            else
            {
                MessagePopup.instance.DisplayMessage("Not enough Coins");
            }
        }
    }

    internal void OpenMoreSeatsPanel()
    {
        if (SeatCount == 8)
        {
            adSubmitButton.SetActive(false);
            freeSubmitButton.SetActive(true);
        }
        else
        {
            adSubmitButton.SetActive(true);
            freeSubmitButton.SetActive(false);
        }

        Utility.OpenGO(moreSeatsGO);

        SoundManager.instance.PlayAudioClip((int)AudioEffect.MoreSeatsSound);
    }

    public void SwapSeats(Transform first, Transform second, int firstLvl = 0, int secondLvl = 0)
    {
        //Debug.Log($"Swapping {firstLvl} with {secondLvl}");

        if (first.childCount > 0)
            first.GetChild(0).SetParent(second, false);


        if (second.childCount > 1)
            second.GetChild(0).SetParent(first, false);
    
        UpdateGrid(
            (int)((first.localPosition.y - startPos.y) / spacing.y), 
            (int)((first.localPosition.x - startPos.x) / spacing.x),
            secondLvl);

        UpdateGrid(
            (int)((second.localPosition.y - startPos.y) / spacing.y), 
            (int)((second.localPosition.x - startPos.x) / spacing.x),
            firstLvl);
    }

    public void MergeSeats(Transform first, Transform second, int lvl = 0)
    {
        int nextLvl = lvl + 1;

        //Debug.Log($"Merging {lvl} & {lvl} into {nextLvl}");

        if (first.childCount == 0 || second.childCount == 0) return;

        Transform firstChild = first.GetChild(0);
        Transform secondChild = second.GetChild(0);

        // For Visuals, can be removed if performance issues

        firstChild.SetParent(secondChild.parent, false);

        firstChild.GetChild(0).gameObject.SetActive(false);
        secondChild.GetChild(0).gameObject.SetActive(false);

        firstChild = firstChild.GetChild(3);
        secondChild = secondChild.GetChild(3);

        firstChild.localPosition = secondChild.localPosition;

        // Play Merge Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.MergeSound);

        // Vibrate 
        SoundManager.instance.VibrateDevice();

        firstChild.LeanMoveLocal(secondChild.localPosition + Vector3.left * 2f, .15f).setLoopPingPong(1);
        secondChild.LeanMoveLocal(secondChild.localPosition + Vector3.right * 2f, .15f).setLoopPingPong(1).setOnComplete(() =>
        {
            try
            {
                if (firstChild != null) RemoveHuggy(huggyGO: firstChild.parent.gameObject, huggyLevel: lvl);
                if (secondChild != null)
                {
                    GameManager.instance.MoneyManager.RewardPlayerOnMerge(nextLvl, secondChild.position);
                    RemoveHuggy(huggyGO: secondChild.parent.gameObject, huggyLevel: lvl);
                }

                // Add Huggy on additional level and update Grid Info
                AddHuggy(huggyLevel: nextLvl, parent: second);

                UpdateGrid(
                  (int)((second.localPosition.y - startPos.y) / spacing.y),
                  (int)((second.localPosition.x - startPos.x) / spacing.x),
                  nextLvl);

                // Raise Merged Huggy Event
                MergedHuggy?.Invoke(nextLvl);

                mergeCount++;

                if (mergeCount >= upperSpawnChance || (mergeCount >= lowerSpawnChance && UnityEngine.Random.Range(0, 2) == 0))
                {
                    AddPuzzlePieceInSeat(first);

                    mergeCount = 0;
                }
                else
                {
                    UpdateGrid(
                        (int)((first.localPosition.y - startPos.y) / spacing.y),
                        (int)((first.localPosition.x - startPos.x) / spacing.x),
                        0);
                }

                // Check if MaxHuggyLevel increased
                if (nextLvl > MaxHuggyLevelUnlocked)
                {
                    MaxHuggyLevelUnlocked = nextLvl;

                    MaxHuggyLevelIncreased?.Invoke(MaxHuggyLevelUnlocked, SeatCost[lvl]);
                }
            }
            catch
            {
                //Debug.Log("Error while merging");
            }         
        });     
    }  

    private GameObject AddSeat(int x, int y)
    {
        GameObject bushGO = Instantiate(bushPrefab, holderT) as GameObject;

        bushGO.transform.localPosition = startPos + new Vector2(spacing.x * x, spacing.y * y);

        return bushGO;
    }

    internal void AddHuggyInEmptySeat(int huggyLevel = 1, int spawnIntro = 0, Vector3 spawnPos = default)
    {
        int i = 0, j = 0;

        if (FindHuggy(ref i, ref j))
        {
            AddHuggy(huggyLevel: huggyLevel,
                        parent: holderT.GetChild(i * rows + j),
                        spawnIntro: spawnIntro,
                        spawnPos: spawnPos);

            UpdateGrid(i, j, huggyLevel);
        }
    }

    internal void AddGiftInEmptySeat()
    {
        int i = 0, j = 0;

        if (FindHuggy(ref i, ref j))
        {
            AddGift(parent: holderT.GetChild(i * rows + j));

            // 99 for Gift
            UpdateGrid(i, j, 99);
        }
    }

    internal void OpenGift(GameObject giftGO)
    {
        currentGiftGO = giftGO;
        openGiftCount++;

        if (openGiftCount > openGiftCountThreshold)
        {
            openGiftCount = 0;

            huggyBefore.sprite = huggySprites[MaxHuggyLevelUnlocked - 3];           
            huggyAfter.sprite = huggySprites[MaxHuggyLevelUnlocked - 1];
            
            huggyBeforeText.text = $"Lv.{MaxHuggyLevelUnlocked - 2}";
            huggyAfterText.text = $"Lv.{MaxHuggyLevelUnlocked}";

            freeUpgradeText.text = $"Get <color=red>Lv.{MaxHuggyLevelUnlocked}</color> Huggy for free!";

            Utility.OpenGO(freeUpgradePanel);

            // Play Sound
            SoundManager.instance.PlayAudioClip((int)AudioEffect.MoreSeatsSound);
        }
        else
        {
            CashInGift(false);
        }
    }

    public void CloseFreeUpgradePanel()
    {
        // Increase Ad Counter
        adCountOnFreeUpgradeClosed += 1;

        if (adCountOnFreeUpgradeClosed >= setAdCountOnFreeUpgradeClosed)
        {
            adCountOnFreeUpgradeClosed = 0;

            AdManager.instance.ShowInterstitialAd();
        }

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.CloseButtonClicked);

        Utility.CloseGO(freeUpgradePanel);

        CashInGift(false);
    }

    private void CashInGift(bool max)
    {
        RemoveGift(currentGiftGO);

        if (currentGiftGO != null)
        {
            AddHuggy(huggyLevel: max ? MaxHuggyLevelUnlocked : MaxHuggyLevelUnlocked - 2, parent: currentGiftGO.transform.parent, spawnIntro: 1);

            UpdateGrid(
                (int)((currentGiftGO.transform.parent.localPosition.y - startPos.y) / spacing.y),
                (int)((currentGiftGO.transform.parent.localPosition.x - startPos.x) / spacing.x),
                max ? MaxHuggyLevelUnlocked : MaxHuggyLevelUnlocked - 2);
        }

        OpenedGift?.Invoke();
    }

    public void FreeUpgrade()
    {
        // Play Ad 
        AdManager.instance.ShowRewardedVideo();

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.YESButtonClicked);

        Utility.CloseGO(freeUpgradePanel);

        CashInGift(true);
    }

    internal void AddPuzzlePieceInSeat(Transform parent)
    {
        AddPuzzlePiece(parent: parent);

        // 100 for Puzzle Piece
        UpdateGrid(
            (int)((parent.localPosition.y - startPos.y) / spacing.y),
            (int)((parent.localPosition.x - startPos.x) / spacing.x),
            100);
    }

    internal bool FindHuggy(ref int i, ref int j, int level = 0)
    {
        for (int a = 0; a < currentRows; a++)
        {
            for (int b = 0; b < rows; b++)
            {
                if (a == currentRows - 1 && b >= limit) return false;
                
                if (this.Grid[a, b] == level)
                {
                    i = a;
                    j = b;

                    return true;
                }              
            }
        }

        return false;
    }

    // Helper Hand
    private void DisplayHelperHandOnTimer()
    {
        if (!helperHandEnabled || handsEngaged || HuggyGenerator.AUTO_MERGE) return;

        currentHandTimer += 1;

        if (currentHandTimer >= recurringHandTimer)
        {
            currentHandTimer = 0;

            StartCoroutine(MoveHelperHand());
        }
    }

    IEnumerator MoveHelperHand()
    {
        for (int i = 0; i < 3; i++)
        {
            (index1, index2, lvl) = FindCommonHuggies();

            if (index1 != -1 && index2 != -1 && lvl != -1)
            {
                handsEngaged = true;
                huggyHand.SetActive(true);

                whiteColor.a = 1f;
                huggyHandSR.color = whiteColor;
                huggyHand.transform.position = holderT.GetChild(index1).position;

                huggyHand.LeanMove(holderT.GetChild(index2).position, .5f).setOnComplete(() => 
                LeanTween.value(1f, 0f, .5f).setDelay(.5f).setOnUpdate(val =>
                {
                    whiteColor.a = val;
                    huggyHandSR.color = whiteColor;
                }).setOnComplete(() =>
                {
                    huggyHand.SetActive(false);
                    handsEngaged = false;
                }));
            }

            yield return huggyHandWait;
        }
    }

    internal void StopHelperHand()
    {
        StopCoroutine(MoveHelperHand());

        huggyHand.LeanCancel();
        huggyHand.SetActive(false);
        handsEngaged = false;
     
        currentHandTimer = 0;
    }

    // Merge Common Huggies 

    internal void MergeCommonHuggies()
    {     
        (index1, index2, lvl) = FindCommonHuggies();

        if (index1 == -1 || index2 == -1 || lvl == -1) return;

        MergeSeats(first: holderT.GetChild(index1),
            second: holderT.GetChild(index2),
            lvl);
    }

    private (int,int,int) FindCommonHuggies()
    {
        if (seatFilled < 2) return (-1, -1, -1);

        SortedList<int, int> unsorted = new SortedList<int, int>();
        int count = 0;

        for (int i = 0; i < currentRows; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (i == currentRows - 1 && j >= limit) break;

                unsorted.Add(count, this.Grid[i, j]);
                count++;               
            }
        }

        var sorted = unsorted.OrderBy(x => x.Value);
        KeyValuePair<int, int> firstPair = new KeyValuePair<int, int>(-1, -1);

        foreach (var secondPair in sorted)
        {   
            if (secondPair.Value != 0 && secondPair.Value != 99 && secondPair.Value != 100 && secondPair.Value == firstPair.Value)
            {
                //Debug.Log($"First: Lvl: {firstPair.Value} Index: {firstPair.Key} Second: Lvl: {secondPair.Value} Index: {secondPair.Key}");
              
                return (firstPair.Key, secondPair.Key, firstPair.Value);
            }

            firstPair = secondPair;
        }

        return (-1, -1, -1);
    }
    
    // Add Huggy, define the way huggy is spawned into the game: 0 means nothing, one means bounce in, two is travel to spot

    private void AddHuggy(int huggyLevel, Transform parent, int spawnIntro = 0, Vector3 spawnPos = default)
    {
        if (huggyLevel > 0 && huggyLevel < huggyPrefabs.Length)
        {
            GameObject huggyGO = Instantiate(huggyPrefabs[huggyLevel - 1], parent) as GameObject;

            //Debug.Log("Adding Huggy");

            if (spawnIntro == 1)
            {
                huggyGO.transform.localScale = Vector3.zero;
                huggyGO.LeanScale(Vector3.one, .5f).setEaseInOutBounce();
            }
            else if (spawnIntro == 2)
            {
                huggyGO.LeanMoveLocal(Vector3.zero, .3f).setFrom(huggyGO.transform.InverseTransformPoint(spawnPos));
            }

            seatFilled += 1;

            // Check if Seats are Full, if so then set FULL to true
            if (seatFilled >= SeatCount)
            {
                FULL = true;
            }

            // Update the StarsPerSec 
            GameManager.instance.MoneyManager.IncrementStarsPerSec(starsPerSecData[huggyLevel - 1]);
        }
    }

    private void AddGift(Transform parent)
    {
        GameObject giftGO = Instantiate(giftPrefab, parent) as GameObject;

        seatFilled += 1;

        // Check if Seats are Full, if so then set FULL to true
        if (seatFilled >= SeatCount)
        {
            FULL = true;
        }
    }

    private void AddPuzzlePiece(Transform parent)
    {
        GameObject puzzlePieceGO = Instantiate(puzzlePiecePrefab, parent) as GameObject;

        seatFilled += 1;

        // Check if Seats are Full, if so then set FULL to true
        if (seatFilled >= SeatCount)
        {
            FULL = true;
        }
    }

    public void RemoveHuggyFromSeat(GameObject huggyGO, int huggyLevel = 1)
    {
        //Debug.Log("Removing Huggy");

        // NOTE: Update Grid before removing huggy, due to using DestroyImmediate the reference to huggyGO is lost
        UpdateGrid(
            (int)((huggyGO.transform.parent.localPosition.y - startPos.y) / spacing.y),
            (int)((huggyGO.transform.parent.localPosition.x - startPos.x) / spacing.x),
            0);

        RemoveHuggy(huggyGO, huggyLevel);

        // Play Sell Huggy Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.SellHuggySound);

        // Refund Stars for selling Huggy
        float sellAmount = Mathf.Round(600 * Mathf.Pow(sellAmountMultiplier, huggyLevel - 1));

        GameManager.instance.MoneyManager.AddStars(sellAmount);

        // Display the amount 
        sellInfoText.text = "+" + Utility.ConvertToKMB(sellAmount);
        Utility.PopInOutGO(sellStarTransform, 100f, 1.2f);
    }

    private void RemoveHuggy(GameObject huggyGO, int huggyLevel)
    {
        if (huggyGO != null)
        {
            DestroyImmediate(huggyGO);

            // Set FULL to False because Huggy removed

            seatFilled -= 1;

            FULL = false;

            // Update the StarsPerSec 
            GameManager.instance.MoneyManager.IncrementStarsPerSec(-starsPerSecData[huggyLevel - 1]);
        }
    }

    private void RemoveGift(GameObject giftGO)
    {
        if (giftGO != null)
        {
            Destroy(giftGO);

            // Set FULL to False because Gift removed

            seatFilled -= 1;

            FULL = false;
        }
    }

    public void RemovePuzzlePieceFromSeat(GameObject puzzlePieceGO)
    {
        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.BubblePop);

        UpdateGrid(
            (int)((puzzlePieceGO.transform.parent.localPosition.y - startPos.y) / spacing.y),
            (int)((puzzlePieceGO.transform.parent.localPosition.x - startPos.x) / spacing.x),
            0);

        RemovePuzzlePiece(puzzlePieceGO);
    }

    private void RemovePuzzlePiece(GameObject puzzlePieceGO)
    {
        if (puzzlePieceGO != null)
        {
            Destroy(puzzlePieceGO);

            // Set FULL to False because Puzzle Piece removed

            seatFilled -= 1;

            FULL = false;
        }
    }

    private void UpdateGrid(int i, int j, int huggyLevel)
    {
        //Debug.Log($"i,j: {i},{j} & huggyLevel: {huggyLevel}");

        this.Grid[i, j] = huggyLevel;
    }

    private void RearrangeGrid(int from, int to)
    {
        int totalCount = from * from;
        int count = 0;

        int[] temp = new int[totalCount];

        for (int i = 0; i < from; i++)
        {
            for (int j = 0; j < from; j++)
            {
                temp[count] = this.Grid[i, j];
                count++;
            }
        }

        count = 0;

        for (int i = 0; i < to; i++)
        {
            for (int j = 0; j < to; j++)
            {
                if (count >= totalCount)
                {
                    this.Grid[i, j] = 0;
                }
                else
                {
                    this.Grid[i, j] = temp[count];
                    count++;
                }
            }
        }
    }

    internal List<int> GridInfo()
    {
        List<int> gridInfo = new List<int>();

        for (int i = 0; i < currentRows; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if (i == currentRows - 1 && j >= limit) break;

                gridInfo.Add(this.Grid[i, j]);
            }
        }

        return gridInfo;
    }
}
