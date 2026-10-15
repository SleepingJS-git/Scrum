using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    public CrouchType CrouchingType;
    public float DefaultPOV = 80f;
    public enum CrouchType
    {
        Holding,
        Toggle
    }
    
}
