using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Director : MonoBehaviour
{
    Player player;
    float timer = 200f;
    float comboTimer = 6;
    float _comboTimer;
    int combo = 0;
    int score = 0;

    //ui stuff
    Image health;
    TextMeshProUGUI ammoCounter;
    TextMeshProUGUI time;
    TextMeshProUGUI scoreCounter;
    TextMeshProUGUI comboCounter;



    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        health = FindObjectOfType<Canvas>().transform.Find("Health").GetChild(0).GetComponent<Image>();
        ammoCounter = FindObjectOfType<Canvas>().transform.Find("Ammo").GetChild(0).GetComponent<TextMeshProUGUI>();
        time = FindObjectOfType<Canvas>().transform.Find("Timer").GetChild(0).GetComponent<TextMeshProUGUI>();
        scoreCounter = FindObjectOfType<Canvas>().transform.Find("Score").GetChild(0).GetComponent<TextMeshProUGUI>();
        comboCounter = FindObjectOfType<Canvas>().transform.Find("Combo").GetChild(0).GetComponent<TextMeshProUGUI>();
        player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= 1 * Time.deltaTime;
        _comboTimer -= 1 * Time.deltaTime;
        if (timer<1)
        { gameOver(); }

        if(combo>0 && _comboTimer <=0)
        {
            addScore(combo * 1000);
            combo = 0;
        }
        updateUI();
    }

    void updateUI()
    {
        health.fillAmount = player.getHealth() / 100;
        ammoCounter.text = player.getGun().clip.ToString() + "/" + player.getGun().pool.ToString();
        scoreCounter.text = score.ToString();
        if (combo > 1) 
        { 
            comboCounter.enabled = true;
            comboCounter.text = combo.ToString() + "x";
        }
        else 
        { comboCounter.enabled = false; }

        System.TimeSpan ts = System.TimeSpan.FromSeconds(timer);
        string timeText = string.Format("{0:d2}:{1:d2}", ts.Minutes, ts.Seconds);
        time.text = timeText;

        //convert time into minutes and seconds
    }

    public void addScore(int pass)
    {
        score += pass;
    }

    public void addTime(int pass)
    {
        timer += pass;
    }

    public void upCombo()
    {
        combo += 1;
        _comboTimer = comboTimer; // reset combo countdown
    }

    void gameOver()
    {
        
    }
}
