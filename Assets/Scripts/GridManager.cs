using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;

namespace VoxelWater
{
    public class GridManager : MonoBehaviour
    {
        Grid[,,] Grids;
        public List<Grid> GridsList;
        public Grid[,] GridsParallel;
        public int[] GridsCount;
        public GameObject GridPrefab;
        //(GridSize -1 )/2
        //nelyginis
        public int GridSize = 21;
        //(GridManagerSize -1 )/2
        public int GridOffset = 50;
        //nelyginis
        public int GridManagerSize = 101;

        void Awake()
        {
            Grids = new Grid[GridManagerSize, GridManagerSize, GridManagerSize];
            GridsParallel = new Grid[7, (int)((GridManagerSize * GridManagerSize * GridManagerSize) / 7)];
            GridsCount = new int[7];
            GridsList = new List<Grid>();
            GridOffset = (GridManagerSize - 1) / 2;
        }

        private void Update()
        {
            UpdateGridsParallel();
        }

        private void UpdateGrids()
        {
            int count = GridsList.Count;
            for(int i=0; i<count; i++)
            {
                Grid grid = GridsList[i];
                grid.UpdateCellsInfo();
            }
        }

        private void UpdateGridsParallel()
        {
            for (int i = 0; i < 7; i++)
            {
                if(GridsCount[i]!=0)
                    UpdateGridCategory(i);
            }
        }

