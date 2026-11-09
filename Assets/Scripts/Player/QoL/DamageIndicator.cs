using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private Image indicatorImg;
    [SerializeField] private Color color;
    [SerializeField] private float delayedTime, fadeTime, maxAlpha;
    private float dt;
    private bool initialized = false, delayed = true;
    private Vector3 direction;
    private Transform playerTransform;
    private RectTransform rect;
    public void Init(Vector3 s, Transform p)
    {
        s.y = 0f;
        playerTransform = p;

        direction = s - p.position;
        direction.Normalize();

        rect = transform as RectTransform;
        initialized = true;
        delayed = true;
        dt = 0f;
    }

    void Update()
    {
        if (!initialized) return;

        Vector3 localDirection = playerTransform.InverseTransformDirection(direction);

        float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;

        rect.localRotation = Quaternion.Euler(0f, 0f, -angle);

        dt += Time.deltaTime;

        if (delayed)
        {
            if (dt >= delayedTime)
            {
                delayed = false;
                dt = 0f;
            }
            else return;
        }

        if (dt >= fadeTime)
        {
            Destroy(gameObject);
            return;
        }

        float t = dt / fadeTime;
        float a = Mathf.Lerp(maxAlpha, 0f, t);

        Color newColor = new Color(color.r, color.b, color.g, a);

        indicatorImg.color = newColor;
    }
}
