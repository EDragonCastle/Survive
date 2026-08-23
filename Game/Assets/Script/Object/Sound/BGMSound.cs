using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class BGMSound : MonoBehaviour
{
    private AudioSource audioSource;

    public async UniTask AudioSetup(BGM bgm, float volume)
    {
        if(audioSource == null)
            audioSource = this.GetComponent<AudioSource>();

        string bgmName = BGMToClipName(bgm);
        var resourceManager = Locator<ResourceManager>.Get();
        var audioClip = await resourceManager.Get<AudioClip>(bgmName);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Release()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    private string BGMToClipName(BGM bgm)
    {
        string clipName = "";
        switch (bgm)
        {
            case BGM.Main:
                clipName = "MainClip";
                break;
            case BGM.Title:
                clipName = "TitleClip";
                break;
        }
        return clipName;
    }
}
