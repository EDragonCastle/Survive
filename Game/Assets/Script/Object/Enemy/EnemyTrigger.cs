using UnityEngine;
public class EnemyTrigger : MonoBehaviour, IDamageable
{
    public Enemy enemy;
    
    public void TakeDamage(int damage)
    {
        enemy.TakeDamage(damage);
    }
}
