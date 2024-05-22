using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoxelWater;
using Debug = UnityEngine.Debug;

public class DiagnosticGrid : MonoBehaviour
{
    public string filename = "diagnostic.txt";
    private StreamReader fileReader;
    private StreamWriter fileWriter;
    public GridManager grid;
    public Diagnostic diagnostic;
    public int ThreadNum = 1;
    public int GridSizeNum = 1;

    void Start()
    {
        GridSizeNumber();
    }

    private void ThreadNumber()
    {
        grid = GameObject.Find("GridManager").GetComponent<GridManager>();
        diagnostic = GameObject.Find("Diagnostic").GetComponent<Diagnostic>();

        string filePath = Application.dataPath + "/" + filename;
        fileReader = new StreamReader(filePath);

        ThreadNum = Int32.Parse(fileReader.ReadToEnd());
        fileReader.Close();

        if (ThreadNum == 0)
            EditorApplication.isPlaying = false;

        diagnostic.filename = grid.GridSize + "b" + ThreadNum + "ng.csv";
        grid.ThreadNum = ThreadNum;
    }

    private void GridSizeNumber(int threads = 6)
    {
        grid = GameObject.Find("GridManager").GetComponent<GridManager>();
        diagnostic = GameObject.Find("Diagnostic").GetComponent<Diagnostic>();

        string filePath = Application.dataPath + "/" + filename;
        fileReader = new StreamReader(filePath);

        GridSizeNum = Int32.Parse(fileReader.ReadToEnd());
        fileReader.Close();

        if (GridSizeNum == 0)
            EditorApplication.isPlaying = false;

        int[] gridSizeArr = { 3, 5, 7, 11, 21, 51 };
        int gridSize = gridSizeArr[GridSizeNum - 1];
        diagnostic.filename = gridSize + "b" + threads + "_1.csv";
        grid.ThreadNum = threads;
        grid.GridSize = gridSize;
    }

    public void EndProcess()
    {
        string filePath = Application.dataPath + "/" + filename;
        fileWriter = new StreamWriter(filePath);
        fileWriter.WriteLine(ThreadNum-1);
        //fileWriter.WriteLine(GridSizeNum - 1);
        fileWriter.Close();

        SceneManager.LoadScene("PerformanceTest2");
    }
}
