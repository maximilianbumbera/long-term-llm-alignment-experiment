/*using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class CharacterControls : MonoBehaviour
{
    public Transform spriteTransform;

    public float speed = 0;
    public float gravity = 10.0f;
    public float jumpHeight = 3.0f;

    public float maxVelocityChange = 10.0f;
    public bool canJump = true;
    private bool grounded = false;

    public float sprintSpeed = 15f;
    public float walkSpeed = 10f;
    public bool isSprinting = false;

    public float maxstamina = 100f;
    public float currentstamina = 100f;

    public bool groundeds;
    public Vector3 velinka;

    float timer = 0.0f;
    public bool controlsEnabled = true;


    void Awake()
    {
        GetComponent<Rigidbody>().freezeRotation = true;
        GetComponent<Rigidbody>().useGravity = false;
    }

    void FixedUpdate()
    {
        if (!controlsEnabled)
        {
            // hard stop so character doesn't slide
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        groundeds = grounded;
        speed = walkSpeed;
        velinka = GetComponent<Rigidbody>().velocity;

        Quaternion targetRotation = Quaternion.identity;

        if (grounded)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (currentstamina > 1)
                {
                    isSprinting = true;
                    speed = sprintSpeed;
                    currentstamina -= 0.9f;

                    // ⬇️ naklonenie dopredu pri sprintovaní
                    targetRotation = Quaternion.Euler(30f, 0f, 0f);
                }
                else
                {
                    isSprinting = false;
                    speed = walkSpeed;
                }
            }
            else
            {
                isSprinting = false;
                speed = walkSpeed;

                if (currentstamina < maxstamina)
                    currentstamina += (Time.deltaTime * 11);
            }

            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= speed;

            Vector3 velocity = GetComponent<Rigidbody>().velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);

            if (canJump && Input.GetButton("Jump"))
            {
                GetComponent<Rigidbody>().velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            }
        }

        // ⬇️ plynulé natočenie spritu
        if (spriteTransform != null)
        {
            Quaternion normalRotation = Quaternion.identity;
            Quaternion target = isSprinting ? targetRotation : normalRotation;
            spriteTransform.localRotation = Quaternion.Slerp(spriteTransform.localRotation, target, Time.deltaTime * 5f);
        }

        GetComponent<Rigidbody>().AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));
        grounded = false;
    }

    void OnCollisionStay()
    {
        grounded = true;
    }


   
    float CalculateJumpVerticalSpeed()
    {

        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }
}*/



using UnityEngine;


public class CharacterControls : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float sprintSpeed = 15f;

    public bool controlsEnabled = true;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;

        rb.useGravity = true;
    }

    void FixedUpdate()
    {
        if (controlsEnabled == false)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0f, z).normalized;
        move = transform.TransformDirection(move);

        float speed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = walkSpeed;
        }


        rb.velocity = new Vector3(move.x * speed, rb.velocity.y, move.z * speed);

    }
}
 

