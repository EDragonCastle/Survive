using UnityEngine;
using UnityEngine.UI;

public class ExperiencePoint : MonoBehaviour, IChannel
{
    private Slider expGauge;
    private int currentEXP = 0;
    private int maxEXP = 50;
    private int level = 1;

    [SerializeField]
    private float maxEXPRatio = 1.25f;

    private void Awake()
    {
        expGauge = this.gameObject.GetComponent<Slider>();
        expGauge.value = 0f;
    }

    private void Start()
    {
        var battleManager = Locator<BattleManager>.Get();
        battleManager.SetLevel(level);
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.EXP, HandleEvent);
        eventManager.Subscription(ChannelInfo.GameReset, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.EXP, HandleEvent);
        eventManager.Subscription(ChannelInfo.GameReset, HandleEvent);
    }

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.EXP:
                if (information is int expPoint)
                    Expoint(expPoint);
                break;
            case ChannelInfo.GameReset:
                ResetExp();
                break;
        }
    }

    private void Expoint(int value)
    {
        currentEXP += value;

        if (currentEXP >= maxEXP)
        {
            var eventManager = Locator<EventManager>.Get();
            eventManager.Notify(ChannelInfo.AbilitySelect, true);
            currentEXP = 0;
            maxEXP = Mathf.RoundToInt(maxEXP * maxEXPRatio);
            level++;
            var battleManager = Locator<BattleManager>.Get();
            battleManager.SetLevel(level);
        }
      
        expGauge.value = (float)currentEXP / maxEXP;
    }

    private void ResetExp()
    {
        currentEXP = 0;
        maxEXP = 50;
        expGauge.value = (float)currentEXP / maxEXP;
        level = 1;
        var battleManager = Locator<BattleManager>.Get();
        battleManager.SetLevel(level);
    }
}

