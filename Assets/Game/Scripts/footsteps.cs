using UnityEngine;

public class footsteps : MonoBehaviour
{
    public AudioSource stepSound;

    public float walkStepDelay = 0.5f;
    public float sprintStepDelay = 0.3f;

    float timer;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        bool isMoving = x != 0 || z != 0;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (!isMoving)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        float delay = isSprinting ? sprintStepDelay : walkStepDelay;

        if (timer >= delay)
        {
            stepSound.pitch = Random.Range(0.9f, 1.1f);
            stepSound.volume = isSprinting ? 0.8f : 0.5f;
            stepSound.Play();

            timer = 0f;
        }
    }
}
