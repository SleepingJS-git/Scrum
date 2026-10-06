using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    public CrouchType crouchType;
    public enum CrouchType
    {
        Hold,
        Toggle
    }
}
