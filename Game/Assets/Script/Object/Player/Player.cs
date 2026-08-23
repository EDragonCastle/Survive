using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class Player : MonoBehaviour, IChannel, IDamageable
{
    [SerializeField]
    public float moveSpeedOrigin = 1.0f;

    private float moveSpeed;

    [SerializeField]
    public PlayerHP playerHP;

    public GameObject character;
    public GameObject effectSlot;
    public GameObject weaponSlot;
    public ReLoading reloadingObject;

    private List<IWeapon> weaponList;

    private Vector2 input;
    private Rigidbody2D rigidBody;

    private bool isDead = false;
    private Animator animator;
    private bool dontRotation = false;

    public PlayerTrigger trigger;
    public int maxHP = 100;
    private int hp;

    private BattleManager battleManager;
    public bool isPause = false;

    public List<IWeapon> GetWeapons() => weaponList;
    public void SetDead(bool isDead) => this.isDead = isDead;

    private void Awake()
    {
        rigidBody = this.GetComponent<Rigidbody2D>();
        weaponList = new List<IWeapon>();
        hp = maxHP;

        battleManager = Locator<BattleManager>.Get();
        battleManager.ProvidePlayer(this);
        moveSpeed = moveSpeedOrigin;
        isDead = true;
    }

    private void Start()
    {
        animator = character.gameObject.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Subscription(ChannelInfo.PlayerAnimation, HandleEvent);
        eventManager.Subscription(ChannelInfo.Pause, HandleEvent);
        eventManager.Subscription(ChannelInfo.GameReset, HandleEvent);
    }

    private void OnDisable()
    {
        var eventManager = Locator<EventManager>.Get();
        eventManager.Unsubscription(ChannelInfo.PlayerAnimation, HandleEvent);
        eventManager.Unsubscription(ChannelInfo.Pause, HandleEvent);
        eventManager.Unsubscription(ChannelInfo.GameReset, HandleEvent);
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;
        
        // Physics Movement
        rigidBody.MovePosition(rigidBody.position + input.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (isDead || isPause)
            return;  

        // Key Input Movement
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        AnimationSetting(input);

        UsingWeapon();

        moveSpeed = moveSpeedOrigin + battleManager.GetStat().addtiveMoveSpeed;    
    }

    private void AnimationSetting(Vector2 input)
    {
        if(input.x != 0 || input.y != 0)
            animator.SetBool("isWalking", true);
        else
            animator.SetBool("isWalking", false);

        if (dontRotation)
            return;

        if(input.x > 0)
        {
            // 오른쪽을 봤다.
            character.transform.rotation = Quaternion.identity;
        }
        else if(input.x < 0)
        {
            // 왼쪽을 봤다.
            character.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void UsingWeapon()
    {
        if (weaponList.Count == 0)
            return;
    
        foreach(var weapon in weaponList)
        {
            weapon.UpdateCoolTime(Time.deltaTime);

            if (weapon.CanAttack())
                weapon.Attack();
        }
    }

    public void HandleEvent(ChannelInfo channel, object information = null)
    {
        switch(channel)
        {
            case ChannelInfo.PlayerAnimation:
                if(information is bool active)
                    dontRotation = active;
                break;
            case ChannelInfo.Pause:
                if (information is bool isEnable)
                    isPause = isEnable;
                break;
            case ChannelInfo.GameReset:
                PlayerReset();
                break;
        }
    }

    public async UniTask EquipmentWeapon(WeaponType weaponType)
    {
        WeaponSetting setting = new WeaponSetting();
        IWeapon weapon = await setting.Execute(weaponType, weaponSlot);
        weapon.SetUp(this);
        weaponList.Add(weapon);

    }

    public void TakeDamage(int damage)
    {
        hp -= damage;

        playerHP.SetHP(hp, maxHP);

        if (hp <= 0)
            Dead();
    }

    private void Dead()
    {
        isDead = true;
        reloadingObject.gameObject.SetActive(false);
        var eventManager = Locator<EventManager>.Get();
        eventManager.Notify(ChannelInfo.GameOver, true);

        var soundManager = Locator<SoundManager>.Get();
        int random = Random.Range((int)SFX.GameOver1, (int)SFX.GameOver3 + 1);
        soundManager.PlaySFX((SFX)random);

        // WeaponSlot에 있는 무기들의 부모를 null로 하고 싶은데 어떻게
        List<Transform> children = new();
        foreach(Transform child in weaponSlot.transform) {
            children.Add(child);
        }
        foreach(Transform child in effectSlot.transform) {
            children.Add(child);
        }
        foreach(Transform child in children) {
            child.SetParent(null);
        }
        weaponList.Clear();
    }

    public void SetHP(int hp)
    {
        maxHP = hp;
        this.hp = maxHP;
    }

    private void PlayerReset()
    {
        dontRotation = false;

        hp = maxHP;
        playerHP.SetHP(hp, maxHP);

        Stat stat = new Stat();
        stat.addtiveDamage = 0;
        stat.addtiveMoveSpeed = 0;
        stat.attackSpeedMultipier = 0;
        stat.hpMultipier = 1;
        stat.count = 0;

        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        var battleManager = Locator<BattleManager>.Get();
        battleManager.SetStat(stat);
    }

    public void RestartButton()
    {
        isDead = true;

        List<Transform> children = new();
        foreach (Transform child in weaponSlot.transform)
        {
            children.Add(child);
        }
        foreach (Transform child in effectSlot.transform)
        {
            children.Add(child);
        }
        foreach (Transform child in children)
        {
            child.SetParent(null);
        }
        weaponList.Clear();
    }
}
 
// Player에서 State Pattern까지는 필요 없어 보인다.
public enum WeaponType
{
    Knife,
    Rifle,
    Sniper,
}
