using UnityEngine;
using TMPro;
using DG.Tweening;

public class ReLoading : MonoBehaviour
{
    public GameObject reloadingObject;
    public TextMeshPro reloading;

    private Sequence reloadingSequence;
    private Tween dotsTween;

    private const string loadingBaseText = "Reloading";
    private const string loadingDots = "...";
    private const float typingTime = 0.2f;
    private const float dotInterval = 0.3f;

    public float reloadingTime = 2f;
    public float subReloadingTime = 0.05f;

    private void OnEnable()
    {
        ExecuteReloading();
    }

    private void OnDisable()
    {
        ResetValue();
    }

    private void ExecuteReloading()
    {
        ResetValue();
        reloadingObject.transform.localScale = new Vector3(0, 1, 1);

        reloadingSequence = DOTween.Sequence();

        var battleManager = Locator<BattleManager>.Get();
        var stat = battleManager.GetStat();

        float subReloading = subReloadingTime * stat.count;
        float subTime = reloadingTime - subReloading;

        if (subTime <= 0f)
            subTime = subReloadingTime;

        reloadingSequence.Join(
            reloadingObject.transform.DOScaleX(1f, subTime).SetEase(Ease.Linear)
        );

        reloadingSequence.Join(
            DOTween.To(() => 0, charCount => UpdateTypingText(charCount), loadingBaseText.Length, typingTime)
                .SetEase(Ease.Linear)
        );

        reloadingSequence.InsertCallback(typingTime, StartDotsLoop);

        reloadingSequence.OnComplete(() =>
        {
            dotsTween?.Kill();
            this.gameObject.SetActive(false);
            var eventManager = Locator<EventManager>.Get();
            eventManager.Notify(ChannelInfo.ReLoading);
        });
    }

    private void StartDotsLoop()
    {
        dotsTween = DOTween.To(() => 0, dotCount => UpdateDotsText(dotCount), 3, dotInterval * 3)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void ResetValue()
    {
        reloadingSequence?.Kill();
        dotsTween?.Kill();
        reloading.text = "";
    }

    private void UpdateTypingText(int charCount)
    {
        int count = Mathf.Clamp(charCount, 0, loadingBaseText.Length);
        reloading.text = loadingBaseText.Substring(0, count);
    }

    private void UpdateDotsText(int dotCount)
    {
        int count = Mathf.Clamp(dotCount, 0, 3);
        reloading.text = loadingBaseText + new string('.', count);
    }
}
