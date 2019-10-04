using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullCollision : MonoBehaviour {

	//Prefab Explosion Effect
	public GameObject explosion;
	public GameObject flame;
	public MeshRenderer skull;
	public CapsuleCollider hitBox;
	public FallingSkull parentScript;

	/*-----------------------------------------*/
	// When object collides with floor collider//
	void OnCollisionEnter(Collision collision){
		StartCoroutine (explode ());
	}

	/*-------------------------------------------------------*/
	// Enables explosion animation and cleans up after delay //
	IEnumerator explode()
	{
		explosion.SetActive (true);
		flame.SetActive (false);
		skull.enabled = false;
		hitBox.enabled = true;
		yield return new WaitForSeconds (2f);
		//Tells parent to begin Cleanup
		parentScript.cleanUp ();
	}

}
