using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Seat : MonoBehaviour
{
    [SerializeField] private Transform original;
    [SerializeField] private Transform copy;
    [SerializeField] private int level;
    [SerializeField] private float starAmount;
    [SerializeField] private Transform starTransform;
    [SerializeField] private TextMeshPro starAmountText;
    [SerializeField] private GameObject highlightGO;

    private Vector3 localPos, worldPos;

    private int tickCount;
    private bool held = false;

    private void OnEnable()
    {
        Timer.TickEvent += AnimateHuggy;
        GameManager.instance.TouchManager.pickedHuggy += TurnOnHighlight;
        GameManager.instance.TouchManager.droppedHuggy += TurnOffHighlight;
    }

    private void OnDisable()
    {
        Timer.TickEvent -= AnimateHuggy;
        GameManager.instance.TouchManager.pickedHuggy -= TurnOnHighlight;
        GameManager.instance.TouchManager.droppedHuggy -= TurnOffHighlight;
    }

    public Transform GetCopy()
    {
        copy.GetComponent<SpriteRenderer>().sortingOrder = 100;

        localPos = copy.localPosition;
        worldPos = copy.position;

        held = true;

        return copy;
    }

    public int GetLevel()
    {
        return level;
    }

    public void ReturnToSeat(float time = 0.3f)
    {
        held = false;

        copy.GetComponent<SpriteRenderer>().sortingOrder = 0;

        copy.LeanMoveLocal(localPos, time);
    }

    public Vector3 GetWorldPos()
    {
        return worldPos;
    }

    private void AnimateHuggy()
    {
        tickCount += 1;

        if (tickCount == 2)
        {
            tickCount = 0;


            original.LeanScale(original.localScale * 1.2f, .25f).setLoopPingPong(1);
            if (!held) copy.LeanScale(original.localScale * 1.2f, .25f).setLoopPingPong(1);

            Utility.PopInOutGO(starTransform, 1f, 1f);

            if (HuggyGenerator.DOUBLE_REWARD)
            {
                GameManager.instance.MoneyManager.AddStars(starAmount * 2);
                starAmountText.text = "+" + Utility.ConvertToKMB(starAmount * 2);
            }
            else
            {
                GameManager.instance.MoneyManager.AddStars(starAmount);
                starAmountText.text = "+" + Utility.ConvertToKMB(starAmount);
            }
        }
    } 

    public void TurnOnHighlight(int huggyLevel)
    {
        if (huggyLevel == level)
        {
            highlightGO.SetActive(true);
        }
    }

    public void TurnOffHighlight()
    {
        highlightGO.SetActive(false);       
    }
}
