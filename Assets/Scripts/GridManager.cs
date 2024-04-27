using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace VoxelWater
{
    public class GridManager : MonoBehaviour
    {
        Grid[,,] Grids;
        public List<Grid> GridsList;
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

        private void UpdateGridsParallel()
        {
            for (int i = 0; i < 7; i++)
            {
                //fix this where array is only filled with active grids
                if(GridsCount[i]!=0)
                    UpdateGridCategoryOptimized(i);
            }
        }

        private void UpdateGridCategory(int ind)
        {
            int count = GridsCount[ind];
            int countActive = 0;
            //first update
            for (int j = 0; j < count; j++)
            {
                if(GridsParallel[ind, j].GridInfo.Active)
                {
                    GridsParallel[ind, j].UpdateGridCellsInfo();
                    countActive++;
                } 
            }
            Debug.Log(countActive);
            
            if (countActive == 0)
                return;

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

            NativeArray<bool> collidersArr = new NativeArray<bool>(gridSizeFullCI * count, Allocator.TempJob);
            //copy all
            for (int j = 0; j < count; j++)
            {
                Grid grid = GridsParallel[ind, j];
                if (GridsParallel[ind, j].GridInfo.Active == false)
                    continue;
                
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
                    collidersArr[k + index2] = grid.Colliders[k];
                }
                gridInfoArr[j] = grid.GridInfo;
            }
            
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

                collidersArr = collidersArr,
            };

            JobHandle dependency = new JobHandle();
            JobHandle scheduledependency = update.Schedule(0, dependency);
            JobHandle scheduleparalleljob = update.ScheduleParallel(count, 1, scheduledependency);

            scheduleparalleljob.Complete();
            
            //last update
            for (int j = 0; j < count; j++)
            {
                if (GridsParallel[ind, j].GridInfo.Active == false)
                    continue;
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

            collidersArr.Dispose();
        }

        private void UpdateGridCategoryOptimized(int ind)
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
            //Debug.Log(countActive);

            if (countActive == 0)
                return;

            int gridSizeFull = GridSize * GridSize * GridSize;
            int gridSizeFullCI = (GridSize + 2) * (GridSize + 2) * (GridSize + 2);
            NativeArray<CellInfo> newCellsArr = new NativeArray<CellInfo>(gridSizeFull * countActive, Allocator.TempJob);
            NativeArray<int> newCellsCountArr = new NativeArray<int>(countActive, Allocator.TempJob);
            NativeArray<CellInfo> updatedCellsArr = new NativeArray<CellInfo>(gridSizeFull * countActive, Allocator.TempJob);
            NativeArray<int> updatedCellsCountArr = new NativeArray<int>(countActive, Allocator.TempJob);

            NativeArray<CellInfo> cellsInfo_listArr = new NativeArray<CellInfo>(gridSizeFull * countActive, Allocator.TempJob);
            NativeArray<int> cellsInfoCountArr = new NativeArray<int>(countActive, Allocator.TempJob);
            NativeArray<CellInfo> cellsInfoArr = new NativeArray<CellInfo>(gridSizeFullCI * countActive, Allocator.TempJob);
            NativeArray<GridInfo> gridInfoArr = new NativeArray<GridInfo>(countActive, Allocator.TempJob);

            NativeArray<bool> collidersArr = new NativeArray<bool>(gridSizeFullCI * countActive, Allocator.TempJob);
            //copy all
            int activeIndex = 0;
            for (int j = 0; j < count; j++)
            {
                Grid grid = GridsParallel[ind, j];
                if (grid.GridInfo.Active == false)
                {
                    continue;
                }     

                int index1 = activeIndex * gridSizeFull;
                cellsInfoCountArr[activeIndex] = grid.CellsInfoCount;
                for (int k = 0; k < cellsInfoCountArr[activeIndex]; k++)
                {
                    cellsInfo_listArr[k + index1] = grid.CellsInfo_list[k];
                }
                int index2 = activeIndex * gridSizeFullCI;
                for (int k = 0; k < gridSizeFullCI; k++)
                {
                    cellsInfoArr[k + index2] = grid.CellsInfo[k];
                    collidersArr[k + index2] = grid.Colliders[k];
                }
                gridInfoArr[activeIndex] = grid.GridInfo;
                activeIndex++;
            }

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

                collidersArr = collidersArr,
            };

            JobHandle dependency = new JobHandle();
            JobHandle scheduledependency = update.Schedule(0, dependency);
            JobHandle scheduleparalleljob = update.ScheduleParallel(countActive, 1, scheduledependency);

            scheduleparalleljob.Complete();

            //last update
            activeIndex = 0;
            for (int j = 0; j < count; j++)
            {
                if (GridsParallel[ind, j].GridInfo.Active == false)
                    continue;
                GridsParallel[ind, j].CreateAndUpdateGridCells(activeIndex, ref newCellsArr, ref newCellsCountArr, ref updatedCellsArr, ref updatedCellsCountArr,
                                        ref cellsInfo_listArr, ref cellsInfoArr);
                activeIndex++;
            }

            newCellsArr.Dispose();
            newCellsCountArr.Dispose();
            updatedCellsArr.Dispose();
            updatedCellsCountArr.Dispose();

            cellsInfo_listArr.Dispose();
            cellsInfoCountArr.Dispose();
            cellsInfoArr.Dispose();
            gridInfoArr.Dispose();

            collidersArr.Dispose();
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

        public void CreateGrid(int x, int y, int z)
        {
            GameObject newGrid = Instantiate(GridPrefab, transform);
            newGrid.transform.position = new Vector3(x, y, z);

            Grid gridScript = newGrid.GetComponent<Grid>();
            gridScript.Initiate(x, y, z);

            PutIntoGridManager(x, y, z, gridScript);
        }

        public void PutIntoGridManager(int x, int y, int z, Grid grid)
        {
            //expansion?
            Grids[x+GridOffset, y + GridOffset, z + GridOffset] = grid;
            //list
            GridsList.Add(grid);
            int num = grid.GridInfo.Num;
            GridsParallel[num, GridsCount[num]] = grid;
            GridsCount[num]++;
        }
    }
}
