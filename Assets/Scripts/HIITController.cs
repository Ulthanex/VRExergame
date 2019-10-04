using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Enum of the various intensity states of exercise
public enum ExerciseProtocolState{
	trainingLow,
	trainingHigh,
	lowIntensity,
	highIntensity,
	finish
}
	
public class HIITController : MonoBehaviour {

	//Current state of the exercise
    public ExerciseProtocolState currentState;

	//Reference to the Chaser character's controller
    //public astronautController astronautController;
	public chaserController chaseController;

	//Reference to Boss enemy for conqueror condition
	public BossEnemy conquerorBoss;

	//References to the Game manager
    public GameManager gameManager;

	//Tracks motion type from the kinect
    public motionTypeTracker motionTypeTracker;

    //Interprets movement and calculates speed for avatar
	public motionEnhancer motionEnhancer;

    //Reference to the meshCollider that constitues the ground
	public MeshCollider ground;

    //Maximum number of intervals in the exercise
    public float intervalNumber;

    [Header("Training Parameters")] //duration of training exercise
	public bool isTraining = false;
	public float trainingLowDuration;
	public float trainingHighDuration;


    [Header("Low Int Parameters")] //duration of Low Intensity Exercise
    public float lowIntDuration;

    [Header("High Int Parameters")] //duration of high itensity exercise
    public float highIntDuration;

	[Header("GUI Elements")]
	public Text timerText;
	public Text promptText;

    //private vars
    private static HIITController _instance;
	private bool mixedHighFlag = true; //Determines which high intensity phase to activate dependendnt on true/false
    private int currentIntervals = 0; //Counts up what interval in the exercise we are at (low-> High -> low... -> Finish)
	private float countdownTimer; //countsdown the current interval
    private float startCountdown = 5.0f; //Time taken before game ends

    /*----------------------------*/
    // Assigns Singleton instance //
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    /*------------------------*/
    //Singleton Getter Method //
    public static HIITController Instance { get { return _instance; } }

    /*-----------------------------*/
    // Use this for initialization //
    void Start () {
        StartCoroutine(checkTracking());
    }

    /*--------------------------------*/
    // Searches for tracking presence //
    IEnumerator checkTracking()
    {
        while (!motionTypeTracker.Tracking)
        {
            yield return new WaitForSeconds(0.5f);
        }

        //Starts activities dependent on whether we are tracking  or calibrating new values
        if (!calibrationManager.Instance.calibrateNew) {
            StartCoroutine(startGameCountdown());
        }
    }

	/*--------------------------------------------------------------------------------*/
	//Starts a countdown on the interface screen leading to the beginning of the game //
	IEnumerator startGameCountdown()
	{
		float elapsedTime = 0.0f;
		bool counting = true;
		bool preSizeUp = true;
		float timeLeft;
		promptText.text = "Get Ready";

		while (counting)
		{
			if (elapsedTime < startCountdown)
			{
				elapsedTime += Time.deltaTime;
				timeLeft = startCountdown - elapsedTime;
				if (preSizeUp && (timeLeft < 3.1f))
				{
					//timer.fontSize = 100;
					promptText.text = "Get Set";
					preSizeUp = false;
				}
				// timer.text = (timeLeft).ToString("F1");
			}
			else
			{
				//starts game after the countdown, wiping the prompt text after a further 1.5 seconds
				promptText.text = "Go!";
				//  timer.text = "";
				//  timer.fontSize = 60;


				// Enables the player controller component, allowing movement of the character model
				PlayerController.Instance.enabled = true;  

				//starts Either low-intensity for normal exercise, or Training intervals 
				if (isTraining) {
					StartCoroutine (trainingL ()); //Starts training routine
					StartCoroutine (checkIntervalTiming ()); //Keeps track of interval timing
				} else {
					StartCoroutine(lowIntensity());
					StartCoroutine(intervalTiming());
				}


				yield return new WaitForSeconds(1.5f);
				promptText.text = "";
				counting = false;
			}
			yield return null;
		}
	}


