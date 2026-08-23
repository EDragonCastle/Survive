using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Bullet : MonoBehaviour, IEntity, IAttackable
{
    private int objectKey;
    private Vector3 originScale = Vector3.one;

    private Rigidbody2D rigidBody2D;

    public GameObject bulletCollider;
    public GameObject bulletTrigger;
    private int damage = 10;

    private CancellationTokenSource lifeTimeToken;

    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void OnDespawn()
    {
        this.transform.localScale = originScale;

        lifeTimeToken?.Cancel();
        lifeTimeToken?.Dispose();
        lifeTimeToken = null;
    }

    public void OnSpawn()
    {
        originScale = this.transform.localScale;
    }

    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        if (parent != null)
            this.transform.SetParent(parent);

        this.transform.localPosition = position;
        this.transform.rotation = rotation;
        this.transform.localScale = originScale * multiplier;
    }

    public void Launch(float speed)
    {
        if (rigidBody2D == null)
            rigidBody2D = this.GetComponent<Rigidbody2D>();

        rigidBody2D.linearVelocity = transform.up * speed;
    }

    public void BulletSetup(bool isTrigger, float lifeTime = 2f)
    {
        if(isTrigger)
        {
            lifeTimeToken?.Cancel();
            lifeTimeToken?.Dispose();
            lifeTimeToken = new CancellationTokenSource();

            bulletCollider.SetActive(false);
            bulletTrigger.SetActive(true);
            AutoRelease(lifeTime, lifeTimeToken.Token).Forget();
        }
        else
        {
            bulletCollider.SetActive(true);
            bulletTrigger.SetActive(false);
        }
    }

    public void BulletSetup(BulletType bulletType)
    {
        switch (bulletType)
        {
            case BulletType.normal:
                break;
            case BulletType.shotgun:
                break;
        }
    }

    private async UniTask AutoRelease(float lifeTime, CancellationToken token)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(lifeTime), cancellationToken: token);
        var factoy = Locator<Factory>.Get();
        factoy.Release(this);
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

public enum BulletType
{
    normal,
    shotgun,
}