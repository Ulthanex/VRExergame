using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataSaver : MonoBehaviour {

    public static DataSaver saver;

    //Unsaved variables
    public string userID;
    public int sprintResistance;
    public bool straight;
	public personalityType hexType;

    //Saved variables
    public int[] trackRecording;
    public int[] truckRecording;
    public float[] ghostSpeedRecording;
    public float[] ghostLeanRecording;

	/*----------------------------------------*/
	// On Awake, create semi-singleton object //
    void Awake () {
        DontDestroyOnLoad(gameObject); //Retains object after scene load
        saver = this; //Sets singleton instance
	}

	/*----------------------------------------------------------------------------------------------------*/
	// Saves a parsed recording of the track, the trucks, player speed & Player leaning to create a ghost //
    public void Save(int[] parsedTrackRecording, int[] parsedTruckRecording, float[] parsedPlayerSpeedRecording, float[] parsedPlayerLeanRecording)
    {
        string fileLocation = Application.persistentDataPath + "/" + userID + ".dat";

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(fileLocation);

        userData data = new userData();
        data.trackRecording = parsedTrackRecording;
        data.truckRecording = parsedTruckRecording;
        data.ghostSpeedRecording = parsedPlayerSpeedRecording;
        data.ghostLeanRecording = parsedPlayerLeanRecording;

        bf.Serialize(file, data);
        file.Close();
    }

	/*-------------------------------------------------*/
	// Loads ghost data recording with supplied UserID //
    public void Load() {
        string fileLocation = Application.persistentDataPath + "/" + userID + ".dat";

        if (File.Exists(fileLocation))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fileLocation, FileMode.Open);
            userData data = (userData)bf.Deserialize(file);
            file.Close();

            trackRecording = data.trackRecording;
            truckRecording = data.truckRecording;
            ghostSpeedRecording = data.ghostSpeedRecording;
            ghostLeanRecording = data.ghostLeanRecording;
        }
    }

}

[Serializable]
public class userData
{
    public int[] trackRecording;
    public int[] truckRecording;
    public float[] ghostSpeedRecording;
    public float[] ghostLeanRecording;

}
