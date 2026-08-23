using UnityEngine;
using Cysharp.Threading.Tasks;

public class ShotgunBullet : MonoBehaviour, IEntity, IStat
{
    private float effectTime = 0.2f;
    private int objectKey;
    private Vector3 originScale = Vector3.one;

    private int damage = 10;

    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void OnDespawn()
    {
        this.transform.localScale = originScale;
    }

    public void OnSpawn()
    {
        originScale = this.transform.localScale;
        Release(effectTime).Forget();
    }

    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        if (parent != null)
            this.transform.SetParent(parent);

        this.transform.localPosition = position;
        this.transform.rotation = rotation;
        this.transform.localScale = originScale * multiplier;
    }

    private async UniTask Release(float lifeTime)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(lifeTime));
        var factory = Locator<Factory>.Get();
        factory.Release(this);
    }

    public int GetAttackPoint() => damage + AddtiveDamage();

    public void SetAttackPoint(int damageInput) => damage = damageInput;

    public int GetHP()
    {
        throw new System.NotImplementedException();
    }

    public void SetHP(int _hp)
    {
        throw new System.NotImplementedException();
    }

    private int AddtiveDamage()
    {
        var battleManager = Locator<BattleManager>.Get();
        var stat = battleManager.GetStat();
        return stat.addtiveDamage;
    }
}
