using UnityEngine;

public class GameOver : MonoBehaviour, IChannel
{
    public GameObject gameover;

    private void Awake()
    {
        gameover.SetActive(false);
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.GameOver, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.GameOver, HandleEvent);
    }

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.GameOver:
                if (information is bool isActive)
                {
                    Time.timeScale = 0;
                    gameover.SetActive(isActive);
                    var factory = Locator<Factory>.Get();
                    factory.ReleaseAll();
                }
                break;
        }
    }

    public void ReStart()
    {
        // weapon select로 다시 가야한다.
        gameover.SetActive(false);
        var eventManager = Locator<EventManager>.Get();
        EnableTitle enableTitle = new EnableTitle();
        enableTitle.isEnablePlayer = true;
        enableTitle.isEnableTitle = false;
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
}