	/*----------------*/
	// Interval Timer //
	IEnumerator checkIntervalTiming()
	{
		//While we are still actively running
		while (currentState != ExerciseProtocolState.finish)
		{
			
			//If Countdown Timer is still active
			if (countdownTimer > 0) {
				countdownTimer -= Time.deltaTime; //Decrement timer

				switch (currentState) {

				//High Intensity // -------------------------------------------------------------
				case ExerciseProtocolState.highIntensity:
					if (countdownTimer <= 5) { //5secs left on High Intensity
						if (currentIntervals == intervalNumber) { //Just about to finish
							promptText.text = "";
						} else {
							promptText.text = "Get ready to slow down";
						}
					} else if (countdownTimer > highIntDuration - 5) { //just transitioned
						switch (GameManager.Instance.gameCondition) {

						case personalityType.Conqueror:
							promptText.text = "Chase down the boss for big points!";
							break;

						case personalityType.Survivor:
							promptText.text = "Flee for your life! and your points";
							break;

						}
					} else {
						promptText.text = "";
					}
					break;

				//low intensity // -----------------------------------------------------------------
				case ExerciseProtocolState.lowIntensity:
					if (countdownTimer <= 5) { //5secs left on High Intensity
						if (currentIntervals == intervalNumber) { //Just about to finish
							promptText.text = "";
						} else {
							promptText.text = "Get ready to pick up the pace";
						}
					} else if (countdownTimer >  lowIntDuration - 5) { //Just transitioned
						switch(GameManager.Instance.gameCondition){

						case personalityType.Conqueror:
							promptText.text = "Clear multiple enemies for a score multiplier!";
							break;

						case personalityType.Survivor:
							promptText.text = "Dodge enemies to avoid losing points!";
							break;
						
						}
					} else {
						promptText.text = "";
					}
					break;

				//low intensity Training Mode //----------------------------------------------------- 
				case ExerciseProtocolState.trainingLow:
					if (countdownTimer <= 3) { //5secs left on High Intensity
						promptText.text = "Pick up the pace";
					} else {
						promptText.text = "";
					}
					break;
				

				//High intensity Training Mode //----------------------------------------------------- 
				case ExerciseProtocolState.trainingHigh:
					if (countdownTimer <= 3) { //5secs left on High Intensity
						promptText.text = "Slow the pace down";
					} else {
						promptText.text = "";
					}
					break;
				}


			}
			timerText.text =  countdownTimer.ToString ("0.0");
			yield return null;
		}
		//Exercise has ended
		promptText.text = "Finish!!!";
	}

	/*----------------*/
	// Interval Timer // 
	IEnumerator intervalTiming()
	{
		//While we are still actively running
		while (currentState != ExerciseProtocolState.finish) {

			//If Countdown Timer is still active
			if (countdownTimer > 0) {

				if (currentState == ExerciseProtocolState.lowIntensity && countdownTimer <= 3) {
					promptText.text = "Slow the pace down";
				} else if (currentState == ExerciseProtocolState.highIntensity && countdownTimer <= 3) {
					promptText.text = "Pick up the pace";
				} else if (currentState == ExerciseProtocolState.finish) {
					promptText.text = "Finish";
				} else {
					promptText.text = "";
				}

				countdownTimer -= Time.deltaTime; //Decrement timer
				timerText.text = countdownTimer.ToString ("0.0");
			}
			yield return null;
		}
	}




	/*------------------------------------------------------------*/
	// Training Intervals -- Repeats high intensity/low intensity // ------------------------------------
	/*------------------------------------------------------------*/
   
    /*---------------------------------------*/
    //Co routine for training phase exercise // 
    IEnumerator trainingL()
    {
		//Loops Low and High Intensity for training until stopped overwise

			yield return new WaitForSeconds(1.8f);//wait for transition / clear up of high intensity
			currentState = ExerciseProtocolState.trainingLow; //sets the current exercise state to training
			countdownTimer = trainingLowDuration; //Sets our countdown timer

            //initiate setup for start of training phase -- tells the game manager to create game objects
			GameManager.Instance.transitionTile(); //Transition tiles

			//ceases control of the function for the alloted duration of the training phase
			yield return new WaitForSeconds(trainingLowDuration);

            //finish training phase
			GameManager.Instance.clearGameComponents(); //Clears existing components
            GameManager.Instance.setSpawning(false); //stops spawning objects

			//saves the period of training speed data
			motionEnhancer.saveIntervalSpeed(ExerciseProtocolState.trainingLow);

            //Transitions to high intensity exercise
            StartCoroutine(trainingH());
    }

