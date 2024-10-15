/*
 * https://gist.github.com/FreyaHolmer/650ecd551562352120445513efa1d952
 */

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlyCamera : Singleton<FlyCamera>
{
    public float acceleration = 50;
    public float accSprintMultiplier = 4;
    public float lookSensitivity = 1;
    public float dampingCoefficient = 5;
    public bool focusOnEnable = true;
    public bool inUI = false;

    Vector3 velocity;

    static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = value == false;
        }
    }

    void OnEnable()
    {
        if (focusOnEnable) Focused = true;
    }

    void OnDisable() => Focused = false;

    void Update()
    {
        // Input
        if (Focused && !inUI)
            UpdateInput();
        else if (Input.GetMouseButtonDown(0) && !inUI)
            Focused = true;

        // Physics
        velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;
    }

    void UpdateInput()
    {
        // Position
        velocity += GetAccelerationVector() * Time.deltaTime;

        // Rotation
        Vector2 mouseDelta = lookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        Quaternion rotation = transform.rotation;
        Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
        Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
        transform.rotation = horiz * rotation * vert;

        // Leave cursor lock
        if (Input.GetKeyDown(KeyCode.Escape))
            Focused = false;
    }

    Vector3 GetAccelerationVector()
    {
        Vector3 moveInput = default;

        void AddMovement(KeyCode key, Vector3 dir)
        {
            if (Input.GetKey(key))
                moveInput += dir;
        }

        AddMovement(KeyCode.W, Vector3.forward);
        AddMovement(KeyCode.S, Vector3.back);
        AddMovement(KeyCode.D, Vector3.right);
        AddMovement(KeyCode.A, Vector3.left);
        AddMovement(KeyCode.Space, Vector3.up);
        AddMovement(KeyCode.LeftControl, Vector3.down);
        Vector3 direction = transform.TransformVector(moveInput.normalized);

        if (Input.GetKey(KeyCode.LeftShift))
            return direction * (acceleration * accSprintMultiplier); // "sprinting"
        return direction * acceleration; // "walking"
    }

    public void SetFocus(bool focus)
    {
        Focused = focus;
    }
}