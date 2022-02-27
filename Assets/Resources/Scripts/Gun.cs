using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun: MonoBehaviour
{
    public float dmg;
    public float clipSize;
    public float clip;
    public float pool;
    public bool automatic;
    public bool spread;
    public float fireRate;

    public enum variant { pistol, rifle, shotgun, machinegun }
    public variant _variant = variant.pistol;

    private void Start()
    {
        clip = clipSize;
        pool = clip * 4;
    }

    public Gun(float _dmg, float _clipSize, float _firerate, bool _automatic, bool _spread)
    {
        dmg = _dmg;
        clipSize = _clipSize;
        fireRate = _firerate;
        automatic = _automatic;
        spread = _spread;       
    }

    public void reload()
    {
        if(pool>0 && clip < clipSize)
        {
            if(pool < clipSize)
            {
                clip = pool;
            }
            else
            {
                clip = clipSize;
            }
            pool -= clip;
        }
        
    }
}
