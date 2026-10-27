using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BuildableModelPreview : MonoBehaviour
{
    [SerializeField] private RawImage previewImage;
    [SerializeField] private string previewLayerName = "UIModelPreview";
    [SerializeField, Range(64, 1024)] private int textureSize = 256;
    [SerializeField] private float spinSpeed = 35f;
    [SerializeField] private Vector3 cameraAngles = new Vector3(20f, -25f, 0f);
    [SerializeField, Min(0.1f)] private float lightIntensity = 3f;
    [SerializeField] private int previewRendererIndex = -1;

    private static int nextRigIndex;

    private GameObject sourcePrefab;
    private GameObject rig;
    private Transform pivot;
    private GameObject model;
    private Camera previewCamera;
    private RenderTexture renderTexture;

    private readonly List<Mesh> bakedMeshes = new List<Mesh>();

    private bool needsRebuild;
    private bool spinning = true;
    private int previewLayer;

    public void ShowPrefab(GameObject prefab)
    {
        sourcePrefab = prefab;
        needsRebuild = true;

        if (isActiveAndEnabled) Rebuild();
    }

    public void SetSpinning(bool value)
    {
        spinning = value;
    }

    private void OnEnable()
    {
        if (needsRebuild) Rebuild();
        if (rig) rig.SetActive(sourcePrefab != null);
    }

    private void OnDisable()
    {
        if (rig) rig.SetActive(false);
    }

    private void LateUpdate()
    {
        if (spinning && pivot && model) pivot.Rotate(Vector3.up, spinSpeed * Time.unscaledDeltaTime, Space.Self);
    }

    private bool EnsureRig()
    {
        if (rig) return true;

        previewLayer = LayerMask.NameToLayer(previewLayerName);


        if (previewLayer < 0 || !previewImage)
        {
            Debug.LogError($"{name}: assign Preview Image and create the '{previewLayerName}' layer.", this);
            return false;
        }

        previewImage.raycastTarget = false;

        rig = new GameObject($"UI Model Preview {nextRigIndex}");
        rig.transform.position = new Vector3(nextRigIndex++ * 30f, -10000f, 0f);

        pivot = new GameObject("Spin Pivot").transform;
        pivot.SetParent(rig.transform, false);

        previewCamera = new GameObject("Preview Camera").AddComponent<Camera>();
        previewCamera.GetUniversalAdditionalCameraData().SetRenderer(previewRendererIndex);
        previewCamera.transform.SetParent(rig.transform, false);
        previewCamera.transform.localRotation = Quaternion.Euler(cameraAngles);
        previewCamera.transform.localPosition = previewCamera.transform.localRotation * new Vector3(0f, 0f, -4f);

        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.cullingMask = 1 << previewLayer;
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = 0.9f;
        previewCamera.nearClipPlane = 0.1f;
        previewCamera.farClipPlane = 10f;
        previewCamera.allowHDR = false;
        previewCamera.allowMSAA = false;
        previewCamera.useOcclusionCulling = false;

        renderTexture = new RenderTexture(textureSize, textureSize, 24, RenderTextureFormat.ARGB32);
        renderTexture.name = $"{name} Model Preview";
        renderTexture.antiAliasing = 1;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.Create();

        previewCamera.targetTexture = renderTexture;
        previewImage.texture = renderTexture;

        AddLight("Key Light", new Vector3(-2f, 3f, -3f), lightIntensity);
        AddLight("Fill Light", new Vector3(2f, 1f, 2f), lightIntensity * 0.5f);

        return true;
    }

    private void AddLight(string lightName, Vector3 position, float intensity)
    {
        Light light = new GameObject(lightName).AddComponent<Light>();

        light.transform.SetParent(rig.transform, false);
        light.transform.localPosition = position;

        light.type = LightType.Point;
        light.range = 10f;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
        light.cullingMask = 1 << previewLayer;
    }

    private void Rebuild()
    {
        DestroyModel();

        if (previewImage) previewImage.enabled = sourcePrefab != null;

        if (!sourcePrefab)
        {
            needsRebuild = false;

            if (rig) rig.SetActive(false);

            return;
        }

        if (!EnsureRig()) return;

        needsRebuild = false;
        rig.SetActive(true);
        pivot.localRotation = Quaternion.identity;

        // Show only the highest-detail LOD.
        HashSet<Renderer> excluded = new HashSet<Renderer>();

        foreach (LODGroup group in sourcePrefab.GetComponentsInChildren<LODGroup>(true))
        {
            LOD[] lods = group.GetLODs();

            for (int i = 1; i < lods.Length; i++)
            {
                foreach (Renderer renderer in lods[i].renderers) excluded.Add(renderer);
            }

            if (lods.Length > 0)
            {
                foreach (Renderer renderer in lods[0].renderers) excluded.Remove(renderer);
            }
        }

        model = new GameObject("Centered Model");
        model.transform.SetParent(pivot, false);

        CopyVisuals(sourcePrefab.transform, model.transform, true, excluded);

        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"{sourcePrefab.name}: no visible MeshRenderer or SkinnedMeshRenderer meshes found for the UI preview.", this);
            return;
        }

        Bounds bounds = renderers[0].bounds;

        foreach (MeshRenderer renderer in renderers) bounds.Encapsulate(renderer.bounds);

        Vector3 center = pivot.InverseTransformPoint(bounds.center);
        float radius = Mathf.Max(bounds.extents.magnitude, 0.001f);
        float scale = 0.75f / radius;

        model.transform.localScale = Vector3.one * scale;
        model.transform.localPosition = -center * scale;
    }

    private void CopyVisuals(Transform source, Transform parent, bool isRoot, HashSet<Renderer> excluded)
    {
        if (!isRoot && !source.gameObject.activeSelf) return;

        GameObject copy = new GameObject(source.name);
        copy.layer = previewLayer;

        copy.transform.SetParent(parent, false);
        copy.transform.localPosition = isRoot ? Vector3.zero : source.localPosition;
        copy.transform.localRotation = source.localRotation;
        copy.transform.localScale = source.localScale;

        Mesh mesh = null;
        Renderer sourceRenderer = source.GetComponent<MeshRenderer>();

        if (sourceRenderer && sourceRenderer.enabled && !excluded.Contains(sourceRenderer))
        {
            MeshFilter filter = source.GetComponent<MeshFilter>();

            if (filter) mesh = filter.sharedMesh;
        }
        else
        {
            SkinnedMeshRenderer skinned = source.GetComponent<SkinnedMeshRenderer>();

            if (skinned && skinned.enabled && !excluded.Contains(skinned) && skinned.sharedMesh)
            {
                mesh = new Mesh();

                skinned.BakeMesh(mesh);
                bakedMeshes.Add(mesh);

                sourceRenderer = skinned;
            }
        }

        if (mesh && sourceRenderer)
        {
            copy.AddComponent<MeshFilter>().sharedMesh = mesh;

            MeshRenderer renderer = copy.AddComponent<MeshRenderer>();

            renderer.sharedMaterials = sourceRenderer.sharedMaterials;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        foreach (Transform child in source) CopyVisuals(child, copy.transform, false, excluded);
    }

    private void DestroyModel()
    {
        if (model)
        {
            model.SetActive(false);
            Destroy(model);
        }

        model = null;

        foreach (Mesh mesh in bakedMeshes) Destroy(mesh);

        bakedMeshes.Clear();
    }

    private void OnDestroy()
    {
        DestroyModel();

        if (previewImage) previewImage.texture = null;
        if (previewCamera) previewCamera.targetTexture = null;

        if (renderTexture)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (rig) Destroy(rig);
    }
}