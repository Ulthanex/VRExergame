using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;
using System.IO;

//TODO: PLAY WITH KNEE BEND TO DISCERN BETWEEN RUNNING AND JUMPING BETTER
public class motionEnhancer : MonoBehaviour
{
    public bool jumpShake;
    public List<GameObject> performanceBars;
    public GameObject promptPanel;

    public motionTypeTracker motionTypeTracker;
    private motionTypeTracker.LocomotionState currentState; //Current locomotion state
    private motionTypeTracker.LocomotionState previousState; //previous locomotion state

    public float runningMotionSmoothing;
    public float jumpFactor;
    public float runFactor;

    public headRotationAdjuster headOffsetCalculator;
    public Transform avatarHead;
    public Transform avatarWaist;
    public Transform avatarKneeLeft;
    public Transform avatarKneeRight;

    private float initialAvatarHeight;
    private float previousKneeWaistDistance;
    private float previousKneeWaistDisplacement;
    private float previousHeadHeight;
    private enum FeetState
    {
        bothDown,
        leftUpRightDown,
        rightUpLeftDown,
        bothUp
    };
    private FeetState previousFeetState;
    public float feetAlternationReward;
    public float feetAlternationDecayFactor;
    public float feetAlternationCap;
    private float feetAlternationFactor;

    private Vector3 movementVector;
    private Vector3 savedMovementParameters;
    public float jumpingForwardDecay;
    public float feetAlternationCapAfterJump;

    private KinectManager kinectManager;

    public List<GameObject> movementParamPanels;

    //run speed measures: 1) knee transform relative to waist (not the same thing as knee relative to head because of different update rates)
    //                    2) head displacement?
    //                    3) feet alternation perhaps combined with leg movement speed


    /*-------------------*/
    // Physical measures //---------------------------------------------------
    /*-------------------*/
    public struct intervalSpeeds
    {
        public ExerciseProtocolState interval;
        public float averageSpeed;
		public float averageLateralSpeed;
        public List<float> Speeds;
		public List<float> lateralSpeeds;
    }
    private List<float> currentIntervalSpeeds = new List<float>(); //Measures the current snapshot of speed values taken for said intensity interval
    private List<float> currentIntervalLateralSpeeds = new List<float>(); //Measures the current snapshot of speed values taken for said intensity interval
    public List<intervalSpeeds> sessionIntervalSpeeds = new List<intervalSpeeds>(); //Combines all exercise intervals together


    //TODO: CHECK NORMALISED PARAMETERS
    //      IMPLEMENT AVERAGING

