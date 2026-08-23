using UnityEngine;
using UnityEngine.EventSystems;

public class SettingButton : MonoBehaviour
{
    public void SelectSetting()
    {
        Time.timeScale = 0;
        var eventManager = Locator<EventManager>.Get();
        eventManager.Notify(ChannelInfo.Setting, true);
    }
}
