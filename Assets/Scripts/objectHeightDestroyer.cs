using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectHeightDestroyer : MonoBehaviour {

    public float heightThreshold;

	// Use this for initialization
	void Start () {
        StartCoroutine(heightCheck());
	}


    IEnumerator heightCheck()
    {
        while (true)
        {
            if (transform.position.y < heightThreshold)
            {
                Destroy(gameObject);
                break;
            } else {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
