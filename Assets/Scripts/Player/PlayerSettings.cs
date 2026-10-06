using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    public CrouchType CrouchingType;
    public enum CrouchType
    {
        Holding,
        Toggle
    }
    
}
