//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
////using System.IO.Ports;
//using UnityEngine.UI;
//
//public class SerialPortCommunicator : MonoBehaviour
//{
//    //private SerialPort bikePort;
//    
//    private const float iterationWait = 0.1f;
//
//    [SerializeField]
//    private float crashDropOff;
//
//    [SerializeField]
//    private Text rpmHUD;
//
//    public int resistance;
//    public int rpm;
//    public float crashResistance = 1.0f;
//    public float constantVal = 0.11f;
//
//    private int totalResistance;
//
//    void Start()
//    {
//        StartCoroutine(SerialBikeInOutLoop());
//    }
//
//    void Update()
//    {
//        if(crashResistance != 1)
//        {
//            float reductionStep = Time.deltaTime * crashDropOff;
//            if (1 + reductionStep > crashResistance)
//            {
//                crashResistance = 1;
//            }
//            else
//            {
//                crashResistance -= reductionStep;
//            }
//        }
//    }
//
//    IEnumerator SerialBikeInOutLoop()
//    {
//        //Console.OutputEncoding = System.Text.Encoding.ASCII;
//            
//        byte[] readBuffer = new byte[5];
//        //for (int i = 0; i < 100; ++i)
//        //readBuffer[i] = '\0';
//        bikePort = new SerialPort("COM1", 9600, Parity.Even, 7, StopBits.Two);
//        //readBuffer = new char[100];
//        //for (int i = 0; i < 100; ++i)
//        //{
//        //    readBuffer[i] = '\0';
//        //}
//
//        Debug.Log("Port O");
//        bikePort.Open();
//        Debug.Log(" O Port");
//
//        bikePort.WriteLine("0,SP,0\r\n");
//        while(true)
//        {
//            // Following the protocol sequence of the bike.Read write sequence from the 
//            // original PARVO Medics software is emulated.Derived from the data from sniffed data HTML doc
//            // in the root of the repository, BikeData folder (<root>/BikeData/test-bike-read.html)
//            // Sequence from the session with Tom Nightingale
//
//            bikePort.Write("0,RM.\r\n");
//            bikePort.Read(readBuffer, 0, 5);
//            //readBuffer[6] = '\n';
//            // Debug.Log("RM "+i+ Convert.ToBase64String(readBuffer));
//            String a =Encoding.UTF8.GetString(readBuffer);
//            string stringRPM = a.Substring(2);
//            if(int.TryParse(stringRPM, out rpm))
//            {
//
//            }
//            else
//            {
//                rpm = 0;
//            }
//
//            rpmHUD.text = rpm.ToString() + "RPM";
//            //Debug.Log("RM " + a);
//
//            //bikePort.Write("0,HR.\r\n");
//            //bikePort.Read(readBuffer, 0, 5);
//            ////readBuffer[6] = '\n';
//            ////Debug.Log("HR" + i + Convert.ToBase64String(readBuffer));
//            // a = Encoding.UTF8.GetString(readBuffer);
//
//            //Debug.Log("HR " + a);
//
//            //bikePort.Write("0,PM.\r\n");
//            //bikePort.Read(readBuffer, 0, 5);
//            //readBuffer[6] = '\n';
//            //Debug.Log("PM" + i + Convert.ToBase64String(readBuffer));
//            // a = Encoding.UTF8.GetString(readBuffer);
//
//            //Debug.Log("PM " + a);
//            
//            float resitanceFloat = (resistance * crashResistance) * rpm * constantVal;
//            totalResistance = Mathf.FloorToInt(resitanceFloat);
//            print(totalResistance);
//            print(rpm);
//            bikePort.Write("0,SP," + totalResistance + "\r\n");
//            bikePort.Read(readBuffer, 0, 5);
//            // readBuffer[6] = '\n';
//            //Debug.Log("SP" + i + Convert.ToBase64String(readBuffer));
//            a = Encoding.UTF8.GetString(readBuffer);
//
//            //Debug.Log("SP " + a);
//
//            //bikePort.Write("0,SP," + i + "\r\n");
//            //Console.WriteLine(readBuffer);
//            ////Console.WriteLine(readValue);
//            yield return new WaitForSeconds(iterationWait);
//
//        }
//    }
//}
//
//
