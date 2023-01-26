using UnityEngine;
using TMPro;

public class MessagePopup : MonoBehaviour
{
    public static MessagePopup instance;

    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private RectTransform parentTransform;
    [SerializeField] private int textLimit = 5;

    private int textCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void DisplayMessage(string message)
    {
        if (textCount >= textLimit) return;

        var popupGO = Instantiate(popupPrefab, parentTransform) as GameObject;
        textCount++;

        popupGO.GetComponentInChildren<TextMeshProUGUI>().text = message;

        popupGO.LeanScaleY(1f, .1f).setOnComplete(() =>
            popupGO.LeanMoveLocalY(popupGO.transform.localPosition.y + 300f, 1f).setOnComplete(() =>
                popupGO.GetComponent<CanvasGroup>().LeanAlpha(0, .5f).setOnComplete(() =>
                {
                    Destroy(popupGO);
                    textCount--;
                })
            )
        );      
    }
}
