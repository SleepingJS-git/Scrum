using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{
    public TextMeshProUGUI interactText;
    public TextMeshProUGUI ammo;
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI health;
    public GameObject youAreDeadObj;
    
    [Header("Crosshair")]
    public RectTransform crosshair;
    public RectTransform topRect;
    public RectTransform bottomRect;
    public RectTransform leftRect;
    public RectTransform rightRect;
    public float maxCrosshairSpread = 50f;
    public float spreadLerpSpeed = 12f;

    [Header("Hit Marker")]
    public Image hitmarker;
    public Image killmarker;

    private float _cursorSize;
    private float targetSpread;

    private Vector2 topStartPos;
    private Vector2 bottomStartPos;
    private Vector2 leftStartPos;
    private Vector2 rightStartPos;
    private Coroutine hitmarkerRoutine;
    private Coroutine deathMarkerRoutine;

    private void Awake()
    {
        topStartPos = topRect.anchoredPosition;
        bottomStartPos = bottomRect.anchoredPosition;
        leftStartPos = leftRect.anchoredPosition;
        rightStartPos = rightRect.anchoredPosition;
    }

    private void Update()
    {
        topRect.anchoredPosition = Vector2.Lerp(
            topRect.anchoredPosition,
            topStartPos + Vector2.up * targetSpread,
            spreadLerpSpeed * Time.deltaTime
        );

        bottomRect.anchoredPosition = Vector2.Lerp(
            bottomRect.anchoredPosition,
            bottomStartPos + Vector2.down * targetSpread,
            spreadLerpSpeed * Time.deltaTime
        );

        leftRect.anchoredPosition = Vector2.Lerp(
            leftRect.anchoredPosition,
            leftStartPos + Vector2.left * targetSpread,
            spreadLerpSpeed * Time.deltaTime
        );

        rightRect.anchoredPosition = Vector2.Lerp(
            rightRect.anchoredPosition,
            rightStartPos + Vector2.right * targetSpread,
            spreadLerpSpeed * Time.deltaTime
        );
    }
    public void SetCursorSize(float size)
    {
        _cursorSize = size;
    }
    public void CrosshairSpread(float spread)
    {
        float t = Mathf.InverseLerp(0f, .05f, spread);
        // Base cursor size + how much spread increases it
        targetSpread = Mathf.Lerp(0f, maxCrosshairSpread, t);
    }

    public void Hitmarker()
    {
        if (hitmarkerRoutine != null) StopCoroutine(hitmarkerRoutine);
        hitmarkerRoutine = StartCoroutine(MarkerRoutine(hitmarker, .35f));
    }

    public void Deathmarker()
    {
        if (deathMarkerRoutine != null) StopCoroutine(deathMarkerRoutine);
        deathMarkerRoutine = StartCoroutine(MarkerRoutine(killmarker, .5f));
    }


    private IEnumerator MarkerRoutine(Image img, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            float a = Mathf.Lerp(1f, 0f, t);

            img.color = new Color(1f, 1f, 1f, a);
            t += Time.deltaTime / duration;

            yield return null;
        }

        img.color = new Color(1f, 1f, 1f, 0f);
    }
}
