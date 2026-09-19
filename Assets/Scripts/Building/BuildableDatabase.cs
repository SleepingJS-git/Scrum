using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildableDatabase : MonoBehaviour
{
    public static BuildableDatabase Instance;
    [SerializeField] private Buildable[] buildablePrefabs;
    private Dictionary<ulong, Buildable> data;
    public static List<ulong> AvailablePrefabIds => Instance.data.Keys.ToList();
    void Awake()
    {
        Instance = this;
        data = new Dictionary<ulong, Buildable>();

        foreach(Buildable b in buildablePrefabs)
            data[b.Id] = b;
    }

    public static Buildable GetBuildable(ulong id)
    {
        return Instance.data[id];
    }
    public static Buildable GetBuildableAtIndex(int idx)
    {
        return Instance.data.ElementAt(idx).Value;
    }
}
