using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    public WeaponData data;
    public Transform holder;
    public WeaponDrop weaponDropTemplate; // This will be moved to a singleton manager.
    public float spawnTime;
    private float _dt;
    private WeaponDrop drop;
    public void SpawnWeapon()
    {
        drop = Instantiate(weaponDropTemplate, holder);
        drop.transform.localPosition = Vector3.zero;
        drop.CreateWeapon(data);
    }

    void Update()
    {
        if (drop) return;
        
        _dt -= Time.deltaTime;
        if (_dt > 0f) return;

        SpawnWeapon();
        _dt = spawnTime;
    }
}
