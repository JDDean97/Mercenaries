using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    float health = 100;
    Animator anim;
    public List<Collider> hurtList;
    public int score = 1000;
    Player target;
    const int dmg = 20;
    NavMeshAgent nav;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        foreach(Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
        GetComponent<Rigidbody>().isKinematic = false;
        target = FindObjectOfType<Player>();
        nav = GetComponent<NavMeshAgent>();      
    }

    // Update is called once per frame
    void Update()
    {
        nav.SetDestination(target.transform.position);
        attack();
        move();
    }

    void move()
    {
        Vector3 dir = transform.InverseTransformDirection(nav.steeringTarget - transform.position);
        dir = dir.normalized;
        //Debug.Log(dir);
        //Debug.DrawRay(transform.position, transform.TransformDirection(dir), Color.green);
        //Debug.DrawRay(nav.steeringTarget, Vector3.up * 10, Color.magenta);
        anim.SetFloat("Horz", dir.x);
        anim.SetFloat("Fwd", Mathf.Clamp(dir.z, 0, 1));
    }

    public void grabbing(bool g)
    {
        if(g)
        { anim.SetBool("Grabbing", true); }
        else
        {
            anim.SetBool("Grabbing", false);
        }
    }

    public void grabCheck()
    {
        Collider[] cols = Physics.OverlapCapsule(transform.position, transform.position + Vector3.up, 0.6f); //do this instead
        foreach(Collider c in cols)
        {
            if(c.GetComponent<Player>())
            {
                c.GetComponent<Player>().initGrab(this);
                grabbing(true);
                break;
            }
        }
    }    

    void attack()
    {
        const float distMargin = 1; //distance to be within for initiating attack
        const float rotMargin = 25; //how close the enemy must be looking at player
        if (Vector3.Distance(transform.position, target.transform.position) > distMargin)
        { return; }

        if (Mathf.Abs(Vector3.SignedAngle(transform.forward, target.transform.position - transform.position, Vector3.up)) > rotMargin)
        { return; }

        const float chance = 2;
        if (Random.Range(0, 10) < chance)
        {
            //grab
            anim.SetTrigger("Grab");
        }
        else
        {
            //attack
            anim.SetTrigger("Attack");
        }
    }

    public void attackPlayer()
    {
        target.hurt(dmg);
    }

    public void hurt(Collider col,float dmg)
    {
        health -= dmg;
        Debug.Log("Health: " + health);
        if (health<=0)
        { die(); }
        for(int iter = 0;iter<hurtList.Count;iter++)
        {
            Debug.Log(col.name);
            if(hurtList[iter] == col)
            {
                anim.SetFloat("HurtType", (float)iter / 4);
                //Debug.Log(anim.GetFloat("HurtType"));
            }
        }
        anim.SetTrigger("Hurt"); //test statement
        if (Random.Range(0, 6) < 2)
        {
            anim.SetTrigger("Hurt");
        }
    }
    public void hurt(Vector3 vel, float dmg) //use for melee and explosions
    {
        health -= dmg;
        Debug.Log("Health: " + health);
        if (health <= 0)
        { die(vel); }
        else
        { anim.SetTrigger("Stunned"); }
    }

    void die()
    {
        FindObjectOfType<Director>().addScore(score);
        FindObjectOfType<Director>().upCombo();
        //Destroy(gameObject);
        anim.enabled = false;
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
        }
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());    
    }

    void die(Vector3 vel)
    {
        FindObjectOfType<Director>().addScore(score);
        FindObjectOfType<Director>().upCombo();
        anim.enabled = false;
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
        }
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());
        Destroy(transform.Find("ActionBox").gameObject);

        //add force to ragdoll
        transform.Find("Armature/pelvis").GetComponent<Rigidbody>().AddForce(vel,ForceMode.Impulse);
        transform.Find("Armature/pelvis/torso").GetComponent<Rigidbody>().AddForce(vel, ForceMode.Impulse);
    }
}
