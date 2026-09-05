using UnityEngine;

public static class Layer
{
    public static int Ground{ get { return 1 << 6; }}
    public static int Wall{ get { return 1 << 7; }}
    public static int Entity{ get { return 1 << 8; }}
    public static int Player{ get { return 1 << 9; }}
    public static int Buildable{ get { return 1 << 10; }}
    public static int Interactable{ get { return 1 << 11; }}
    public static int Projectile{ get { return 1 << 12; }}
    public static int Ragdoll{ get { return 1 << 13; }}
    public static int WeaponOverlay{ get { return 1 << 14; }}
}
