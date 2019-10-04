using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class motionTypeTracker : MonoBehaviour {

    public Transform avatarHead;
    public headRotationAdjuster headOffsetCalculator;
    public Transform avatarLeftKnee;
    public Transform avatarRightKnee;
    public Transform avatarLeftFoot;
    public Transform avatarRightFoot;

    public List<GameObject> performanceBars;
    
    public GameObject promptPanel;

    public float footGroundedErrorMargin;
    public float jumpThreshold;
    public float groundThreshold;
    public bool leftFootOnAir;
    public bool rightFootOnAir;

    //is motion type being tracked
    private bool tracking;
    public bool Tracking
    {
        get
        {
            return tracking;
        }
    }

    public enum LocomotionState
    {
        PreJump,
        Jumping,
        Running,
        Ambiguous
    }
    private LocomotionState currentState;

    private float previousHeadHeight;
    public bool headGoingUp;
	private bool midJump = false;

    //private float previousLKneeBend;
    //private float previousRKneeBend;
    //public bool kneeLBendingDown;
    //public bool kneeRBendingDown;

    private Vector3 feetHeightStanding;
    public Vector3 headHeightRunning;
    public Vector3 headHeightJumping;

    private Vector3 kneeRotationRunning;
    private Vector3 kneeRotationJumping;

    private Vector3 kneeHeightRunning;
    private Vector3 kneeHeightJumping;

    private Vector3 footHeightRunning;
    private Vector3 footHeightJumping;

    private Vector3 headKneeDistanceJumping;

    //running params
    public Vector3 kneeWaistDispositionRunning;
    public Vector3 headDispositionRunning;
    public Vector3 headDispositionJumping;

    //add head displacement movement speed



    // Use this for initialization
    //void Start () {
    //}

	/*---------------------------------*/
    // Update is called once per frame //
    void Update()
    {
		//Headset is tracking
        if (tracking) 
        {
            groundThreshold = feetHeightStanding.y + feetHeightStanding.y * footGroundedErrorMargin; //Find ground threshold
			float headRotationOffset = headOffsetCalculator.getOffset(); //Calculates rotation offset (lean) of user head
            float headHeight = headRotationOffset + transform.InverseTransformPoint(avatarHead.position).y;//head height in terms of real world (rotation dislocation eliminated)
            float headDisposition = headHeight - previousHeadHeight; //Determines disposition between previous and current head height
            if (headDisposition > 0.01f) // Y-head height increase == head moving up
            {
                headGoingUp = true;
                //promptPanel.GetComponent<UnityEngine.UI.Text>().text = "True!";
            }
            else if (headDisposition < -0.01f) // Y-head height decrease == head moving down
            {
                headGoingUp = false;
                //promptPanel.GetComponent<UnityEngine.UI.Text>().text = "False!";
            }
            previousHeadHeight = headHeight;

            //if (avatarLeftKnee.localEulerAngles.z - previousLKneeBend > 0.1f)
            //{
            //    kneeLBendingDown = false;
            //}
            //else if (avatarLeftKnee.localEulerAngles.z - previousLKneeBend < -0.1f)
            //{
            //    kneeLBendingDown = true;
            //}
            //if (avatarRightKnee.localEulerAngles.z - previousRKneeBend > 0.1f)
            //{
            //    kneeRBendingDown = false;
            //}
            //else if (avatarRightKnee.localEulerAngles.z - previousRKneeBend < -0.1f)
            //{
            //    kneeRBendingDown = true;
            //}
            //previousLKneeBend = avatarLeftKnee.localEulerAngles.z;
            //previousRKneeBend = avatarRightKnee.localEulerAngles.z;


            //update which feet are in the air ---------------------
            updateFeetParams();

			//Determines scoring for possibility of head height indicating running
            float headHeightScoreRun = Mathf.InverseLerp(headHeightJumping.x, headHeightRunning.y, headHeight);
            performanceBars[4].GetComponent<Slider>().value = headHeightScoreRun;
			//Determines scoring for possibility of head height indicating a pre-jump
            float headHeightScorePreJump = Mathf.InverseLerp(headHeightRunning.y, headHeightJumping.x, headHeight);
            performanceBars[0].GetComponent<Slider>().value = headHeightScorePreJump;

			//Determines scoring for possibility of knee bend indicating a jump
            float kneeBendScoreJump = Mathf.InverseLerp(kneeRotationJumping.y, kneeRotationJumping.x, avatarLeftKnee.localEulerAngles.z) / 2 + Mathf.InverseLerp(kneeRotationJumping.y, kneeRotationJumping.x, avatarRightKnee.localEulerAngles.z) / 2;
            //performanceBars[1].GetComponent<Slider>().value = kneeBendScoreJump;
            //Determines scoring for possibility of knee bend indicating running
			float kneeBendScoreRun = 1 - kneeBendScoreJump;
            performanceBars[5].GetComponent<Slider>().value = kneeBendScoreRun;

			//last part is just avg. This was added in order for the jump to not be identified too early due to head moving upwards whereas the knees are still bent due to lower update rate of kinect
			float headKneeDistanceScoreJumping = Mathf.InverseLerp(headKneeDistanceJumping.x, headKneeDistanceJumping.z, (2 * (headHeight) - (headRotationOffset + transform.InverseTransformPoint(avatarLeftKnee.position).y) - (headRotationOffset + transform.InverseTransformPoint(avatarRightKnee.position).y)) / 2);        performanceBars[2].GetComponent<Slider>().value = headKneeDistanceScoreJumping;
            float headHeightScoreJumping = Mathf.InverseLerp(headHeightRunning.y, headHeightJumping.z, headHeight);
            performanceBars[3].GetComponent<Slider>().value = headHeightScoreJumping;

			//Both Feet on the ground---------------------------------------------------------------------------------
			if (!leftFootOnAir && !rightFootOnAir) {

				//If we are in prejump // was midjump (one foot of ground) but now both feet are down, consider prejump was incorrect and reset state
				if (midJump && currentState == LocomotionState.PreJump ) {
					midJump = false;
                    currentState = LocomotionState.Ambiguous;
					//Debug.Log ("Coming out of false PreJump");
				}

				//consult head and knee params to discern between pre-jump and running
				if (headHeightScorePreJump + kneeBendScoreJump > headHeightScoreRun + kneeBendScoreRun) {
					currentState = LocomotionState.PreJump;
					//Debug.Log ("Head and knees indicate pre-jump");
				}
				//condition is set because we want to maintain the prejump state as the user goes up and attempts to perform a jump
				else if (!(currentState == LocomotionState.PreJump && headGoingUp)) { //
					//running determined based on head speed
					float headDisplacement = Mathf.Abs (headHeight - previousHeadHeight);
					if (headDisplacement > (headDispositionRunning.z + headDispositionJumping.y) / 2) {
						if (headHeight > headHeightRunning.y) {
							currentState = LocomotionState.Jumping;
						} else {
							currentState = LocomotionState.PreJump;
						}
					} else {
						currentState = LocomotionState.Running;
					}

					//if (!headGoingUp && kneeLBendingDown && kneeRBendingDown)//if head is going down, and both knees are bending down we are most probably preparing for jump
					//{
					//    currentState = LocomotionState.PreJump;
					//}
					//else
					//{
					//    currentState = LocomotionState.Running;
					//}

					//currentState = LocomotionState.Running;
				} else {
					//We are no longer detected entering pre-jump && we were in pre-jump with a detected head rise
					//Debug.Log ("Prejump, heading going up but no feet off ground");
				}

				//if (!headGoingUp && kneeLBendingDown && kneeRBendingDown)//if head is going down, and both knees are bending down we are most probably preparing for jump
				//{
				//    currentState = LocomotionState.PreJump;
				//}
				//else
				//{
				//    currentState = LocomotionState.Running;
				//}

				//currentState = LocomotionState.Running;

			//Both Feet in the air -=-=-==--==-==---=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
			} else if (leftFootOnAir && rightFootOnAir) { //&& currentState == LocomotionState.PreJump) //last condition is set in case the 2 feet are momentarily captured to be off the ground while the user is running
				float jumpScore;

				//float jumpScore = Mathf.InverseLerp(headKneeDistanceJumping.x,headKneeDistanceJumping.z,(2*(headOffsetCalculator.getOffset()+avatarHead.position.y)-avatarLeftKnee.position.y- avatarRightKnee.position.y)/2);//last part is just avg. This was added in order for the jump to not be identified too early due to head moving upwards whereas the knees are still bent due to lower update rate of kinect
				//float headKneeDistanceScoreJumping = Mathf.InverseLerp(headKneeDistanceJumping.x, headKneeDistanceJumping.z, (2 * (headOffsetCalculator.getOffset()+avatarHead.position.y) - avatarLeftKnee.position.y - avatarRightKnee.position.y) / 2);//last part is just avg. This was added in order for the jump to not be identified too early due to head moving upwards whereas the knees are still bent due to lower update rate of kinect
				//performanceBars[2].GetComponent<Slider>().value = headKneeDistanceScoreJumping;
				//float headHeightScoreJumping = Mathf.InverseLerp(headHeightRunning.y, headHeightJumping.z, headOffsetCalculator.getOffset()+avatarHead.transform.position.y);
				//performanceBars[3].GetComponent<Slider>().value = headHeightScoreJumping;

				//giving 2 chances on jumping when both feet are up (1 based on head speed and 2 based on head height and distance of knee to head)
				jumpScore = headKneeDistanceScoreJumping + headHeightScoreJumping;
				if (jumpScore > jumpThreshold) {
					currentState = LocomotionState.Jumping;
				} else {
					float headDisplacement = Mathf.Abs (headDisposition);
					//head speed changing point is the average between head speed when running and head speed when jumping (fast parts)
					if (headDisplacement > (headDispositionRunning.z + headDispositionJumping.y) / 2) {
						if (currentState == LocomotionState.Running) {//signifying previous state
							currentState = LocomotionState.Ambiguous;//here the user can be starting or finishing a jump which will show in the next few frames, or could be instances of running with both feet momentarily on air
						}
					}
					//else{
					//    currentState = LocomotionState.Running;
					//}
				}
				midJump = false; //Either we are running (not midjump) // In the air (not midjump as currently in air) // or landing (finished jump -- not midjump)


				//jumpScore = headKneeDistanceScoreJumping + headHeightScoreJumping;
				//if (jumpScore > jumpThreshold)
				//{
				//    currentState = LocomotionState.Jumping;
				//}


            
			//Detects one foot in air -------------------------------------------
			//   second condition statement added since we want to maintain the prejump state while the user is launching but only one of his 2 legs was causght on the air so far
			//   }else if ((leftFootOnAir ^ rightFootOnAir) && !(currentState == LocomotionState.PreJump && headGoingUp)){
			}else if (leftFootOnAir ^ rightFootOnAir){
                //Debug.Log("One foot is in the air");
                //if we were in the PreJump state, indicate that we are midjump, if we however enter both feet on the ground state while midjump
				//Consider the prejump state a false reading and return to running
				if (currentState == LocomotionState.PreJump && headGoingUp) {
					midJump = true;
				} else {
					currentState = LocomotionState.Running;
				}


				//based solely on head speed when one of 2 feet is on air

                //float headDisplacement = Mathf.Abs(headDisposition);
                //if (headDisplacement > (headDispositionRunning.z + headDispositionJumping.y) / 2)
                //{
                //    if (headHeight > headHeightRunning.y)
                //    {
                //        currentState = LocomotionState.Jumping;
                //    }
                //    else
                //    {
                //        currentState = LocomotionState.PreJump;
                //    }
                //}
                //else
                //{
                //    currentState = LocomotionState.Running;
                //}

                //if (!headGoingUp && kneeLBendingDown && kneeRBendingDown)//if head is going down, and both knees are bending down we are most probably preparing for jump
                //{
                //    currentState = LocomotionState.PreJump;
                //}
                //else
                //{
                //    currentState = LocomotionState.Running;
                //}

                //currentState = LocomotionState.Running;
            }


            switch (currentState)
            {
                case LocomotionState.PreJump:
                    //if (leftFootOnAir ^ rightFootOnAir) { 
                  // promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Pre-Jump One Foot!";
                  //  }else if(!leftFootOnAir && !rightFootOnAir)
                 //  {
                  //  promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Pre-Jump ground!";
                 //   }
                    break;
                case LocomotionState.Jumping:
                 //   promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Jumping!";
                    break;
                case LocomotionState.Running:
                  //  promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Running!";
                    break;
                case LocomotionState.Ambiguous:
                  //  promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Ambiguous!";
                    break;
                default:
                    break;
            }
        }
    }

	/*----------------------------------------------------------*/
	//Determines state of player feet for other methods to read //
    private void updateFeetParams()
    {
        if (headOffsetCalculator.getOffset()+ avatarLeftFoot.position.y > groundThreshold)
        {
            leftFootOnAir = true;
            //promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Left Foot up!";
        }
        else
        {
            leftFootOnAir = false;
            //promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Left Foot down!";
        }

        if (headOffsetCalculator.getOffset() + avatarRightFoot.position.y > groundThreshold)
        {
            rightFootOnAir = true;
            //promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Right Foot up!";
        }
        else
        {
            rightFootOnAir = false;
            //promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Right Foot down!";
        }
    }


    /*----------------------------------------------------------------------*/
	// Updates motion tracker with parameters retrieved from the calibrator //
    public void trackMovement(Vector3 feetHeightStanding, Vector3 headHeightRunning, Vector3 headHeightJumping, Vector3 kneeRotationRunning, Vector3 kneeRotationJumping, Vector3 kneeHeightRunning, Vector3 kneeHeightJumping, Vector3 footHeightRunning, Vector3 footHeightJumping, Vector3 headKneeDistanceJumping, Vector3 kneeWaistDistanceRunning, Vector3 headDispositionRunning, Vector3 headDispositionJumping)
    {
        this.feetHeightStanding = feetHeightStanding;
        this.headHeightRunning = headHeightRunning;
        this.headHeightJumping = headHeightJumping;

        this.kneeRotationJumping = kneeRotationJumping;
        this.kneeRotationRunning = kneeRotationRunning;

        this.kneeHeightJumping = kneeHeightJumping;
        this.kneeHeightRunning = kneeHeightRunning;

        this.footHeightJumping = footHeightJumping;
        this.footHeightRunning = footHeightRunning;

        this.headKneeDistanceJumping = headKneeDistanceJumping;

        this.headDispositionRunning = headDispositionRunning;
        this.kneeWaistDispositionRunning = kneeWaistDistanceRunning;
        this.headDispositionJumping = headDispositionJumping;

        groundThreshold = feetHeightStanding.y;
        groundThreshold += groundThreshold * footGroundedErrorMargin;

        tracking = true;
    }


	//Getter for left foot up //
    public bool getLeftFootUp()
    {return leftFootOnAir;}

	//Getter for right foot up //
    public bool getRightFootUp()
    {return rightFootOnAir;}

	//Getter for current locomotion state
    public LocomotionState getCurrentState()
    {return currentState;}

}
