using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float reach = 3f;
    private Transform _cam;
    [SerializeField] private Interactable _selected;
    [SerializeField] private PlayerHud _hud;
    private Player _main;
    public void Init(bool isOwner, Transform cam, PlayerHud h)
    {
        if (isOwner)
        {
            _cam = cam;
            _hud = h;
            _main = GetComponent<Player>();
        }
    }
    public void OnInteract()
    {
        if (_selected)
        {
            _selected.OnInteract(_main);
            Unselect();
        }
    }
    public void Interaction()
    {
        if (Physics.Raycast(_cam.position, _cam.forward, out RaycastHit hit, reach, Layer.Interactable, QueryTriggerInteraction.Ignore))
        {
            if (_selected)
            {
                if (_selected.gameObject != hit.collider.gameObject)
                {
                    Unselect();
                    _selected = hit.collider.GetComponent<Interactable>();
                    Select();
                }
            }
            else
            {
                _selected = hit.collider.GetComponent<Interactable>();
                Select();
            }
        }
        else
            Unselect();
    }

    private void Select()
    {
        _hud.interactText.text = _selected.message;
    }
    private void Unselect()
    {
        _selected = null;
        _hud.interactText.text = "";
    }
}   
