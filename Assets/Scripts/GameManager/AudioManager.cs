using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float defaultBgmVolume = 0.5f;
    [SerializeField] private float defaultSfxVolume = 1f;
    [SerializeField] private float defaultFadeDuration = 1f;

    public void PlayBgm(AudioClip clip, float volume = -99)
    {
        bgmSource.DOKill();
        bgmSource.volume = volume <= 0 ? defaultBgmVolume : volume;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void StopSfx()
    {
        sfxSource.Stop();
    }

    public void FadeOutBgm(float duration = -99)
    {
        bgmSource.DOKill();
        bgmSource.DOFade(0f, duration <= 0 ? defaultFadeDuration : duration);
    }

    public void FadeInBtm(AudioClip clip, float duration = -99)
    {
        bgmSource.DOKill();
        bgmSource.DOFade(1f, duration <= 0 ? defaultFadeDuration : duration);
    }
}
