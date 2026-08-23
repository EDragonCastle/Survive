using Cysharp.Threading.Tasks;

public class AbilitySetting
{
    public async UniTask<AbilityInformation> Execute(SkillType type) => await AbilitySetup(type);

    private async UniTask<AbilityInformation> AbilitySetup(SkillType type)
    {
        AbilityInformation information = new AbilityInformation();
        var resourceManager = Locator<ResourceManager>.Get();
        Stat addStat = new Stat();
        switch (type)
        {
            case SkillType.MoveSpeed:
                var moveSpeedTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[MoveSpeed]");
                information.explanationText = "이동 속도를 증가시킨다.";
                information.titleText = "이동 속도 증가";
                information.titleImage = await moveSpeedTask;
                addStat.addtiveMoveSpeed = 1;
                break;
            case SkillType.AttackPoint:
                var attackPointTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[Attack]");
                information.explanationText = "공격력을 증가시킨다.";
                information.titleText = "공격력 증가";
                information.titleImage = await attackPointTask;
                addStat.addtiveDamage = 1;
                break;
            case SkillType.AttackSpeed:
                var attackSpeedTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[AttackSpeed]");
                information.explanationText = "공격 속도를 증가시킨다.";
                information.titleText = "공격 속도 증가";
                information.titleImage = await attackSpeedTask;
                addStat.attackSpeedMultipier = 0.1f;
                break;
            case SkillType.HP:
                var hpTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[Health]");
                information.explanationText = "체력을 증가시킨다.";
                information.titleText = "체력 증가";
                information.titleImage = await hpTask;
                addStat.hpMultipier = 0.1f;
                break;
            case SkillType.Upgrade:
                var upgradeTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[Upgrade]");
                information.explanationText = "장비 및 스킬을 강화합니다.";
                information.titleText = "업그레이드";
                information.titleImage = await upgradeTask;
                addStat.isUpgrade = true;
                break;
            case SkillType.SpecialWeapon:
                var specialKnifeTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[Attack]");
                var specialGunTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[AttackSpeed]");
                var specialMixTask = resourceManager.Get<UnityEngine.Sprite>("GameIcon[AttackSpeed]");

                bool isKnife = false;
                bool isRifle = false;

                addStat.count = 1;

                var speicalWeapons = GetWeapons();
                foreach(var weapon in speicalWeapons) {
                    if(weapon is Knife knife) {
                        isKnife = true;
                    }   
                    else if(weapon is Sniper sniper || weapon is Rifle rifle) {
                        isRifle = true;
                    }
                }

                if(isKnife && isRifle) {
                    information.titleImage = await specialMixTask;
                    information.titleText = "검기 추가 & 총 재장전 속도 증가";
                    information.explanationText = "검기를 추가합니다.\n총의 재장전 속도를 늘립니다.";
                }
                else if(isKnife) {
                    information.titleImage = await specialKnifeTask;
                    information.titleText = "검기 추가";
                    information.explanationText = "검기를 추가합니다.";
                }
                else {
                    information.titleImage = await specialGunTask;
                    information.titleText = "총 재장전 속도 증가";
                    information.explanationText = "총의 재장전 속도가 감소합니다.";
                }
                break;
        }
        information.stat = addStat;

        return information;
    }

    private System.Collections.Generic.List<IWeapon> GetWeapons()
    {
        var battleManager = Locator<BattleManager>.Get();
        var player = battleManager.GetPlayer();
        var weapons = player.GetWeapons();
        return weapons;
    }
}



public struct AbilityInformation
{
    public UnityEngine.Sprite titleImage;
    public string titleText;
    public string explanationText;
    public Stat stat;
}