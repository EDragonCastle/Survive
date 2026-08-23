using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

[DefaultExecutionOrder(2)]
public class ChoiseAbilityWindow : MonoBehaviour
{
    // 뱀서는 스킬이 정해져있는데 이거는 아니니까 계속 증가하는 방향으로 진행하자.
    private List<SkillType> randomStatList = new List<SkillType>();
    private List<SkillType> currentStatSelection = new List<SkillType>();
    public List<AbilityWindow> abilitys;

    private void Awake()
    {
        Initalize();
    }

    private async void OnEnable()
    {
        // 선택창이 뜬다.
        await PrepareSkillWindow();
    }

    private async UniTask PrepareSkillWindow()
    {
        // randomStatList에서 3개를 뽑아야 한다.
        // 그리고 abilitys에 넣으면 된다.
        currentStatSelection = RandomStatSelect(abilitys.Count);

        int level = await UpgradeLevel();

        List<UniTask> tasks = new List<UniTask>();
        for(int i = level; i < abilitys.Count; i++)
        {
            tasks.Add(abilitys[i].Setup(currentStatSelection[i]));
        }

        await UniTask.WhenAll(tasks);
    }

    private List<SkillType> RandomStatSelect(int count)
    {
        List<SkillType> pool = new List<SkillType>(randomStatList);
        List<SkillType> result = new List<SkillType>();

        for(int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            result.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
        return result;
    }

    public void SelectSkill(int index)
    {
        abilitys[index].Select();
    }

    private async UniTask<int> UpgradeLevel()
    {
        var battleManager = Locator<BattleManager>.Get();
        int level = battleManager.GetLevel();
        if (level % 3 == 0) {
            await abilitys[0].Setup(SkillType.Upgrade);
            return 1;
        }
        return 0;
    }

    private void Initalize()
    {
        randomStatList.Add(SkillType.MoveSpeed);
        randomStatList.Add(SkillType.AttackPoint);
        randomStatList.Add(SkillType.AttackSpeed);
        randomStatList.Add(SkillType.SpecialWeapon);
        randomStatList.Add(SkillType.HP);
    }

}

public enum SkillType
{
    MoveSpeed,
    AttackPoint,
    AttackSpeed,
    HP,
    Upgrade,
    SpecialWeapon,
    End,
}

// Stat에 장난질을 할까?
// 줄 수도 있고 안 줄 수도 있게 만들자.
// 너무 많은 것을 하지는 말자.
