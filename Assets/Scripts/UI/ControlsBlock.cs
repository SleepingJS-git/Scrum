using UnityEngine;

public class ControlsBlock : MonoBehaviour
{
    [SerializeField] private GameObject controlBlock;
    [SerializeField] private GamePhase visiblePhase;


    // Update is called once per frame
    void Update()
    {
       if(GameManager.Instance == null)
        {
            controlBlock.SetActive(true);
        }
        else
        {
            if(GameManager.GamePhase == visiblePhase)
            {
                controlBlock.SetActive(true);
            }
            else
            {
                controlBlock.SetActive(false);
            }
        }
    }
}
