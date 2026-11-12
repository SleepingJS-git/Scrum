using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    public CrouchType CrouchingType;
    public float DefaultPOV = 80f;
    public bool DevMode;
    public int MeleeIndicatorVersion = 1;
    public enum CrouchType
    {
        Holding,
        Toggle
    }
    
}
