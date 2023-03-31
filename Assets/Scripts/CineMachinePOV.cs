using Cinemachine;
using UnityEngine;

public class CineMachinePOV : CinemachineExtension {
    [SerializeField]
    private float clampAngle = 80f;
    [SerializeField]
    private float horizonalSpeed = 10f;
    [SerializeField]
    private float verticalSpeed = 10f;

    private InputManager inputManager;
    private Vector3 startingRotation;

    protected override void Awake() {
        inputManager = InputManager.Instance;
        base.Awake();
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
        if (vcam.Follow) {
            if (stage == CinemachineCore.Stage.Aim) {
                if (startingRotation == null) startingRotation = transform.localRotation.eulerAngles;
                Vector2 deltaInput = inputManager.GetMouseDelta();
                startingRotation.x += horizonalSpeed * deltaInput.x * Time.deltaTime;
                startingRotation.y += verticalSpeed * deltaInput.y * Time.deltaTime;
                startingRotation.y = Mathf.Clamp(startingRotation.y, -clampAngle, clampAngle);
                state.RawOrientation = Quaternion.Euler(-startingRotation.y, startingRotation.x, 0f);
            }
        }
    }
}
