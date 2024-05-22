using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using VoxelWater;
using Debug = UnityEngine.Debug;

public class Diagnostic : MonoBehaviour
{
    public string filename = "test.csv";
    public string filename1 = "test.csv";
    public string filename2 = "test.csv";
    public int CellCountOld = -1;
    public int CellCount = 0;
    public float QuitTime = 60;

    private float totalElapsedTime = 0f;
    private StreamWriter fileWriter;
    private StreamWriter fileWriter1;
    private StreamWriter fileWriter2;

    private string frameTimesCsv = "";
    public string frameTimesCsv1 = "";
    public string frameTimesCsv2 = "";
    private string totalElapsedTimesCsv = "";
    private string cellCountCsv = "";
    public string cellCountCsv1 = "";
    public string cellCountCsv2 = "";

    Stopwatch timer;

    public bool stopWithCount = false;
    public int maxCount = 0;

    public DiagnosticGrid DiagnosticGrid;

    void Start()
    {
        //DiagnosticGrid = GameObject.Find("DiagnosticGrid").GetComponent<DiagnosticGrid>();
        string filePath = Application.dataPath + "/"+ filename;
        string filePath1 = Application.dataPath + "/" + filename1;
        string filePath2 = Application.dataPath + "/" + filename2;
        fileWriter = new StreamWriter(filePath);
        fileWriter1 = new StreamWriter(filePath1);
        fileWriter2 = new StreamWriter(filePath2);

        timer = new Stopwatch();
        timer.Start();
    }

    void Update()
    {
        CellCountTimer();
    }

    void CellCountTimer()
    {
        timer.Stop();

        TimeSpan timeTaken = timer.Elapsed;


        float frameTimeMs = Time.deltaTime * 1000f;

        totalElapsedTime = Time.time;

        frameTimesCsv += timeTaken.TotalMilliseconds.ToString() + ",";

        totalElapsedTimesCsv += totalElapsedTime.ToString("F2") + ",";

        cellCountCsv += CellCount.ToString() + ",";

        //Debug.Log("Frame Time: " + frameTimeMs.ToString("F2") + " ms | Total Elapsed Time: " + totalElapsedTime.ToString("F2") + " s | Stopwatch: "+ timeTaken.TotalMilliseconds.ToString() + " CellCount: " +CellCount.ToString());

        timer.Restart();
        timer.Start();

        if (totalElapsedTime >= QuitTime || (stopWithCount && CellCount == CellCountOld))
        {
            if (fileWriter != null)
            {
                fileWriter.WriteLine(frameTimesCsv.TrimEnd(',') + "\n" + cellCountCsv.TrimEnd(','));
                fileWriter.Close();
                fileWriter1.WriteLine(frameTimesCsv1.TrimEnd(',') + "\n" + cellCountCsv1.TrimEnd(','));
                fileWriter1.Close();
                fileWriter2.WriteLine(frameTimesCsv2.TrimEnd(',') + "\n" + cellCountCsv2.TrimEnd(','));
                fileWriter2.Close();
            }
            //DiagnosticGrid.EndProcess();
            //Debug.Log("quit");
            EditorApplication.isPlaying = false;
        }
        CellCountOld = CellCount;
    }

    public void ProcessTimer(Stopwatch timer1)
    {
        TimeSpan timeTaken = timer1.Elapsed;


        float frameTimeMs = Time.deltaTime * 1000f;
        //Debug.Log(timeTaken.TotalMilliseconds.ToString());

        totalElapsedTime = Time.time;

        frameTimesCsv += timeTaken.TotalMilliseconds.ToString() + ",";

        totalElapsedTimesCsv += totalElapsedTime.ToString("F2") + ",";

        cellCountCsv += CellCount.ToString() + ",";

        //Debug.Log("Frame Time: " + frameTimeMs.ToString("F2") + " ms | Total Elapsed Time: " + totalElapsedTime.ToString("F2") + " s | Stopwatch: "+ timeTaken.TotalMilliseconds.ToString() + " CellCount: " +CellCount.ToString());

        if (totalElapsedTime >= QuitTime || (stopWithCount && CellCount >= maxCount))
        {
            if (fileWriter != null)
            {
                //fileWriter.WriteLine(frameTimesCsv.TrimEnd(',') + "\n" + totalElapsedTimesCsv.TrimEnd(','));
                fileWriter.WriteLine(frameTimesCsv.TrimEnd(',') + "\n" + cellCountCsv.TrimEnd(','));
                fileWriter.Close();
            }
            //DiagnosticGrid.EndProcess();
            //Debug.Log("quit");
            EditorApplication.isPlaying = false;
        }
        CellCountOld = CellCount;
    }

    public void ProcessTimer1(Stopwatch timer1)
    {
        TimeSpan timeTaken = timer1.Elapsed;
        frameTimesCsv1 += timeTaken.TotalMilliseconds.ToString() + ",";
        cellCountCsv1 += CellCount.ToString() + ",";
    }

    public void ProcessTimer2(Stopwatch timer1)
    {
        TimeSpan timeTaken = timer1.Elapsed;
        frameTimesCsv2 += timeTaken.TotalMilliseconds.ToString() + ",";
        cellCountCsv2 += CellCount.ToString() + ",";
    }
    /*
    void OnApplicationQuit()
    {
        if (fileWriter != null)
        {
            //fileWriter.WriteLine(frameTimesCsv.TrimEnd(',') + "\n" + totalElapsedTimesCsv.TrimEnd(','));
            fileWriter.WriteLine(frameTimesCsv.TrimEnd(',') + "\n" + cellCountCsv.TrimEnd(','));
            fileWriter.Close();
        }
    }
    */


    public void IncreaseCellCount()
    {
        CellCount++;
    }
}
