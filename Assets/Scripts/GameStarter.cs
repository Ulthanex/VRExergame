using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStarter : MonoBehaviour {
    [SerializeField]
    GameObject VCamera, player, playerModel, standinModel, vehicles;
    
    [SerializeField]
    Text timer, prompt;

    //[SerializeField]
    //ExerciseProtocol EP;
    
	//[SerializeField]
	//GameObject ghost;

    [SerializeField]
    private float rotationTime = 3.0f, panTime = 3.0f, countdown = 5.0f;
    private float rotationSpeed;
    private float totalRotation = 0.0f;
    private float rotationStep;

	/*-----------------------------*/
	// Use this for initialization //
	void Start () {
        Time.timeScale = 1.0f;
        rotationSpeed = 360.0f / rotationTime;
        //StartCoroutine(rotateCamera());
        //playerModel.active = false;
        startGame();
    }

	/*---------------------------------------------------------------------------------------------*/
	//Rotates the camera around the player 720 degrees over the course of set total rotation speed //
    IEnumerator rotateCamera()
    {
        prompt.text = "This is you.";
        bool rotating = true;

        while (rotating)
        {
            rotationStep = rotationSpeed * Time.deltaTime;
            if (rotationStep > 720.0f - totalRotation)
            {
                rotationStep = 360.0f - totalRotation;
                totalRotation = 0.0f;
                rotating = false;
                VCamera.transform.RotateAround(player.transform.position, Vector3.up, rotationStep);
                prompt.text = "";
				//ends the camera rotation by starting a camera movement sub routine
                //yield return StartCoroutine(moveCamera(player.transform.position + (Vector3.up * .195f) + (player.transform.forward * 0.1f), player.transform.rotation,  panTime)); 
				yield return StartCoroutine(startCountdown());
			}
            else
            {
                totalRotation += rotationStep;
            }
            VCamera.transform.RotateAround(player.transform.position, Vector3.up, rotationStep);
            yield return null;
        }
    }

	/*----------------------------------------------------------------------*/
	//Moves the camera to a set position within the Player models transform //
    IEnumerator moveCamera(Vector3 endPos, Quaternion endRot, float time)
    {
        Vector3 startingPos = VCamera.transform.position;
        Quaternion startingRot = VCamera.transform.rotation;
        float elapsedTime = 0.0f;
        bool panning = true;

        while (panning)
        {
            if (elapsedTime < time)
            {
                elapsedTime += Time.deltaTime;
                VCamera.transform.position = Vector3.Lerp(startingPos, endPos, (elapsedTime / time));
                VCamera.transform.rotation = Quaternion.Lerp(startingRot, endRot, (elapsedTime / time));
                yield return null;
            }
            else
            {
                panning = false;
				//Once panned, a countdown routine is initiated
                yield return StartCoroutine(startCountdown());
            }
        }
    }

	/*--------------------------------------------------------------------------------*/
	//Starts a countdown on the interface screen leading to the beginning of the game //
    IEnumerator startCountdown()
    {
        //hides the standin model for the player
		standinModel.SetActive(false);
		//Turns the player model visible
        playerModel.SetActive(true);
        float elapsedTime = 0.0f;
        bool counting = true;
        bool preSizeUp = true;
        float timeLeft;
        prompt.text = "Get Ready";

        while (counting)
        {
            if(elapsedTime < countdown)
            {
                elapsedTime += Time.deltaTime;
                timeLeft = countdown - elapsedTime;
                if(preSizeUp && (timeLeft < 3.1f)){
                    timer.fontSize = 100;
                    prompt.text = "Get Set";
                    preSizeUp = false;
                }
                timer.text = (timeLeft).ToString("F1");
            }
            else
            {
                prompt.text = "Go!";
                timer.text = "";
                timer.fontSize = 60;
				//starts game after the countdown, wiping the prompt text after a further 1.5 seconds
                startGame();
                yield return new WaitForSeconds(1.5f);
                prompt.text = "";
                counting = false;
            }
            yield return null;
        }
    }

    
	/*-----------------------------------------------------------*/
	//Enables Movement of the player and activates game elements //
    void startGame()
    {

        //Enable protocols and player/ghost
        //EP.enabled = true;

		//Enables the player controller component, allowing movement of the character model
        player.GetComponent<PlayerController>().enabled = true;

		//Activate Ghost (FeedFoward)
        //ghost.SetActive(true);
    }
}
