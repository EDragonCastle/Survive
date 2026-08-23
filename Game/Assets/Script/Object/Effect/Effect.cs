using UnityEngine;

public class Effect : MonoBehaviour, IEntity, IAttackable
{
    private int objectKey;
    private Vector3 originScale = Vector3.one;
    public float effectTime = 0.5f;
    private int damage = 4;

    private void Awake()
    {
        originScale = this.transform.localScale;
    }

    public void OnDespawn()
    {
        this.transform.localScale = originScale;
    }

    public void OnSpawn()
    {
        originScale = this.transform.localScale;
        Invoke("Release", effectTime);
    }

    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;
    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        if (parent != null)
            this.transform.SetParent(parent);

        this.transform.localPosition = position;
        this.transform.localRotation = rotation;
        this.transform.localScale = originScale * multiplier;
    }

    private void Release()
    {
        var factory = Locator<Factory>.Get();
        factory.Release(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var targetDamageInterface = collision.gameObject.GetComponent<IDamageable>();
        targetDamageInterface.TakeDamage(GetAttackPoint());
    }

    public int GetAttackPoint() => damage + AddtiveDamage();
    public void SetAttackPoint(int damageInput) => damage = damageInput;

    private int AddtiveDamage()
    {
        var battleManager = Locator<BattleManager>.Get();
        var stat = battleManager.GetStat();
        return stat.addtiveDamage;
    }
}
