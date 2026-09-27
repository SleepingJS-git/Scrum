using TMPro;
using UnityEngine;

public class ControlsBlock : MonoBehaviour
{
    [SerializeField] private CanvasGroup controlBlock;
    [SerializeField] private GamePhase visiblePhase;


    // Update is called once per frame
    void Update()
    {
       if(GameManager.Instance == null)
        {
            controlBlock.alpha = 1;
        }
        else
        {
            if(GameManager.GamePhase == visiblePhase)
            {
                controlBlock.alpha = 1;
            }
            else
            {
                controlBlock.alpha = 0;
            }
        }
    }
}
