using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChooseGames : MonoBehaviour
{
    public int NewPlayer { get; private set; } = 1;

    [SerializeField] private GameObject choicePanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private TextMeshProUGUI choiceText;
    [SerializeField] private List<GameObject> gamesList;
    [SerializeField] private List<GameObject> selectedList;
    [SerializeField] private GameObject oneTimeClaimPanel;

    public List<int> SelectedGames { get; private set; } = new List<int>();

    private bool phase1 = true;

    private void Awake()
    {
        // Check if New Player, if yes then do the following
        if (NewPlayer == 1)
        {
            panelCanvasGroup.gameObject.SetActive(true);
            GameManager.instance.HuggyGenerator.IsNewPlayer();
            GameManager.instance.SeatManager.IsNewPlayer();

            // Play Sound
            SoundManager.instance.PlayAudioClip((int)AudioEffect.NewPlayer);
        }
        else
        {
            GameManager.instance.TutorialManager.enabled = false;
        }
    }
    
    public void LoadData(int newPlayer, int[] selectedGames)
    {
        NewPlayer = newPlayer;

        if (NewPlayer == 0)
        {
            SelectedGames.AddRange(selectedGames);
            GameManager.instance.RewardManager.UpdateSkinsAndRewardsPanels(SelectedGames);
        }
    }

    public void SelectGame(int index)
    {
        // Hard coded selection logic

        if (phase1)
        {
            if (SelectedGames.Count == 0)
            {
                SelectedGames.Add(index);
                selectedList[index].SetActive(true);
            }
            else
            {
                if (SelectedGames.Contains(index))
                {
                    SelectedGames.Remove(index);
                    selectedList[index].SetActive(false);
                }
                else
                {
                    selectedList[SelectedGames[0]].SetActive(false);
                    selectedList[index].SetActive(true);
                    SelectedGames[0] = index;
                }
            }
        }
        else
        {
            if (SelectedGames.Count == 1)
            {
                SelectedGames.Add(index);
                selectedList[index].SetActive(true);
            }
            else
            {
                if (SelectedGames.Contains(index))
                {
                    SelectedGames.Remove(index);
                    selectedList[index].SetActive(false);
                }
                else
                {
                    if (SelectedGames.Count == 2)
                    {
                        selectedList[index].SetActive(true);
                        SelectedGames.Add(index);
                    }
                    else
                    {
                        selectedList[SelectedGames[2]].SetActive(false);
                        selectedList[index].SetActive(true);
                        SelectedGames[2] = index;
                    }
                }
            }
        }
    }

    public void SubmitSelectedGames()
    {
        if (phase1)
        {
            if (SelectedGames.Count == 1)
            {
                phase1 = false;

                choiceText.text = "You can choose two more games that you want to apply";

                gamesList[SelectedGames[0]].GetComponentInChildren<Button>().enabled = false;

                gamesList.ForEach(GO => GO.LeanScale(Vector3.zero, .5f).setEaseInBack().setOnComplete(() => GO.LeanScale(Vector3.one, .5f).setEaseOutBack()));
            }
            else
            {
                MessagePopup.instance.DisplayMessage("Select atleast one game!");

                //Debug.Log("Select atleast 1 game!");
            }
        }
        else if (NewPlayer == 1)
        {
            panelCanvasGroup.LeanAlpha(0, .5f);

            choicePanel.LeanScale(Vector3.zero, .5f).setEaseInBack().setOnComplete(() => {
                choicePanel.SetActive(false);
                panelCanvasGroup.gameObject.SetActive(false);
                oneTimeClaimPanel.SetActive(true);
              
                Utility.OpenGO(oneTimeClaimPanel);
            });

            NewPlayer = 0;

            // If Selected Games are less than 3 then add games

            int gameCount = SelectedGames.Count;

            for (int gameId = 0; gameId < 9; gameId++)
            {
                if (gameCount == 3) break;

                if (!SelectedGames.Contains(gameId))
                {
                    SelectedGames.Add(gameId);
                    gameCount++;
                }
            }

            GameManager.instance.RewardManager.UpdateSkinsAndRewardsPanels(SelectedGames);
        }
    }
}
