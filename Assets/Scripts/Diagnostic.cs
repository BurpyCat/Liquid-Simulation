using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Diagnostic : MonoBehaviour
{
    public string filename = "test.csv";
    public int CellCount = 0;
    public float QuitTime = 60;

    private float totalElapsedTime = 0f;
    private StreamWriter fileWriter;
    
    private string frameTimesCsv = "";
    private string totalElapsedTimesCsv = "";
    private string cellCountCsv = "";

    Stopwatch timer;


    void Start()
    {
        string filePath = Application.dataPath + "/"+ filename;
        fileWriter = new StreamWriter(filePath);

        timer = new Stopwatch();
        timer.Start();
    }

    void Update()
    {
        timer.Stop();

        TimeSpan timeTaken = timer.Elapsed;


        float frameTimeMs = Time.deltaTime * 1000f;

        totalElapsedTime = Time.time;

        frameTimesCsv += timeTaken.TotalMilliseconds.ToString() + ",";

        totalElapsedTimesCsv += totalElapsedTime.ToString("F2") + ",";

        cellCountCsv += CellCount.ToString() + ",";

        Debug.Log("Frame Time: " + frameTimeMs.ToString("F2") + " ms | Total Elapsed Time: " + totalElapsedTime.ToString("F2") + " s | Stopwatch: "+ timeTaken.TotalMilliseconds.ToString() + " CellCount: " +CellCount.ToString());

        timer.Restart();
        timer.Start();

        if (totalElapsedTime >= QuitTime)
        {
            Debug.Log("quit");
            EditorApplication.isPlaying = false;
        }
    }

    void OnApplicationQuit()
    {
        if (fileWriter != null)
        {
            //fileWriter.WriteLine(frameTimesCsv.TrimEnd(',') + "\n" + totalElapsedTimesCsv.TrimEnd(','));
            fileWriter.WriteLine(frameTimesCsv.TrimEnd(',') + "\n" + cellCountCsv.TrimEnd(','));
            fileWriter.Close();
        }
    }


    public void IncreaseCellCount()
    {
        CellCount++;
    }
}
