using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyController : MonoBehaviour
{

    public GameObject follow;
    public GameObject neck;
    public Vector3 offset;
    public Vector3 neckOffset;

    public bool useNeck;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (useNeck)
        {
            //this.transform.position = head.transform.position+(head.transform.position-neck.transform.position) ;

            //factors dicplacement of neck from head into account
            this.transform.position = this.transform.position + (follow.transform.position - neck.transform.position) + neckOffset;
        }
        else
        {
            this.transform.position = follow.transform.position + offset;
        }
        
    }

    
}
