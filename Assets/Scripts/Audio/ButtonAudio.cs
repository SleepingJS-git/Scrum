using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class ButtonAudio : MonoBehaviour, IPointerEnterHandler, IMeshModifier
{
    [Header("Idle Skew")]
    [SerializeField] private Vector2 skewAmountRange = new Vector2(0.025f, 0.05f);
    [SerializeField] private Vector2 skewCyclesPerSecondRange = new Vector2(0.15f, 0.30f);

    private Button button;
    private Image buttonImage;
    private float skewAmount;
    private float skewSpeed;
    private float skewPhase;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        skewAmount = Random.Range(skewAmountRange.x, skewAmountRange.y);
        skewSpeed = Random.Range(skewCyclesPerSecondRange.x, skewCyclesPerSecondRange.y);
        skewPhase = Random.value;
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlayClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayClick);

        if (buttonImage != null)
            buttonImage.SetVerticesDirty();
    }

    private void Update()
    {
        if (buttonImage == null || !buttonImage.isActiveAndEnabled || skewAmount == 0f)
            return;

        skewPhase = Mathf.Repeat(skewPhase + skewSpeed * Time.unscaledDeltaTime, 1f);
        buttonImage.SetVerticesDirty();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || !button.isActiveAndEnabled || !button.IsInteractable())
            return;

        AudioManager.Instance?.PlayButtonHover();
    }

    private void PlayClick()
    {
        AudioManager.Instance?.PlayButtonClick();
    }

    public void ModifyMesh(VertexHelper vertices)
    {
        if (!isActiveAndEnabled || buttonImage == null || skewAmount == 0f)
            return;

        
        float angle = skewPhase * Mathf.PI;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        Vector2 center = buttonImage.rectTransform.rect.center;

        UIVertex vertex = default;
        for (int i = 0; i < vertices.currentVertCount; i++)
        {
            vertices.PopulateUIVertex(ref vertex, i);

            float distance = Vector2.Dot((Vector2)vertex.position - center, perpendicular);
            Vector2 offset = direction * (distance * skewAmount);
            vertex.position.x += offset.x;
            vertex.position.y += offset.y;

            vertices.SetUIVertex(vertex, i);
        }
    }

    // Legacy overload required by IMeshModifier.
    void IMeshModifier.ModifyMesh(Mesh mesh) { }
}