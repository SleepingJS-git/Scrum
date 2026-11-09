using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CeilingDetection : MonoBehaviour
{
    [SerializeField] private CharacterController cc;
    [SerializeField] private Player player;
    [SerializeField] private MultiAimConstraint ceilingConstraint;

    // Update is called once per frame
    void Update()
    {
        if (player.MoveState.Value == 1 && player.Move.isStuckOnCeiling)
        {
            Vector3 origin = player.transform.position + Vector3.down * (cc.height * 0.35f);

            bool ceiling = 
            Physics.Raycast(
                origin,
                Vector3.up,
                cc.height + 0.25f,
                Layer.Wall | Layer.Ground,
                QueryTriggerInteraction.Ignore
            );

            ceilingConstraint.weight = ceiling ? .85f : 0f;
        }
        

    }
}
