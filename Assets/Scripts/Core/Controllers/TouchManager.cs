using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class TouchManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;

    private RaycastHit2D hit;
    private Transform item;

    // Held Item Info
    private SeatManager seatManager;
    private Seat seat;
    private Transform heldItem;
    private Vector3 worldPos, offset;

    private readonly string SEAT_TAG = "Seat", ADD_TAG = "Add",
        GIFT_TAG = "Gift", PUZZLE_TAG = "Puzzle", REMOVE_TAG = "Remove";

    // Recycle Huggy Swap Info
    [SerializeField] private GameObject recycleGO;
    [SerializeField] private GameObject recycleText;
    [SerializeField] private GameObject addHuggyGO;
    [SerializeField] private GameObject costText;

    // Highlight Huggy
    public Action<int> pickedHuggy;
    public Action droppedHuggy;

    // Check for UI Raycast
    private PointerEventData eventDataCurrentPosition;
    private List<RaycastResult> results = new List<RaycastResult>();

    private void Start()
    {
        seatManager = GameManager.instance.SeatManager;
    }

    private void Update()
    {
        CheckForTouch();
        MoveHeldItem();
    }

    private bool GetTouchDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    private bool GetTouchUp()
    {
        return Input.GetMouseButtonUp(0);
    }

    private bool IsPointerOverGameObject()
    {
        eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private Vector3 GetTouchPosition()
    {
        return Input.mousePosition;
    }

    private void CheckForTouch()
    {
        if (GetTouchDown() && !IsPointerOverGameObject())
        {
            if (ShootRaycast())
            {
                if (hit.transform.childCount > 0)
                {
                    item = hit.transform.GetChild(0);

                    if (item.CompareTag(SEAT_TAG))
                    {
                        seat = item.GetComponent<Seat>();
                        heldItem = seat.GetCopy();

                        if (!TutorialManager.TutorialOn)
                        {
                            recycleGO.SetActive(true);
                            recycleText.SetActive(true);

                            addHuggyGO.SetActive(false);
                            costText.SetActive(false);
                        }

                        offset = worldPos - seat.GetWorldPos();

                        // Raise Picked Huggy Event
                        pickedHuggy?.Invoke(seat.GetLevel());

                        seat.TurnOffHighlight();
                    }                               
                }                      
            }
        }

        if (GetTouchUp())
        {
            if (seat == null)
            {
                if (ShootRaycast() && !IsPointerOverGameObject())
                {
                    if (hit.transform.childCount > 0)
                    {
                        item = hit.transform.GetChild(0);

                        if (item.CompareTag(GIFT_TAG))
                        {
                            seatManager.OpenGift(item.gameObject);
                        }
                        else if (item.CompareTag(PUZZLE_TAG))
                        {
                            seatManager.RemovePuzzlePieceFromSeat(item.gameObject);
                            GameManager.instance.RewardManager.OpenPuzzlePiecePanel();
                        }
                        else if (item.CompareTag(ADD_TAG))
                        {
                            seatManager.OpenMoreSeatsPanel();

                            // Play Sound
                            SoundManager.instance.PlayAudioClip((int)AudioEffect.MoreSeatsSound);
                        }
                    }
                }
            }
            else
            {
                // Check for Raycast, if hit then proceed or Return to Seat

                if (ShootRaycast())
                {
                    // Check if childCount is more than 0, can be false for Empty Bush & Remove 

                    if (hit.transform.childCount > 0)
                    {
                        item = hit.transform.GetChild(0);

                        // Check if filled Bush has Seat Tag or not, if not then Return to Seat

                        if (item.CompareTag(SEAT_TAG))
                        {
                            // Check if same Seat as held one, if same then Return to Seat

                            if (item.gameObject != seat.gameObject)
                            {
                                Seat secondSeat = item.GetComponent<Seat>();

                                // Check if the Seat Levels are same, if same then Swap or else Merge

                                if (seat.GetLevel() == secondSeat.GetLevel())
                                {
                                    // Merge with filled Seat

                                    seatManager.MergeSeats(
                                        first: seat.transform.parent,
                                        second: secondSeat.transform.parent,
                                        seat.GetLevel());
                                }
                                else
                                {
                                    // Swap with filled Seat

                                    Transform secondItem = secondSeat.GetCopy();

                                    seatManager.SwapSeats(
                                        first: seat.transform.parent,
                                        second: secondSeat.transform.parent,
                                        firstLvl: seat.GetLevel(),
                                        secondLvl: secondSeat.GetLevel());

                                    heldItem.position = seat.GetWorldPos();
                                    secondItem.position = secondSeat.GetWorldPos();

                                    seat.ReturnToSeat();
                                    secondSeat.ReturnToSeat();
                                }
                            }
                            else
                            {
                                seat.ReturnToSeat();
                            }
                        }
                        else
                        {
                            seat.ReturnToSeat();
                        }
                    }
                    else
                    {
                        if (hit.transform.CompareTag(REMOVE_TAG))
                        {
                            // Remove Seat

                            if (TutorialManager.TutorialOn)
                            {
                                seat.ReturnToSeat();
                            }
                            else
                            {
                                seatManager.RemoveHuggyFromSeat(seat.gameObject, seat.GetLevel());
                            }
                        }
                        else
                        {
                            // Swap with empty Seat

                            if (!TutorialManager.TutorialOn)
                            {
                                seatManager.SwapSeats(
                                    first: seat.transform.parent,
                                    second: hit.transform,
                                    firstLvl: seat.GetLevel());

                                heldItem.position = seat.GetWorldPos();
                            }

                            seat.ReturnToSeat();
                        }
                    }
                }
                else
                {
                    seat.ReturnToSeat();
                }

                seat = null;
                heldItem = null;

                recycleGO.SetActive(false);
                recycleText.SetActive(false);

                addHuggyGO.SetActive(true);
                costText.SetActive(true);

                // Raise Dropped Huggy Event
                droppedHuggy?.Invoke();
            }         
        }
    }

    private bool ShootRaycast()
    {
        worldPos = mainCam.ScreenToWorldPoint(GetTouchPosition());

        hit = Physics2D.Raycast(worldPos, Vector2.zero);

        return hit;
    }

    private void MoveHeldItem()
    {
        if (heldItem != null)
        {
            heldItem.position = mainCam.ScreenToWorldPoint(GetTouchPosition()) - offset;
        }
    }
}
