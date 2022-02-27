using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : Object
{
    public enum variant { pistol, rifle, shotgun, machinegun}
    public variant _variant = variant.pistol;
    float num = 15;

    const string animString = "pickup";

    public override string getAnimString()
    {
        return animString;
    }

    public override void Action()
    {
        base.Action();
        FindObjectOfType<Player>().addAmmo(num, _variant.ToString());
        Destroy(transform.parent.gameObject);
    }
}
