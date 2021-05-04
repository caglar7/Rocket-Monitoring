using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class testScript : MonoBehaviour
{

    SerialPort sp = new SerialPort("COM6", 9600);

    bool readAvailable = true;
    float readPeriod = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        sp.Open();
        sp.ReadTimeout = 1;
    }

    // Update is called once per frame
    void Update()
    {
        readPeriod -= Time.deltaTime;
        if(readPeriod <= 0f)
        {
            readPeriod = 0.5f;
            readAvailable = true;
        }

        if (readAvailable)
        {
            readAvailable = false;
        }
        else
            return;

        if(sp.IsOpen)
        {
            try
            {
                Debug.Log(sp.ReadLine());
            }catch(System.Exception)
            {

            }
        }    

    }
}
