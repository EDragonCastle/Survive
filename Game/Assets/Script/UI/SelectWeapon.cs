using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

[DefaultExecutionOrder(1)]
public class SelectWeapon : MonoBehaviour
{
    public async void Select(int index)
    {
        var battleManager = Locator<BattleManager>.Get();
        var player = battleManager.GetPlayer();
        player.SetDead(true);

        switch(index)
        {
            case 0:
                await player.EquipmentWeapon(WeaponType.Knife);
                break;
            case 1:
                await player.EquipmentWeapon(WeaponType.Rifle);
                break;
            case 2:
                await player.EquipmentWeapon(WeaponType.Sniper);
                break;
        }

        var eventManager = Locator<EventManager>.Get();
        EnableTitle enableTitle = new EnableTitle();
        enableTitle.isEnablePlayer = false;
        enableTitle.isEnableTitle = false;
        eventManager.Notify(ChannelInfo.GameStart, enableTitle);
        eventManager.Notify(ChannelInfo.GameReset);
        Time.timeScale = 1;
        player.SetDead(false);
        eventManager.Notify(ChannelInfo.SpawnMonster);
    }
}
