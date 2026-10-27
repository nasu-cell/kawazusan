using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class InGameController : MonoBehaviour
{
    float elapsedTime = 0f;
    public float ElapsedTime => elapsedTime;
    [SerializeField] private GameHUDController gameHUDController;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private FrogController frogController;
    [SerializeField] private SatietyController satietyController;
    [SerializeField] private ClearPanel clearPanel;
    bool isGameOver = false;
    bool isTimerRunning = true;

    void Start()
    {
        // Initialize game state
        elapsedTime = 0f;
        gameHUDController.InitHUD(gameConfig.SatietyMax);
        satietyController.Filled += OnSatietyFilled;

    }

    public void Update()
    {
        if (isGameOver || !isTimerRunning) return;
        elapsedTime += Time.deltaTime;
        gameHUDController.UpdateTimer(elapsedTime);
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void OnSatietyFilled()
    {
        if (isGameOver)
        {
            return;
        }

        // Handle satiety filled event
        isGameOver = true;
        isTimerRunning = false;
        frogController.SetTongueInputEnabled(false);
        if (SEManager.Instance != null)
        {
            SEManager.Instance.PlaySE(SoundData.SoundType.GameClear);
        }

        clearPanel.ShowAsync(elapsedTime).Forget();
        gameHUDController.FilledSatiety();
    }
    

}
