using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class calibrationManager : MonoBehaviour
{
    public bool calibrateNew;
    public int participantNo;

    public Transform playerMovement;

    public GameObject promptPanel;
    public motionTypeTracker motionTypeTracker;

    public float captureRate;
    public int standingDataAmount;
    public int runningDataAmount;
    public int jumpingDataAmount;

	//-------------------------//
	/* Head Calibration values */
	//-------------------------//
	public headRotationAdjuster headOffsetCalculator;
	[Header("Player Model Transforms:")]
	public Transform avatarHead;
	//Vector Position of body height whilst standing (kinect)
    private Vector3 feetHeightParamsStanding;
	//Vector Position of head whilst running (kinect?)
    private Vector3 headHeightParamsRunning;
	//Vector Position of head whilst jumping (kinect?)
    private Vector3 headHeightParamsJumping;

    //-------------------------//
	/* Knee Calibration Values */
	//-------------------------//
    public Transform avatarKneeLeft;
    public Transform avatarKneeRight;
    private Vector3 kneeHeightParamsRunning;
    private Vector3 kneeRotationParamsRunning;
    private Vector3 kneeHeightParamsJumping;
    private Vector3 kneeRotationParamsJumping;
	private Vector3 headKneeDistanceJumping;

	//-------------------------//
	/* Feet Calibration Values */
	//-------------------------//
    public Transform avatarFootLeft;
    public Transform avatarFootRight;
    private Vector3 footHeightParamsRunning;
    private Vector3 footHeightParamsJumping;

    
    /*----------------*/
    //speed parameters//
    public Transform avatarWaist;
    private Vector3 kneeWaistDispositionRunning;
    private float previousKneeWaistDistanceRunning;
    //private Vector3 kneeWaistDistanceJumping;

    private Vector3 headDispositionRunning;
    private Vector3 headDispositionJumping;
    private float previousHeadHeight;
    //private Vector3 HeadDispositionJumping;

    //Singleton Reference
    private static calibrationManager _instance;


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
    public static calibrationManager Instance { get { return _instance; } }

    // Use this for initialization
    void Start()
    {
        //Sets initial default values for head/knee/feet values
		feetHeightParamsStanding = new Vector3(100, 0, 0);
        headHeightParamsRunning = new Vector3(100, 0, 0);//setting x to 100 because we will measure the min
        headHeightParamsJumping = new Vector3(100, 0, 0);

        kneeHeightParamsRunning = new Vector3(100, 0, 0);
        kneeHeightParamsJumping = new Vector3(100, 0, 0);
        kneeRotationParamsRunning = new Vector3(359, 0, 0);
        kneeRotationParamsJumping = new Vector3(359, 0, 0);

        footHeightParamsRunning = new Vector3(100, 0, 0);
        footHeightParamsJumping = new Vector3(100, 0, 0);

        headKneeDistanceJumping = new Vector3(100, 0, 0);

        //kneeWaistDistanceJumping = new Vector3(100, 0, 0);
        kneeWaistDispositionRunning = new Vector3(100, 0, 0);
        previousKneeWaistDistanceRunning = Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeLeft.position).y) + Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeRight.position).y);
        headDispositionRunning = new Vector3(100, 0, 0);
        headDispositionJumping = new Vector3(100, 0, 0);
        previousHeadHeight = headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarHead.position).y;

		//If we need to calibrate for a new player
        if (calibrateNew)
        {
            //Start calibrating new user
			StartCoroutine(CalibrateStanding());
        }
        else
        {
			//Load existing head/knee/feet calibration values
            loadCalibrationParameters();
            motionTypeTracker.trackMovement(
           feetHeightParamsStanding,
           headHeightParamsRunning,
           headHeightParamsJumping,
           kneeRotationParamsRunning,
           kneeRotationParamsJumping,
           kneeHeightParamsRunning,
           kneeHeightParamsJumping,
           footHeightParamsRunning,
           footHeightParamsJumping,
           headKneeDistanceJumping,
           kneeWaistDispositionRunning,
           headDispositionRunning,
           headDispositionJumping);

        }

    }

	/*-------------------------------------------------------------------------------------------------------------*/
	//Receives  an observed value, a reference to the Vector 3 accumulating the data, and the amount to be recorded//
	//before updating the accumulator array based on the given value                                               //
    private void addToMeasurements(float value, ref Vector3 measurements, int dataAmount)
    {
        //x is min
        if (value < measurements.x)
        {
            measurements.x = value;
        }
        //z is max
        else if (value > measurements.z)
        {
            measurements.z = value;
        }
        //add a proportion of the value based on how much data we gather for the average
        measurements.y += value / dataAmount;
    }

	/*-----------------------------------------------------------*/
	// Couroutine that calibrates the standing values of the user//
    IEnumerator CalibrateStanding()
    {
        //delay making sure the user is running before capturing
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Stand";
        int dataGathered = 0;
        yield return new WaitForSeconds(5f);
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Recording!";

        //capturing data
        while (dataGathered < standingDataAmount)
        {
            dataGathered++;
			//Gathers measurements from left foot and right foot positions in regards to head offseet, accumulating it within the FeetHeight paramater
			//to get an average value of the players Feet Height
            addToMeasurements(headOffsetCalculator.getOffset() + avatarFootLeft.position.y, ref feetHeightParamsStanding, standingDataAmount * 2);//adding feet height when standing
            addToMeasurements(headOffsetCalculator.getOffset() + avatarFootRight.position.y, ref feetHeightParamsStanding, standingDataAmount * 2);
            yield return new WaitForSeconds(1 / captureRate);
        }
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Done!";

        StartCoroutine(CalibrateRunning());
    }

	/*-----------------------------*/
    //start to gather running data //
    IEnumerator CalibrateRunning()
    {
        //delay making sure the user is running before capturing
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Run!";
        int dataGathered = 0;
        yield return new WaitForSeconds(3f);
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Recording!";
        //capturing data
        while (dataGathered < runningDataAmount)
        {
            //float headDispositon = avatarHead.position.y - headHeightParamsStanding.y;

            dataGathered++;
            //getting position relative to player object because we want measurements according to what happens in the actual world, whereas player object moves head according to game rules
            addToMeasurements(headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarHead.position).y, ref headHeightParamsRunning, runningDataAmount);//adding head height when running

            addToMeasurements(avatarKneeLeft.position.y, ref kneeHeightParamsRunning, runningDataAmount * 2);//adding knee height when running
            addToMeasurements(avatarKneeRight.position.y, ref kneeHeightParamsRunning, runningDataAmount * 2);//from both avatar knees

            addToMeasurements(avatarKneeLeft.localEulerAngles.z, ref kneeRotationParamsRunning, runningDataAmount * 2);//adding knee rotation when running
            addToMeasurements(avatarKneeRight.localEulerAngles.z, ref kneeRotationParamsRunning, runningDataAmount * 2);//from both avatar knees

            addToMeasurements(avatarFootLeft.position.y, ref footHeightParamsRunning, runningDataAmount * 2);//adding foot height when running
            addToMeasurements(avatarFootRight.position.y, ref footHeightParamsRunning, runningDataAmount * 2);//from both avatar feet

            //running params
            float kneeWaistDistance = Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeLeft.position).y) + Mathf.Abs(avatarWaist.InverseTransformPoint(avatarKneeRight.position).y);
            float kneeWaistDisplacement = Mathf.Abs(kneeWaistDistance - previousKneeWaistDistanceRunning);
            previousKneeWaistDistanceRunning = kneeWaistDistance;
            addToMeasurements(kneeWaistDisplacement, ref kneeWaistDispositionRunning, runningDataAmount);

            float headHeight = headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarHead.position).y;
            float headDisplacement = Mathf.Abs(headHeight - previousHeadHeight);
            previousHeadHeight = headHeight;
            addToMeasurements(headDisplacement, ref headDispositionRunning, runningDataAmount);

            yield return new WaitForSeconds(1 / captureRate);
        }
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Done!";
        Debug.Log(kneeHeightParamsRunning.x);
        Debug.Log(kneeHeightParamsRunning.y);
        Debug.Log(kneeHeightParamsRunning.z);
        Debug.Log("Knee Angle Running");
        Debug.Log(kneeRotationParamsRunning.x);
        Debug.Log(kneeRotationParamsRunning.y);
        Debug.Log(kneeRotationParamsRunning.z);
        Debug.Log("Foot Height Running");
        Debug.Log(footHeightParamsRunning.x);
        Debug.Log(footHeightParamsRunning.y);
        Debug.Log(footHeightParamsRunning.z);

		//Starts to gather jumping calibration data
        StartCoroutine(CalibrateJumping());
    }

	/*----------------------------*/
    //start to gather running data//
    IEnumerator CalibrateJumping()
    {
        int dataGathered = 0;

        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Jump!";
        //delay making sure the user is jumping before capturing
        yield return new WaitForSeconds(3f);
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Recording!";
        //capturing data
        while (dataGathered < jumpingDataAmount)
        {
            dataGathered++;
            addToMeasurements(headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarHead.position).y, ref headHeightParamsJumping, jumpingDataAmount);//adding head height when jumping

            addToMeasurements(avatarKneeLeft.position.y, ref kneeHeightParamsJumping, jumpingDataAmount * 2);//adding knee height when jumping
            addToMeasurements(avatarKneeRight.position.y, ref kneeHeightParamsJumping, jumpingDataAmount * 2);

            addToMeasurements(avatarKneeLeft.localEulerAngles.z, ref kneeRotationParamsJumping, jumpingDataAmount * 2);//adding knee rotation when jumping
            addToMeasurements(avatarKneeRight.localEulerAngles.z, ref kneeRotationParamsJumping, jumpingDataAmount * 2);

            addToMeasurements(avatarFootLeft.position.y, ref footHeightParamsJumping, runningDataAmount * 2);//adding foot height when jumping
            addToMeasurements(avatarFootRight.position.y, ref footHeightParamsJumping, runningDataAmount * 2);

            addToMeasurements(headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarHead.position).y - (headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarKneeLeft.position).y), ref headKneeDistanceJumping, jumpingDataAmount * 2);//adding distance of knees from head when jumping (relative to player position and adjusting according to head rotation offset)
            addToMeasurements(headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarHead.position).y - (headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarKneeRight.position).y), ref headKneeDistanceJumping, jumpingDataAmount * 2);

            float headHeight = headOffsetCalculator.getOffset() + playerMovement.InverseTransformPoint(avatarHead.position).y;
            float headDisplacement = Mathf.Abs(headHeight - previousHeadHeight);
            previousHeadHeight = headHeight;
            //minimum value head displacement in jumping takes is the average head disposition when running, in order to find an average of the fast parts of jumping, and not parts where the head slows down, since those are the ones we want to distinguish from actual running
            addToMeasurements(Mathf.Clamp(headDisplacement, headDispositionRunning.y, float.MaxValue), ref headDispositionJumping, jumpingDataAmount);

            yield return new WaitForSeconds(1 / captureRate);
        }
        promptPanel.GetComponent<UnityEngine.UI.Text>().text = "Done!";
        Debug.Log(kneeHeightParamsJumping.x);
        Debug.Log(kneeHeightParamsJumping.y);
        Debug.Log(kneeHeightParamsJumping.z);
        Debug.Log("Knee Angle Jumping");
        Debug.Log(kneeRotationParamsJumping.x);
        Debug.Log(kneeRotationParamsJumping.y);
        Debug.Log(kneeRotationParamsJumping.z);
        Debug.Log("Foot Height Jumping");
        Debug.Log(footHeightParamsJumping.x);
        Debug.Log(footHeightParamsJumping.y);
        Debug.Log(footHeightParamsJumping.z);


        saveCalibrationParameters();
		//Passes the current user movement ranges to the motion tracker and begins tracking
        motionTypeTracker.trackMovement(
            feetHeightParamsStanding,
            headHeightParamsRunning,
            headHeightParamsJumping,
            kneeRotationParamsRunning,
            kneeRotationParamsJumping,
            kneeHeightParamsRunning,
            kneeHeightParamsJumping,
            footHeightParamsRunning,
            footHeightParamsJumping,
            headKneeDistanceJumping,
            kneeWaistDispositionRunning,
            headDispositionRunning,
            headDispositionJumping);

    }

	/*-------------------------------------------------------*/
    //Saves Player Calibration Parameters -- doesnt override //
    private void saveCalibrationParameters()
    {
        string path = "Assets/Resources/calPar" + participantNo + ".txt";

        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(feetHeightParamsStanding.ToString("G7"));
        writer.WriteLine(headHeightParamsRunning.ToString("G7"));
        writer.WriteLine(headHeightParamsJumping.ToString("G7"));
        writer.WriteLine(kneeRotationParamsRunning.ToString("G7"));
        writer.WriteLine(kneeRotationParamsJumping.ToString("G7"));
        writer.WriteLine(kneeHeightParamsRunning.ToString("G7"));
        writer.WriteLine(kneeHeightParamsJumping.ToString("G7"));
        writer.WriteLine(footHeightParamsRunning.ToString("G7"));
        writer.WriteLine(footHeightParamsJumping.ToString("G7"));
        writer.WriteLine(headKneeDistanceJumping.ToString("G7"));
        writer.WriteLine(kneeWaistDispositionRunning.ToString("G7"));
        writer.WriteLine(headDispositionRunning.ToString("G7"));
        writer.WriteLine(headDispositionJumping.ToString("G7"));
        writer.WriteLine(" ");
        writer.Close();
    }

	/*---------------------------------------------------*/
	// Loads calibration parameters of a set participant //
    private void loadCalibrationParameters()
    {
        string path = "Assets/Resources/calPar" + participantNo + ".txt";


        StreamReader reader = new StreamReader(path);

        feetHeightParamsStanding = parseVector3(reader.ReadLine());
        headHeightParamsRunning = parseVector3(reader.ReadLine());
        headHeightParamsJumping = parseVector3(reader.ReadLine());
        kneeRotationParamsRunning = parseVector3(reader.ReadLine());
        kneeRotationParamsJumping = parseVector3(reader.ReadLine());
        kneeHeightParamsRunning = parseVector3(reader.ReadLine());
        kneeHeightParamsJumping = parseVector3(reader.ReadLine());
        footHeightParamsRunning = parseVector3(reader.ReadLine());
        footHeightParamsJumping = parseVector3(reader.ReadLine());
        headKneeDistanceJumping = parseVector3(reader.ReadLine());
        kneeWaistDispositionRunning = parseVector3(reader.ReadLine());
        headDispositionRunning = parseVector3(reader.ReadLine());
        headDispositionJumping = parseVector3(reader.ReadLine());

        reader.Close();
    }

	/*---------------------------------------------*/
	// Converts a string into a parseable Vector 3 //
    Vector3 parseVector3(string label)
    {
        string[] values = label.Substring(1, label.Length - 2).Split(',');
        return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
    }


}