using UnityEngine;

public class OptionController : MonoBehaviour
{
    public void ChangeMasterVolume(float volume)
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.ChangeMasterVolume(volume);
        }

        if (SEManager.Instance != null)
        {
            SEManager.Instance.ChangeMasterVolume(volume);
        }
    }

    public void ChangeBGMVolume(float volume)
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.ChangeBGMVolume(volume);
        }
    }

    public void ChangeSEVolume(float volume)
    {
        if (SEManager.Instance != null)
        {
            SEManager.Instance.ChangeSEVolume(volume);
        }
    }
}