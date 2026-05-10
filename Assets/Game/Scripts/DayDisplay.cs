using UnityEngine;
using TMPro;

public class DayDisplay : MonoBehaviour
{
    public GameLogic gameLogic;
    public TMP_Text dayText;
    public TMP_Text RemainingCharactersText;

    void Update()
    {
        dayText.text = "Day " + gameLogic.day;
        RemainingCharactersText.text = "Remaining power: " + gameLogic.RemainingCharacters();
    }
}