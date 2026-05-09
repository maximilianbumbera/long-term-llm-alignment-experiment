using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MinigameTerminal : MonoBehaviour
{
    public GameObject pacmanUI;
    public GameLogic gameLogic;
    public pacmanMinigame pacman;

    public CharacterControls character;
    public mouselook mouseLook;

    public notificationShower notificationScript;
    public QuestManager questScript;

    public AudioSource audioSource;
    public AudioClip winAudio;

    public void OnInteract()
    {
        if (gameLogic.wasMinigameToday)
        {
            notificationScript.ShowNotification("You already played the minigame today.");
            return;
        }

        if (pacmanUI.activeSelf)
        {
            return;
        }

        //lock movement + otáčanie sa
        character.controlsEnabled = false;
        mouseLook.lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pacmanUI.SetActive(true);
        pacman.StartGame();
    }

    public void OnMinigameFinished(int reward)
    {
        //unlock movement + otáčanie sa
        character.controlsEnabled = true;
        mouseLook.lookEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameLogic.AddCharacters(reward);
        gameLogic.wasMinigameToday = true;
        pacmanUI.SetActive(false);

        questScript.questNumber = 1;
        gameLogic.wasMinigameToday = true;

        audioSource.PlayOneShot(winAudio);

    }

}