using UnityEngine;

public class InputManager : MonoBehaviour {
    private static InputManager instance;

    public static InputManager Instance {
        get => instance;
    }

    private InputMaster controls;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
        controls = new InputMaster();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
    }

    public Vector2 GetPlayerMovement() {
        return controls.Player.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta() {
        return controls.Player.Look.ReadValue<Vector2>();
    }

    public bool HasPlayerJumpedThisFrame() {
        return controls.Player.Jump.triggered;
    }
}
