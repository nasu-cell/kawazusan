using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Sound/SoundData")]
public class SoundData : ScriptableObject
{
    public enum SoundType
    {
        // 必要な音の種類をここに追加する
        ButtonClick,
        TapToStart,
        FlogMouthOpen,
        FlogSwallowing,
        FlyTrapping,
        Damaged,
        GameClear,
        IngameBGM,
        ResultBGM,
        TitleBGM,

    }

    [SerializeField] private SoundType soundType;
    [SerializeField] private float volume = 1f;
    [SerializeField] private AudioClip audio;

    public SoundType Type => soundType;
    public float Volume => volume;
    public AudioClip Audio => audio;

    public bool IsCorrectSound(SoundType soundType)
    {
        return this.soundType == soundType;
    }
}