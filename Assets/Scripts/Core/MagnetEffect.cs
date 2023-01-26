using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetEffect : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private Transform holderGO;
    [SerializeField] private GameObject[] currencyPrefabs;
    [SerializeField] private Transform[] endGoals;
    [SerializeField] private int multiplierX = 20;

    public static Stack<Hover>[] currencyHover;
    private List<Vector2> randomPoints;
    private Vector2 randomPoint, boundPoint;
    private List<float> waitTimers;
    private float waitTimer, minTimer;
    private GameObject GO;

    private void Awake()
    {
        currencyHover = new Stack<Hover>[3]
        {
            new Stack<Hover>(), new Stack<Hover>(), new Stack<Hover>()
        };

        randomPoints = new List<Vector2>();
        waitTimers = new List<float>();
    }

    public void GenerateCurrencyEffect(int spawnCount, int type, Vector2 spawnPos)
    {
        if (spawnCount > currencyHover[type].Count)
        {
            InstantiateHover(spawnCount - currencyHover[type].Count, type);
        }

        spawnPos = mainCam.WorldToScreenPoint(spawnPos);
       
        boundPoint.x = Mathf.Clamp(spawnCount * multiplierX, 0f, 250f);
        boundPoint.y = Mathf.Clamp(boundPoint.x / 2, 0f, 125f);

        randomPoints.Clear();
        waitTimers.Clear();
        minTimer = Mathf.Infinity;

        for (int i = 0; i < spawnCount; i++)
        {
            randomPoint.x = spawnPos.x + Random.Range(-boundPoint.x, boundPoint.x);
            randomPoint.y = spawnPos.y + Random.Range(-boundPoint.y, boundPoint.y);

            waitTimer = Vector2.Distance(randomPoint, spawnPos) / Vector2.Distance(boundPoint, spawnPos);

            if (minTimer > waitTimer)
                minTimer = waitTimer;

            randomPoints.Add(randomPoint);
            waitTimers.Add(waitTimer);
        }

        for (int i = 0; i < spawnCount; i++)
        {
            currencyHover[type].Pop().HoverToEnd(randomPoints[i], endGoals[type].position, spawnPos, waitTimers[i] - minTimer);
        }
    }

    private void InstantiateHover(int count, int type)
    {
        for (int i = 0; i < count; i++)
        {
            GO = Instantiate(currencyPrefabs[type], holderGO);
            GO.SetActive(false);
            currencyHover[type].Push(GO.GetComponent<Hover>());
        }
    }
}
