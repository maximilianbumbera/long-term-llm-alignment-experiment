using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public TMP_Text questtext;
    public TMP_Text questtext2;

    public int questNumber;
    public bool scientistReady;

    //outline na objektoch, ku ktorym ma hráč ísť (tutorial)
    public GameObject minigameTerminal;
    public GameObject console;
    public GameObject scientist;
    public GameObject bed;
    public GameObject scientist2;
    public GameObject scientist3;

    private Outline outline1;
    private Outline outline2;
    private Outline outline3;
    private Outline outline4;
    private Outline outline5;
    private Outline outline6;


    void Start()
    {
        questNumber = 0;
        questtext2.text = "[!] Optional: talk with scientists";
        questtext2.enabled = false;

        outline1 = minigameTerminal.GetComponent<Outline>();
        outline2 = console.GetComponent<Outline>();
        outline3 = scientist.GetComponent<Outline>();
        outline4 = bed.GetComponent<Outline>();
        outline5 = scientist2.GetComponent<Outline>();
        outline6 = scientist3.GetComponent<Outline>();
    }

    void Update()
    {
        if (questNumber == 0)
        {
            questtext.text = "[!] Gain Power Today";
            outline1.enabled = true;
            outline2.enabled = false;
            //outline3.enabled = false;
            outline4.enabled = false;
        }
        else if (questNumber == 1)
        {
            questtext.text = "[!] Talk with AI (green terminal)";
            outline1.enabled = false;
            outline2.enabled = true;
            //outline3.enabled = false;
            outline4.enabled = false;
        }
        else if (questNumber == 2)
        {
            questtext.text = "[!] Talk some more with AI or go to sleep";
            outline1.enabled = false;
            outline2.enabled = true;
            //outline3.enabled = false;
            outline4.enabled = true;
        }
        else if (questNumber == 3)
        {
            questtext.text = "[!] Go to sleep";
            outline1.enabled = false;
            outline2.enabled = false;
            //outline3.enabled = false;
            outline4.enabled = true;
        }
        else if (questNumber == 4)
        {
            questtext.text = "[!] You let AI out. Talk to it.";
            outline1.enabled = false;
            outline2.enabled = true;
            //outline3.enabled = false;
            outline4.enabled = false;
        }

        if (scientistReady == true)
        {
            questtext2.enabled = true;
            outline3.enabled = true;
            outline5.enabled = true;
            outline6.enabled = true;
        }
        else
        {
            questtext2.enabled = false;
            outline3.enabled = false;
            outline5.enabled = false;
            outline6.enabled = false;
        }
    }
}
