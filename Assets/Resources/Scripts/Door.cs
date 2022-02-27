using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Object
{
    const string animString = "door";

    public override string getAnimString()
    {
        return animString;
    }

    public override void Action()
    {
        base.Action();
        //GetComponent<Animator>().SetTrigger("Open");
    }
}
