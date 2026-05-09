using UnityEngine;

public class InteractionRaycaster : MonoBehaviour
{
    public float range = 5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range))
            {
                hit.collider.SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

}