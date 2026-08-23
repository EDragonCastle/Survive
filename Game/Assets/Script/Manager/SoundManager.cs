using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// Sound Object를 관리하고 있는 Manager
/// </summary>
public class SoundManager 
{
    private GameObject bgmObject;
    private Dictionary<SFX, List<SFXSound>> sfxSounds;

    private float sfxSound = 1.0f;
    private float bgmSound = 1.0f;

    public void SetSFXSound(float sfx) => sfxSound = sfx;
    public void SetBGMSound(float bgm) => bgmSound = bgm;

    public SoundManager()
    {
        sfxSounds = new Dictionary<SFX, List<SFXSound>>();
    }

    /// <summary>
    /// BGM Sound를 실행한다.
    /// </summary>
    /// <param name="bgm">Enum으로 관리되고 있는 bgm type</param>
    public void PlayBGM(BGM bgm)
    {
        // bgmObject null이면 resourceManager에서 받아오고 Setting을 한다.
        if (bgmObject == null)
            LoadResource(bgm).Forget();
        else
            PlayBGMSound(bgm).Forget();
    }

    /// <summary>
    /// BGM Sound를 제거한다.
    /// </summary>
    public void DestoryBGM()
    {
        ReleaseBGM();
    }

    /// <summary>
    /// SFX Sound를 실행한다.
    /// </summary>
    /// <param name="sfx">효과음</param>
    public void PlaySFX(SFX sfx, float pitch = 1)
    {
        // 여기는 무조건 resourceManager에서 받아오고 Setting을 해야 한다.
        LoadResource(sfx, sfxSound, pitch).Forget();
    }


    public void AllStopSFX(SFX sfx)
    {
        if (sfxSounds.ContainsKey(sfx))
        {
            var soundList = sfxSounds[sfx];
            foreach(var sound in soundList) {
                sound.Release();
            }
            sfxSounds[sfx].Clear();
        }
    }
    
    public void StopSFX(SFX sfx, SFXSound sfxSound)
    {
        if(sfxSounds.ContainsKey(sfx))
        {
            var soundList = sfxSounds[sfx];

            foreach(var sound in soundList) {
                if(sound == sfxSound) {
                    sfxSound.Release();
                    break;
                }    
            }
            soundList.Remove(sfxSound);
        }
    }

    private async UniTask LoadResource(BGM bgm)
    {
        var resourceManager = Locator<ResourceManager>.Get();
        var _bgmObject = await resourceManager.Get<GameObject>("BGM");
        bgmObject = GameObject.Instantiate(_bgmObject);
        await PlayBGMSound(bgm);
    }

    private async UniTask LoadResource(SFX sfx, float volume = 1, float pitch = 1)
    {
        var resourceManager = Locator<ResourceManager>.Get();
        var sfxObjectTask = resourceManager.Get<GameObject>("SFX");
        var factory = Locator<Factory>.Get();

        var sfxObject = await sfxObjectTask;
        var sfxComponent = sfxObject.GetComponent<SFXSound>();
        var newSFX = factory.Create<SFXSound>(sfxComponent, Vector3.zero, Quaternion.identity);

        if(!sfxSounds.ContainsKey(sfx)) {
            sfxSounds.Add(sfx, new List<SFXSound>());
        }

        sfxSounds[sfx].Add(newSFX);

        await newSFX.AudioSetUp(sfx, volume, pitch);
    }

    private async UniTask PlayBGMSound(BGM bgm)
    {
        var bgmComponent = bgmObject.GetComponent<BGMSound>();
        await bgmComponent.AudioSetup(bgm, bgmSound);
    }

    private void ReleaseBGM()
    {
        var bgmComponent = bgmObject.GetComponent<BGMSound>();
        bgmComponent.Release();
    }
}
