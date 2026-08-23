using UnityEngine;

public class BulletCollider : MonoBehaviour
{
    public Bullet bullet;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        var factory = Locator<Factory>.Get();
        factory.Release(bullet);

        var targetDamageInterface = collision.gameObject.GetComponent<IDamageable>();
        targetDamageInterface.TakeDamage(bullet.GetAttackPoint());
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        var targetDamageInterface = collision.gameObject.GetComponent<IDamageable>();
        targetDamageInterface.TakeDamage(bullet.GetAttackPoint());
    }
}
