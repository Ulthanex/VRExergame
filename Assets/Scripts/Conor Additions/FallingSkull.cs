using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSkull : MonoBehaviour {

	[Header("Particle Parameters")]
	public ParticleSystem drippingFire;
	private ParticleSystem.EmissionModule em;
	public int fireRate = 1;
	public int maxFireRate = 4;
	[Header("Object Parameters")]
	public GameObject skull;
	//public float fallDuration = 5;

	//private float progress = 0;
	//private Vector3 startPosition;
	//private Vector3 endPosition = Vector3.zero;

	/*----------------*/
	// initialization //
	void Start () {
		//Gets Emission module of particle system
		em = drippingFire.emission;
		//Starts Coroutine to initate dropping of skull
		StartCoroutine (initiateDrop ());
	}

	/*-------------------------------------------------------------------------------*/
 	// Increases particle emission until duration is met, then enables skull to drop //
	IEnumerator initiateDrop()
	{
		do {
			em.rateOverTime = fireRate; //Sets emission of particle effect
			fireRate++; //Increases rate of particle emission for next yield
			//Debug.Log("Fire rate = " + fireRate + " : FireDuration = " + maxFireRate);
			yield return new WaitForSeconds (2f); //waits for 2seconds +- milliseconds for next frame
		}while (fireRate <= maxFireRate); //repeat until firerate exceeds 
		em.enabled = false; //disables fire dripping effect
		skull.SetActive (true); //enables skull to begin falling
	}

	/*-----------------------------------------------*/
	// Deletes gameobject after successful animation //
	public void cleanUp(){
		Destroy (this.gameObject);
	}

	/*---------------------------------------------------------------------------------*/
	// Detect Collisions with explosion collider -- Decrements player score            //
	void OnTriggerEnter(Collider col){

		//If Player enters hitbox trigger -- transition to recoil
		if (col.tag == "Player") {
		
			//Decrement Player Score -- To Be done
			Debug.Log("PLAYER SCORE DOWN");
		}
	}
			
}
