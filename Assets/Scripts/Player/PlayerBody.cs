using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerBody : MonoBehaviour
{
    public Animator animator;
    public Transform rightHand;
    public MultiAimConstraint rightArmRig;
    public Renderer bodyRenderer;
    public Renderer[] renderersToColor;
    [Tooltip("First in list is order -> RBYG")] public Material[] playerColors;
    public Ragdoll ragdollPrefab;
    public Animator fpsPlayerModel;
    public Animator fpsLegModel;
    public Animator weaponAnimator;
    public GameObject accessoryFolder;
    public OnHitData onHitData;
    public Transform hipTransform;
    [SerializeField] private float blendDampTime;
    private Material playerMaterial;
    private bool isOwner;
    private DeathCamera deathCam;
    private List<Ragdoll> ragdolls = new List<Ragdoll>();
    public void Init(bool IsOwner, ulong ownerClientId)
    {
        ShowBodyRenderer(!IsOwner);
        onHitData = new OnHitData();

        SetPlayerMaterial(playerColors[ownerClientId]);

        isOwner = IsOwner;
    }

    public void SetDeathCamera(DeathCamera cam)
    {
        deathCam = cam;
    }

    /// <summary>
    /// [Called by ClientRpc] (onDeathEffects)
    /// </summary>
    public void Ragdoll()
    {
        // Turn on the renderer so that the instantiated body has its renderer on
        bodyRenderer.enabled = false;
        accessoryFolder.SetActive(false);
        if (fpsPlayerModel)
            fpsPlayerModel.gameObject.SetActive(false);
        if (fpsLegModel)
            fpsLegModel.gameObject.SetActive(false);

        animator.enabled = false;

        // Turn off the renderer to hide the dead player's actual renderer
        // bodyRenderer.gameObject.SetActive(false);

        // foreach (Rigidbody rb in animator.GetComponentsInChildren<Rigidbody>())
        // {
        //     rb.useGravity = true;
        //     rb.isKinematic = false;
        //     rb.linearVelocity = Vector3.zero;

        //     rb.AddExplosionForce(
        //         onHitData.explosionForce * 10f, 
        //         onHitData.sourceHit, 2f, onHitData.upwardsModifier,
        //         ForceMode.Impulse);
        // }

        Ragdoll ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        ragdoll.Create(hipTransform, playerMaterial, onHitData);

        ragdolls.Add(ragdoll);

        if (isOwner)
            deathCam.SetTarget(ragdoll.spineTransform);
    }
    
    public void ClearRagdolls()
    {
        for (int i = ragdolls.Count - 1; i >= 0; i--)
        {
            Destroy(ragdolls[i]);
            ragdolls.RemoveAt(i);
        }
    }

    public void SetPlayerMaterial(Material newMaterial)
    {
        foreach(Renderer r in renderersToColor)
            r.material = newMaterial;

        playerMaterial = newMaterial;
    }

    public void ShowBodyRenderer(bool showRenderer)
    {
        StartCoroutine(BodyRendererRoutine(showRenderer));
    }

    private IEnumerator BodyRendererRoutine(bool showRenderer)
    {
        yield return new WaitForEndOfFrame();

        bodyRenderer.enabled = showRenderer;
        accessoryFolder.SetActive(showRenderer);

        if (fpsPlayerModel)
            fpsPlayerModel.gameObject.SetActive(!showRenderer);
        if (fpsLegModel)
            fpsLegModel.gameObject.SetActive(!showRenderer);
    }

    public void UnRagdoll()
    {
        ShowBodyRenderer(true);

        foreach (Rigidbody rb in animator.GetComponentsInChildren<Rigidbody>())
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        animator.enabled = true;

    }
    public void Play(string para, bool val)
    {
        animator.SetBool(para, val);
        
        if (fpsPlayerModel)
            fpsPlayerModel.SetBool(para, val);
    }

    public void Play(string para, float val)
    {
        animator.SetFloat(para, val);

        if (fpsPlayerModel)
            fpsPlayerModel.SetFloat(para, val);
    }

    public void SetInt(string para, int val, bool excludeFpsModel = false)
    {
        animator.SetInteger(para, val);

        if (excludeFpsModel) return;
        if (fpsPlayerModel)
            fpsPlayerModel.SetInteger(para, val);
    }

    public void CrossBlendLocomotion(string para, float val)
    {
        animator.SetFloat(para, val, blendDampTime, Time.deltaTime);
        if (fpsPlayerModel)
            fpsPlayerModel.SetFloat(para, val, blendDampTime, Time.deltaTime);
    }

    public void PlayTrigger(string para, bool excludeFpsModel = false)
    {
        animator.SetTrigger(para);

        if (excludeFpsModel) return;
        if (fpsPlayerModel)
            fpsPlayerModel.SetTrigger(para);
    }

    public void Play(string name)
    {
        animator.Play(name);

        if (fpsPlayerModel)
            fpsPlayerModel.Play(name);
    }

    public void PlayKick()
    {
        PlayTrigger("Kick");
        fpsLegModel.SetTrigger("Kick");
    }

    public void PlayWeaponAnim(string animName)
    {
        weaponAnimator.Play(animName);
    }
}
