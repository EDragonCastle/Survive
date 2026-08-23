using UnityEngine;

public class ShotgunTrigger : MonoBehaviour
{
    public ShotgunBullet bullet;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var targetDamageInterface = collision.gameObject.GetComponent<IDamageable>();
        targetDamageInterface.TakeDamage(bullet.GetAttackPoint());
    }
}
