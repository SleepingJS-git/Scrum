using UnityEngine;

public class HeadBobbing : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private Player main;
    [SerializeField] private CharacterController cc;

    [Header("Tuneables")]
    [SerializeField, Range(0, 0.1f)] private float _walkAmplitude = 0.003f;
    [SerializeField, Range(0, 30f)] private float _walkFrequency = 14.0f;
    [SerializeField, Range(0, 0.1f)] private float _crouchAmplitude = 0.0015f;
    [SerializeField, Range(0, 30f)] private float _crouchFrequency = 8.0f;
    [SerializeField, Range(0, 0.1f)] private float _sprintAmplitude = 0.05f;
    [SerializeField, Range(0, 30f)] private float _sprintFrequency = 18.0f;

    public Vector3 bobMotion;

    private float _toggleSpeed = 3.0f;
    private Vector3 _startPos;
    private float _amplitude;
    private float _frequency;

    void Start()
    {
        _startPos = _cameraHolder.localPosition;
    }

    void Update()
    {
        EvaluateMotion();
        CheckMotion();
        ResetPosition();
    }

    private void EvaluateMotion()
    {
        _startPos.y = main.Move.TargetCamLevel;

        switch ((MovementState)main.MoveState.Value)
        {
            case MovementState.Standing:
                _amplitude = _walkAmplitude;
                _frequency = _walkFrequency;
                break;

            case MovementState.Crouching:
                _amplitude = _crouchAmplitude;
                _frequency = _crouchFrequency;
                break;

            case MovementState.Sprinting:
                _amplitude = _sprintAmplitude;
                _frequency = _sprintFrequency;
                break;

            case MovementState.Sliding:
                _amplitude = 0f;
                _frequency = 0f;
                break;
        }
    }

    private void PlayMotion(Vector3 motion)
    {
        bobMotion = motion;
        _cameraHolder.localPosition += motion;
    }

    private void CheckMotion()
    {
        float speed = new Vector3(
            cc.velocity.x,
            0f,
            cc.velocity.z
        ).magnitude;

        if (speed < _toggleSpeed) return;
        if (!cc.isGrounded) return;

        PlayMotion(FootStepMotion());
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;

        pos.y += Mathf.Sin(Time.time * _frequency) * _amplitude;
        pos.x += Mathf.Cos(Time.time * _frequency / 2f) * _amplitude * 2f;

        return pos;
    }

    private void ResetPosition()
    {
        _cameraHolder.localPosition = Vector3.Lerp(
            _cameraHolder.localPosition,
            _startPos,
            Time.deltaTime
        );

        bobMotion = _cameraHolder.localPosition - _startPos;
    }
}