	/*---------------------------------------*/
	//Co routine for training phase exercise // 
	IEnumerator trainingH()
	{

		//yield return new WaitForSeconds(1.1f);//waiting for field to clear up
		currentState = ExerciseProtocolState.trainingHigh; //Set current exercise state to high intensity
		countdownTimer = trainingHighDuration; //Sets our countdown timer

		//initiate setup for start of high intensity phase
		GameManager.Instance.transitionTile(); //Transition tiles

		//Spawn appropriate high intensity challenge
		switch (GameManager.Instance.gameCondition) {
		case personalityType.Conqueror:
			//Activate the boss lich
			conquerorBoss.transform.position = new Vector3 (0, 0, PlayerController.Instance.transform.position.z + 13f);
			conquerorBoss.gameObject.SetActive (true);
			break;

		case personalityType.Survivor:
			//Activates the chaser to begin following the player
			chaseController.setState(ExerciseProtocolState.highIntensity);
			break;

		case personalityType.Mixed:
			//Activate the lich on first HighIntensity then alternate (true = conq / false = surv)
			if (mixedHighFlag) {
				conquerorBoss.transform.position = new Vector3 (0, 0, PlayerController.Instance.transform.position.z + 13f);
				conquerorBoss.gameObject.SetActive (true);
			} else {
				chaseController.setState(ExerciseProtocolState.highIntensity);
			}
			break;
		}

		//wait for X seconds
		yield return new WaitForSeconds(trainingHighDuration);

		//saves the period of high intensity speed data
		motionEnhancer.saveIntervalSpeed(ExerciseProtocolState.trainingHigh); 

		//finish high intensity phase -----------------------------------------------------------------------
		switch (GameManager.Instance.gameCondition) {
		case personalityType.Conqueror:
			conquerorBoss.HighIntensityEnd (); //Deactivates lich
			break;

		case personalityType.Survivor:
			chaseController.setState(ExerciseProtocolState.lowIntensity); //sets the chaser to low intensity
			break;

		case personalityType.Mixed:
			if (mixedHighFlag) {
				//Deactivates lich
				conquerorBoss.HighIntensityEnd (); 
			} else {
				//sets the chaser to low intensity
				chaseController.setState (ExerciseProtocolState.lowIntensity); 
			}
			//Alternates which high intensity we will have next
			mixedHighFlag = !mixedHighFlag; 
			//If Mixed condition, increment segment
			GameManager.Instance.incrementMixedSegment();
			break;
		}

		//Enables enemy spawning again
		GameManager.Instance.setSpawning(true);

		//Start Low intensity
		StartCoroutine(trainingL());
		
	}


	/*------------------------------------------------------------*/
	// Exercise Intervals -- Repeats high intensity/low intensity // ------------------------------------
	/*------------------------------------------------------------*/


	/*--------------------------------------------*/
	//Co routine for low intensity phase exercise //
    IEnumerator lowIntensity()
    {
		Debug.Log("Low Intensity -- Begin");
        //if the current exercise phase number has not reached the total
        if (intervalNumber > currentIntervals)
        {
            yield return new WaitForSeconds(1.8f);//wait for transition / clear up of high intensity
			currentState = ExerciseProtocolState.lowIntensity; //Set current state to low intensity
            currentIntervals++; //Increment current Exercise intervals
			countdownTimer = lowIntDuration; //Sets our countdown timer
            
			//initiate setup for start of low intensity phase
			GameManager.Instance.transitionTile(); //Transition tiles

				//gameManager.spawnObjectsRandom(lava,(int)Mathf.Ceil((motionEnhancer.calculateBaseRunningSpeed(1 / 53f)) * multiplierLava), probability, true);
	            //gameManager.spawnObjectsRandom(truck, (int)Mathf.Ceil((motionEnhancer.calculateBaseRunningSpeed(1 / 53f)) * multiplierTruck), 1, false);
           
			//wait for 90 seconds
            yield return new WaitForSeconds(lowIntDuration);

			//saves the period of low intensity speed data
			motionEnhancer.saveIntervalSpeed(ExerciseProtocolState.lowIntensity);

            //finish low intensity phase
			GameManager.Instance.clearGameComponents();
			GameManager.Instance.setSpawning(false); //stops spawning objects

            //initiate next exercise phase
            StartCoroutine(highIntensity());
        }
        else
        {
			//Finished exercise
            currentState = ExerciseProtocolState.finish;
			finishCleanup();
        }

    }

