using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private GameObject firstTimeAgreementGO, normalAgreementGO;
    [SerializeField] private GameObject firstTimeInfoGO;
    [SerializeField] private GameObject tickGO;
    [SerializeField] private GameObject enterTheGamePanel;

    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float firstTimeAdditionalLoadingTime = 2f;
    [SerializeField] private float setLoadingTime = 3f;

    private AsyncOperation operation;
    private float loadingTime = 0f;
    private bool loadingDone;

    private readonly string agreementPref = "Agreed";
    private bool agreedPrivacyPolicyAndTermsOfUse = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startGameSound;

    [SerializeField] private GameObject huggyParadiseTitle;

    private void OnDisable()
    {
        PlayerPrefs.SetInt(agreementPref, agreedPrivacyPolicyAndTermsOfUse ? 1 : 0);
    }

    private void OnEnable()
    {
        agreedPrivacyPolicyAndTermsOfUse = PlayerPrefs.GetInt(agreementPref, 0) == 1;
    }

    private void Start()
    {
        huggyParadiseTitle.LeanScale(Vector3.one, 1f).setEaseOutBounce();

        PlayOneShotAudio(startGameSound);

        CheckForAgreement();

        operation = SceneManager.LoadSceneAsync((int)SceneIndexes.MAIN);

        StartCoroutine(LoadSceneProgress());
    }

    private void CheckForAgreement()
    {
        if (!agreedPrivacyPolicyAndTermsOfUse)
        {
            setLoadingTime += firstTimeAdditionalLoadingTime;

            normalAgreementGO.SetActive(false);

            firstTimeAgreementGO.SetActive(true);
            firstTimeInfoGO.SetActive(true);

            ToggleAgreement();
        }
    }

    public void ToggleAgreement()
    {
        agreedPrivacyPolicyAndTermsOfUse = !agreedPrivacyPolicyAndTermsOfUse;

        tickGO.SetActive(agreedPrivacyPolicyAndTermsOfUse);

        //Debug.Log($"Agreement: {agreedPrivacyPolicyAndTermsOfUse}");
    }

    IEnumerator LoadSceneProgress()
    {
        operation.allowSceneActivation = false;

        while (!loadingDone && !operation.isDone)
        {
            if (loadingTime >= setLoadingTime)
            {
                loadingDone = true;
            }

            loadingTime += Time.deltaTime;
            loadingText.text = $"{Mathf.Clamp(Mathf.RoundToInt(loadingTime / setLoadingTime * 100f), 0, agreedPrivacyPolicyAndTermsOfUse ? 100 : 99)}%";

            yield return null;
        }

        if (agreedPrivacyPolicyAndTermsOfUse)
        {
            AllowSceneActivation();
        }
        else
        {
            Utility.OpenGO(enterTheGamePanel);
        }
    } 

    private void AllowSceneActivation()
    {
        operation.allowSceneActivation = true;
    }

    public void ConfirmAgreement()
    {
        agreedPrivacyPolicyAndTermsOfUse = true;

        AllowSceneActivation();
    }

    public void QuitGame()
    {
        agreedPrivacyPolicyAndTermsOfUse = false;

        Application.Quit();
    }

    public void PlayOneShotAudio(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }
}

enum SceneIndexes
{
    LOADING,
    MAIN
}
