using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lavaBlockManager : MonoBehaviour {

    float maxLength;
    public float scaleFactor;
    public Vector2 moveRate;

    private motionEnhancer motionEnhancer;
    private Material lavaMat;

	// Use this for initialization
	void Start () {
        motionEnhancer = GameObject.Find("Player").GetComponent<motionEnhancer>();
        lavaMat = GetComponent<MeshRenderer>().material;
        //float maxLength = motionEnhancer.jumpFactor * motionEnhancer.jumpForwardScale * scaleFactor;
        maxLength = motionEnhancer.jumpFactor * motionEnhancer.runFactor * scaleFactor;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y,Random.Range(maxLength/2,maxLength));
        lavaMat.mainTextureScale = new Vector2(lavaMat.mainTextureScale.x, transform.localScale.z);
	}
	
	// Update is called once per frame
	void Update () {
        lavaMat.mainTextureOffset += moveRate * Time.deltaTime;
        
    }
}
