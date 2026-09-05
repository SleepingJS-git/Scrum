using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    [Tooltip("X is for left and Right, Y is for Up and Down")]
    public Vector2 sensitivity;

    private float xRot;
    public void Init()
    {
        xRot = 0f;
    }

    /// <summary>
    /// Makes the player look by rotating the camera and the player transform.
    /// </summary>
    /// <param name="input"></param>
    public void Look(Vector2 input)
    {
        // Camera rotation for looking up and down
        xRot -= input.y * sensitivity.y;
        xRot = Mathf.Clamp(xRot, -80f, 80f);    // Prevents looking behind by moving up and down

        Debug.Log($"X: {input.x * sensitivity.x} | Y: {input.y * sensitivity.y}");

        // Apply up/down rotation to camera transform
        cam.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        // Rotate Player Transform for looking left and right
        transform.Rotate(0f, input.x * sensitivity.x, 0f);
    }
}
