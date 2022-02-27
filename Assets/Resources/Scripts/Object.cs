using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    public bool explodable = true;
    public bool shootable = true;
   

    public virtual string getAnimString()
    {
        return "";
    }

    public virtual void Action()
    {

    }
}
