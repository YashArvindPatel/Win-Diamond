using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardManager : MonoBehaviour
{
    // Rewards 
    [Header("Rewards")]

    [SerializeField] private GameObject rewardsPanel;
    [SerializeField] private Transform rewardGameBGParent;
    [SerializeField] private Transform rewardGameListParent;
    [SerializeField] private Transform rewardRewardListParent;

    private readonly GameObject[] rewardGameBG = new GameObject[3];
    private readonly GameObject[] rewardGameList = new GameObject[3];
    private readonly GameObject[] rewardRewardList = new GameObject[3];

    private int rewardIndex = 0;

    // Skins
    [Header("Skins")]

    [SerializeField] private GameObject skinsPanel;
    [SerializeField] private Transform skinGameBGParent;
    [SerializeField] private Transform skinGameListParent;
    [SerializeField]  private Transform skinRewardListParent;

    private readonly GameObject[] skinGameBG = new GameObject[3];
    private readonly GameObject[] skinGameList = new GameObject[3];
    private readonly GameObject[] skinRewardList = new GameObject[3];

    private int skinIndex = 0;
    public int[][] SkinPieceCount { get; private set; }

    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float minScale = .9f;

    // Display Skin
    [SerializeField] private GameObject[] displaySkins;

    private GameObject[] selectedDisplaySkins = new GameObject[3];
    private int displayCount = 2;

    // Puzzle Piece
    [SerializeField] private GameObject puzzlePiecePanel;
    [SerializeField] private GameObject puzzlePieceMagnetAnim;
    [SerializeField] private Transform puzzlePieceMainParent;

    private GameObject[] puzzlePieceParents = new GameObject[3];
    private Transform[] puzzlePieces = new Transform[3];
    private TextMeshProUGUI[] puzzlePieceCount = new TextMeshProUGUI[3];
    private int[] openedPieces = new int[3], pieceAmounts = new int[3];

    // More Pieces 
    [SerializeField] private GameObject morePiecesPanel;
    [SerializeField] private Transform morePiecesMainParent;

    private GameObject[] morePiecesParents = new GameObject[3];
    private Transform[] morePieces = new Transform[3];
    private int gamePieceIndex, skinPieceIndex;
    private Reward claimedRewardGO;

    //AdManager Interstitial
    [Header("AdManager Interstitial")]
    [SerializeField] private int setAdCountOnOpenPiece = 3;

    private int adCountOnOpenPiece;

    public void LoadData(int[][] skinPieceCount)
    {
        SkinPieceCount = skinPieceCount;
    }

    private void Start()
    {
        for (int i = 0; i < selectedDisplaySkins.Length; i++)
        {
            if (selectedDisplaySkins[i] == null) selectedDisplaySkins[i] = displaySkins[i];
        }

        StartCoroutine(DisplaySkinInLoop());
    }

    IEnumerator DisplaySkinInLoop()
    {
        while (true)
        {
            selectedDisplaySkins[displayCount].LeanScale(Vector3.zero, .15f).setFrom(Vector3.one).setOnComplete(() =>
            {
                displayCount = displayCount > 1 ? 0 : displayCount + 1;
                selectedDisplaySkins[displayCount].LeanScale(Vector3.one, .15f).setFrom(Vector3.zero);
            });

            yield return new WaitForSeconds(2f);
        }
    }

    internal void UpdateSkinsAndRewardsPanels(List<int> selectedGames)
    {
        //Debug.Log($"Selected Games: {selectedGames[0]},{selectedGames[1]},{selectedGames[2]}");

        List<int> reArrangedList = new List<int> { selectedGames[1], selectedGames[0], selectedGames[2] };

        foreach (GameObject GO in displaySkins)
        {
            GO.transform.localScale = Vector3.zero;
        }

        // Set Active GOs
        for (int i = 0; i < reArrangedList.Count; i++)
        {
            // Assign
            rewardGameBG[i] = rewardGameBGParent.GetChild(reArrangedList[i]).gameObject;
            rewardGameList[i] = rewardGameListParent.GetChild(reArrangedList[i]).gameObject;
            rewardRewardList[i] = rewardRewardListParent.GetChild(reArrangedList[i]).gameObject;

            skinGameBG[i] = skinGameBGParent.GetChild(reArrangedList[i]).gameObject;
            skinGameList[i] = skinGameListParent.GetChild(reArrangedList[i]).gameObject;
            skinRewardList[i] = skinRewardListParent.GetChild(reArrangedList[i]).gameObject;

            // Puzzle Piece
            puzzlePieceParents[i] = puzzlePieceMainParent.GetChild(reArrangedList[i]).gameObject;
            puzzlePieceParents[i].SetActive(true);
            Transform pieceRT = puzzlePieceParents[i].transform.GetChild(0).GetChild(0);
            puzzlePieces[i] = pieceRT.GetChild(pieceRT.childCount - 1);
            puzzlePieceCount[i] = puzzlePieceParents[i].transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();

            // More Pieces
            morePiecesParents[i] = morePiecesMainParent.GetChild(reArrangedList[i]).gameObject;
            pieceRT = morePiecesParents[i].transform.GetChild(0).GetChild(0);
            morePieces[i] = pieceRT.GetChild(pieceRT.childCount - 1);

            // Set Active
            rewardGameList[i].SetActive(true);
            skinGameList[i].SetActive(true);

            // Button Call
            int index = i;

            rewardGameList[i].transform.GetComponentInChildren<Button>().onClick.AddListener(() => OpenGameReward(index));
            skinGameList[i].transform.GetComponentInChildren<Button>().onClick.AddListener(() => OpenGameSkin(index));

            rewardGameList[i].transform.GetComponentInChildren<Button>().onClick.AddListener(() => SoundManager.instance.PlayAudioClip((int)AudioEffect.BackOrInfoButtonClicked));
            skinGameList[i].transform.GetComponentInChildren<Button>().onClick.AddListener(() => SoundManager.instance.PlayAudioClip((int)AudioEffect.BackOrInfoButtonClicked));

            // Add Display
            selectedDisplaySkins[i] = displaySkins[selectedGames[i]];
        }

        // Re-arrange GOs
        for (int i = 0; i < reArrangedList.Count; i++)
        {
            rewardGameBG[i].transform.SetSiblingIndex(i);
            rewardGameList[i].transform.SetSiblingIndex(i);
            rewardRewardList[i].transform.SetSiblingIndex(i);

            skinGameBG[i].transform.SetSiblingIndex(i);
            skinGameList[i].transform.SetSiblingIndex(i);
            skinRewardList[i].transform.SetSiblingIndex(i);

            puzzlePieces[i].parent.parent.parent.SetSiblingIndex(i);

            morePieces[i].parent.parent.parent.SetSiblingIndex(i);
        }

        for (int i = 0; i < displaySkins.Length; i++)
        {
            displaySkins[i].SetActive(selectedGames.Contains(i) ? true : false);
        }

        selectedDisplaySkins[displayCount].LeanCancel(true);

        // Initialize SkinPieceCount if null
        if (SkinPieceCount == null)
        {
            SkinPieceCount = new int[3][]
            {
                new int[skinRewardList[0].transform.GetChild(0).childCount],
                new int[skinRewardList[1].transform.GetChild(0).childCount],
                new int[skinRewardList[2].transform.GetChild(0).childCount]
            };
        }
    }

    public void OpenRewardsPanel()
    {
        GameManager.instance.MoneyManager.RefreshRewardCoinsAndDiamonds();

        // Extra check to close rest children GO
        for (int i = 3; i < rewardGameListParent.childCount; i++)
        {
            rewardGameListParent.GetChild(i).gameObject.SetActive(false);
        }

        Utility.OpenGOBounce(rewardsPanel);

        SoundManager.instance.PlayAudioClip((int)AudioEffect.RewardsButtonClicked);

        // Optional
        OpenGameReward(1);
    }

    public void CloseRewardsPanel()
    {
        if (TutorialManager.TutorialOn)
        {
            GameManager.instance.TutorialManager.ProgressFocusArea();
        }

        Utility.CloseGO(rewardsPanel);

        SoundManager.instance.PlayAudioClip((int)AudioEffect.BackOrInfoButtonClicked);
    }

    public void OpenSkinsPanel()
    {
        // Extra check to close rest children GO
        for (int i = 3; i < skinGameListParent.childCount; i++)
        {
            skinGameListParent.GetChild(i).gameObject.SetActive(false);
        }

        Utility.OpenGOBounce(skinsPanel);

        // Optional
        OpenGameSkin(1);
    }

    public void OpenGameReward(int index)
    {
        if (TutorialManager.TutorialOn)
        {
            GameManager.instance.TutorialManager.ProgressFocusArea();
        }

        if (index == rewardIndex) return;

        rewardGameBG[rewardIndex].SetActive(false);
        rewardGameList[rewardIndex].LeanScale(Vector3.one * minScale, .2f);
        rewardRewardList[rewardIndex].SetActive(false);

        rewardIndex = index;

        rewardGameBG[rewardIndex].SetActive(true);
        rewardGameList[rewardIndex].LeanScale(Vector3.one * maxScale, .2f);
        rewardRewardList[rewardIndex].SetActive(true);
    }

    public void OpenGameSkin(int index)
    {
        if (index == skinIndex) return;

        skinGameBG[skinIndex].SetActive(false);
        skinGameList[skinIndex].LeanScale(Vector3.one * minScale, .2f);
        skinRewardList[skinIndex].LeanMoveLocalX(skinIndex < index ? 1200f : -1200f, .5f).setFrom(0f).setOnComplete(() =>
        {
            skinRewardList[skinIndex].SetActive(false);
            skinIndex = index;
        });

        skinGameBG[index].SetActive(true);
        skinGameList[index].LeanScale(Vector3.one * maxScale, .2f);
        skinRewardList[index].SetActive(true);
        skinRewardList[index].LeanMoveLocalX(0f, .5f).setFrom(skinIndex < index ? -1200f : 1200f);
    }

    // Puzzle Piece
    internal void OpenPuzzlePiecePanel()
    {
        puzzlePieceParents[0].SetActive(false);
        puzzlePieceParents[2].SetActive(false);

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.PieceReveal);

        LeanTween.delayedCall(.25f, () =>
        {
            SoundManager.instance.PlayAudioClip((int)AudioEffect.PieceReveal);

            puzzlePieceParents[0].SetActive(true);
        });

        LeanTween.delayedCall(.5f, () => 
        {
            SoundManager.instance.PlayAudioClip((int)AudioEffect.PieceReveal);

            puzzlePieceParents[2].SetActive(true);
        });

        Utility.OpenGO(puzzlePiecePanel);

        GetRandomPuzzlePieces();
    }

    private void GetRandomPuzzlePieces()
    {
        for (int game = 0; game < puzzlePieces.Length; game++)
        {
            int skin = Random.Range(0, puzzlePieces[game].childCount);

            openedPieces[game] = skin;
            puzzlePieces[game].GetChild(skin).gameObject.SetActive(true);

            int amount = Random.Range(1, 5);

            pieceAmounts[game] = amount;
            puzzlePieceCount[game].text = $"{amount}";
        }
    }

    public void ClaimPuzzlePieces()
    {
        // Increase Ad Counter
        adCountOnOpenPiece += 1;

        if (adCountOnOpenPiece >= setAdCountOnOpenPiece)
        {
            adCountOnOpenPiece = 0;

            AdManager.instance.ShowInterstitialAd();
        }

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.CloseButtonClicked);

        // Increment the piece count
        for (int game = 0; game < puzzlePieces.Length; game++)
        {
            //Debug.Log($"Game{game}: Skins: {openedPieces[game]} & Amount: {pieceAmounts[game]}");

            AddPuzzlePieces(game, openedPieces[game], pieceAmounts[game]);
        }

        Utility.CloseGO(puzzlePiecePanel);

        puzzlePieceMagnetAnim.SetActive(true);
        puzzlePieceMagnetAnim.LeanDelayedCall(1.25f, () => puzzlePieceMagnetAnim.SetActive(false));

        LeanTween.delayedCall(.5f, () =>
        {
            for (int game = 0; game < puzzlePieces.Length; game++)
            {
                foreach (Transform child in puzzlePieces[game])
                {
                    child.gameObject.SetActive(false);
                }
            }
        });
    }

    // More Pieces
    internal void OpenMorePiecesPanel(Reward rewardGO, int parentIndex, int selfIndex)
    {
        gamePieceIndex = parentIndex;
        skinPieceIndex = selfIndex;
        claimedRewardGO = rewardGO;

        morePiecesParents[gamePieceIndex].SetActive(true);
        morePieces[gamePieceIndex].GetChild(skinPieceIndex).gameObject.SetActive(true);

        // Play Sound
        SoundManager.instance.PlayAudioClip((int)AudioEffect.PieceReveal);

        Utility.OpenGO(morePiecesPanel);
    }

    public void ClaimMorePieces()
    {
        // Play Ad
        AdManager.instance.ShowRewardedVideo();

        AddPuzzlePieces(gamePieceIndex, skinPieceIndex, 2);
        claimedRewardGO.RefreshInfo();

        CloseMorePiecesPanel();
    }

    public void CloseMorePiecesPanel()
    {
        Utility.CloseGO(morePiecesPanel);

        LeanTween.delayedCall(.5f, () =>
        {
            morePiecesParents[gamePieceIndex].SetActive(false);

            foreach (Transform child in morePieces[gamePieceIndex])
            {
                child.gameObject.SetActive(false);
            }
        });
    }

    private void AddPuzzlePieces(int game, int skin, int amount)
    {
        try
        {
            SkinPieceCount[game][skin] += amount;
        }
        catch
        {
            //Debug.Log("Error while adding Skin Piece");
        }
    }
}