	/*----------------------------*/
    // Use this for initialization//
    void Start()
    {
        movementVector = transform.position; //Sets initial movement vector to match player transform position
        initialAvatarHeight = transform.position.y; //Sets initial avatar height
        previousKneeWaistDistance = Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeLeft.position).y) + Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeRight.position).y);
        previousKneeWaistDisplacement = 0;
        previousMeasuredKneeWaistDistance = previousKneeWaistDistance;
        previousHeadHeight = headOffsetCalculator.getOffset() +transform.InverseTransformPoint( avatarHead.position).y;
        previousFeetState = FeetState.leftUpRightDown;//TODO:CHANGE TO SOMETHING BETTER
        kinectManager = KinectManager.Instance; //sets kinect reference
        savedMovementParameters = Vector3.zero; 
        StartCoroutine(speedCalculator());
        StartCoroutine(runningParameterCalculator());
    }

    public float jumpSmoothFactor;
    public float fadeInTime;
    public float fadeOutTime;
    public float magnitude;
    public float roughness;

	/*---------------------------------*/
    // Update is called once per frame //
    void Update()
    {
        //float headHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.position).y;
        if (motionTypeTracker.Tracking)
        {
            
			//Update Speed Meters arrow rotation -- used to indicate player speed
			movementParamPanels[2].GetComponent<RectTransform>().rotation = Quaternion.Lerp(movementParamPanels[2].GetComponent<RectTransform>().rotation, speedArrowRotation, 0.1f);
           
			updateMovementVector();
            //transform.position += movementVector * Time.deltaTime;
            
            //if (currentState == motionTypeTracker.LocomotionState.Jumping)
            //{
            //use the raw y value since we want no delays in the avatar jumping motion
            Vector3 interpolatedNextPosition = Vector3.Lerp(transform.position, movementVector, runningMotionSmoothing);
            //interpolation factor needs to be larger as the head approaches its initial position
           
            float interpolatedHeight = Mathf.Lerp(transform.position.y, movementVector.y, jumpSmoothFactor);
            
            //if the avatar gets too low (note we allow the negative velocity to maintain a linear vertical movement, and stop it only when our actual position is at the lower limits)
            if (interpolatedHeight<initialAvatarHeight)
            {
                interpolatedHeight = initialAvatarHeight;
                movementVector = new Vector3(movementVector.x, initialAvatarHeight, movementVector.z);//resetting y velocity
                transform.position = new Vector3(interpolatedNextPosition.x, interpolatedHeight, interpolatedNextPosition.z);//same as the one in other conditional, since we want to adjust position before shaking
                if (jumpShake)
                {
                    CameraShaker.Instance.ShakeOnce(magnitude, roughness, fadeInTime, fadeOutTime);
                }
                
            }
            else
            {
                transform.position = new Vector3(interpolatedNextPosition.x, interpolatedHeight, interpolatedNextPosition.z);
            }
            
            //}
            //else
            //{
            //    transform.position = Vector3.Lerp(transform.position, movementVector, runningMotionSmoothing);
            //}

            //Debug.Log(movementVector);
        }
    }

    float baseJumpHeight;
    float currentHeight;

    float currentVelocity;
    float previousVelocity;
    float finalVelocity;

    float retainedVelocity;
    float jumpForwardVelocity;

    public float decayAtPrejump;
    public float retainedDecay;

    public float previousVelocityWeight;
    public float previousKneeHeightWeight;
    public float w1;
    public float w2;
    public float w3;
    public float w4;

    public float jumpForwardScale;
    public float jumpEnhancement;

    public float maxKneeBend = 0.51f;

    private Vector3 lastRecordedPosition;
    private Quaternion speedArrowRotation;
    public float avatarSpeed;
    public float avatarLateralSpeed;


	/*-------------------------*/
	// Determines avatar speed //
    private IEnumerator speedCalculator()
    {
        while (true)
        {
            if (motionTypeTracker.Tracking)
            {
                //Determines speed from relative change from last recorded z-pos to newest
				avatarSpeed = (transform.position - lastRecordedPosition).z*10;
                avatarLateralSpeed = (transform.position - lastRecordedPosition).x * 10;
                //Saves values to current exercise interval holder values
                currentIntervalSpeeds.Add(avatarSpeed);
                currentIntervalLateralSpeeds.Add(avatarLateralSpeed);

                //Updates rotation of Speed Meter arrow
                speedArrowRotation = Quaternion.Euler(Vector3.forward * Mathf.Lerp(104, -98, Mathf.InverseLerp(0, calculateBaseRunningSpeed(0.4f), avatarSpeed)));
                //Updates Speed Meter 
                movementParamPanels[3].GetComponent<Text>().text = ((int)avatarSpeed).ToString();
                lastRecordedPosition = transform.position;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

	/*----------------------------------------------------------------------------------------------------------*/
	//Saves the current snapshot of Speed values (Forward/Lateral) appending it with the current exercise state // 
	public void saveIntervalSpeed(ExerciseProtocolState state){
		float avgSpeed = 0;
		float avgLatSpeed = 0;
		intervalSpeeds snapshot;
		int i = 0;

		//Average the speed values for current interval
		for (i = 0; i < currentIntervalSpeeds.Count; i++) {
			avgSpeed += currentIntervalSpeeds [i];
		}
		avgSpeed /= currentIntervalSpeeds.Count;

		//Average the lateral speed values
		for (i = 0; i < currentIntervalLateralSpeeds.Count; i++) {
			avgLatSpeed += currentIntervalLateralSpeeds [i];
		}
		avgLatSpeed /= currentIntervalLateralSpeeds.Count;

		//Save values to a snapshot
		snapshot.interval = state;
		snapshot.averageSpeed = avgSpeed;
		snapshot.averageLateralSpeed = avgLatSpeed;
		snapshot.Speeds = currentIntervalSpeeds;
		snapshot.lateralSpeeds = currentIntervalLateralSpeeds;
		sessionIntervalSpeeds.Add (snapshot);

		//reset collected speed lists
		currentIntervalSpeeds.Clear();
		currentIntervalLateralSpeeds.Clear();
	}


    float kneeWaistDistanceNormalised;
    float kneeWaistDisplacementNormalised;
    float headDisplacementNormalised;


	/*--------------------*/
	// Running Calculator //
    private IEnumerator runningParameterCalculator()
    {
        while (true)
        {
            if (motionTypeTracker.Tracking)
            {
                //dealing with forward motion
                //factors that need constant update
                //knee factors
                float kneeWaistDistance = Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeLeft.position).y) + Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeRight.position).y);
                float finalKneeWaistDistance = previousKneeHeightWeight * previousKneeWaistDistance + (1 - previousKneeHeightWeight) * kneeWaistDistance;//introducing a gradual change in knee height to make the transition in speed smoother (using the same weighting as velocity smoothing)
                                                                                                                                                         //uses range of max to min avatar distance (should be normalised based on user params)
                kneeWaistDistanceNormalised = Mathf.InverseLerp(maxKneeBend * 2, 0, finalKneeWaistDistance);//NEW IMPORTANT PARAMETER - 0.51 is the distance of the avatar's knee to waist in t-pose, hence max distance. The users distance is mapped to the avatar via the ms-sdk plugin, hence we are able to use these values. In future work could change this to extract directly from the users actual values.

                float kneeWaistDisplacement = Mathf.Abs(kneeWaistDistance - previousKneeWaistDistance);
                float finalKneeWaistDisplacement = previousKneeHeightWeight * previousKneeWaistDisplacement + (1 - previousKneeHeightWeight) * kneeWaistDisplacement;//knee movement smoothing
                //uses range of max disposition to min disposition
                kneeWaistDisplacementNormalised = Mathf.InverseLerp(motionTypeTracker.kneeWaistDispositionRunning.x, motionTypeTracker.kneeWaistDispositionRunning.z, kneeWaistDisplacement);
                previousKneeWaistDistance = kneeWaistDistance;
                previousKneeWaistDisplacement = kneeWaistDisplacement;
                //movementParamPanels[1].GetComponent<Text>().text = "Knee Waist: " + kneeWaistDisplacementNormalised;
                performanceBars[1].GetComponent<Slider>().value = kneeWaistDisplacementNormalised;

                //head factors
                float headHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.position).y;
                float headDisplacement = Mathf.Abs(headHeight - previousHeadHeight);
                //uses range of max displacement to min displacement
                headDisplacementNormalised = Mathf.InverseLerp(motionTypeTracker.headDispositionRunning.x, motionTypeTracker.headDispositionRunning.z, headDisplacement);//can add some addition to the max value and weigh the overall value higher so that it reaches values above 1 with higher than max disposition
                previousHeadHeight = headHeight;
                //movementParamPanels[0].GetComponent<Text>().text = "Head Displacement: " + headDisplacementNormalised;
                performanceBars[0].GetComponent<Slider>().value = headDisplacementNormalised;

            }
            yield return new WaitForSeconds(1 / 50f);//calibrator capture rate
        }
       
    }

    private float headHeightBeforeJump;
    //private float forwardVelocity;


	/*----------------------------------------------------------------------------------------------*/
	//Updates the current motion vector of the player dependent on locomotion state & prior factors //
    private void updateMovementVector()
    {
        recordUserParams();
        
		//dealing with lateral motion -- Left & Right (made adjustments to fit them in the 
		float xOffset = kinectManager.GetUserPosition(kinectManager.GetPrimaryUserID()).x; //the offset of the player from the center of the kinect camera
		float xPosition = Mathf.Lerp(-2.4f, 2.4f, Mathf.InverseLerp(-1f, 1f, xOffset)); //Inverse lerp to find ratio position of kinect to lerp and find x position 

		//Sets new Vector 3 position to match new x position and existing y/z
		movementVector = new Vector3(xPosition, movementVector.y, movementVector.z);


        //Find Users current locomotion state
        previousState = currentState;
        currentState = motionTypeTracker.getCurrentState();
        if (previousState == null)
        {
            previousState = currentState;
        }


		//Determine actions based on transition of states 
		switch (currentState) {

		case motionTypeTracker.LocomotionState.PreJump: //Winding up to jump ------------------------------------------------------
			//TODO: FIND MAGNITUDES AND RATES OF INCREASE FOR EACH FACTOR AND NORMALISE

			//Determines headheight by adding head offset to locally converted avatar head y position
			headHeightBeforeJump = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
			float decay = decayAtPrejump;

			//If previous state was Jumping
			if(previousState == motionTypeTracker.LocomotionState.Jumping)
			{

				//implicitly we continue moving forward with our jumping forward velocity
				decay += jumpEnhancement;//lessen the decay effect to recover from running pose
				retainedVelocity = finalVelocity;//retain our jumping velocity
				//retainedVelocity = 0;//after a jump we need to gather some velocity by running to jump ahead again

			}
			//If previous state was running
			else if(previousState== motionTypeTracker.LocomotionState.Running)
			{
				
				//implicitly we continue moving forward with our final velocity
				retainedVelocity = finalVelocity;
			}
				
			finalVelocity *= decay; //Offsets our final velocity by decay factor
			retainedVelocity *= retainedDecay;//old jump

			//Adjusts final movement vector by pre-jump locomotion velocity
			movementVector = new Vector3(movementVector.x, movementVector.y, movementVector.z + finalVelocity);
			break;


		case motionTypeTracker.LocomotionState.Running: //Running ---------------------------------------------------------------------------
			//TODO: FIND MAGNITUDES AND RATES OF INCREASE FOR EACH FACTOR AND NORMALISE

			//Determines headheight by adding head offset to locally converted avatar head y position
			headHeightBeforeJump = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;

			//If Previous state was jumping
			if (previousState == motionTypeTracker.LocomotionState.Jumping)
			{
				feetAlternationFactor += 0.3f;//for smooth transition to running
			}

			previousVelocity = finalVelocity;//velocity we had at the last frame

			//feet factors (added in running to avoid having abrupt cuts when returning from a jump)
			feetAlternationFactor -= feetAlternationDecayFactor * Time.deltaTime;
			FeetState currentFeetState = getFeetState();

			//If Previous feet state include one foot up && current feet state includes opposite foot up --- I.E Running
			if ((previousFeetState == FeetState.leftUpRightDown && currentFeetState == FeetState.rightUpLeftDown) || (previousFeetState == FeetState.rightUpLeftDown && currentFeetState == FeetState.leftUpRightDown))
			{
				feetAlternationFactor += feetAlternationReward;
				previousFeetState = currentFeetState;
			}
			feetAlternationFactor = Mathf.Clamp(feetAlternationFactor, 0, feetAlternationCap);
			performanceBars[2].GetComponent<Slider>().value = feetAlternationFactor;

			if (feetAlternationFactor == 0)
			{
				currentVelocity = 0f;
			}
			else
			{
				//head displacement and feet alternation weights need to be minimal since they dont bear much impact on the rate of speed
				currentVelocity = runFactor * (w1 * feetAlternationFactor + w2 * headDisplacementNormalised + w3 * kneeWaistDisplacementNormalised + w4 * kneeWaistDistanceNormalised);//different speed system based on addition of params instead of mult. (WORK MORE ON THIS, I.E TRY REMOVING KNEE-WAIST PARAM)
			}
			//final velocity is a weighted sum of our previous frame velocity and the newly calculated velocity
			finalVelocity = previousVelocityWeight * previousVelocity + (1 - previousVelocityWeight) * currentVelocity * Time.deltaTime;
		
			//combination of factors
			movementVector = new Vector3(movementVector.x, movementVector.y, movementVector.z + finalVelocity);
			break;


		case motionTypeTracker.LocomotionState.Jumping: //Jumping -------------------------------------------------------------------

			//If Previous state was preparing to jump
			if (previousState == motionTypeTracker.LocomotionState.PreJump)
			{
				//this jumping mechanism was added since as we are scaling the avatar position upwards, we create an infinite scaling since we are constantly moving upwards - need the relative position of a limb relative to the avatar position to eliminate the constant upward motion affect.
				baseJumpHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
				//jumpForwardVelocity = baseJumpHeight - headHeightBeforeJump;
				finalVelocity = retainedVelocity;
			}
			//Previous state was running or something else
			else if (previousState == motionTypeTracker.LocomotionState.Running|| previousState == motionTypeTracker.LocomotionState.Ambiguous)
			{
				//this jumping mechanism was added since as we are scaling the avatar position upwards, we create an infinite scaling since we are constantly moving upwards - need the relative position of a limb relative to the avatar position(which is what keeps moving upwards) to eliminate the constant upward motion affect.
				baseJumpHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
				//jumpForwardVelocity = baseJumpHeight - headHeightBeforeJump;
			}

			//Determines height by adding head offset to locally converted avatar head y position
			currentHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
			float nextHeight = (currentHeight - baseJumpHeight) * jumpFactor;//CLASSIC JUMP MECHANISM (COMPROMISES PLAYER JUMPING ABLILITY)
		
			//Decays Jump velocity and sets final movement vector
			finalVelocity *= jumpingForwardDecay;//another decay for the forward movement when jumping
			movementVector = new Vector3(movementVector.x, nextHeight, movementVector.z + finalVelocity);
			break;

		case motionTypeTracker.LocomotionState.Ambiguous: //Other -------------------------
			//stay with previous velocity if in ambiguous state
			movementVector = new Vector3(movementVector.x, movementVector.y, movementVector.z + finalVelocity);
			break;

		}

//        if (currentState == motionTypeTracker.LocomotionState.PreJump)
//        {
//            headHeightBeforeJump = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
//            float decay = decayAtPrejump;
//            if(previousState == motionTypeTracker.LocomotionState.Jumping)
//            {
//                //movementVector = new Vector3(movementVector.x, initialAvatarHeight, transform.position.z);//also freezing the position in order to not carry any additional forward motion from jumping
//                //feet alternation capping
//                //if (feetAlternationFactor > feetAlternationCapAfterJump)
//                //{
//                //    feetAlternationFactor = feetAlternationCapAfterJump;
//                //}
//
//                //implicitly we continue moving forward with our jumping forward velocity
//                decay += jumpEnhancement;//lessen the decay effect to recover from running pose
//                retainedVelocity = finalVelocity;//retain our jumping velocity
//                //retainedVelocity = 0;//after a jump we need to gather some velocity by running to jump ahead again
//            }
//            else if(previousState== motionTypeTracker.LocomotionState.Running)
//            {
//                //feet alternation capping
//                //if (feetAlternationFactor > feetAlternationCapAfterJump)
//                //{
//                //    feetAlternationFactor = feetAlternationCapAfterJump;
//                //}
//
//                //implicitly we continue moving forward with our final velocity
//
//                retainedVelocity = finalVelocity;
//            }
//
//            finalVelocity *= decay;
//            retainedVelocity *= retainedDecay;//old jump
//
//            //combination of factors
//            movementVector = new Vector3(movementVector.x, movementVector.y, movementVector.z + finalVelocity);//TODO: FIND MAGNITUDES AND RATES OF INCREASE FOR EACH FACTOR AND NORMALISE
//        }
//        else if (currentState == motionTypeTracker.LocomotionState.Running)
//        {
//            headHeightBeforeJump = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
//            if (previousState == motionTypeTracker.LocomotionState.Jumping)
//            {
//                feetAlternationFactor += 0.3f;//for smooth transition to running
//                //feet alternation capping
//                //if (feetAlternationFactor > feetAlternationCapAfterJump)
//                //{
//                //    feetAlternationFactor = feetAlternationCapAfterJump;
//                //}
//                //movementVector = new Vector3(movementVector.x, initialAvatarHeight, transform.position.z);//also freezing the position in order to not carry any additional forward motion from jumping
//                
//                //implicitly we continue moving forward with our forward jumping velocity
//            }
//            else if (previousState == motionTypeTracker.LocomotionState.PreJump)
//            {
//                //nothing to add here yet
//            }
//            previousVelocity = finalVelocity;//velocity we had at the last frame
//
//            //feet factors (added in running to avoid having abrupt cuts when returning from a jump)
//            feetAlternationFactor -= feetAlternationDecayFactor * Time.deltaTime;
//            FeetState currentFeetState = getFeetState();
//            if ((previousFeetState == FeetState.leftUpRightDown && currentFeetState == FeetState.rightUpLeftDown) || (previousFeetState == FeetState.rightUpLeftDown && currentFeetState == FeetState.leftUpRightDown))
//            {
//                feetAlternationFactor += feetAlternationReward;
//                previousFeetState = currentFeetState;
//            }
//            feetAlternationFactor = Mathf.Clamp(feetAlternationFactor, 0, feetAlternationCap);
//            //movementParamPanels[2].GetComponent<Text>().text = "Feet Alternation: " + feetAlternationFactor;
//            performanceBars[2].GetComponent<Slider>().value = feetAlternationFactor;
//
//            //currentVelocity = feetAlternationFactor * headDisplacementNormalised * kneeWaistDisplacement * runFactor;//this can be too abrupt, so it is partially added to the final velocity to smoothen out the running motion
//            //currentVelocity = w1 * feetAlternationFactor + w2 * headDisplacementNormalised + w3 * kneeWaistDisplacement * kneeWaistDistanceWeighted;//different speed system based on addition of params instead of mult. (WORK MORE ON THIS, I.E TRY REMOVING KNEE-WAIST PARAM)
//
//            if (feetAlternationFactor == 0)
//            {
//                currentVelocity = 0f;
//            }
//            else
//            {
//                //head displacement and feet alternation weights need to be minimal since they dont bear much impact on the rate of speed
//                currentVelocity = runFactor * (w1 * feetAlternationFactor + w2 * headDisplacementNormalised + w3 * kneeWaistDisplacementNormalised + w4 * kneeWaistDistanceNormalised);//different speed system based on addition of params instead of mult. (WORK MORE ON THIS, I.E TRY REMOVING KNEE-WAIST PARAM)
//            }
//           
//            
//
//            finalVelocity = previousVelocityWeight * previousVelocity + (1 - previousVelocityWeight) * currentVelocity * Time.deltaTime;//next velocity is a weighted sum of our previous frame velocity and the newly calculated velocity
//            //retainedVelocity = finalVelocity;//saving in case of a prejump (not necessary here since its done in initial enter on prejump)
//
//            
//            //combination of factors
//            movementVector = new Vector3(movementVector.x, movementVector.y, movementVector.z + finalVelocity);//TODO: FIND MAGNITUDES AND RATES OF INCREASE FOR EACH FACTOR AND NORMALISE
//
//        }
//        else if(currentState == motionTypeTracker.LocomotionState.Jumping)
//        {
//            
//            if (previousState == motionTypeTracker.LocomotionState.PreJump)
//            {
//                //this jumping mechanism was added since as we are scaling the avatar position upwards, we create an infinite scaling since we are constantly moving upwards - need the relative position of a limb relative to the avatar position to eliminate the constant upward motion affect.
//                baseJumpHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
//                //jumpForwardVelocity = baseJumpHeight - headHeightBeforeJump;
//                finalVelocity = retainedVelocity;
//            }
//            else if (previousState == motionTypeTracker.LocomotionState.Running|| previousState == motionTypeTracker.LocomotionState.Ambiguous)
//            {
//                //this jumping mechanism was added since as we are scaling the avatar position upwards, we create an infinite scaling since we are constantly moving upwards - need the relative position of a limb relative to the avatar position(which is what keeps moving upwards) to eliminate the constant upward motion affect.
//                baseJumpHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
//                //jumpForwardVelocity = baseJumpHeight - headHeightBeforeJump;
//            }
//
//            
//            
//            currentHeight = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
//            float nextHeight = (currentHeight - baseJumpHeight) * jumpFactor;//CLASSIC JUMP MECHANISM (COMPROMISES PLAYER JUMPING ABLILITY)
//            //float nextHeight =Mathf.Lerp(0, motionTypeTracker.headHeightJumping.z+jumpEnhancement, Mathf.InverseLerp(baseJumpHeight,motionTypeTracker.headHeightJumping.z,currentHeight));
//            //float finalHeight = (nextHeight < 0) ? baseJumpHeight + nextHeight : baseJumpHeight + nextHeight;//negative next height indicates real life head height lower than the real life head height the jump was identified at. Both if conditions do the same thing to indicate that although unintuitive, i want to continue the rate of decrease so that there is no change in smoothed speed of the jump
//
//            finalVelocity *= jumpingForwardDecay;//another decay for the forward movement when jumping
//            //finalVelocity =jumpForwardScale* Mathf.Abs(nextHeight - currentHeight);
//            //movementVector = new Vector3(movementVector.x, nextHeight, movementVector.z + jumpForwardScale * Mathf.Abs(nextHeight - currentHeight));
//            movementVector = new Vector3(movementVector.x, nextHeight, movementVector.z + finalVelocity);
//
//        }
//        else if (currentState == motionTypeTracker.LocomotionState.Ambiguous)
//        {
//            //stay with previous velocity if in ambiguous state
//            movementVector = new Vector3(movementVector.x, movementVector.y, movementVector.z + finalVelocity);
//        }
    }
    

	/*-----------------------------------------------------------------*/
	// Determines the current state of feet position within the player //
    private FeetState getFeetState()
    {
        bool leftFootUp = motionTypeTracker.getLeftFootUp();
        bool rightFootUp = motionTypeTracker.getRightFootUp();
        if (leftFootUp)
        {
            if (rightFootUp)
            {
                return FeetState.bothUp;
            }
            else
            {
                return FeetState.leftUpRightDown;
            }
        }
        else
        {
            if (rightFootUp)
            {
                return FeetState.rightUpLeftDown;
            }
            else
            {
                return FeetState.bothDown;
            }
        }
    }

	/*-----------------------------------------*/
	// Determines base running speed of player //
    public float calculateBaseRunningSpeed(float factor)
    {
        return runFactor * (w1 * factor + w2 * factor + w3 * factor + w4 * factor);
    }


    public HIITController protocolController;
    private float previousMeasuredKneeWaistDistance;
    private bool previousHeadDirection = false;
    private bool saved = false;

    private float avgMaxHeadHeightJumpLow;
    private float jumpNoLow;

    private float avgKneeHeightRunLow;
    private float avgKneeDispRunLow;
    private float kneeMeasurementNoLow;

    private float stepNoLow;


    private float avgKneeHeightRunHigh;
    private float avgKneeDispRunHigh;
    private float kneeMeasurementNoHigh;

    private float stepNoHigh;


	/*---------------------------------------*/
	//Record User Parameters during exercise //
    private void recordUserParams()
    {
			
		//While Exercise protocol Hasn't ended --------------------------------------------------------
		if (HIITController.Instance.currentState != ExerciseProtocolState.finish)
        {
     		//As long as we are successfully tracking information
			if (motionTypeTracker.Tracking)
            {
                //countSteps -- Detect alternation in feet state ============================================
                FeetState currentFeetState = getFeetState();
                if ((previousFeetState == FeetState.leftUpRightDown && currentFeetState == FeetState.rightUpLeftDown) || (previousFeetState == FeetState.rightUpLeftDown && currentFeetState == FeetState.leftUpRightDown))
                {
                   
					//Step Count for Low Intensity
					if (HIITController.Instance.currentState == ExerciseProtocolState.lowIntensity)
                    {
                        stepNoLow++;
                    }
                    
					//Step count for High Intensity
					else if (HIITController.Instance.currentState == ExerciseProtocolState.highIntensity)
                    {
                        stepNoHigh++;
                    }
                }

                //knee height, disposition ========================================================
                float kneeWaistDistance = Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeLeft.position).y) + Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeRight.position).y);
                	//Takes relative distance from waist to the knees , higher value indicates standing still, closer value reaches 0
					//the more it indicates that both knees match waist height
				movementParamPanels[0].GetComponent<Text>().text = avatarWaist.transform.position.ToString();
                float kneeWaistDisplacement = Mathf.Abs(kneeWaistDistance - previousKneeWaistDistance);
                previousMeasuredKneeWaistDistance = kneeWaistDistance;
              
				//Average Knee height and displacement for low intensity
				if (HIITController.Instance.currentState == ExerciseProtocolState.lowIntensity)
                {
                    avgKneeHeightRunLow += kneeWaistDistance;
                    avgKneeDispRunLow += kneeWaistDisplacement;
                    kneeMeasurementNoLow += 1;
                }
                
				//Average Knee height and waist displacement for high intensity
				else if (HIITController.Instance.currentState == ExerciseProtocolState.highIntensity)
                {
                    avgKneeHeightRunHigh += kneeWaistDistance;
                    avgKneeDispRunHigh += kneeWaistDisplacement;
                    kneeMeasurementNoHigh += 1;
                }

                //height jump ========================================================
                if (currentState == motionTypeTracker.LocomotionState.Jumping)
                {
                    bool currentHeadDirection = motionTypeTracker.headGoingUp;
                    if (currentHeadDirection == false && previousHeadDirection == true)
                    {
                 
						if (protocolController.currentState == ExerciseProtocolState.lowIntensity)
                        {
                            avgMaxHeadHeightJumpLow = headOffsetCalculator.getOffset() + transform.InverseTransformPoint(avatarHead.transform.position).y;
                            jumpNoLow++;
                        }
                    }
                    previousHeadDirection = currentHeadDirection;
                }//------------------------------------------------------------------

			}
		//If Exercise has ended and we are saving values
        }else if (!saved){
            avgMaxHeadHeightJumpLow /= jumpNoLow;
            avgKneeHeightRunLow /= kneeMeasurementNoLow;
            avgKneeHeightRunHigh /= kneeMeasurementNoHigh;
            avgKneeDispRunLow /= kneeMeasurementNoLow;
            avgKneeDispRunHigh /= kneeMeasurementNoHigh;
			//save paremeters to file
            saveToFile();
            saved = true;
        }
    }


    public calibrationManager calibrator;
	/*--------------------------------------------------------*/
	// Saves Exercise parameters to a resource folder for use //
    private void saveToFile()
    {
        string path = "Assets/Resources/res" + calibrator.participantNo + ".txt";

        StreamWriter writer = new StreamWriter(path, true);
        //Condition
		writer.WriteLine(GameManager.Instance.gameCondition.ToString());
		//Number of Jumps
        writer.WriteLine(jumpNoLow.ToString("G7"));
        //average max jump height
        writer.WriteLine(avgMaxHeadHeightJumpLow.ToString("G7"));
        //Number of steps for each intensity
        writer.WriteLine(stepNoLow.ToString("G7"));
        writer.WriteLine(stepNoHigh.ToString("G7"));
        //Average KneeHeight(relative to waist)
        writer.WriteLine(avgKneeHeightRunLow.ToString("G7"));
        writer.WriteLine(avgKneeHeightRunHigh.ToString("G7"));
        //Average Knee Disposition
        writer.WriteLine(avgKneeDispRunLow.ToString("G7"));
        writer.WriteLine(avgKneeDispRunHigh.ToString("G7"));
        //---------------------------------------

		//Average Lateral + forward speeds for each interval
		for( int i = 0; i < sessionIntervalSpeeds.Count; i++){
			writer.WriteLine (sessionIntervalSpeeds [i].interval.ToString ()); //Write interval type
			writer.WriteLine(sessionIntervalSpeeds[i].averageLateralSpeed.ToString("G7"));//Write averaged lateral speed
			writer.WriteLine(sessionIntervalSpeeds[i].averageSpeed.ToString("G7"));//Write averaged spped for the interval
		}
		writer.WriteLine(" ");
        writer.Close();

		//Write unaveraged lateral + forward speeds to Json File
		string json = JsonUtility.ToJson (sessionIntervalSpeeds);

		// Save in a separate file
		string path2 = "Assets/Resources/" + GameManager.Instance.gameCondition.ToString() + calibrator.participantNo + ".txt";
		StreamWriter writer2 = new StreamWriter(path2, true);
		writer2.WriteLine (json);
		writer2.WriteLine(" ");
		writer2.Close();


    }
}
