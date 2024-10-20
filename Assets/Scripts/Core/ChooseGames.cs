using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChooseGames : MonoBehaviour
{
    public int NewPlayer { get; private set; } = 1;

    [SerializeField] private GameObject oneTimeClaimPanel;

    private void Awake()
    {
        // Check if New Player, if yes then do the following
        if (NewPlayer == 1)
        {
            Utility.OpenGO(oneTimeClaimPanel);

            NewPlayer = 0;

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
    
    public void LoadData(int newPlayer)
    {
        NewPlayer = newPlayer;
    }
}
