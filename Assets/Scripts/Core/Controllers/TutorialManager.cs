using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // Tutorial Parameters
    [Header("Tutorial Parameters")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private GameObject[] focusAreas;

    private int currentFocusArea = -1;

    public static bool TutorialOn = false;

    // First Time Reward
    [Header("First Time Reward")]
    [SerializeField] private GameObject firstTimeRewardPanel;

    private void OnDisable()
    {
        GameManager.instance.SeatManager.MergedHuggy -= ProgressFocusArea;
        GameManager.instance.HuggyGenerator.GeneratedHuggy -= ProgressFocusArea;
        GameManager.instance.SeatManager.BuyHuggy -= ProgressFocusArea;
    }

    private void Start()
    {
        GameManager.instance.SeatManager.MergedHuggy += ProgressFocusArea;
        GameManager.instance.HuggyGenerator.GeneratedHuggy += ProgressFocusArea;
        GameManager.instance.SeatManager.BuyHuggy += ProgressFocusArea;
    }

    public void CollectFirstTimeReward()
    {
        GameManager.instance.MoneyManager.AddDiamonds(3000, 20);

        Utility.CloseGO(firstTimeRewardPanel);

        SoundManager.instance.PlayAudioClip((int)AudioEffect.CloseButtonClicked);

        StartTutorial();
    }

    private void StartTutorial()
    {
        TutorialOn = true;

        tutorialPanel.SetActive(true);
        ProgressFocusArea(.5f);
    }

    private void StopTutorial()
    {
        GameManager.instance.HuggyGenerator.enabled = true;
        GameManager.instance.SeatManager.helperHandEnabled = true;
        TutorialOn = false;
        tutorialPanel.SetActive(false);
        this.enabled = false;
    }

    private void ProgressFocusArea(int huggyLevel)
    {
        ProgressFocusArea(currentFocusArea == 7 ? .5f : 2f);
    }

    public void ProgressFocusArea(float timer = .5f)
    {
        if (tutorialImage.raycastTarget) return;

        TurnOnBlocker();
        if (currentFocusArea >= 0)
            Utility.CloseGO(focusAreas[currentFocusArea]);
        currentFocusArea += 1;

        GameManager.instance.SeatManager.helperHandEnabled = currentFocusArea == 5;

        if (currentFocusArea >= focusAreas.Length)
        {
            StopTutorial();
            return;
        }

        LeanTween.delayedCall(timer, () =>
        {
            Utility.OpenGO(focusAreas[currentFocusArea]);
            TurnOffBlocker();
        });
    }

    private void TurnOnBlocker()
    {
        tutorialImage.raycastTarget = true;
    }
    
    private void TurnOffBlocker()
    {
        LeanTween.delayedCall(.5f, () => tutorialImage.raycastTarget = false); 
    }
}
