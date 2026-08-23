using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class AbilityWindow : MonoBehaviour
{
    public Image titleImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI explanationText;

    private Stat stat;

    public async UniTask Setup(SkillType type)
    {
        this.gameObject.SetActive(false);
        AbilitySetting weaponSetting = new AbilitySetting();
        var result = await weaponSetting.Execute(type);

        titleImage.sprite = result.titleImage;
        titleText.text = result.titleText;
        explanationText.text = result.explanationText;
        stat = result.stat;
        this.gameObject.SetActive(true);
    }

    public void Select()
    {
        // Upgrade는 어떻게 처리해야할까?
        var battleManager = Locator<BattleManager>.Get();
        var currentStat = battleManager.GetStat();
        
        var player = battleManager.GetPlayer();
        Stat newStat = currentStat + stat;
        battleManager.SetStat(newStat);

        if(stat.isUpgrade)
        {
            // 여기서 Upgrade를 
            var upgradeWeapons = player.GetWeapons();
            foreach (var weapon in upgradeWeapons) {
                weapon.UpGrade();
            }
        }

        // 체력을 최대로 올린다.
        int maxHP = Mathf.RoundToInt(player.maxHP * newStat.hpMultipier);
        player.playerHP.SetHP(maxHP, maxHP);
        player.SetHP(maxHP);

        var eventManager = Locator<EventManager>.Get();
        eventManager.Notify(ChannelInfo.AbilitySelect, false);
    }
}
