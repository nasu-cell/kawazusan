using UnityEngine;

public class TitleController : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject creditMenu;
    [SerializeField] private TitleFrogTransitionController titleFrogTransition;

    public void TapToStart()
    {
        if (titleFrogTransition != null)
        {
            if (!titleFrogTransition.PlayStartTransition())
            {
                return;
            }
        }
        else
        {
            sceneTransition.SceneTrans(SceneTransition.SceneType.InGame);
        }

        SEManager.Instance.PlaySE(SoundData.SoundType.TapToStart);
    }

    public void ShowOption()
    {
        if(optionMenu != null)
            optionMenu.SetActive(true);
        if(creditMenu != null)
            creditMenu.SetActive(false);
        SEManager.Instance.PlaySE(SoundData.SoundType.ButtonClick);
    }

    public void ShowCredit()
    {
        if(optionMenu != null)
            optionMenu.SetActive(false);
        if(creditMenu != null)
            creditMenu.SetActive(true);
        SEManager.Instance.PlaySE(SoundData.SoundType.ButtonClick);
    }

    public void CloseMenu()
    {
        if(optionMenu != null)
            optionMenu.SetActive(false);
        if(creditMenu != null)
            creditMenu.SetActive(false);
        SEManager.Instance.PlaySE(SoundData.SoundType.ButtonClick);
        SEManager.Instance.PlaySE(SoundData.SoundType.TapToStart);
    }

    public void ReturnTitle()
    {
        sceneTransition.SceneTransWithAnimator(SceneTransition.SceneType.Title);
        SEManager.Instance.PlaySE(SoundData.SoundType.TapToStart);
    }

    public void RestartGame()
    {
        sceneTransition.SceneTransWithAnimator(SceneTransition.SceneType.InGame);
        SEManager.Instance.PlaySE(SoundData.SoundType.TapToStart);
    }
}
