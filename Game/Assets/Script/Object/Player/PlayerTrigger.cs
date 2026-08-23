using UnityEngine;
using System.Collections.Generic;


public class PlayerTrigger : MonoBehaviour
{
    [SerializeField]
    private int enemyCount = 30;

    private float detectRadius = 1f;
    private readonly HashSet<Transform> enemyInRange = new HashSet<Transform>();

    public void AttackRadiusSetting(float radius)
    {
        var boxCollider2D = this.GetComponent<CircleCollider2D>();
        boxCollider2D.radius = radius;
        detectRadius = radius;
    }

    private void Start()
    {
        var boxCollider2D = this.GetComponent<CircleCollider2D>();
        detectRadius = boxCollider2D.radius;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enemyInRange.Add(collision.transform);

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        enemyInRange.Remove(collision.transform);
    }

    // count가 있을텐데 count가 일정 수 이상이면 Ring 별로 구분하는 건?
    public Transform FindNearest()
    {
        if (enemyInRange.Count == 0)
            return null;

        if (enemyInRange.Count < enemyCount)
            return FindNearestLinear();

        var enemyManger = Locator<EnemyManager>.Get();
        return enemyManger.FindNearest(this.transform.position, detectRadius);
    }

    private Transform FindNearestLinear()
    {
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach(var enemy in enemyInRange)
        {
            if (enemy == null) continue;

            float distance = ((Vector2)enemy.position - (Vector2)this.transform.position).sqrMagnitude;

            if(minDistance > distance) {
                minDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

}

// 생각을 해보자.
// Attack Collider를 만드는게 더 나을 것 같기도하다.

// 근데 내 게임에서의 Trigger는 직접 경험치를 먹으러 가는게 아니라 몬스터를 잡으면 레벨업을 시킬 것이다.
// 그렇다면 이 Trigger는 공격 사거리로 대체해도 괜찮을 것 같다.

// 