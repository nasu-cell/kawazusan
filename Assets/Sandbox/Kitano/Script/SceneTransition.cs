using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public enum SceneType
    {
        InGame,
        Title
    }

    [SerializeField] private Animator transitionAnim;
    [SerializeField] private string animatedTransitionState = "trans";
    [SerializeField, Min(0f)] private float animatedTransitionDuration = 1.5f;
    [SerializeField] private string entranceTransitionState = "None";
    [SerializeField, Min(0f)] private float entranceTransitionDuration = 0.5f;
    [SerializeField] private string inGameSceneName = "InGame";
    [SerializeField] private string titleSceneName = "Title";

    private bool isTransitioning;
    private bool shouldPlayEntranceOnStart;
    private static bool playEntranceOnNextScene;

    private void Awake()
    {
        shouldPlayEntranceOnStart = playEntranceOnNextScene;
        playEntranceOnNextScene = false;

        if (shouldPlayEntranceOnStart && transitionAnim != null)
        {
            ShowTransitionGraphics();
            transitionAnim.enabled = true;
            return;
        }

        HideTransitionGraphics();
    }

    private void Start()
    {
        if (!shouldPlayEntranceOnStart || transitionAnim == null)
        {
            return;
        }

        // Animator state initialization is complete by Start. Rebind first so
        // the entrance clip always begins from its first frame after a scene load.
        transitionAnim.Rebind();
        transitionAnim.Update(0f);
        transitionAnim.Play(entranceTransitionState, 0, 0f);
        transitionAnim.Update(0f);
        StartCoroutine(FinishEntranceAnimation());
    }

    private void HideTransitionGraphics()
    {
        if (transitionAnim != null)
        {
            transitionAnim.enabled = false;
        }

        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true))
        {
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.raycastTarget = false;
        }
    }

    private void ShowTransitionGraphics()
    {
        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true))
        {
            Color color = graphic.color;
            color.a = 1f;
            graphic.color = color;
            graphic.raycastTarget = true;
        }
    }

    private IEnumerator FinishEntranceAnimation()
    {
        yield return new WaitForSecondsRealtime(entranceTransitionDuration);
        HideTransitionGraphics();
    }

    public void SceneTrans(SceneType sceneType)
    {
        if (isTransitioning)
        {
            return;
        }

        string sceneName = GetSceneName(sceneType);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"SceneTypeに対応するシーン名が設定されていません: {sceneType}");
            return;
        }

        isTransitioning = true;
        playEntranceOnNextScene = false;
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void SceneTransWithAnimator(SceneType sceneType)
    {
        if (isTransitioning)
        {
            return;
        }

        string sceneName = GetSceneName(sceneType);
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        isTransitioning = true;
        playEntranceOnNextScene = true;

        if (transitionAnim == null)
        {
            SceneManager.LoadSceneAsync(sceneName);
            return;
        }

        ShowTransitionGraphics();
        transitionAnim.enabled = true;
        transitionAnim.Play(animatedTransitionState, 0, 0f);
        transitionAnim.Update(0f);
        StartCoroutine(LoadSceneAfterAnimation(sceneName));
    }

    private IEnumerator LoadSceneAfterAnimation(string sceneName)
    {
        yield return new WaitForSecondsRealtime(animatedTransitionDuration);
        SceneManager.LoadSceneAsync(sceneName);
    }

    private string GetSceneName(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.InGame:
                {
                    BGMManager.Instance.PlayBGM(SoundData.SoundType.IngameBGM);
                    return inGameSceneName;
                }

            case SceneType.Title:
                {
                    BGMManager.Instance.PlayBGM(SoundData.SoundType.TitleBGM);
                    return titleSceneName;
                }

            default:
                return string.Empty;
        }
    }

}
