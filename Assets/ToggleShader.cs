using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ToggleShader : MonoBehaviour
{
    [SerializeField] UniversalRendererData URPData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleURP(bool toggleState)
    {
        foreach(ScriptableRendererFeature srf in URPData.rendererFeatures)
        {
            srf.SetActive(toggleState);
        }
    }
}