	/*--------------------------------------------*/
	//Co routine for High intensity phase exercise //
    IEnumerator highIntensity()
    {
		Debug.Log("High Intensity -- Begin");
        if (intervalNumber > currentIntervals)
        {
            
            //yield return new WaitForSeconds(1.1f);//waiting for field to clear up
			currentState = ExerciseProtocolState.highIntensity; //Set current exercise state to high intensity
			currentIntervals++; //Increase current intervals
			countdownTimer = highIntDuration; //Sets our countdown timer

            //initiate setup for start of high intensity phase
			GameManager.Instance.transitionTile(); //Transition tiles

            	//gameManager.spawnObjectsRandom(truck, (int)Mathf.Ceil((motionEnhancer.calculateBaseRunningSpeed(1 / 53f)) * multiplierTruck), 1,false);

			//Spawn appropriate high intensity challenge
			switch (GameManager.Instance.gameCondition) {
			case personalityType.Conqueror:
				//Activate the boss lich
				conquerorBoss.transform.position = new Vector3 (0, 0, PlayerController.Instance.transform.position.z + 13f);
				conquerorBoss.gameObject.SetActive (true);
				break;

			case personalityType.Survivor:
				//Activates the chaser to begin following the player
				chaseController.setState(ExerciseProtocolState.highIntensity);
				break;

			case personalityType.Mixed:
				//Activate the lich on first HighIntensity then alternate (true = conq / false = surv)
				if (mixedHighFlag) {
					conquerorBoss.transform.position = new Vector3 (0, 0, PlayerController.Instance.transform.position.z + 13f);
					conquerorBoss.gameObject.SetActive (true);
				} else {
					chaseController.setState(ExerciseProtocolState.highIntensity);
				}
				break;
			}
            
			//wait for X seconds
            yield return new WaitForSeconds(highIntDuration);

			//saves the period of high intensity speed data
			motionEnhancer.saveIntervalSpeed(ExerciseProtocolState.highIntensity); 

			//finish high intensity phase -----------------------------------------------------------------------
			switch (GameManager.Instance.gameCondition) {
			case personalityType.Conqueror:
				conquerorBoss.HighIntensityEnd (); //Deactivates lich
				break;

			case personalityType.Survivor:
				chaseController.setState(ExerciseProtocolState.lowIntensity); //sets the chaser to low intensity
                    break;

			case personalityType.Mixed:
				if (mixedHighFlag) {
					//Deactivates lich
					conquerorBoss.HighIntensityEnd (); 
				} else {
					//sets the chaser to low intensity
					chaseController.setState (ExerciseProtocolState.lowIntensity); 
				}
				//Alternates which high intensity we will have next
				mixedHighFlag = !mixedHighFlag; 
				//increment segment
				GameManager.Instance.incrementMixedSegment();
				break;
			}

			//Enables enemy spawning again
			GameManager.Instance.setSpawning(true);
			//Start Low intensity
            StartCoroutine(lowIntensity());
        }
        else
        {
            currentState = ExerciseProtocolState.finish;
			finishCleanup();
        }
	}


	/*-------------------------------------------*/
	//Cleans up after all intervals are complete //
	public void finishCleanup(){
		GameManager.Instance.clearGameComponents(); //Clear existing enemies
		GameManager.Instance.setSpawning(false); //stops spawning objects
		conquerorBoss.gameObject.SetActive(false);//Disable either high intensity element if active
		chaseController.gameObject.SetActive (false);
		GameManager.Instance.endSession (); //end exercise session
	}


}
