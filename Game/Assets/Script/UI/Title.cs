using UnityEngine;

public class Title : MonoBehaviour, IChannel
{
    public GameObject selectPlayer;
    public GameObject title;

    public void Awake()
    {
        selectPlayer.SetActive(false);
        title.SetActive(true);
        var soundManger = Locator<SoundManager>.Get();
        soundManger.PlayBGM(BGM.Title);
    }

    public void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.GameStart, HandleEvent);
    }

    public void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.GameStart, HandleEvent);
    }

    public void GameStart()
    {
        selectPlayer.SetActive(true);
        title.SetActive(false);

        var soundManger = Locator<SoundManager>.Get();
        soundManger.PlayBGM(BGM.Main);
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
            case ChannelInfo.GameStart:
                if(information is EnableTitle isEnanleTitle) {
                    title.SetActive(isEnanleTitle.isEnableTitle);
                    selectPlayer.SetActive(isEnanleTitle.isEnablePlayer);

                    if(isEnanleTitle.isEnableTitle) {
                        var soundManger = Locator<SoundManager>.Get();
                        soundManger.PlayBGM(BGM.Title);
                    }

                    if (isEnanleTitle.isEnablePlayer) {
                        var soundManger = Locator<SoundManager>.Get();
                        soundManger.PlayBGM(BGM.Main);
                    }
                }
                break;
        }
    }
}

public struct EnableTitle
{
    public bool isEnableTitle;
    public bool isEnablePlayer;
}