using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class SFXSound : MonoBehaviour, IEntity
{
    private int objectKey;
    private AudioSource audioSource;
    private SFX currentSFX;

    public int GetObjectKey() => objectKey;
    public void SetObjectKey(int _key) => objectKey = _key;

    public void OnDespawn()
    {
        // 여기서 뭔가 해야하나? 초기화해줘야 하지 않나?
        audioSource.clip = null;
        audioSource.Stop();
    }

    public void OnSpawn()
    {

    }

    public void SetTransform(Vector3 position, Quaternion rotation, float multiplier = 1, Transform parent = null)
    {
        this.transform.position = position;
        this.transform.rotation = rotation;

        if (parent != null)
            this.transform.SetParent(parent);
    }

    // 외부에서 실행하는 거로 하자.
    public async UniTask AudioSetUp(SFX sfx, float volume, float pitch)
    {
        if (audioSource == null)
            audioSource = this.GetComponent<AudioSource>();

        currentSFX = sfx;
        string clipName = SFXToClipName(sfx);
        var resourceManager = Locator<ResourceManager>.Get();
        var audioClip = await resourceManager.Get<AudioClip>(clipName);

        audioSource.clip = audioClip;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        
        if(audioSource.clip != null)
            audioSource.Play();

        await AutoRelease(audioSource.clip.length);
    }

    public void Release()
    {
        var factory = Locator<Factory>.Get();
        factory.Release(this);
    }

    private async UniTask AutoRelease(float duration)
    {
        // sfx 시간이 끝날 때까지 대기한다.
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));

        var soundManager = Locator<SoundManager>.Get();
        soundManager.StopSFX(currentSFX, this);

    }

    private string SFXToClipName(SFX sfx)
    {
        string name = "";

        switch (sfx)
        {
            case SFX.PistolShot:
                name = "PistolShotClip";
                break;
            case SFX.PistolReloading:
                name = "PistolReloadingClip";
                break;
            case SFX.RevolverShot:
                name = "RevolverShotClip";
                break;
            case SFX.RevolverReloading:
                name = "RevolverReLoadingClip";
                break;
            case SFX.ShotgunShot:
                name = "ShotgunShotClip";
                break;
            case SFX.ShotgunReloading:
                name = "ShotgunReloadClip";
                break;
            case SFX.SniperShot1:
                name = "Sniper1Clip";
                break;
            case SFX.SniperShot2:
                name = "Sniper2Clip";
                break;
            case SFX.SniperShot3:
                name = "Sniper3Clip";
                break;
            case SFX.SniperShot4:
                name = "Sniper4Clip";
                break;
            case SFX.SniperReloading:
                name = "SniperReloadingClip";
                break;
            case SFX.UziReloading:
                name = "UziReloadingClip";
                break;
            case SFX.UziShot1:
                name = "UziShot1Clip";
                break;
            case SFX.UziShot2:
                name = "UziShot2Clip";
                break;
            case SFX.UziShot3:
                name = "UziShot3Clip";
                break;
            case SFX.UziShot4:
                name = "UziShot4Clip";
                break;
            case SFX.RifleShot:
                name = "RifleShot";
                break;
            case SFX.RifleReloading:
                name = "RifleReloading";
                break;
            case SFX.KnifeSwing:
                name = "SwingClip";
                break;
            case SFX.KnifeUpper:
                name = "UpperClip";
                break;
            case SFX.Aura1:
                name = "Aura1Clip";
                break;
            case SFX.Aura2:
                name = "Aura2Clip";
                break;
            case SFX.Aura3:
                name = "Aura3Clip";
                break;
            case SFX.GameOver1:
                name = "LoseClip1";
                break;
            case SFX.GameOver2:
                name = "LoseClip2";
                break;
            case SFX.GameOver3:
                name = "LoseClip3";
                break;
        }
        return name;
    }
}
