using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    public Animator animator;
    public Transform rightHand;
    public Renderer bodyRenderer;
    public void Init(bool IsOwner)
    {
        bodyRenderer.gameObject.SetActive(!IsOwner);
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
