using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Music")]
    [SerializeField] private AudioSource mainAudioSource;
    [SerializeField] private float bgMusicDelay = 1f;
    [SerializeField] private GameObject musicToggleOn, musicToggleOff;
    
    [Header("Sound")]
    [SerializeField] private AudioSource playOneShotAudioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private GameObject soundToggleOn, soundToggleOff;

    [Header("Haptic")]
    [SerializeField] private GameObject hapticToggleOn;
    [SerializeField] private GameObject hapticToggleOff;

    public bool Music { get; private set; } = true;
    public bool Sound { get; private set; } = true;
    public bool Haptic { get; private set; } = true;

    private float audioLength;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        audioLength = mainAudioSource.clip.length;
        InvokeRepeating("PlayBGMusic", 0f, audioLength + bgMusicDelay);

        RefreshBGMusic();
        RefreshSound();
    }

    public void LoadData(bool music, bool sound, bool haptic)
    {
        Music = music;
        Sound = sound;
        Haptic = haptic;
    }

    // Music

    private void PlayBGMusic()
    {
        RefreshBGMusic();
    }

    public void ToggleBGMusic()
    {
        Music = !Music;

        RefreshBGMusic();
    }

    private void RefreshBGMusic()
    {
        if (Music)
        {
            mainAudioSource.Play();
        }
        else
        {
            mainAudioSource.Stop();
        }

        musicToggleOn.SetActive(Music);
        musicToggleOff.SetActive(!Music);
    }

    // Sound

    public void PlayAudioClip(int enumIndex)
    {
        playOneShotAudioSource.PlayOneShot(audioClips[enumIndex]);
    }

    public void ToggleSound()
    {
        Sound = !Sound;

        RefreshSound();
    }

    private void RefreshSound()
    {
        playOneShotAudioSource.mute = !Sound;

        soundToggleOn.SetActive(Sound);
        soundToggleOff.SetActive(!Sound);
    }

    // Haptic

    public void VibrateDevice()
    {
        if (Haptic)
            Handheld.Vibrate();
    }

    public void ToggleHaptic()
    {
        Haptic = !Haptic;

        RefreshHaptic();
    }

    private void RefreshHaptic()
    {
        hapticToggleOn.SetActive(Haptic);
        hapticToggleOff.SetActive(!Haptic);
    }
}

public enum AudioEffect
{
    CloseButtonClicked,
    RewardCashIn,
    GotItClicked,
    RewardsButtonClicked,
    SettingsButtonClicked,
    ShopButtonClicked,
    MoreSeatsSound,
    DailyTaskOrAchievementClicked,
    AutoMergeClicked,
    CoinDoubleClicked,
    YESButtonClicked,
    MergeSound,
    HuggyGeneratedSound,
    SellHuggySound,
    SettingsInnerButtonClicked,
    BackOrInfoButtonClicked,
    ShopInnerButtonClicked,
    PieceReveal, 
    BubblePop,
    CoinShort,
    DiamondShort,
    StarStart,
    StarEnd,
    NewPlayer
}
