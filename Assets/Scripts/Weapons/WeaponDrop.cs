using UnityEngine;

public class WeaponDrop : MonoBehaviour
{
    public Rigidbody rb;
    private RuntimeWeapon runtimeWeapon;
    public void CreateWeapon(WeaponData d, bool enableRB)
    {
        runtimeWeapon = new RuntimeWeapon(d);
        GameObject model = Instantiate(d.weaponModel, transform);

        if (!enableRB) Destroy(rb);
    }

    public void DropWeapon(RuntimeWeapon r)
    {
        runtimeWeapon = r;
    }

    public void Equip(Player player)
    {
        player.combat.EquipWeapon(runtimeWeapon);
        Destroy(gameObject);
    }
}