        private void UpdateGridCategory(int ind)
        {
            int count = GridsCount[ind];

            //first update
            for (int j = 0; j < count; j++)
            {
                GridsParallel[ind, j].UpdateGridCellsInfo();
            }

            //parallel update
            /*
            int gridSizeFull = GridSize * GridSize * GridSize;
            int gridSizeFullCI = (GridSize+2) * (GridSize + 2) * (GridSize + 2);
            CellInfo[] newCellsArr = new CellInfo[gridSizeFull * count];
            int[] newCellsCountArr = new int[count];
            CellInfo[] updatedCellsArr = new CellInfo[gridSizeFull * count];
            int[] updatedCellsCountArr = new int[count];

            CellInfo[] cellsInfo_listArr = new CellInfo[gridSizeFull * count];
            int[] cellsInfoCountArr = new int[count];
            CellInfo[] cellsInfoArr = new CellInfo[gridSizeFullCI * count];
            GridInfo[] gridInfoArr = new GridInfo[count];
            //copy all
            for (int j = 0; j < count; j++)
            {
                Grid grid = GridsParallel[ind, j];
                int index1 = j*gridSizeFull;
                cellsInfoCountArr[j] = grid.CellsInfoCount;
                for(int k=0; k< cellsInfoCountArr[j]; k++)
                {
                    cellsInfo_listArr[k + index1] = grid.CellsInfo_list[k];
                }
                int index2 = j * gridSizeFullCI;
                for (int k = 0; k < gridSizeFullCI; k++)
                {
                    cellsInfoArr[k + index2] = grid.CellsInfo[k];
                }
                gridInfoArr[j] = grid.GridInfo;
            }
            //mashalah
            for (int j = 0; j < count; j++)
            {
                Grid.UpdateGridCellState(j, ref newCellsArr, ref newCellsCountArr, ref updatedCellsArr, ref updatedCellsCountArr,
                                        ref cellsInfo_listArr, cellsInfoCountArr, ref cellsInfoArr, gridInfoArr);
            }
            */
            int gridSizeFull = GridSize * GridSize * GridSize;
            int gridSizeFullCI = (GridSize + 2) * (GridSize + 2) * (GridSize + 2);
            NativeArray<CellInfo> newCellsArr = new NativeArray<CellInfo>(gridSizeFull * count, Allocator.TempJob);
            NativeArray<int> newCellsCountArr = new NativeArray<int>(count, Allocator.TempJob);
            NativeArray<CellInfo> updatedCellsArr = new NativeArray<CellInfo>(gridSizeFull * count, Allocator.TempJob);
            NativeArray<int> updatedCellsCountArr = new NativeArray<int>(count, Allocator.TempJob);

            NativeArray<CellInfo> cellsInfo_listArr = new NativeArray<CellInfo>(gridSizeFull * count, Allocator.TempJob);
            NativeArray<int> cellsInfoCountArr = new NativeArray<int>(count, Allocator.TempJob);
            NativeArray<CellInfo> cellsInfoArr = new NativeArray<CellInfo>(gridSizeFullCI * count, Allocator.TempJob);
            NativeArray<GridInfo> gridInfoArr = new NativeArray<GridInfo>(count, Allocator.TempJob);
            //copy all
            for (int j = 0; j < count; j++)
            {
                Grid grid = GridsParallel[ind, j];
                int index1 = j * gridSizeFull;
                cellsInfoCountArr[j] = grid.CellsInfoCount;
                for (int k = 0; k < cellsInfoCountArr[j]; k++)
                {
                    cellsInfo_listArr[k + index1] = grid.CellsInfo_list[k];
                }
                int index2 = j * gridSizeFullCI;
                for (int k = 0; k < gridSizeFullCI; k++)
                {
                    cellsInfoArr[k + index2] = grid.CellsInfo[k];
                }
                gridInfoArr[j] = grid.GridInfo;
            }
            //mashalah
            
            UpdateGridsParallel update = new UpdateGridsParallel
            {
                newCellsArr = newCellsArr,
                newCellsCountArr = newCellsCountArr,
                updatedCellsArr = updatedCellsArr,
                updatedCellsCountArr = updatedCellsCountArr,

                cellsInfo_listArr = cellsInfo_listArr,
                cellsInfoCountArr = cellsInfoCountArr,
                cellsInfoArr = cellsInfoArr,
                gridInfoArr = gridInfoArr,
            };

            JobHandle dependency = new JobHandle();
            JobHandle scheduledependency = update.Schedule(0, dependency);
            JobHandle scheduleparalleljob = update.ScheduleParallel(count, 1, scheduledependency);

            scheduleparalleljob.Complete();
            
            /*
            for (int j = 0; j < count; j++)
            {
                Grid.UpdateGridCellState2(j, ref newCellsArr, ref newCellsCountArr, ref updatedCellsArr, ref updatedCellsCountArr,
                                        ref cellsInfo_listArr, cellsInfoCountArr, ref cellsInfoArr, gridInfoArr);
            }
            */
            //last update
            for (int j = 0; j < count; j++)
            {
                GridsParallel[ind, j].CreateAndUpdateGridCells(j, ref newCellsArr, ref newCellsCountArr, ref updatedCellsArr, ref updatedCellsCountArr,
                                        ref cellsInfo_listArr, ref cellsInfoArr);
            }

            newCellsArr.Dispose();
            newCellsCountArr.Dispose();
            updatedCellsArr.Dispose();
            updatedCellsCountArr.Dispose();

            cellsInfo_listArr.Dispose();
            cellsInfoCountArr.Dispose();
            cellsInfoArr.Dispose();
            gridInfoArr.Dispose();
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
                //PutIntoGridInfo(Grids[X,Y,Z], Grids[X + 1, Y, Z].Cells[newx, y, z], x,y,z);
                return Grids[X + 1, Y, Z].Cells[newx, y,z];
            }
            else if (x <= -1)
            {
                if (Grids[X - 1, Y, Z] == null)
                    return null;
                
                int newx = x + GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X - 1, Y, Z].Cells[newx, y, z], x, y, z);
                return Grids[X - 1, Y, Z].Cells[newx, y, z];
            }
            else if (y >= GridSize)
            {
                if (Grids[X, Y + 1, Z] == null)
                    return null;

                int newy = y - GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y + 1, Z].Cells[x, newy, z], x, y, z);
                return Grids[X, Y + 1, Z].Cells[x, newy, z];
            }
            else if (y <= -1)
            {
                if (Grids[X, Y - 1, Z] == null)
                    return null;

                int newy = y + GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y - 1, Z].Cells[x, newy, z], x, y, z);
                return Grids[X, Y - 1, Z].Cells[x, newy, z];
            }
            else if (z >= GridSize)
            {
                if (Grids[X, Y, Z + 1] == null)
                {
                    return null;
                }

                int newz = z - GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y, Z + 1].Cells[x, y, newz], x, y, z);
                return Grids[X, Y, Z + 1].Cells[x, y, newz];
            }
            else if (z <= -1)
            {
                if (Grids[X, Y, Z - 1] == null)
                {
                    return null;
                }

                int newz = z + GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y, Z - 1].Cells[x, y, newz], x, y, z);
                return Grids[X, Y, Z - 1].Cells[x, y, newz];
            }
            return Grids[X, Y, Z].Cells[x, y, z];
        }

        //temporary
        public void PutIntoGridInfo(Grid grid, Cell cell, int x, int y, int z)
        {
            if (cell == null)
                return;
            else
            {
                CellInfoUtility.Put(cell.Cellinfo, x + 1, y + 1, z + 1, grid.GridSizeCI, grid.CellsInfo);
            }
        }

        public void CreateGrid(int X, int Y, int Z)
        {
            GameObject newGrid = Instantiate(GridPrefab, transform);
            newGrid.transform.position = new Vector3(X, Y, Z);

            Grid gridScript = newGrid.GetComponent<Grid>();
            gridScript.Initiate(X, Y, Z);

            PutIntoGridManager(X, Y, Z, gridScript);
        }

        public void PutIntoGridManager(int X, int Y, int Z, Grid grid)
        {
            //expansion?
            Grids[X+GridOffset, Y + GridOffset, Z + GridOffset] = grid;
            //list
            GridsList.Add(grid);
            int num = grid.GridInfo.Num;
            GridsParallel[num, GridsCount[num]] = grid;
            GridsCount[num]++;
        }
    }
}
