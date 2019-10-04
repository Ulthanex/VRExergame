using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firetrap : MonoBehaviour {

	public ParticleSystem fire;
	private ParticleSystem.EmissionModule em;
	public BoxCollider hitBox;
	public float activeDuration = 5f;
	public float inactiveDuration = 3f;

	//Initialization
	void Start () {
		em = fire.emission;
		fire.Play();
		StartCoroutine (activateTrap());
	}

	/*----------------------------*/
	// Activates particle emission//
	IEnumerator activateTrap()
	{
		em.enabled = true;
		hitBox.enabled = true;
		yield return new WaitForSeconds(activeDuration);
		StartCoroutine (deactivateTrap ());
	}

	/*------------------------------*/
	// Deactivates particle emission//
	IEnumerator deactivateTrap()
	{
		em.enabled = false;
		hitBox.enabled = false;
		yield return new WaitForSeconds(activeDuration);
		StartCoroutine (activateTrap ());
	}

}
