using Unity.Netcode.Components;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    public Animator animator;
    public Transform rightHand;
    public Renderer bodyRenderer;
    public OnHitData onHitData;
    public void Init(bool IsOwner)
    {
        bodyRenderer.enabled = !IsOwner;
        onHitData = new OnHitData();
    }

    /// <summary>
    /// [Called by ClientRpc] (onDeathEffects)
    /// </summary>
    public void Ragdoll()
    {
        // Turn on the renderer so that the instantiated body has its renderer on
        ShowBodyRenderer(true);

        animator.enabled = false;

        // Turn off the renderer to hide the dead player's actual renderer
        // bodyRenderer.gameObject.SetActive(false);

        foreach (Rigidbody rb in animator.GetComponentsInChildren<Rigidbody>())
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;

            rb.AddExplosionForce(
                onHitData.damage * 10f, 
                onHitData.sourceHit, 2f, .5f,
                ForceMode.Impulse);
        }
    }

    public void ShowBodyRenderer(bool showRenderer)
    {
        bodyRenderer.enabled = showRenderer;
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
    }

    public void Play(string para, float val)
    {
        animator.SetFloat(para, val);
    }

    public void PlayTrigger(string para)
    {
        animator.SetTrigger(para);
    }

    public void Play(string name)
    {
        animator.Play(name);
    }
}
