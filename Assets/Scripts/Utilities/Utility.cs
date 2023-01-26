using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class Utility : MonoBehaviour
{
    public static Action PanelOpened;
    public static Action PanelClosed;

    public static void OpenGO(GameObject GO)
    {
        GO.SetActive(true);
        GO.GetComponent<CanvasGroup>().alpha = 0f;
        GO.GetComponent<CanvasGroup>().LeanAlpha(1f, .5f);
        GO.transform.GetChild(0).LeanScale(Vector3.one, .5f).setEaseOutBack();

        PanelOpened?.Invoke();
    }

    public static void CloseGO(GameObject GO)
    {
        GO.GetComponent<CanvasGroup>().LeanAlpha(0f, .3f);
        GO.transform.GetChild(0).LeanScale(Vector3.zero, .3f).setEaseInBack().setOnComplete(() => GO.SetActive(false));

        PanelClosed?.Invoke();
    }

    public static void OpenGOBounce(GameObject GO)
    {
        GO.SetActive(true);
        GO.GetComponent<CanvasGroup>().alpha = 0f;
        GO.GetComponent<CanvasGroup>().LeanAlpha(1f, .5f);
        GO.transform.GetChild(0).LeanScale(Vector3.one, .5f).setEaseOutBounce();

        PanelOpened?.Invoke();
    }

    public static void PopInOutGO(Transform GO, float toPos, float toScale)
    {
        var starPos = GO.localPosition;
        GO.LeanMoveLocalY(starPos.y + toPos, .5f).setOnComplete(() => GO.localPosition = starPos);
        GO.LeanScale(Vector3.one * toScale, .5f).setFrom(Vector3.zero).setEaseOutExpo().setOnComplete(() =>
        GO.localScale = Vector3.zero);
    }

    public static string ConvertToKMB(float amount)
    {
        if (amount > 999999999)
        {
            return $"{Math.Round(amount / 1000000000, 2)}b";
        }
        else if (amount > 999999)
        {
            return $"{Math.Round(amount / 1000000, 2)}m";
        }
        else if (amount > 999)
        {
            return $"{Math.Round(amount / 1000, 2)}k";
        }

        return $"{amount}";
    }

    public static string ConvertToMMSS(float amount)
    {
        return TimeSpan.FromSeconds(amount).ToString(@"mm\:ss");

        //return $"{Mathf.Floor(amount / 60).ToString("00")}:{Mathf.Floor(amount % 60).ToString("00")}";
    }

    public static string ConvertToHHMMSS(float amount)
    {
        return TimeSpan.FromSeconds(amount).ToString(@"hh\:mm\:ss");

        //return $"{Mathf.Floor(amount / 3600).ToString("00")}:{Mathf.Floor(Mathf.Floor(amount / 60) % 60).ToString("00")}:{Mathf.Floor(amount % 60).ToString("00")}";
    }

    public static string ConvertToGroupFormat(float amount)
    {
        return amount.ToString("N0");
    }

    public static void PrivacyPolicy()
    {
        Application.OpenURL("https://www.privacypolicies.com/live/853f0205-85ba-4810-93cf-bdcad9bb3abd");
    }

    public static void TermsAndConditions()
    {
        Application.OpenURL("https://www.termsfeed.com/live/45040f13-ff0d-4023-9d0f-60c3c635e372");
    }

    public static void ContactUs()
    {
        string email = "kingblingrules@gmail.com";
        string subject = UnityWebRequest.EscapeURL("Gift Redeem Feedback Huggy Paradise").Replace("+", "%20");
        string body = UnityWebRequest.EscapeURL("** Give your feedback here **").Replace("+", "%20");

        Application.OpenURL($"mailto:{email}?subject={subject}&body={body}");
    }
}
