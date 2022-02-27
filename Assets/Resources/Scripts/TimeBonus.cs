using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBonus : Object
{
    const int seconds = 15;
    const string animString = "melee";

    public override string getAnimString()
    {
        return animString;
    }

    public override void Action()
    {
        base.Action();
        FindObjectOfType<Director>().addTime(seconds);
        //TODO: instantiate particles
        Destroy(gameObject);
    }
}
