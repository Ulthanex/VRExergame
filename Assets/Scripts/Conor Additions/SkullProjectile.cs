using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullProjectile : MonoBehaviour {

	//The end point for our projectile
	public float projectileSpeed;
	//The Time until the object destroys its self
	public float decayTime;
	private float decayProgress = 0;
	//Whether the projectile has exploded
	private bool exploded = false;
	//Gameobject of the skull
	public GameObject skull;
	public GameObject fireTrail;
	public GameObject explosion;
	//our rigidbody
	private Rigidbody r;

	/*--------------*/
	// Start Method //
	void Start(){
		StartCoroutine (destroySelf ());
	}

	/*-------------------------------------------------------------------------------------------------*/
	// Coroutine that destroys ones self after a set duration if we have have not collided before hand //
	IEnumerator destroySelf(){
		yield return new WaitForSeconds(decayTime);
		if(!exploded){Destroy (this.gameObject);}
	}
		

	/*--------------------------------------------------*/
	//Launches the projectile in the supplied direction //
	public void launchProjectile(Vector3 direction){
		Vector3 projDirection = (direction - transform.position).normalized;
		this.GetComponent<Rigidbody> ().AddForce (projDirection * projectileSpeed);
	}

	/*-------------------------------------------------------*/
	// Enables explosion animation and cleans up after delay //
	IEnumerator explode()
	{
		explosion.SetActive(true);
		fireTrail.SetActive(false);
		skull.SetActive(false);
		yield return new WaitForSeconds (2f);
		Destroy (this.gameObject);
	}

	/*-------------------------------------------------------------------------------*/
	// Detect Collisions with our collider, destroys object hitting ground or player //
	void OnTriggerEnter(Collider col){

		//If Player enters hitbox trigger -- transition to dieing
		if(col.tag == "Player" || col.tag =="Ground" || col.tag == "RightBox" || col.tag == "LeftBox") {
			exploded = true;
			this.GetComponent<Rigidbody> ().velocity = Vector3.zero;
			StartCoroutine (explode ());
		}
	}

}
