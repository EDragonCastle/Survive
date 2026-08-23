using UnityEngine;

public class GameClear : MonoBehaviour, IChannel
{
    public GameObject gameClear;

    private void Awake()
    {
        gameClear.SetActive(false);
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.GameClear, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.GameClear, HandleEvent);
    }

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch (channel)
        {
            case ChannelInfo.GameClear:
                if (information is bool isActive)
                {
                    Time.timeScale = 0;
                    gameClear.SetActive(isActive);
                    var factory = Locator<Factory>.Get();
                    factory.ReleaseAll();
                }
                break;
        }
    }

    public void ReStart()
    {
        var battleManager = Locator<BattleManager>.Get();
        var player = battleManager.GetPlayer();

        player.RestartButton();

        // weapon select로 다시 가야한다.
        gameClear.SetActive(false);
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
