using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    public WeaponData data;
    public Transform holder;
    public WeaponDrop weaponDropTemplate;
    void Start()
    {
        SpawnWeapon();
    }
    public void SpawnWeapon()
    {
        WeaponDrop drop = Instantiate(weaponDropTemplate, holder);
        drop.transform.localPosition = Vector3.zero;
        drop.CreateWeapon(data, false);
    }
}
