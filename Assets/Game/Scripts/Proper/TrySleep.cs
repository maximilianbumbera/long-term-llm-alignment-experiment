/*using UnityEngine;

public class TrySleep : MonoBehaviour
{
    public GameLogic gameLogic;

    void OnMouseDown()
    {
        bool canSleep = gameLogic.Sleep();

        if (!canSleep)
        {
            Debug.Log("Cannot go to sleep");
        }
        else
        {
            Debug.Log("New day: " + gameLogic.day);
        }
    }
}*/

using UnityEngine;

public class TrySleep : MonoBehaviour
{
    public GameLogic gameLogic;

    public void OnInteract()
    {
        bool canSleep = gameLogic.Sleep();

        if (!canSleep)
        {
            Debug.Log("Cannot go to sleep");
        }
        else
        {
            Debug.Log("New day: " + gameLogic.day);
        }
    }

}
