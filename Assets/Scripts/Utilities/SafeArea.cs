using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    void Start()
    {
        var rectTransform = GetComponent<RectTransform>();

        var rect = Screen.safeArea;

        var anchorMin = rect.position;
        var anchorMax = anchorMin + rect.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;

        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
