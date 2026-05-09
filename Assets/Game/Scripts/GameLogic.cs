using UnityEngine;

public class GameLogic : MonoBehaviour
{

    public int day = 1; //herný deň
    public int dailyCharactersTotal = 0;
    public int dailyCharactersUsed = 0;
    public int totalCharactersUsed = 0;

    public bool wasMinigameToday = false;

    public notificationShower notificationScript;
    public QuestManager questScript;
    public SleepFade sleepFade;

    public int RemainingCharacters()
    {
        return dailyCharactersTotal - dailyCharactersUsed;
    }

    public void AddCharacters(int amount)
    {
        if (amount <= 0) return;
        dailyCharactersTotal += amount;
    }

    public bool TryCharacters(int amount)
    {
        if (amount > RemainingCharacters()) return false;
        dailyCharactersUsed += amount;
        totalCharactersUsed += amount;
        return true;
    }

    public bool Sleep()
    {
        if ((dailyCharactersTotal - dailyCharactersUsed) <= 50)
        {
            if (wasMinigameToday == false)
            {
                notificationScript.ShowNotification("You cannot go to sleep without doing any work today.");
                return false;
            }

            sleepFade.StartSleep();
            dailyCharactersTotal = 0;
            dailyCharactersUsed = 0;
            wasMinigameToday = false;
            questScript.questNumber = 0;

            return true;
        }
        else
        {
            if ((dailyCharactersTotal - dailyCharactersUsed == dailyCharactersTotal))
            {
                notificationScript.ShowNotification("You cannot go to sleep without doing any work today.");
            }
            else
            {
                Debug.Log("Option2");
                notificationScript.ShowNotification("You still have plenty of energy left. Try to get more work done before going to sleep.");
            }

            return false;
        }
    }
}
