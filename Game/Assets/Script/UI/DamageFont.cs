using UnityEngine;
using DG.Tweening;
using TMPro;

public class DamageFont : MonoBehaviour, IEntity
{
    private TextMeshPro damageText;
    [SerializeField]
    private float floatDistance = 1f;
    [SerializeField]
    private float duration = 0.6f;

    [SerializeField]
    private Ease moveEase;
    [SerializeField]
    private Ease fadeEase;

    private int objectKey;

    private Vector3 originLocalScale;

    private void Awake()
    {
        damageText = this.gameObject.GetComponent<TextMeshPro>();
        var meshRenderer = damageText.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = 10;
        originLocalScale = this.transform.localScale;
    }


    public void OnDespawn()
    {
        this.transform.localScale = originLocalScale;
    }

    public void OnSpawn()
    {

    }

    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        if (parent != null)
            this.transform.SetParent(parent);
 
        this.transform.localPosition = position;
        this.transform.rotation = rotation;
        this.transform.localScale *= multiplier;
    }

    public void Setup(int damage, bool isCritical)
    {
        damageText.text = damage.ToString();

        if (isCritical)
            damageText.color = Color.red;
        else
            damageText.color = Color.white;

        Vector3 targetPosition = transform.position + Vector3.up * floatDistance;

        transform.DOMove(targetPosition, duration).SetEase(moveEase);
        damageText.DOFade(0f, duration).SetEase(fadeEase).OnComplete(() => {
            var factory = Locator<Factory>.Get();
            factory.Release(this);
        });
    }
}
