using System.Collections.Generic;
using UnityEngine;

public class WeaponDatabase : MonoBehaviour
{
    public static WeaponDatabase Instance;
    public static WeaponDrop TemplateDrop => Instance.weaponDropTemplate;   // Template WeaponDrop
    [SerializeField] private WeaponDrop weaponDropTemplate;
    [SerializeField] private WeaponData[] weaponDatas;
    private Dictionary<ulong, WeaponData> data;
    void Awake()
    {
        Instance = this;
        data = new Dictionary<ulong, WeaponData>();

        foreach(WeaponData w in weaponDatas)
            data[w.weaponID] = w;
    }

    public static WeaponData GetWeapon(ulong id)
    {
        return Instance.data[id];
    }
}
