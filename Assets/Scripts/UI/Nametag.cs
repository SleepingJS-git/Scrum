using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Nametag : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject nametag;
    [SerializeField] private TMP_Text nameText;

    [Header("Distance Scaling")]
    [SerializeField] private float minScaleDistance = 3f;
    [SerializeField] private float maxScaleDistance = 25f;
    [SerializeField] private float maxScaleMultiplier = 1.4f;

    private PlayerStats playerStats;
    private Camera localCamera;
    private Vector3 baseScale;
    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();

        baseScale = nametag.transform.localScale;
    }

    public override void OnNetworkSpawn()
    {
        PlayerManager.ToBuild += RefreshMainCam;
        PlayerManager.ToCombat += RefreshMainCam;

        PlayerManager.ToBuild += () => {
            ToggleSelf(true);
        };
        PlayerManager.ToCombat += () => {
            ToggleSelf(false);
        };

        playerStats.PlayerName.OnValueChanged += OnPlayerNameChanged;

        nameText.text = playerStats.PlayerName.Value.ToString();
        
        PlayerManager.ToBuilding += RefreshLocalCam;
        PlayerManager.ToCombat += RefreshLocalCam;

        if (!IsOwner) return;
  

        PlayerManager.ToBuilding += () =>
        {
            ToggleViewSelfTag(true);
        };
        PlayerManager.ToCombat += () =>
        {
            ToggleViewSelfTag(false);
        };

        nameText.color = Color.green;
    }

    public override void OnNetworkDespawn()
    {
        playerStats.PlayerName.OnValueChanged -= OnPlayerNameChanged;
    }

    private void RefreshMainCam()
    {
        localCamera = Camera.main;
    }

    private void ToggleSelf(bool isOn)
    {
        nameText.text = isOn ? playerStats.PlayerName.Value.ToString() : "";
    }

    private void OnPlayerNameChanged(
        FixedString64Bytes oldName,
        FixedString64Bytes newName)
    {
        nameText.text = newName.ToString();
    }

    private void RefreshLocalCam()
    {
        localCamera = Camera.main;
    }

    private void ToggleViewSelfTag(bool viewSelf)
    {
        nameText.text = viewSelf ? playerStats.PlayerName.Value.ToString(): "";
    }

    private void LateUpdate()
    {
        // if (IsOwner || !nametag.activeSelf)
        //     return;

        if (localCamera == null)
            localCamera = Camera.main;

        if (localCamera == null)
            return;

        nameText.enabled = CanSeeNametag();

        // nametag faces client player
        nametag.transform.rotation = localCamera.transform.rotation;

        float distance = Vector3.Distance(
            localCamera.transform.position,
            nametag.transform.position
        );

        float t = Mathf.InverseLerp(
            minScaleDistance,
            maxScaleDistance,
            distance
        );

        t = Mathf.SmoothStep(0f, 1f, t);

        float scaleMultiplier = Mathf.Lerp(
            1f,
            maxScaleMultiplier,
            t
        );

        nametag.transform.localScale = baseScale * scaleMultiplier;
    }

    private bool CanSeeNametag()
    {
        Vector3 direction = nametag.transform.position - localCamera.transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(localCamera.transform.position,direction.normalized,out RaycastHit hit, distance))
        {
            return hit.transform.root == transform.root;
        }

        return true;
    }
}