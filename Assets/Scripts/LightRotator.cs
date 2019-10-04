using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRotator : MonoBehaviour {

    [SerializeField]
    private float rotateRate;

    [SerializeField]
    Transform light1, light2;
	
	// Update is called once per frame
	void Update () {
        light1.Rotate(Vector3.up * rotateRate * Time.deltaTime);
        light2.Rotate(Vector3.up * rotateRate * Time.deltaTime);
    }
}
