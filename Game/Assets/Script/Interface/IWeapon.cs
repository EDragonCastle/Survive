public interface IWeapon
{
    public bool CanAttack();
    public void Attack();
    public void UpdateCoolTime(float deltaTime);
    public void SetUp(Player player);
    public void UpGrade();
}


// IWeapon에 뭐가 있어야 할 지 생각하자.

// 공격할 수 있는지에 대한 유무를 알고 있어야겠네?
// 그리고 공격하는 기능이 있겠다.

// Stat 정보는 따로 Interface를 만드는 것도 나쁘지 않아 보인다.

// IWeapon은 쿨타임을 가지지 않고 methord만 가지고 있는다.


// 1. public bool IsAttackable();
// 2. public void Attack();
// Weapon에서 Player를 알고 있으면 좋긴해.

// Setup에 문제가 있네.
// 검에만 사용할 것 같은데