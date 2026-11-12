using System.Collections;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public Rigidbody[] rigidbodies;
    public Renderer bodyRenderer;
    public Transform rootTransform; // hips
    public Transform spineTransform;
    public void Create(Transform source, Material material, OnHitData onHitData)
    {
        
        bodyRenderer.material = material;

        StartCoroutine(RagdollRoutine(source, onHitData));
    }

    private IEnumerator RagdollRoutine(Transform source, OnHitData onHitData)
    {
        foreach(Rigidbody rb in rigidbodies)
            rb.isKinematic = true;
        yield return new WaitForEndOfFrame();
        CopyBoneTransforms(source, rootTransform);
        Physics.SyncTransforms();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;

            rb.AddExplosionForce(
                onHitData.explosionForce * 10f, 
                onHitData.sourceHit, 2f, onHitData.upwardsModifier,
                ForceMode.Impulse);
        }
    }

    private void CopyBoneTransforms(Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;

        for (int i = 0; i < source.childCount; i++)
        {
            Transform sourceChild = source.GetChild(i);
            Transform targetChild = target.Find(sourceChild.name);

            if (targetChild != null)
            {
                CopyBoneTransforms(sourceChild, targetChild);
            }
        }
    }
}
