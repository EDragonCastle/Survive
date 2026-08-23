using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;

public class Setting : MonoBehaviour, IChannel
{
    [SerializeField]
    private AudioMixer audioMixer;

    public GameObject setting;

    public Slider bgm;
    public Slider sfx;

    private float maxValue;
    private async void Awake()
    {
        setting.SetActive(false);
        maxValue = bgm.maxValue;
        await AudioSetup();
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.Setting, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.Setting, HandleEvent);
    }

    public void RestartGame()
    {
        setting.SetActive(false);

        var factory = Locator<Factory>.Get();
        factory.ReleaseAll();

        var eventManager = Locator<EventManager>.Get();
        EnableTitle enableTitle = new EnableTitle();
        enableTitle.isEnablePlayer = true;
        enableTitle.isEnableTitle = false;

        var battleManager = Locator<BattleManager>.Get();
        var player = battleManager.GetPlayer();
        player.RestartButton();

        eventManager.Notify(ChannelInfo.GameStart, enableTitle);
    }

    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.Setting:
                if (information is bool isEnable)
                    setting.SetActive(isEnable);
                break;
        }
    }

    public void SetBGMVolum(float sliderValue)
    {
        SetVolume("BGM", sliderValue);
    }
    
    public void SetSFXVolume(float sliderValue)
    {
        SetVolume("SFX", sliderValue);
    }

    private void SetVolume(string parameterName, float sliderValue)
    {
        float normalize = sliderValue / maxValue;

        float db = normalize > 0.0001f ? Mathf.Log10(normalize) * 20f : -80f;

        audioMixer.SetFloat(parameterName, db);
    }

    public void ExitSetting()
    {
        Time.timeScale = 1;
        setting.SetActive(false);
    }

    private async UniTask AudioSetup()
    {
        var resourceManager = Locator<ResourceManager>.Get();
        audioMixer = await resourceManager.Get<AudioMixer>("Main Sound");
    }

}
