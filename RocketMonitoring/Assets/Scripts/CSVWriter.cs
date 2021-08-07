using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// THIS IS AN EXAMPLE EXCEL FILE CODE

public class CSVWriter : MonoBehaviour
{
    string fileName = "";

    // Start is called before the first frame update
    void Start()
    {
        fileName = Application.dataPath + "/test.csv";
    }


    private void WriteCSV()
    {
        TextWriter textWriter = new StreamWriter(fileName, false);
        textWriter.WriteLine("Roc_Lat" + ";" + "Roc_Long" + ";" + "Altitude" + ";" + "Velocity");
        textWriter.Close();

        textWriter = new StreamWriter(fileName, true);
        textWriter.WriteLine("34,23333; 23,3434342; 3500; 132,2");
        textWriter.WriteLine("34,23333; 23,3434342; 3500; 132,2");
        textWriter.WriteLine("35,35353; 23,3434342; 3500; 132,2");
        textWriter.Close();
    }
}
