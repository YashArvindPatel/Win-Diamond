using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateShiny : MonoBehaviour
{
    [SerializeField] private Transform outerShine, innerShine;
    [SerializeField] private float outerSpeed = 30f, innerSpeed = 40f;

    void Update()
    {
        outerShine.localEulerAngles += Vector3.forward * (outerSpeed * Time.deltaTime);
        innerShine.localEulerAngles += Vector3.forward * (-innerSpeed * Time.deltaTime);
    }
}
