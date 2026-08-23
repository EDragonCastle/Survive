using UnityEngine;

public class AbilitySelectParent : MonoBehaviour, IChannel
{
    public GameObject abilitySelectUI;

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.AbilitySelect:
                if (information is bool isActive)
                {
                    if (isActive)
                        Time.timeScale = 0;
                    else
                        Time.timeScale = 1;

                    var eventManager = Locator<EventManager>.Get();
                    eventManager.Notify(ChannelInfo.Pause, isActive);

                    abilitySelectUI.SetActive(isActive);
                }
                break;
        }
    }

    private void Awake()
    {
        abilitySelectUI.SetActive(false);
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.AbilitySelect, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.AbilitySelect, HandleEvent);
    }
}
