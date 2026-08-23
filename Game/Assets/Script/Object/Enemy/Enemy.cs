using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class Enemy : MonoBehaviour, IEntity, IDamageable, IStat
{
    public GameObject target;
  
    public float moveSpeed = 0.05f;
    public Rigidbody2D rigidBody;
    public GameObject collider;
    public GameObject trigger;

    private Vector3 localScale;
    private int objectKey;

    public int maxHP = 100;
    private int hp = 100;

    private int attackDamage = 1;
    private SpriteRenderer renderer;

    private Animator animator;

    private void Awake()
    {
        rigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        localScale = this.transform.localScale;
        renderer = this.gameObject.GetComponent<SpriteRenderer>();
        animator = this.gameObject.GetComponent<Animator>();
    }

    public void OnSpawn()
    {
        localScale = this.transform.localScale;
        var enemyManager = Locator<EnemyManager>.Get();
        enemyManager.Register(this.transform);
        hp = maxHP;
    }

    public void OnDespawn()
    {
        this.transform.localScale = localScale;
        var enemyManager = Locator<EnemyManager>.Get();
        enemyManager.Unregister(this.transform);
        animator.SetBool("isDead", false);

        var battleManager = Locator<BattleManager>.Get();
        var spawner = battleManager.GetSpawner();
        spawner.RemoveEnmey(this);

        collider.SetActive(true);
        trigger.SetActive(true);
    }

    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        if (parent != null)
            this.transform.SetParent(parent);
  
        this.transform.position = position;
        this.transform.rotation = rotation;
        this.transform.localScale = localScale * multiplier;
    }

    public void EnemySetting(EnemyType type)
    {
        EnemyResource(type).Forget();
    }

    private async UniTask EnemyResource(EnemyType type)
    {
        this.gameObject.SetActive(false);

        List<UniTask> tasks = new List<UniTask>();
        
       // var spriteTask = SpriteSetting(type);
       // tasks.Add(spriteTask);
        var animatorTask = AnimatorSetting(type);
        tasks.Add(animatorTask);

        ColliderSetting(type);
        await UniTask.WhenAll(tasks);
        this.gameObject.SetActive(true);
    }

    private async UniTask SpriteSetting(EnemyType type)
    {
        var resourceManager = Locator<ResourceManager>.Get();
        Sprite sprite = null;
        switch (type)
        {
            case EnemyType.Slime:
            sprite = await resourceManager.Get<Sprite>("Slime Sprite");
                break;
            case EnemyType.Fly:
            sprite = await resourceManager.Get<Sprite>("Fly Sprite");
                break;
            case EnemyType.Mini:
            sprite = await resourceManager.Get<Sprite>("Mini Sprite");
                break;
            case EnemyType.Large:
            sprite = await resourceManager.Get<Sprite>("Large Sprite");
                break;
        }
        renderer.sprite = sprite;
    }

    private async UniTask AnimatorSetting(EnemyType type)
    {
        var resourceManager = Locator<ResourceManager>.Get();
        RuntimeAnimatorController animator = null;
        switch (type)
        {
            case EnemyType.Slime:
                animator = await resourceManager.Get<RuntimeAnimatorController>("Slime Animator");
                break;
            case EnemyType.Fly:
                animator = await resourceManager.Get<RuntimeAnimatorController>("Fly Animator");
                break;
            case EnemyType.Mini:
                animator = await resourceManager.Get<RuntimeAnimatorController>("Mini Animator");
                break;
            case EnemyType.Large:
                animator = await resourceManager.Get<RuntimeAnimatorController>("Large Animator");
                break;
        }

        this.animator.runtimeAnimatorController = animator;
    }

    private void ColliderSetting(EnemyType type)
    {
        var circleCollider = collider.GetComponent<CircleCollider2D>();
        
        var boxTrigger = trigger.GetComponent<BoxCollider2D>();

        switch (type)
        {
            case EnemyType.Slime:
                circleCollider.offset = new Vector2(0.015f, -0.20f);
                circleCollider.radius = 0.45f;
                boxTrigger.offset = new Vector2(0.015f, -0.20f);
                boxTrigger.size = new Vector2(0.94f, 0.61f);
                break;
            case EnemyType.Fly:
                circleCollider.offset = new Vector2(0f, -0.02f);
                circleCollider.radius = 0.5f;
                boxTrigger.offset = new Vector2(0f, -0.02f);
                boxTrigger.size = new Vector2(0.88f, 0.84f);
                break;
            case EnemyType.Mini:
                circleCollider.offset = new Vector2(0f, -0.2f);
                circleCollider.radius = 0.4f;
                boxTrigger.offset = new Vector2(0f, -0.19f);
                boxTrigger.size = new Vector2(0.7f, 0.66f);
                break;
            case EnemyType.Large:
                circleCollider.offset = new Vector2(0, -0.1f);
                circleCollider.radius = 0.45f;
                boxTrigger.offset = new Vector2(0f, -0.04f);
                boxTrigger.size = new Vector2(0.84f, 0.93f);
                break;
        }
    }

    public void SetUp(GameObject _target) => target = _target;

    public void TakeDamage(int damage)
    {
        hp -= damage;

        DamageFont(damage).Forget();

        if (hp <= 0)
            Dead();
    }

    private async UniTask DamageFont(int damage)
    {
        var resourceManager = Locator<ResourceManager>.Get();
        var factory = Locator<Factory>.Get();
        var damageFontOrigin = await resourceManager.Get<GameObject>("Damage Font");

        var damageFontComponent = damageFontOrigin.GetComponent<DamageFont>();
        var damageFont = factory.Create<DamageFont>(damageFontComponent, this.transform.position, Quaternion.identity);
        damageFont.Setup(damage, false);
    }

    private void Dead()
    {
        animator.SetBool("isDead", true);

        var enemyManager = Locator<EnemyManager>.Get();
        enemyManager.Unregister(this.transform);

        collider.SetActive(false);
        trigger.SetActive(false);

        var battleManager = Locator<BattleManager>.Get();
        var spawner = battleManager.GetSpawner();
        spawner.RemoveEnmey(this);

        var eventManager = Locator<EventManager>.Get();
        eventManager.Notify(ChannelInfo.EXP, 10);

        Invoke("EnemyRelease", 1f);
    }

    private void EnemyRelease()
    {
        var factory = Locator<Factory>.Get();
        factory.Release(this);
    }

    public int GetAttackPoint() => attackDamage;
    public void SetAttackPoint(int damageInput) => attackDamage = damageInput;

    public int GetHP() => hp;

    public void SetHP(int _hp)
    {
        maxHP = _hp;
        hp = maxHP;
    }
}

public enum EnemyType
{
    Slime,
    Fly,
    Mini,
    Large,
}