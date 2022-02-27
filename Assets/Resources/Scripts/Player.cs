using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody rb;
    Collider col;
    Animator anim;
    Camera cam;
    Gun gun;
    float health = 100;
    float timer = 0;
    enum Status { moving, busy, aiming }
    Status status;
    Ammo[] ammunition = new Ammo[]{new Ammo("pistol", 10),new Ammo("rifle", 10),new Ammo("shotgun", 10), new Ammo("machinegun", 10) };
    float aimPitch;
    public Transform head;
    public bool grabbed = false;
    Enemy grabbedBy;

    public class Ammo
    {
        public enum variant { pistol, rifle, shotgun, machinegun }
        public variant _variant;
        float count;
        List<Gun> guns = new List<Gun>();
        public Ammo(string v,float num)
        {
            switch (v)
            {
                case "pistol":
                    _variant = variant.pistol;
                    break;
                case "rifle":
                    _variant = variant.rifle;
                    break;
                case "shotgun":
                    _variant = variant.shotgun;
                    break;
                case "machinegun":
                    _variant = variant.machinegun;
                    break;
            }

            count = num;
        }

        public void addGun(Gun g)
        {
            guns.Add(g);
        }

        public void removeGun(Gun g)
        {
            guns.Remove(g);
        }

        public void refreshGuns()
        {
            if(guns.Count>0)
            {
                foreach(Gun g in guns)
                {
                    g.pool = count;
                }
            }
        }

        public void refreshCount(Gun g)
        {
            if(guns.Contains(g))
            {
                count = g.pool;
            }
        }

        public void addCount(float num)
        {
            count += num;
        }

        public float GetCount()
        {
            return count;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        cam = Camera.main;
        aimPitch = 0;
        status = Status.moving;
        head = transform.Find("Armature/pelvis/torso/Neck/head").transform;
        gun = GetComponentInChildren<Gun>();
        foreach(Ammo a in ammunition) //tie guns to ammunition pools. call when switching weapon
        {
            if(a._variant.ToString() == gun._variant.ToString())
            {
                a.addGun(gun);
            }
        }
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grabbed)
        { return; }

        foreach(Ammo a in ammunition)
        {
            a.refreshGuns();
        }
        timer += 1 * Time.deltaTime;

        if(Input.GetMouseButton(1))
        {
            if (status != Status.busy)
            {
                status = Status.aiming;
                anim.SetFloat("Fwd", 0);
                aim();
                anim.SetBool("Aim", true);
                if (gun.automatic)
                {
                    if (Input.GetMouseButton(0))
                    {
                        shoot();
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        shoot();
                    }
                }
            }

        }
        else if(Input.GetMouseButtonUp(1))
        {
            {
                if(status == Status.aiming)
                {
                    status = Status.moving;
                    aimPitch = 0;
                    anim.SetFloat("AimVert", aimPitch);
                    anim.SetBool("Aim", false);
                }
            }            
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(gun.clip < gun.clipSize)
            { anim.SetTrigger("Reload"); }            
        }
        if (status == Status.moving)
        { move(false); }
        else
        { move(true); }
        if(status!=Status.busy)
        { rotate(); }
    }

    private void FixedUpdate()
    {
        camStick();
    }

    public void reload()
    {
        gun.reload();
        foreach(Ammo a in ammunition)
        {
            a.refreshCount(gun);
        }
        anim.ResetTrigger("Reload");
    }

    void move(bool reset)
    {
        if(reset)
        {
            anim.SetFloat("Fwd", 0);
            return;
        }
        float vel = Input.GetAxis("Vertical");
        if(Input.GetKey(KeyCode.LeftShift) && vel >0.1f)
        {
            anim.SetBool("Run",true);
        }
        else
        {
            anim.SetBool("Run", false);
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) && vel<-0.1f)
        {
            anim.SetTrigger("QuickTurn");
            status = Status.busy;
        }

        anim.SetFloat("Fwd", vel);
    }

    public void setStatus(int choice)
    {
        switch(choice)
        {
            case 0:
                status = Status.moving;
                resetAllTriggers();
                break;
            case 1:
                status = Status.aiming;
                break;
            case 2:
                status = Status.busy;
                anim.SetFloat("Fwd", 0);
                anim.SetFloat("Horz", 0);
                break;
        }
    }

    void aim()
    {
        float mod = 1f; //sensitivity
        aimPitch += (Input.GetAxis("Vertical") * Time.deltaTime) * mod;
        aimPitch = Mathf.Clamp(aimPitch, -1, 1);
        anim.SetFloat("AimVert", aimPitch);
    }

    void rotate()
    {
        anim.SetFloat("Horz", Input.GetAxis("Horizontal"));
    }

    void camStick()
    {
        const float camSpeed = 2.8f;        
        Quaternion rot = Quaternion.Euler(new Vector3(Vector3.SignedAngle(transform.forward,head.transform.forward,transform.right), 0, 0));
        Quaternion rotOffset = Quaternion.Euler(6 + rot.eulerAngles.x, 0, 0);
        //Debug.Log(rot.eulerAngles);
        Vector3 newPos = Vector3.zero;
        Vector3 offset = new Vector3(0.6f, 0.1f, -1.3f);
        RaycastHit hit;
        Ray ray = new Ray(head.position, transform.rotation * rot * offset);
        LayerMask lm = LayerMask.GetMask("Terrain");
        if(Physics.Raycast(ray,out hit,offset.magnitude,lm))
        {
            newPos = hit.point;
        }
        else
        {
            newPos = ray.origin + ray.direction * offset.magnitude;
        }
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation,transform.rotation * rotOffset,camSpeed * Time.deltaTime);
        cam.transform.position = Vector3.Lerp(cam.transform.position,newPos,camSpeed * Time.deltaTime);
        Debug.DrawRay(ray.origin, ray.direction, Color.red);
    }

    void shoot()
    {
        if (timer >= gun.fireRate && gun.clip > 0)
        {
            //Debug.Log("Shooting");
            timer = 0;
            gun.clip--; //subtract bullet from clip
            LayerMask lm = LayerMask.GetMask("HurtColliders");
            Ray ray = new Ray(gun.transform.GetChild(0).TransformPoint(gun.GetComponentInChildren<LineRenderer>().GetPosition(0)), gun.transform.GetChild(0).TransformPoint(gun.GetComponentInChildren<LineRenderer>().GetPosition(1)) - gun.transform.GetChild(0).TransformPoint(gun.GetComponentInChildren<LineRenderer>().GetPosition(0)));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.GetComponent<Enemy>())
                {
                    Physics.Raycast(ray, out hit, 100, lm);
                    if (hit.transform != null)
                    {
                        hit.transform.GetComponentInParent<Enemy>().hurt(hit.collider, gun.dmg);
                        //Debug.Log("Shot: " + hit.collider.name);
                    }
                }
            }
        }
    }

    public void melee() //always fires twice so change dmg value accordingly
    {
        const int meleeDmg = 25;
        const float meleeForce = 25;
        Ray ray = new Ray(transform.Find("Armature/pelvis").position, transform.forward * 5f);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit))
        {
            if(hit.transform.GetComponent<Enemy>())
            {
                hit.transform.GetComponent<Enemy>().hurt((ray.direction + Vector3.up).normalized * meleeForce, meleeDmg);
            }
            else if (hit.transform.GetComponent<Object>())
            {
               if(hit.transform.GetComponent<Object>().shootable)
               {
                    hit.transform.GetComponent<Object>().Action();
               }
            }
        }
    }

    public void initGrab(Enemy e)
    {
        grabbed = true; //character is in middle of getting grabbed
        anim.SetBool("Grabbed",grabbed); //start grabbed animation
        anim.SetFloat("Fwd", 0); //stop moving
        anim.SetFloat("Horz", 0); //stop turning
        grabbedBy = e; //make sure there is reference to enemy
        StartCoroutine(wiggleCheck()); //start checking for wiggles
    }

    IEnumerator wiggleCheck()
    {
        int wiggleCounter = 0;
        int wiggleGoal = 10;
        if (health < 90)
        { wiggleGoal = (int)(100 - health) / 3; }
        int dir = 1;
        while(wiggleCounter < wiggleGoal)
        {
            if(Input.GetAxisRaw("Horizontal")==dir) //if analog matches dir flip dir
            {
                dir *= -1;
                wiggleCounter++;
            }

            if(wiggleCounter>=wiggleGoal) //break wiggle
            {
                grabbed = false; //no longer grabbed
                anim.SetBool("Grabbed", grabbed); //stop grabbed animation
                grabbedBy.grabbing(false); //tell enemy to stop grabbing                
            }
            hurt(2 * Time.deltaTime);
            yield return null;
        }
    }

    public void hurt(float pass)
    {
        health -= pass;
        if(health <=0)
        {
            die();
        }        
    }

    public void setBusy(int busy)
    {
        //Debug.Log("Changing");
        if(busy == 1)
        {
            status = Status.busy;
        }
        else
        {
            status = Status.moving;
        }
    }

    void die()
    {

    }



    public float getHealth()
    {
        return health;
    }

    public void pickup()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide);
        foreach (Collider c in cols)
        {
            if (c.GetComponentInChildren<AmmoPickup>())
            { c.GetComponentInChildren<AmmoPickup>().Action(); }
        }
    }

    public void addAmmo(float num, string v)
    {
        foreach(Ammo a in ammunition)
        {
            if(a._variant.ToString() == v)
            {
                a.addCount(num);
            }
        }
    }

    public Gun getGun()
    {
        return gun;
    }

    public void callAnim(string a)
    {
        switch(a)
        {
            case "door":
                anim.SetTrigger("DoorOpen");
                break;
            case "pickup":
                anim.SetTrigger("PickUp");
                break;
            case "melee":
                anim.SetTrigger("Melee");
                break;
        }
    }

    public void callAnim(string a, Transform t)
    {
        Vector3 vec = t.position;
        vec.y = transform.position.y;
        transform.rotation = t.rotation;
        switch (a)
        {
            case "door":
                anim.SetTrigger("DoorOpen");
                if(t.localRotation.eulerAngles.y > 5 || t.localRotation.eulerAngles.y < -5)
                { t.GetComponentInParent<Animator>().SetTrigger("Clockwise"); }
                else 
                { t.GetComponentInParent<Animator>().SetTrigger("CounterClock"); }
                
                transform.position = vec;
                break;
            case "pickup":
                anim.SetTrigger("PickUp");
                transform.position = vec;
                break;
            case "melee":
                anim.SetTrigger("Melee");
                transform.position = vec;
                break;
        }
    }

    void resetAllTriggers()
    {
        foreach(AnimatorControllerParameter p in anim.parameters)
        {
            if(p.type == AnimatorControllerParameterType.Trigger)
            {
                anim.ResetTrigger(p.name);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        const float lookMargin = 45;
        //if character isnt looking at action then quit function
        Vector3 dirA = transform.forward;
        Vector3 dirB = other.transform.position - transform.position;
        dirB.y = dirA.y;
        //Debug.Log(Vector3.SignedAngle(dirA, dirB, Vector3.up));
        //Debug.DrawRay(dirA, dirB * 10,Color.red);
        if (Mathf.Abs(Vector3.SignedAngle(dirA,dirB,Vector3.up))>lookMargin)
        { return; }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (other.GetComponentInParent<Enemy>())
            {
                callAnim("melee");
            }
            else if(other.GetComponentInParent<Object>())
            {
                //check for triggerpoints on action object
                List<Transform> points = new List<Transform>();                
                foreach(Transform child in other.transform.parent)
                {
                    if(child.CompareTag("TriggerPoint"))
                    {
                        points.Add(child);                       
                    }
                }
                if(points.Count>0)
                {
                    Transform closest = points[0];
                    foreach (Transform c in points)
                    {
                        float dist = Vector3.Distance(transform.position, c.position);
                        float current = Vector3.Distance(transform.position, closest.position);
                        if(dist<current)
                        {
                            closest = c;
                        }
                    }
                    callAnim(other.GetComponentInParent<Object>().getAnimString(),closest);
                }
                else
                {
                    callAnim(other.GetComponentInParent<Object>().getAnimString());
                }
            }
        }
    }
}
