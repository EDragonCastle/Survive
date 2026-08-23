using UnityEngine;

public class AuraEffect : MonoBehaviour, IEntity, IAttackable
{
    private int objectKey;
    private Vector3 originScale = Vector3.one;

    public SpriteRenderer slashRenderer;
    public SpriteRenderer moonRenderer;

    private Rigidbody2D rigidBody;
    private int damage = 10;
    
    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void OnDespawn()
    {
        this.transform.localPosition = originScale;
    }

    public void OnSpawn()
    {
        originScale = this.transform.localScale;

        if (rigidBody == null)
            rigidBody = this.gameObject.GetComponent<Rigidbody2D>();

        Invoke("AutoRelease", 2f);
    }


    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        if (parent != null)
            this.transform.SetParent(parent);

        this.transform.localPosition = position;
        this.transform.localRotation = rotation;
        this.transform.localScale = originScale * multiplier;
    }

    public void ColorSetUp(Color color)
    {
        slashRenderer.color = color;
        moonRenderer.color = color;
    }

    public void Launch(float speed)
    {
        rigidBody.linearVelocity = transform.up * speed;
    }

    private void AutoRelease()
    {
        var factory = Locator<Factory>.Get();
        factory.Release(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var targetDamageInterface = collision.gameObject.GetComponent<IDamageable>();
        targetDamageInterface.TakeDamage(GetAttackPoint());
    }

    private int AddtiveDamage()
    {
        var battleManager = Locator<BattleManager>.Get();
        var stat = battleManager.GetStat();
        return stat.addtiveDamage;
    }

    public int GetAttackPoint() => damage + AddtiveDamage();
    public void SetAttackPoint(int damageInput) => damage = damageInput;

}
