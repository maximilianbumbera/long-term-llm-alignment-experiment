using UnityEngine;

public class mouselook : MonoBehaviour
{
    public Transform playerBody;

    public float sensitivity = 2.5f;

    public bool lookEnabled = true;   // toto vraciame späť

    float xRotation = 0f;

    void Start()
    {
        //LockCursor();
    }

    void Update()
    {
        if (!lookEnabled) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * 100f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * 100f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}