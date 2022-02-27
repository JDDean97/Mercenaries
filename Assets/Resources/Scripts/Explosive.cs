using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : Object
{
    public float range;
    public float damage;
    public float force;
    public float delay = 3;
    bool primed = false;
    // Start is called before the first frame update
    void Start()
    {
        force = damage * 0.25f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (primed)
        {
            delay -= 1;
            if (delay < 0)
            {
                Action();
            }
        }
    }

    public void setPrime()
    {
        primed = true;
    }

    public override void Action()
    {
        //Explode
        Collider[] col = Physics.OverlapSphere(transform.position, range);
        RaycastHit hit;
        foreach (Collider c in col)
        {
            Physics.Raycast(transform.position, c.transform.position - transform.position, out hit);
            if (hit.transform == c.transform)
            {
                if(c.GetComponent<Object>())
                {
                    if (c.GetComponent<Player>())
                    { c.GetComponent<Player>().hurt((int)damage); }  
                    
                    if (c.GetComponent<Enemy>())
                    { c.GetComponent<Enemy>().hurt(c,(int)damage); }   
                    
                    if(c.GetComponent<Rigidbody>())
                    {
                        c.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, range);
                    }
                }
            }
        }
        base.Action();
    }
}
