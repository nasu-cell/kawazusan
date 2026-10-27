using UnityEngine;
using UnityEngine.EventSystems;

public class FrogController : MonoBehaviour
{
    [SerializeField] private TongueController tongueController;
    [SerializeField] private FrogAppearance frogAppearance;


    void Start()
    {

    }

    void Update()
    {
        Vector2 screenPosition = Input.mousePosition;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            frogAppearance.RotatePupilsTowards(worldPosition);
            tongueController.ExtendTongue(worldPosition);
        }

    }

    public void SetTongueInputEnabled(bool isEnabled)
    {
        tongueController.SetInputEnabled(isEnabled);
    }
}
