using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Timer : MonoBehaviour
{
    public static Action TickEvent;

    private readonly WaitForSeconds waitForSec = new WaitForSeconds(1f);

    void Start()
    {
        StartCoroutine(ticker());

        IEnumerator ticker()
        {
            while (true)
            {
                yield return waitForSec;

                TickEvent?.Invoke();
            }
        }
    }   
}
