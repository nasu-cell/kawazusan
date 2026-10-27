using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupController : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    void Start()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
