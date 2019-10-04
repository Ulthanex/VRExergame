using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bodyTriggerHandler : MonoBehaviour {

    public PlayerController playerController;


    //calls the parent controller passing the trigger information
    void OnTriggerEnter(Collider other)
    {
        playerController.onBodyTrigger(other);
    }
}
