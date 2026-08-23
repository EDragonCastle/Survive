using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class Knife : MonoBehaviour, IWeapon, IEntity
{
    public float coolTimeOrigin = 1.9f;
    public float coolTime = 1.9f;

    public float prepareDuration = 0.2f;
    public float attackDuration = 0.1f;
    public float detectRadius = 5f;
    public float slashDuration = 0.2f;
    public float fourAuraPrepareDuration = 0.05f;
    public float fourAuraDuration = 0.1f;

    public LayerMask enemyLayerMask;

    public float slashAngle = 30f;
    public int auraCount = 1;
    public float auraSpeed = 10f;
    public float effectScale = 1.0f;

    private List<KnifeAttackType> attackTypes = new List<KnifeAttackType>();

    private float currentCoolTime;
    private int objectKey;
    
    private GameObject effectSlot;
    private GameObject character;

    private Vector3 originScale = Vector3.one;

    private bool fourSword = false;

    private GameObject effectOrigin;

    private readonly float[] crossAngles = { 0f, 90f, 180f, 270f };
    private readonly float[] diagonalAngles = { 45f, 135f, 225f, 315f };

    private int index = 0;

    private int smashDamage = 6;
    private int upperDamage = 12;
    private int auraDamage = 10;
    private int fourAuraDamage = 10;

    private KnifeAttackType currentType;
    private bool isAttacking = false;

    public void Attack()
    {
        currentCoolTime = GetCoolTime();
        AttackLogic().Forget();
    }

    public bool CanAttack() => currentCoolTime <= 0f;

    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void OnDespawn()
    {
        this.transform.localScale = originScale;
        index = 0;
    }

    public void OnSpawn()
    {
        originScale = this.transform.localScale;
    }

    public void SetUp(Player player)
    {
        this.character = player.character;
        this.effectSlot = player.effectSlot;

        currentType = KnifeAttackType.Smash;
        attackTypes.Add(currentType);
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

    // 이건 동일할거다.
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

    private async UniTask AttackLogic()
    {
        if (attackTypes.Count == 0)
            return;

        var knifeAttackType = attackTypes[index];
        index++;
        if (index >= attackTypes.Count)
            index = 0;

        await ExecuteAttack(knifeAttackType);
        isAttacking = false;
    }

    private async UniTask ExecuteAttack(KnifeAttackType kinfeAttackType)
    {
        var soundManager = Locator<SoundManager>.Get();
        switch (kinfeAttackType)
        {
            case KnifeAttackType.Smash:
                soundManager.PlaySFX(SFX.KnifeSwing);
                await SmashAnimation();
                break;
            case KnifeAttackType.UpperCut:
                soundManager.PlaySFX(SFX.KnifeUpper);
                await UpperCutAnimation();
                break;
            case KnifeAttackType.SwoardAura:
        
                await SwoardAuraAnimation();
                break;
            case KnifeAttackType.FourAura:

                await FourAuraAnimation();
                break;
        }
    }

    private string EffectName(int number)
    {
        string resourceName = "";
        switch(number)
        {
            case 0:
                resourceName = "AttackCollider 1";
                break;
            case 1:
                resourceName = "AttackCollider 2";
                break;
            case 2:
                resourceName = "AttackCollider 3";
                break;
        }
        return resourceName;
    }

    #region Smash Animation
    private async UniTask SmashAnimation()
    {
        // 검을 원 위치로 보낸다.
        this.transform.localRotation = Quaternion.identity;

        // await 중에 미리 준비한다.
        var factory = Locator<Factory>.Get();
        var resourceManager = Locator<ResourceManager>.Get();
        int random = Random.Range(0, 3);
        var effectOrigin = await resourceManager.Get<GameObject>(EffectName(random));
        var effectComponent = effectOrigin.GetComponent<Effect>();

        await this.transform.DOLocalRotate(new Vector3(0, 0, 20f), prepareDuration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad).ToUniTask();

        // 잠시 대기
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));

        // 공격하는 Tween 실행
        var attackTween = this.transform.DOLocalRotate(new Vector3(0, 0, -230f), attackDuration, RotateMode.FastBeyond360)
        .SetEase(Ease.InQuad);

        // 중간까지 도달했다면
        await UniTask.Delay(System.TimeSpan.FromSeconds(attackDuration * 0.3f));

        // Effect가 실행된다.
        // Rotaion이 180 아니면 0인데 180은 좌측 0은 우측
        Vector3 weaponPosition = Vector3.zero;
        Quaternion effectRotation = Quaternion.Euler(0, 180, 140);

        if (character.transform.localRotation.eulerAngles != Vector3.zero) 
            weaponPosition.x = -0.4f;
        else 
            weaponPosition.x = 0.4f;
        
        effectSlot.transform.localPosition = weaponPosition;
        effectSlot.transform.localRotation = character.transform.localRotation;

        var effect = factory.Create<Effect>(effectComponent, Vector3.zero, effectRotation, parent: effectSlot.transform, ScaleMultiplier: effectScale);

        effect.SetAttackPoint(smashDamage);

        // 나머지 실행 스매쉬 끝
        await attackTween.ToUniTask();
    }
    #endregion

    #region Upper Animation
    private async UniTask UpperCutAnimation()
    {
        // 검을 내려찍기 위치로 보낸다.
        this.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, -130f));

        await UniTask.Delay(System.TimeSpan.FromSeconds(prepareDuration));

        // await 중에 미리 준비한다.
        var factory = Locator<Factory>.Get();
        var resourceManager = Locator<ResourceManager>.Get();
        int random = Random.Range(0, 3);
        var effectOrigin = await resourceManager.Get<GameObject>(EffectName(random));
        var effectComponent = effectOrigin.GetComponent<Effect>();

        // 공격하는 Tween 실행
        var attackTween = this.transform.DOLocalRotate(new Vector3(0, 180, -16), attackDuration, RotateMode.FastBeyond360)
        .SetEase(Ease.InQuad);

        // 중간까지 도달했다면
        await UniTask.Delay(System.TimeSpan.FromSeconds(attackDuration * 0.3f));

        Vector3 weaponPosition = Vector3.zero;
        Quaternion effectRotation = Quaternion.Euler(0, 0, -40);

        if (character.transform.localRotation.eulerAngles != Vector3.zero)
            weaponPosition.x = -0.4f;
        else
            weaponPosition.x = 0.4f;

        effectSlot.transform.localPosition = weaponPosition;
        effectSlot.transform.localRotation = character.transform.localRotation;

        // Effect가 실행된다.
        var effect = factory.Create<Effect>(effectComponent, Vector3.zero, effectRotation, parent: effectSlot.transform, ScaleMultiplier: effectScale);

        effect.SetAttackPoint(upperDamage);
        // 나머지 실행 올려치기 끝
        await attackTween.ToUniTask();
    }
    #endregion

    #region Aura Animation
    private async UniTask SwoardAuraAnimation()
    {
        var battleManager = Locator<BattleManager>.Get();
        var addtiveCount = battleManager.GetStat().count;
        var soundManager = Locator<SoundManager>.Get();

        for (int i = 0; i < auraCount + addtiveCount; i++)
        {
            slashAngle *= -1;
           
            int auraRandom = Random.Range((int)SFX.Aura1, (int)SFX.Aura3 + 1);
            soundManager.PlaySFX((SFX)auraRandom);
            var target = FindNearEnemy();
            float angle = SwordAuraAngle(target);

            float startAngle = angle - slashAngle;
            float endAngle = angle + slashAngle;

            if (character.transform.localRotation.eulerAngles != Vector3.zero) {
                startAngle *= -1;
                endAngle *= -1;
            }

            this.transform.localRotation = Quaternion.Euler(0, 0, startAngle);
            float delta = endAngle - startAngle;

            await this.transform.DOLocalRotate(new Vector3(0, 0, delta), slashDuration, RotateMode.FastBeyond360).SetRelative()
                .SetEase(Ease.OutQuad).ToUniTask();

            await InstanceSwordAura(angle);
        }
    }
    
    private Transform FindNearEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(character.transform.position, detectRadius, enemyLayerMask);

        Transform nearEnemy = null;
        float minDistance = float.MaxValue;

        foreach(var hit in hits)
        {
            float distance = ((Vector2)hit.transform.position - (Vector2)character.transform.position).sqrMagnitude;

            if (distance < minDistance)
            {
                minDistance = distance;
                nearEnemy = hit.transform;
            }
        }

        return nearEnemy;
    }

    private float SwordAuraAngle(Transform target)
    {
        if (target == null)
        {
            if (character.transform.localRotation.eulerAngles != Vector3.zero)
                return 90f;
            else
                return -90f;
        }

        Vector2 direction = target.position - character.transform.position;
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        return angle;
    }

    private async UniTask InstanceSwordAura(float angle)
    {
        var factory = Locator<Factory>.Get();
        
        if(effectOrigin == null)
        {
            var resourceManager = Locator<ResourceManager>.Get();
            effectOrigin = await resourceManager.Get<GameObject>("Aura Effect");
        }

        var effectComponent = effectOrigin.GetComponent<AuraEffect>();

        // Aura Effect Position Rotation
        var auraEffect = factory.Create<AuraEffect>(effectComponent, character.transform.position, Quaternion.Euler(0,0,angle), ScaleMultiplier: effectScale);

        auraEffect.SetAttackPoint(auraDamage);

        // 생성도 하고 발사도 해야하는데?
        auraEffect.ColorSetUp(Color.red);
        auraEffect.Launch(auraSpeed);
    }
    #endregion

    #region Four Aura Animation
    private async UniTask FourAuraAnimation()
    {
        var battleManager = Locator<BattleManager>.Get();
        var addtiveCount = battleManager.GetStat().count;
        var soundManager = Locator<SoundManager>.Get();

        for (int i = 0; i < (auraCount + addtiveCount); i++)
        {
            int aura = Random.Range((int)SFX.Aura1, (int)SFX.Aura3 + 1);
            soundManager.PlaySFX((SFX)aura);
            this.transform.localRotation = Quaternion.Euler(0, 0, -250);

            await UniTask.Delay(System.TimeSpan.FromSeconds(fourAuraPrepareDuration));

            float delta = -30f - (-250f);
            await this.transform.DOLocalRotate(new Vector3(0, 0, delta), fourAuraDuration, RotateMode.FastBeyond360).SetRelative()
            .SetEase(Ease.OutQuad).ToUniTask();

            await InstanceFourAura();
        }
    }

    private async UniTask InstanceFourAura()
    {
        if (effectOrigin == null)
        {
            var resourceManager = Locator<ResourceManager>.Get();
            effectOrigin = await resourceManager.Get<GameObject>("Aura Effect");
        }

        var tasks = new List<UniTask>();

        if(!fourSword) {
            foreach (var angle in crossAngles) {
                tasks.Add(FireSowrdAura(angle));
            }
        }
        else {
            foreach (var angle in diagonalAngles) {
                tasks.Add(FireSowrdAura(angle));
            }
        }

        await UniTask.WhenAll(tasks);
        fourSword = !fourSword;
    }

    private UniTask FireSowrdAura(float angle)
    {
        var factory = Locator<Factory>.Get();
        var effectComponent = effectOrigin.GetComponent<AuraEffect>();
        var auraEffect = factory.Create<AuraEffect>(effectComponent, character.transform.position, Quaternion.Euler(0, 0, angle), ScaleMultiplier: effectScale);

        auraEffect.ColorSetUp(Color.red);
        auraEffect.Launch(auraSpeed);
        auraEffect.SetAttackPoint(fourAuraDamage);

        return UniTask.CompletedTask;
    }

    public void UpGrade()
    {
        switch (currentType)
        {
            case KnifeAttackType.Smash:
                currentType = KnifeAttackType.UpperCut;
                attackTypes.Add(currentType);
                break;
            case KnifeAttackType.UpperCut:
                currentType = KnifeAttackType.SwoardAura;
                attackTypes.Add(currentType);
                break;
            case KnifeAttackType.SwoardAura:
                currentType = KnifeAttackType.FourAura;
                attackTypes.Add(currentType);
                break;
        }
    }
    #endregion

    private float GetCoolTime()
    {
        var battleManager = Locator<BattleManager>.Get();
        var stat = battleManager.GetStat();
        float attackSpeed = Mathf.Abs(stat.attackSpeedMultipier);

        float subTime = coolTimeOrigin * attackSpeed;
        float speed = coolTimeOrigin - subTime;
        if (speed <= 0)
            speed = 0.1f;

        return speed;
    }
}

public enum KnifeAttackType
{
    Smash,      // 스매시 완료
    UpperCut,   // 올려치기 완료
    SwoardAura, // 검기
    FourAura,   // 4방향 검기
}

