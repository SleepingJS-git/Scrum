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
    public CrosshairRect topRect;
    public CrosshairRect bottomRect;
    public CrosshairRect leftRect;
    public CrosshairRect rightRect;
    public float maxCrosshairSpread = 50f;
    public float spreadLerpSpeed = 12f;
    public CrosshairColor CurrentCrosshairColor { get; private set; }

    public Color crosshairRedColor;
    public Color crosshairWhiteColor;

    [Header("Hit Marker")]
    public Image hitmarker;
    public Image killmarker;

    [Header("Damage Indicator")]
    public Transform indicatorFolder;
    public DamageIndicator indicatorPrefab;

    [Header("Bleeding Hud")]
    public Image bleedingHud;
    public float hitFadeTimeIn, hitFadeTimeOut;
    public float beatingRate;   // When low

    [Header("Bleeding Hud Alpha Vals")]
    public float hitAlphaMin = 65f;
    public float hitAlphaMax = 128f, additiveAlpha = 25f, lowAlphaMin = 128f, lowAlphaMax = 168f;

    [Header("Health Bar")]
    public Image healthBar;
    public Color fullHealthColor, medHealthColor, lowHealthColor;

    private float _cursorSize;
    private float _targetSpread;
    private float currentAlpha;
    private Coroutine hitmarkerRoutine;
    private Coroutine deathMarkerRoutine;
    private Coroutine hitFeedbackRoutine;
    private List<CrosshairRect> crosshairRects;

    private void Awake()
    {
        crosshairRects = new List<CrosshairRect>()
        {
            topRect, bottomRect, leftRect, rightRect
        };
        topRect.Init(Vector2.up);
        bottomRect.Init(Vector2.down);
        leftRect.Init(Vector2.left);
        rightRect.Init(Vector2.right);

        currentAlpha = 0f;
    }

    private void Update()
    {
        foreach (CrosshairRect crosshairRect in crosshairRects)
            crosshairRect.Update(_targetSpread, spreadLerpSpeed);
    }
    public void SetCursorSize(float size)
    {
        _cursorSize = size;
    }
    public void CrosshairSpread(float spread)
    {
        float t = Mathf.InverseLerp(0f, .05f, spread);
        // Base cursor size + how much spread increases it
        _targetSpread = Mathf.Lerp(0f, maxCrosshairSpread, t);
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

    /// <summary>
    /// Set the cross hair color
    /// </summary>
    /// <param name="colorInt">0 = white, 1 = red</param>
    public void SetCrosshairColor(CrosshairColor desiredColor)
    {
        Color color = (int) desiredColor == 0 ? crosshairWhiteColor : crosshairRedColor;
        CurrentCrosshairColor = desiredColor;
        foreach (CrosshairRect crosshairRect in crosshairRects)
            crosshairRect.image.color = color;
    }


    public void CreateIndicator(Vector3 s, Transform p)
    {
        DamageIndicator i = Instantiate(indicatorPrefab, indicatorFolder);
        i.Init(s, p);
    }

    public void HitFeedback()
    {
        if (hitFeedbackRoutine != null) 
            StopCoroutine(hitFeedbackRoutine);

        float nextAlpha = currentAlpha <= 0f ? hitAlphaMin :
            Mathf.Min(currentAlpha * 255f + additiveAlpha, hitAlphaMax);

        hitFeedbackRoutine = StartCoroutine(HitFeedbackRoutine(nextAlpha));
    }

    private IEnumerator HitFeedbackRoutine(float targetAlpha)
    {
        float startAlpha = bleedingHud.color.a;
        float t = 0f;
        Color color = bleedingHud.color;
        while (t < 1f)
        {
            t += Time.deltaTime / hitFadeTimeIn;

            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha / 255f, t);
            color = bleedingHud.color;
            color.a = currentAlpha;
            bleedingHud.color = color;
            yield return null;
        }

        currentAlpha = hitAlphaMax / 255f;

        color = bleedingHud.color;
        color.a = currentAlpha;
        bleedingHud.color = color;

        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / hitFadeTimeOut;

            currentAlpha = Mathf.Lerp(targetAlpha  / 255f, 0f, t);
            
            color = bleedingHud.color;
            color.a = currentAlpha;
            bleedingHud.color = color;

            yield return null;
        }

        currentAlpha = 0f;

        color = bleedingHud.color;
        color.a = currentAlpha;
        bleedingHud.color = color;
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        health.SetText(currentHealth.ToString());

        float perc = (float) currentHealth / maxHealth;

        healthBar.fillAmount = perc;

        if (currentHealth > 75) healthBar.color = fullHealthColor;
        else if (currentHealth > 25) healthBar.color = medHealthColor;
        else healthBar.color = lowHealthColor;
    }
}

public enum CrosshairColor
{
    White = 0,
    Red = 1
}

[System.Serializable]
public class CrosshairRect
{
    public RectTransform rect;
    public Image image;
    private Vector2 startingPos;
    private Vector2 assignedVector;

    public void Init(Vector2 vec)
    {
        startingPos = rect.anchoredPosition;
        assignedVector = vec;
    }

    public void Update(float targetSpread, float spreadLerpSpeed)
    {
        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            startingPos + assignedVector * targetSpread,
            spreadLerpSpeed * Time.deltaTime
        );
    }

} 
