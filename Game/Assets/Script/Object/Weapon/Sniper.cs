using UnityEngine;
using Cysharp.Threading.Tasks;

public class Sniper : MonoBehaviour, IWeapon, IEntity, IChannel
{
    private SpriteRenderer renderer;
    private EventManager eventManager;
    public float coolTime = 1f;
    public float reloadingTime = 3f;

    private float currentCoolTime;
    private int currentBullet;
    private int maxBullet;

    private GameObject character;
    private GameObject bulletOrigin;
    private GameObject shotgunBulletOrigin;

    private int objectKey;
    private Vector3 originScale = Vector3.one;
    private Player player;
    private Transform target;
    private PlayerTrigger trigger;

    private bool isReLoading = false;
    private bool isBulletTrigger = false;
    private float bulletSpeed = 10f;
    private int bulletDamage = 10;

    private SniperType sniperType;
    public void Attack()
    {
        // 탄창도 생각해야지.
        if (isReLoading)
            return;

        AttackLogic();
    }

    public bool CanAttack() => currentCoolTime <= 0f;

    public int GetObjectKey() => objectKey;

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch (channel)
        {
            case ChannelInfo.ReLoading:
                ReLoading();
                break;
        }
    }

    public void OnDespawn()
    {
        this.transform.localScale = originScale;
    }

    public void OnSpawn()
    {
        originScale = this.transform.localScale;
    }

    public void SetObjectKey(int _key) => objectKey = _key;


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

    public void SetUp(Player player)
    {
        eventManager = Locator<EventManager>.Get();
        this.player = player;
        trigger = player.trigger;
        character = player.character;
        trigger.gameObject.SetActive(true);
        sniperType = SniperType.R1895;
        SniperSetting(sniperType);
        BulletSetup().Forget();
    }

    public void UpdateCoolTime(float deltaTime)
    {
        if (currentCoolTime >= 0f)
            currentCoolTime -= deltaTime;
        else
            currentCoolTime = 0f;
    }

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

    private void AttackLogic()
    {
        if (!IsEnableShotting())
            return;

        currentCoolTime = coolTime;
        eventManager.Notify(ChannelInfo.PlayerAnimation, true);

        Shot();
        currentBullet--;
      
        if (currentBullet <= 0)
        {
            EmptyBullet();
            return;
        }
    }

    private bool IsEnableShotting()
    {
        target = trigger.FindNearest();

        if (target == null)
        {
            eventManager.Notify(ChannelInfo.PlayerAnimation, false);
            return false;
        }
        else
            return true;
    }

    private void EmptyBullet()
    {
        isReLoading = true;
        player.reloadingObject.gameObject.SetActive(true);
        ReLoadingSound();
        eventManager.Notify(ChannelInfo.PlayerAnimation, false);
    }

    private void ReLoading()
    {
        currentBullet = maxBullet;
        isReLoading = false;
        eventManager.Notify(ChannelInfo.PlayerAnimation, false);
    }

    private void Shot()
    {
        float angle = BulletAngle();
        var factory = Locator<Factory>.Get();

        if (shotgunBulletOrigin == null || bulletOrigin == null)
        {
            currentBullet++;
            return;
        }

        if (sniperType == SniperType.M590)
        {
            var bulletComponent = shotgunBulletOrigin.GetComponent<ShotgunBullet>();

            // 지금 shotgun Bullet은 effectSlot으로 가야해.
            //Vector3 bulletPosition = new Vector3(0.8f, 0, 0);
            Quaternion aimRotation = Quaternion.Euler(0, 0, angle);
            Quaternion effectRotation = character.transform.localRotation * aimRotation;

            player.effectSlot.transform.localPosition = Vector3.zero;
            player.effectSlot.transform.localRotation = effectRotation;

            var bullet = factory.Create<ShotgunBullet>(bulletComponent, Vector3.zero, aimRotation, parent: player.effectSlot.transform);
            bullet.SetAttackPoint(bulletDamage);
        }
        else
        {
            var bulletComponent = bulletOrigin.GetComponent<Bullet>();

            // 해당 angle이 오른쪽을 바라보고 있다면 그대로 진행하고 angle이 왼쪽이면 charcter rotatioin을 180도로 회전시키는데 angle도 y축 기준으로 angle 처리를 해야 한다.
            var bullet = factory.Create<Bullet>(bulletComponent, player.weaponSlot.transform.position, Quaternion.Euler(0, 0, angle));
            bullet.BulletSetup(isBulletTrigger);
            bullet.Launch(bulletSpeed);
            bullet.SetAttackPoint(bulletDamage);
        }
        ShotSound();
    }

    private void ShotSound()
    {
        var soundManager = Locator<SoundManager>.Get();
        switch (sniperType)
        {
            case SniperType.R1895:
                soundManager.PlaySFX(SFX.RevolverShot);
                break;
            case SniperType.M590:
                soundManager.PlaySFX(SFX.ShotgunShot);
                break;
            case SniperType.AWPA:
            case SniperType.AWPB:
                int random = Random.Range((int)SFX.SniperShot1, (int)SFX.SniperShot4 + 1);
                soundManager.PlaySFX((SFX)random);
                break;
        }
    }

    private void ReLoadingSound()
    {
        var soundManager = Locator<SoundManager>.Get();
        var battleManager = Locator<BattleManager>.Get();
        var stat = battleManager.GetStat();
        int count = stat.count;

        switch (sniperType)
        {
            case SniperType.R1895:
                soundManager.PlaySFX(SFX.RevolverReloading);
                break;
            case SniperType.M590:
                soundManager.PlaySFX(SFX.ShotgunReloading);
                break;
            case SniperType.AWPA:
            case SniperType.AWPB:
                soundManager.PlaySFX(SFX.SniperReloading);
                break;
        }
    }

    private async UniTask BulletSetup()
    {
        var resourceManager = Locator<ResourceManager>.Get();
        bulletOrigin = await resourceManager.Get<GameObject>("Bullet");
        shotgunBulletOrigin = await resourceManager.Get<GameObject>("Shotgun Bullet");
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

    private void SniperSetting(SniperType sniperType)
    {
        // 무엇이 바뀌어야 할까?
        // coolTime, reloadingTime, maxBullet, bulletSpeed, Resource, attackRadius
        this.sniperType = sniperType;

        switch (sniperType)
        {
            case SniperType.R1895:
                trigger.AttackRadiusSetting(3f);
                isBulletTrigger = false;
                maxBullet = 6;
                coolTime = 1f;
                bulletSpeed = 20f;
                reloadingTime = 3f;
                bulletDamage = 8;
                break;
            case SniperType.M590:
                trigger.AttackRadiusSetting(2f);
                isBulletTrigger = false;
                maxBullet = 5;
                coolTime = 1f;
                bulletSpeed = 20f;
                reloadingTime = 4f;
                bulletDamage = 10;
                break;
            case SniperType.AWPA:
                trigger.AttackRadiusSetting(10f);
                isBulletTrigger = false;
                maxBullet = 5;
                coolTime = 1f;
                bulletSpeed = 30f;
                reloadingTime = 3f;
                bulletDamage = 100;
                break;
            case SniperType.AWPB:
                trigger.AttackRadiusSetting(10f);
                isBulletTrigger = true;
                maxBullet = 5;
                coolTime = 1f;
                bulletSpeed = 30f;
                reloadingTime = 3f;
                bulletDamage = 100;
                break;
        }
        currentBullet = maxBullet;

        SniperImage(sniperType).Forget();
    }

    private async UniTask SniperImage(SniperType sniperType)
    {
        if (renderer == null)
            renderer = this.GetComponent<SpriteRenderer>();

        renderer.enabled = false;

        var resourceManager = Locator<ResourceManager>.Get();

        switch (sniperType)
        {
            case SniperType.R1895:
                renderer.sprite = await resourceManager.Get<Sprite>("R1895 Sprite");
                break;
            case SniperType.M590:
                renderer.sprite = await resourceManager.Get<Sprite>("M590 Sprite");
                break;
            case SniperType.AWPA:
                renderer.sprite = await resourceManager.Get<Sprite>("AWP Sprite");
                break;
            case SniperType.AWPB:
                renderer.sprite = await resourceManager.Get<Sprite>("AWP Sprite");
                break;
        }

        renderer.enabled = true;
    }

    public void UpGrade()
    {
        switch (sniperType)
        {
            case SniperType.R1895:
                sniperType = SniperType.M590;
                SniperSetting(sniperType);
                break;
            case SniperType.M590:
                sniperType = SniperType.AWPA;
                SniperSetting(sniperType);
                break;
            case SniperType.AWPA:
                sniperType = SniperType.AWPB;
                SniperSetting(sniperType);
                break;
        }
    }
}


public enum SniperType
{
    R1895,
    M590,
    AWPA,
    AWPB,
}