using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace VoxelWater
{
    public class GridManager : MonoBehaviour
    {
        Grid[,,] Grids;
        //public List<Grid> GridsList;
        public GameObject GridPrefab;

        //for parallel work
        public Grid[,] GridsParallel;
        public int[] GridsCount;
        
        //(GridSize -1 )/2
        //nelyginis
        public int GridSize = 21;
        //(GridManagerSize -1 )/2
        public int GridOffset = 50;
        //nelyginis
        public int GridManagerSize = 101;
        /*
        private Stopwatch timer1;
        private Stopwatch timer2;
        public Diagnostic Diagnostics;
        */
        public int ThreadNum = 7;

        void Awake()
        {
            UnityEngine.Debug.Log("threads"+JobsUtility.JobWorkerMaximumCount);
            Grids = new Grid[GridManagerSize, GridManagerSize, GridManagerSize];
            GridsParallel = new Grid[7, (int)((GridManagerSize * GridManagerSize * GridManagerSize) / 7)];
            GridsCount = new int[7];
            //GridsList = new List<Grid>();
            GridOffset = (GridManagerSize - 1) / 2;

            //FindPlacedCells();
        }

        private void Update()
        {
            UpdateGridsParallel();
        }

        private void FindPlacedCells()
        {
            PlacedCell[] placedCellsScript = FindObjectsOfType(typeof(PlacedCell)) as PlacedCell[];
            Vector3 center = FindPlacedCellsCenter(placedCellsScript).gameObject.transform.localPosition;
            Grid grid = CreateGrid((int)center.x, (int)center.y, (int)center.z);
            //grid.CreateCell((int)center.x, (int)center.y, (int)center.z, 1);
        }

        private PlacedCell FindPlacedCellsCenter(PlacedCell[] placedCells)
        {
            float sumX = 0;
            float sumY = 0;
            float sumZ = 0;
            foreach(var cell in placedCells)
            {
                Vector3 position = cell.gameObject.transform.localPosition;
                sumX += position.x;
                sumY += position.y;
                sumZ += position.z;
            }
            float x = sumX / placedCells.Length;
            float y = sumY / placedCells.Length;
            float z = sumZ / placedCells.Length;
            Vector3 absolutePosition = new Vector3(x, y, z);

            Vector3 centerPosition = Vector3.zero;
            PlacedCell centerCell = null;
            foreach (var cell in placedCells)
            {
                Vector3 position = cell.gameObject.transform.localPosition;
                if (centerCell == null)
                {
                    centerPosition = cell.gameObject.transform.localPosition;
                    centerCell = cell;
                }
                else if(PositionDifference(position,absolutePosition) < 
                        PositionDifference(centerPosition, absolutePosition))
                {
                    centerPosition = cell.gameObject.transform.localPosition;
                    centerCell = cell;
                }
            }

            return centerCell;
        }

        private float PositionDifference(Vector3 pos1, Vector3 pos2)
        {
            return Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y) + Math.Abs(pos1.z - pos2.z);
        }

        private void UpdateGridsParallel()
        {
            for (int i = 0; i < 7; i++)
            {
                if (GridsCount[i] != 0)
                    UpdateGridCategory(i);
            }
        }

        private void UpdateGridCategory(int ind)
        {
            int count = GridsCount[ind];
            int countActive = 0;
            //first update
            for (int j = 0; j < count; j++)
            {
                if (GridsParallel[ind, j].GridInfo.Active)
                {
                    GridsParallel[ind, j].UpdateGridCellsInfo();
                    countActive++;
                }
            }

            if (countActive == 0)
                return;
           
            for (int j = 0; j < count; j++)
            {
                Grid grid = GridsParallel[ind, j];
                if (grid.GridInfo.Active == false)
                {
                    continue;
                }
                GridUtility.UpdateGrid(grid.GridInfo, grid.CellsInfo_list, grid.CellsInfoCount, grid.CellsInfo, grid.Colliders,
                                       out CellInfo[] newCells, out int newCellsCount, out CellInfo[] updatedCells, out int updatedCellsCount);

                
                GridsParallel[ind, j].CreateAndUpdateGridCells(grid.GridInfo, grid.CellsInfo_list, grid.CellsInfoCount, grid.CellsInfo, grid.Colliders,
                                                          newCells,   newCellsCount,   updatedCells,  updatedCellsCount);
            }
        }

        public Grid GetGrid(int x, int y, int z, int Xorg, int Yorg, int Zorg)
        {
            int X = Xorg + GridOffset;
            int Y = Yorg + GridOffset;
            int Z = Zorg + GridOffset;

            if (x >= GridSize)
            {
                if (Grids[X + 1, Y, Z] == null)
                    CreateGrid(Xorg + 1, Yorg, Zorg);

                return Grids[X + 1, Y, Z];
            }
            else if (x <= -1)
            {
                if (Grids[X - 1, Y, Z] == null)
                    CreateGrid(Xorg - 1, Yorg, Zorg);

                return Grids[X - 1, Y, Z];
            }
            else if (y >= GridSize)
            {
                if (Grids[X, Y + 1, Z] == null)
                    CreateGrid(Xorg, Yorg + 1, Zorg);

                return Grids[X, Y + 1, Z];
            }
            else if (y <= -1)
            {
                if (Grids[X, Y - 1, Z] == null)
                    CreateGrid(Xorg, Yorg - 1, Zorg);

                return Grids[X, Y - 1, Z];
            }
            else if (z >= GridSize)
            {
                if (Grids[X, Y, Z + 1] == null)
                    CreateGrid(Xorg, Yorg, Zorg + 1);

                return Grids[X, Y, Z + 1];
            }
            else if (z <= -1)
            {
                if (Grids[X, Y, Z - 1] == null)
                    CreateGrid(Xorg, Yorg, Zorg - 1);

                return Grids[X, Y, Z - 1];
            }
            return null;
        }

        public Cell GetCell(int x, int y, int z, int Xorg, int Yorg, int Zorg)
        {
            int X = Xorg + GridOffset;
            int Y = Yorg + GridOffset;
            int Z = Zorg + GridOffset;

            if (x >= GridSize)
            {
                if (Grids[X + 1, Y, Z] == null)
                    return null;

                int newx = x - GridSize;
                return Grids[X + 1, Y, Z].Cells[newx, y,z];
            }
            else if (x <= -1)
            {
                if (Grids[X - 1, Y, Z] == null)
                    return null;
                
                int newx = x + GridSize;
                return Grids[X - 1, Y, Z].Cells[newx, y, z];
            }
            else if (y >= GridSize)
            {
                if (Grids[X, Y + 1, Z] == null)
                    return null;

                int newy = y - GridSize;
                return Grids[X, Y + 1, Z].Cells[x, newy, z];
            }
            else if (y <= -1)
            {
                if (Grids[X, Y - 1, Z] == null)
                    return null;

                int newy = y + GridSize;
                return Grids[X, Y - 1, Z].Cells[x, newy, z];
            }
            else if (z >= GridSize)
            {
                if (Grids[X, Y, Z + 1] == null)
                {
                    return null;
                }

                int newz = z - GridSize;
                return Grids[X, Y, Z + 1].Cells[x, y, newz];
            }
            else if (z <= -1)
            {
                if (Grids[X, Y, Z - 1] == null)
                {
                    return null;
                }

                int newz = z + GridSize;
                return Grids[X, Y, Z - 1].Cells[x, y, newz];
            }
            return Grids[X, Y, Z].Cells[x, y, z];
        }

        public Grid CreateGrid(int x, int y, int z)
        {
            GameObject newGrid = Instantiate(GridPrefab, transform);
            newGrid.transform.position = new Vector3(x, y, z);

            Grid gridScript = newGrid.GetComponent<Grid>();
            gridScript.Initiate(x, y, z);

            PutIntoGridManager(x, y, z, gridScript);

            return gridScript;
        }

        public void PutIntoGridManager(int x, int y, int z, Grid grid)
        {
            //expansion?
            Grids[x+GridOffset, y + GridOffset, z + GridOffset] = grid;
            //list
            //GridsList.Add(grid);
            int num = grid.GridInfo.Num;
            GridsParallel[num, GridsCount[num]] = grid;
            GridsCount[num]++;
        }
    }
}
