using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    [SerializeField]
    private Player player;


    private void OnCollisionStay2D(Collision2D collision)
    {
        var enemyStat = collision.gameObject.GetComponent<IStat>();

        if (enemyStat == null)
            return;

        int damage = enemyStat.GetAttackPoint();

        player.TakeDamage(damage);
    }
}
