public struct Stat
{
    // 합연산으로 하자.
    public int addtiveDamage;
    // 속도도 합연산으로 하자.
    public float addtiveMoveSpeed;
    // 공격 속도는 곱연산으로 하자.
    public float attackSpeedMultipier;
    // HP는 곱연산으로 하자.
    public float hpMultipier;
    // Speical Weapon은 칼은 count고 총은 감소라서 int count로 하고 각자 다르게 구현되게 하면 된다.
    public int count;
    public bool isUpgrade;

    public static Stat operator +(Stat a, Stat b)
    {
        return new Stat {
            addtiveDamage = a.addtiveDamage + b.addtiveDamage,
            addtiveMoveSpeed = a.addtiveMoveSpeed + b.addtiveMoveSpeed,
            attackSpeedMultipier = a.attackSpeedMultipier - b.attackSpeedMultipier,
            hpMultipier = a.hpMultipier + b.hpMultipier,
            count = a.count + b.count,
        };
    }

}
