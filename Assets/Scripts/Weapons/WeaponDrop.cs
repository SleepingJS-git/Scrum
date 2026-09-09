using UnityEngine;

public class WeaponDrop : MonoBehaviour
{
    public Rigidbody rb;
    public float throwForce;
    public float upForce;
    private RuntimeWeapon runtimeWeapon;
    public void CreateWeapon(WeaponData d)
    {
        runtimeWeapon = new RuntimeWeapon(d);
        Instantiate(d.weaponModel, transform);

        Destroy(rb);
    }

    public void CreateWeapon(RuntimeWeapon r, Transform spawnPoint)
    {
        runtimeWeapon = r;
        Instantiate(r.data.weaponModel, transform);
        rb.linearVelocity = Vector3.zero;

        Vector3 finalForce = (spawnPoint.forward * throwForce) + (spawnPoint.up * upForce);
        rb.AddForce(finalForce, ForceMode.Impulse);

        Vector3 finalTorque = (Helper.RandomTorque(1f, 3f) * .25f) + spawnPoint.forward;  
        rb.AddTorque(finalTorque, ForceMode.Impulse);
    }

    public void Equip(Player player)
    {
        player.combat.EquipWeapon(runtimeWeapon);
        Destroy(gameObject);
    }
}
