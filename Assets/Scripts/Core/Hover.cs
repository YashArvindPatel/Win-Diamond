using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    [SerializeField] private int type;
    [SerializeField] private CanvasGroup CG;

    public void HoverToEnd(Vector2 firstStop, Vector2 lastStop, Vector2 spawnPos, float waitTimer)
    {
        transform.localScale = Vector3.one;
        CG.alpha = 1f;
        gameObject.SetActive(true);

        if (type == 2)
        {
            SoundManager.instance.PlayAudioClip((int)AudioEffect.StarStart);
        }

        transform.localPosition = spawnPos;

        transform.LeanMoveLocal(firstStop, .2f).setEaseOutCirc().setOnComplete(() =>
        {
            LeanTween.delayedCall(waitTimer, () => transform.LeanMove(lastStop, .5f).setOnComplete(() =>
            {
                // Play Sound
                if (type == 0)
                {
                    SoundManager.instance.PlayAudioClip((int)AudioEffect.CoinShort);
                }
                else if (type == 1)
                {
                    SoundManager.instance.PlayAudioClip((int)AudioEffect.DiamondShort);
                }
                else if (type == 2)
                {
                    SoundManager.instance.PlayAudioClip((int)AudioEffect.StarEnd);
                }

                // Vibrate 
                SoundManager.instance.VibrateDevice();

                gameObject.LeanScale(Vector3.one * 1.5f, .5f);
                CG.LeanAlpha(0f, .5f).setOnComplete(() =>
                {
                    gameObject.SetActive(false);
                    MagnetEffect.currencyHover[type].Push(this);
                });
            }));
        });
    }
}
