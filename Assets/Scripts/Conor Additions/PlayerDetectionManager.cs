using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetectionManager : MonoBehaviour {
	public HostileEnemy parentScript;

	/*------------------------------------------*/
	// Detect Collision with our hitbox trigger //
	void OnTriggerEnter(Collider other){
		//If Player enters hitbox trigger -- transition to attack
		if(other.tag == "Player") {
			parentScript.playerProximityEnter ();
		}
	}

	/*-----------------------*/
	// Detect Collision exit //
	void OnTriggerExit(Collider other){
		if(other.tag == "Player") {
			parentScript.playerProximityExit ();
		}
	}


}
