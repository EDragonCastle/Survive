using UnityEngine;
using Cysharp.Threading.Tasks;

public class Rifle : MonoBehaviour, IWeapon, IEntity, IChannel
{
    private SpriteRenderer renderer;

    public float coolTime = 1f;
    public float coolTimeOrigin = 1f;
    public float reloadingTime = 2f;

    private int objectKey;
    private EventManager eventManager;

    private GameObject character;

    private float currentCoolTime;
    private int currentBullet;
    private int maxBullet;
    
    private bool isReLoading = false;

    private GameObject bulletOrigin;
    private Vector3 originScale = Vector3.one;
    private PlayerTrigger trigger;

    private Transform target;
    private float bulletSpeed = 10f;
    private Player player;
    private bool isBulletTrigger = false;
    private int bulletDamage = 10;

    private RifleType rifleType;
    private bool isAttacking = false;

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.ReLoading, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.ReLoading, HandleEvent);
    }

    public void Attack()
    {
        // 탄창도 생각해야지.
        if (isReLoading)
            return;

        AttackLogic();
    }

    public bool CanAttack() => currentCoolTime <= 0f;
    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void OnDespawn()
    {
        this.transform.localScale = originScale;
        
    }

    public void OnSpawn()
    {
        originScale = this.transform.localScale;
    }

    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        if (parent != null)
        {
            this.transform.SetParent(parent);
        }

        this.transform.localPosition = position;
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = originScale * multiplier;
    }

    // 이건 초기에만 사용
    public void SetUp(Player player)
    {
        eventManager = Locator<EventManager>.Get();
        trigger = player.trigger;
        character = player.character;
        trigger.gameObject.SetActive(true);
        rifleType = RifleType.Glock;
        RifleSetup(rifleType);
        BulletSetup().Forget();
        this.player = player;
    }
    
    // trigger를 들어갈 수 있는 방법이 없을텐데?
    public void UpdateCoolTime(float deltaTime)
    {
        if (isAttacking)
            return;

        if (currentCoolTime >= 0f)
            currentCoolTime -= deltaTime;
        else
        {
            currentCoolTime = 0f;
            isAttacking = true;
        }
    }

    private async UniTask BulletSetup()
    {
        var resourceManager = Locator<ResourceManager>.Get();
        bulletOrigin = await resourceManager.Get<GameObject>("Bullet");
    }

    private void AttackLogic()
    {
        if (!IsEnableShotting())
            return;

        currentCoolTime = GetCoolTime();
        eventManager.Notify(ChannelInfo.PlayerAnimation, true);

        Shot();
        currentBullet--;

        if (currentBullet <= 0)
        {
            EmptyBullet();
            return;
        }

      
        isAttacking = false;
    }

    private float BulletAngle()
    {
        Vector2 direction = target.position - character.transform.position;
        bool isRight = direction.x >= 0f;

        if (isRight)
            character.transform.rotation = Quaternion.identity;
        else
        {
            character.transform.rotation = Quaternion.Euler(0, 180f, 0f);
            direction.x *= 1f;
        }

        float angle = Vector2.SignedAngle(Vector2.up, direction);
        return angle;
    }

    private void Shot()
    {
        var bulletComponent = bulletOrigin.GetComponent<Bullet>();
        var factory = Locator<Factory>.Get();

        float angle = BulletAngle();

        // 해당 angle이 오른쪽을 바라보고 있다면 그대로 진행하고 angle이 왼쪽이면 charcter rotatioin을 180도로 회전시키는데 angle도 y축 기준으로 angle 처리를 해야 한다.
        var bullet = factory.Create<Bullet>(bulletComponent, player.weaponSlot.transform.position, Quaternion.Euler(0, 0, angle));
        bullet.BulletSetup(isBulletTrigger);
        bullet.Launch(bulletSpeed);
        bullet.SetAttackPoint(bulletDamage);
        ShotSound();

    }

    private void ShotSound()
    {
        var soundManager = Locator<SoundManager>.Get();
        switch (rifleType)
        {
            case RifleType.Glock:
                soundManager.PlaySFX(SFX.PistolShot);
                break;
            case RifleType.MicroUzi:
                int random = Random.Range((int)SFX.UziShot1, (int)SFX.UziShot4 + 1);
                soundManager.PlaySFX((SFX)random);
                break;
            case RifleType.AKA:
                soundManager.PlaySFX(SFX.RifleShot);
                break;
            case RifleType.AKB:
                soundManager.PlaySFX(SFX.RifleShot);
                break;
        }
        
    }

    private void ReloadingSound()
    {
        var soundManager = Locator<SoundManager>.Get();

        switch (rifleType)
        {
            case RifleType.Glock:
                soundManager.PlaySFX(SFX.PistolReloading);
                break;
            case RifleType.MicroUzi:
                soundManager.PlaySFX(SFX.UziReloading);
                break;
            case RifleType.AKA:
            case RifleType.AKB:
                soundManager.PlaySFX(SFX.RifleReloading);
                break;
        }
    }

    private bool IsEnableShotting()
    {
        target = trigger.FindNearest();

        if (target == null) {
            eventManager.Notify(ChannelInfo.PlayerAnimation, false);
            return false;
        }
        else
            return true;
    }

    private void EmptyBullet()
    {
        isReLoading = true;
        isAttacking = false;
        player.reloadingObject.gameObject.SetActive(true);
        player.reloadingObject.reloadingTime = reloadingTime;

        ReloadingSound();
        eventManager.Notify(ChannelInfo.PlayerAnimation, false);
    }

    private void ReLoading()
    {
        currentBullet = maxBullet;
        isReLoading = false;
        eventManager.Notify(ChannelInfo.PlayerAnimation, false);
    }

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.ReLoading:
                ReLoading();
                break;
        }
    }

    // 레벨업 하면 이곳이 바뀌겠지
    private void RifleSetup(RifleType rifleType)
    {
        // 무엇이 바뀌어야 할까?
        // coolTime, reloadingTime, maxBullet, bulletSpeed, Resource, attackRadius

        switch (rifleType)
        {
            case RifleType.Glock:
                trigger.AttackRadiusSetting(3f);
                coolTimeOrigin = 1f;
                reloadingTime = 3f;
                isBulletTrigger = false;
                maxBullet = 17;
                bulletDamage = 4;
                break;
            case RifleType.MicroUzi:
                trigger.AttackRadiusSetting(2.5f);
                coolTimeOrigin = 0.1f;
                reloadingTime = 3f;
                bulletSpeed = 15f;
                maxBullet = 25;
                isBulletTrigger = false;
                bulletDamage = 3;
                break;
            case RifleType.AKA:
                trigger.AttackRadiusSetting(5f);
                coolTimeOrigin = 0.3f;
                reloadingTime = 3f;
                bulletSpeed = 20f;
                maxBullet = 30;
                isBulletTrigger = false;
                bulletDamage = 10;
                break;
            case RifleType.AKB:
                trigger.AttackRadiusSetting(10f);
                coolTimeOrigin = 0.3f;
                reloadingTime = 2f;
                bulletSpeed = 20f;
                maxBullet = 30;
                isBulletTrigger = true;
                bulletDamage = 10;
                break;
        }
        currentBullet = maxBullet;

        RifleImage(rifleType).Forget();
    }

    private async UniTask RifleImage(RifleType rifleType)
    {
        if (renderer == null)
            renderer = this.GetComponent<SpriteRenderer>();

        renderer.enabled = false;

        var resourceManager = Locator<ResourceManager>.Get();
        
        switch (rifleType)
        {
            case RifleType.Glock:
                renderer.sprite = await resourceManager.Get<Sprite>("Glock Sprite");
                break;
            case RifleType.MicroUzi:
                renderer.sprite = await resourceManager.Get<Sprite>("MicroUzi Sprite");
                break;
            case RifleType.AKA:
                renderer.sprite = await resourceManager.Get<Sprite>("AK47 Sprite");
                break;
            case RifleType.AKB:
                renderer.sprite = await resourceManager.Get<Sprite>("AK47 Sprite");
                break;
        }

        renderer.enabled = true;
    }

    public void UpGrade()
    {
        switch (rifleType)
        {
            case RifleType.Glock:
                rifleType = RifleType.MicroUzi;
                RifleSetup(rifleType);
                break;
            case RifleType.MicroUzi:
                rifleType = RifleType.AKA;
                RifleSetup(rifleType);
                break;
            case RifleType.AKA:
                rifleType = RifleType.AKB;
                RifleSetup(rifleType);
                break;
        }
    }

    private float GetCoolTime()
    {
        var battleManager = Locator<BattleManager>.Get();
        var stat = battleManager.GetStat();
        float attackSpeed = Mathf.Abs(stat.attackSpeedMultipier);
        float subTime = coolTimeOrigin * attackSpeed;

        float speed = coolTimeOrigin - subTime;
        if (speed <= 0)
            speed = 0.01f;

        return speed;
    }
}


public enum RifleType
{
    Glock,
    MicroUzi,
    AKA,
    AKB,
}

// 여기서 고민인게 Gun 하나의 Class로 해야 하나 아니면 총 개수에 따라 달라지긴 하는데
// 총은 그렇게 사기는 아닌 것 같은데 처음부터 원거리 공격이 가능하다 정도?
// 칼이 끝까지 도달하면 좋은데
// 총은 뭐가 없네?
// 중간에 선택할 수 있는 기능을 넣어야겠다.


