using System.Collections.Generic;
using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager Instance { get; private set; }

    [SerializeField] private SoundData[] soundDatas;

    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float seVolume = 1f;

    private readonly List<AudioSource> playingAudioSources = new List<AudioSource>();
    private readonly Dictionary<SoundData.SoundType, SoundData> lastPlayedSoundDatas =
        new Dictionary<SoundData.SoundType, SoundData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySE(SoundData.SoundType soundType)
    {
        SoundData[] matchingSoundDatas = GetMatchingSoundDatas(soundType);

        if (matchingSoundDatas.Length == 0)
        {
            Debug.LogWarning($"SoundDataが見つかりません: {soundType}");
            return;
        }

        SoundData soundData = SelectRandomSoundData(soundType, matchingSoundDatas);

        if (soundData.Audio == null)
        {
            Debug.LogWarning($"AudioClipが設定されていません: {soundType}");
            return;
        }

        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = soundData.Audio;
        audioSource.volume = masterVolume * seVolume * soundData.Volume;
        audioSource.Play();

        playingAudioSources.Add(audioSource);
        lastPlayedSoundDatas[soundType] = soundData;

        StartCoroutine(RemoveAudioSourceWhenFinished(audioSource));
    }

    public void ChangeMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdatePlayingAudioVolumes();
    }

    public void ChangeSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        UpdatePlayingAudioVolumes();
    }

    private SoundData[] GetMatchingSoundDatas(SoundData.SoundType soundType)
    {
        List<SoundData> matchingSoundDatas = new List<SoundData>();

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

    private SoundData SelectRandomSoundData(
        SoundData.SoundType soundType,
        SoundData[] matchingSoundDatas)
    {
        if (matchingSoundDatas.Length == 1)
        {
            return matchingSoundDatas[0];
        }

        SoundData lastPlayedSoundData = null;

        if (lastPlayedSoundDatas.TryGetValue(soundType, out SoundData previousSoundData))
        {
            lastPlayedSoundData = previousSoundData;
        }

        SoundData selectedSoundData;

        // 同じ音が連続して再生されないようにする
        do
        {
            int randomIndex = Random.Range(0, matchingSoundDatas.Length);
            selectedSoundData = matchingSoundDatas[randomIndex];
        }
        while (selectedSoundData == lastPlayedSoundData);

        return selectedSoundData;
    }

    private void UpdatePlayingAudioVolumes()
    {
        foreach (AudioSource audioSource in playingAudioSources)
        {
            if (audioSource == null)
            {
                continue;
            }

            SoundData soundData = FindSoundDataByAudioClip(audioSource.clip);

            if (soundData == null)
            {
                continue;
            }

            audioSource.volume = masterVolume * seVolume * soundData.Volume;
        }
    }

    private SoundData FindSoundDataByAudioClip(AudioClip audioClip)
    {
        foreach (SoundData soundData in soundDatas)
        {
            if (soundData == null)
            {
                continue;
            }

            if (soundData.Audio == audioClip)
            {
                return soundData;
            }
        }

        return null;
    }

    private System.Collections.IEnumerator RemoveAudioSourceWhenFinished(
        AudioSource audioSource)
    {
        yield return new WaitWhile(() => audioSource != null && audioSource.isPlaying);

        if (audioSource != null)
        {
            playingAudioSources.Remove(audioSource);
            Destroy(audioSource);
        }
    }
}