using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class opacityController : MonoBehaviour {

    public float lowerAlphaBound;
    public float upperAlphaBound;
    public Transform avatarBody;

    private MeshRenderer meshRenderer;

	// Use this for initialization
	void Start () {
        meshRenderer = GetComponent<MeshRenderer>();

    }
	
	// Update is called once per frame
	void Update () {
        //finding a value
        float updatedA = Mathf.InverseLerp(lowerAlphaBound, upperAlphaBound, Mathf.Abs(avatarBody.InverseTransformPoint(transform.position).z));
        //setting the a value
        meshRenderer.material.color = new Color(1,0,0, updatedA);
        //setting arrow x position to be the same as the player
        transform.position = new Vector3(avatarBody.position.x, transform.position.y, transform.position.z);
    }
}
