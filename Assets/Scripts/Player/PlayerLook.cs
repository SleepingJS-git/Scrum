using Unity.Netcode;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    public Transform camHolder;
    public Transform rigAim;      // For the rig
    [Tooltip("X is for left and Right, Y is for Up and Down")]
    public Vector2 sensitivity;
    [Header("Crouching")]
    public float standCamLevel;
    public float crouchCamLevel;
    public float XRot { get; private set; }
    private Player main;
    public void Init(bool camEnabled)
    {
        main = GetComponent<Player>();
        XRot = 0f;

        cam.gameObject.SetActive(camEnabled);
    }

    /// <summary>
    /// Makes the player look by rotating the camera and the player transform.
    /// </summary>
    /// <param name="input"></param>
    public void Look(Vector2 input)
    {
        // Camera rotation for looking up and down
        XRot -= input.y * sensitivity.y;
        XRot = Mathf.Clamp(XRot, -80f, 80f);    // Prevents looking behind by moving up and down

        // Apply up/down rotation to camera transform
        cam.transform.localRotation = Quaternion.Euler(XRot, 0f, 0f);

        // Rotate Player Transform for looking left and right
        transform.Rotate(0f, input.x * sensitivity.x, 0f);
    }


    public void RotateRigAim(float xRotation)
    {
        rigAim.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
