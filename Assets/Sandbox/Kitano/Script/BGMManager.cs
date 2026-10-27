using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [SerializeField] private SoundData[] soundDatas;

    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    [SerializeField] private float fadeinTime = 1f;
    [SerializeField] private float fadeoutTime = 1f;

    private AudioSource audioSource;
    private Coroutine changeBgmCoroutine;
    private SoundData currentSoundData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if(soundDatas.Length > 0)
            PlayBGM(soundDatas[0].Type);
    }

    public void PlayBGM(SoundData.SoundType soundType)
    {
        SoundData[] matchingSoundDatas = GetMatchingSoundDatas(soundType);

        if (matchingSoundDatas.Length == 0)
        {
            Debug.LogWarning($"SoundDataが見つかりません: {soundType}");
            return;
        }

        SoundData soundData =
            matchingSoundDatas[Random.Range(0, matchingSoundDatas.Length)];

        if (soundData.Audio == null)
        {
            Debug.LogWarning($"AudioClipが設定されていません: {soundType}");
            return;
        }

        if (changeBgmCoroutine != null)
        {
            StopCoroutine(changeBgmCoroutine);
        }

        Debug.Log(soundType);

        changeBgmCoroutine = StartCoroutine(ChangeBGM(soundData));
    }

    public void ChangeMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAudioVolume();
    }

    public void ChangeBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateAudioVolume();
    }

    private SoundData[] GetMatchingSoundDatas(SoundData.SoundType soundType)
    {
        System.Collections.Generic.List<SoundData> matchingSoundDatas =
            new System.Collections.Generic.List<SoundData>();

        foreach (SoundData soundData in soundDatas)
        {
            if (soundData == null)
            {
                continue;
            }

            if (soundData.IsCorrectSound(soundType))
            {
                matchingSoundDatas.Add(soundData);
            }
        }

        return matchingSoundDatas.ToArray();
    }

    private IEnumerator ChangeBGM(SoundData nextSoundData)
    {
        if (audioSource.isPlaying)
        {
            yield return FadeOut();
        }

        currentSoundData = nextSoundData;
        audioSource.clip = currentSoundData.Audio;
        audioSource.volume = 0f;
        audioSource.Play();

        yield return FadeIn();
    }

    private IEnumerator FadeOut()
    {
        if (fadeoutTime <= 0f)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
            yield break;
        }

        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeoutTime)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / fadeoutTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);

            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    private IEnumerator FadeIn()
    {
        if (fadeinTime <= 0f)
        {
            audioSource.volume = GetCurrentVolume();
            yield break;
        }

        float targetVolume = GetCurrentVolume();
        float elapsedTime = 0f;

        while (elapsedTime < fadeinTime)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / fadeinTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, progress);

            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private void UpdateAudioVolume()
    {
        if (!audioSource.isPlaying || currentSoundData == null)
        {
            return;
        }

        audioSource.volume = GetCurrentVolume();
    }

    private float GetCurrentVolume()
    {
        if (currentSoundData == null)
        {
            return 0f;
        }

        return masterVolume * bgmVolume * currentSoundData.Volume;
    }
}