using UnityEngine;

public class boxMovement : MonoBehaviour //dekoračný skript, aby sa box pohyboval po bežiacom páse
{
    public float speed = 5f;
    private float resetZ = -278f;
    public Vector3 startPosition = new Vector3(225.5f, 85.5f, -330f); //nakrokovane na presnu poziciu na mape

    void Start()
    {
        //transform.position = startPosition;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (transform.position.z > resetZ)
        {
            transform.position = startPosition;
        }
    }
}


