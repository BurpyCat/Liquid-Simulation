using System.Collections.Generic;
using UnityEngine;
using System;
using System.Dynamic;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;
using Unity.Jobs;
using System.Diagnostics;
using Unity.VisualScripting;
using Unity.Collections;

namespace VoxelWater
{
    [Serializable]
    public struct GridInfo
    {
        //first grid
        public int X;
        public int Y;
        public int Z;

        public int Num;

        //(GridSize - 1) /2
        public int Offset;
        //nelyginiai!
        public int GridSize;
        //(GridManagerSize - 1) /2
        public int GridOffset;

        public int GridSizeCI;
        public int OffsetCI;

        public bool Active;
    }

    public class Grid : MonoBehaviour
    {
        private List<Cell> Cells_list;
        public Cell[,,] Cells;

        //cellinfo
        //includes only current live cells
        public int CellsInfoCount = 0;
        public CellInfo[] CellsInfo_list;
        //public CellInfo[,,] CellsInfo;
        //includes current cells AND neighbours
        public CellInfo[] CellsInfo;
        public int GridSizeCI;
        public int OffsetCI;

        public GridManager Manager;
        public GameObject Cube;
        public GameObject GridPrefab;
        public int VolumeExcess = 0;

        //grid info
        public GridInfo GridInfo;

        //first grid
        public int X = 0;
        public int Y = 0;
        public int Z = 0;

        //(GridSize - 1) /2
        public int Offset = 50;
        //nelyginiai!
        public int GridSize = 100;
        //(GridManagerSize - 1) /2
        public int GridOffset = 50;


        public bool first = false;

        void Awake()
        {
            Initiate(0, 0, 0);
            if(first)
            {
                Manager.PutIntoGridManager(X, Y, Z, this);
            }
               
        }

        public void Initiate(int X, int Y, int Z)
        {
            Manager = GameObject.Find("GridManager").GetComponent<GridManager>();
            GridSize = Manager.GridSize;
            //+2 to include neighboring cells
            GridSizeCI = GridSize + 2;
            OffsetCI = 1;

            Offset = (GridSize - 1) / 2;
            GridOffset = Manager.GridOffset;
            Cells = new Cell[GridSize, GridSize, GridSize];
            Cells_list = new List<Cell>();

            //cellinfo      
            CellsInfo = CellInfoUtility.Create(GridSizeCI);
            CellsInfo_list = new CellInfo[GridSize* GridSize* GridSize];
            CellsInfoCount = 0;

            this.X = X;
            this.Y = Y;
            this.Z = Z;

            //fill gridInfo
            GridInfo.X = X;
            GridInfo.Y = Y;
            GridInfo.Z = Z;
            GridInfo.Offset = Offset;
            GridInfo.GridSize = GridSize;
            GridInfo.GridOffset = GridOffset;
            GridInfo.GridSizeCI = GridSizeCI;
            GridInfo.OffsetCI = OffsetCI;
            GridInfo.Num = CalcGridNumber();
            GridInfo.Active = true;
        }

        private int CalcGridNumber()
        {
            int x = (X + GridOffset) % 7;
            int y = (Y + GridOffset) % 7;
            int z = (Z + GridOffset) % 7;

            return (x + y * 3 + z * 3 * 3) % 7;
        }

        void Update()
        {
            //UpdateCellsInfo();
        }

        public void UpdateCellsInfo()
        {
            //UpdateGridCellsInfo();

            //UpdateGridCellState();

            //CreateAndUpdateGridCells();

        }

        public void UpdateGridCellsInfo()
        {
            UpdateNeighborCellInfo(CellsInfo);
            UpdateCellInfo(CellsInfo_list, CellsInfoCount, CellsInfo, GridInfo);
        }

        public static void UpdateGridCellState(int ind, ref CellInfo[] newCellsArr, ref int[] newCellsCountArr, ref CellInfo[] updatedCellsArr, ref int[] updatedCellsCountArr,
                                        ref CellInfo[] cellsInfo_listArr, int[] cellsInfoCountArr, ref CellInfo[] cellsInfoArr, GridInfo[] gridInfoArr)
        {
            //copy from bigger arrays
            GridInfo gridInfo = gridInfoArr[ind];
            int fullGridSize = gridInfo.GridSize * gridInfo.GridSize * gridInfo.GridSize;
            int fullGridSizeCI = gridInfo.GridSizeCI * gridInfo.GridSizeCI * gridInfo.GridSizeCI;

            int cellsInfoCount = cellsInfoCountArr[ind];
            CellInfo[] cellsInfo_list = CellInfoUtility.ExtractCellInfoArray(ind, cellsInfo_listArr, fullGridSize, cellsInfoCount);
            CellInfo[] cellsInfo = CellInfoUtility.ExtractCellInfoArray(ind, cellsInfoArr, fullGridSizeCI, fullGridSizeCI);

            CellInfo[] newCells = new CellInfo[fullGridSize];
            int newCellsCount = 0;
            CellInfo[] updatedCells = new CellInfo[fullGridSize];
            int updatedCellsCount = 0;

            //update cell state
            GridUtility.UpdateCells(cellsInfo_list, cellsInfoCount, cellsInfo, gridInfo,
                newCells, ref newCellsCount, updatedCells, ref updatedCellsCount);

            //copy to bigger arrays
            newCellsArr = CellInfoUtility.InjectCellInfoArray(ind, newCellsArr, fullGridSize, newCells, newCellsCount);
            newCellsCountArr[ind] = newCellsCount;
            updatedCellsArr = CellInfoUtility.InjectCellInfoArray(ind, updatedCellsArr, fullGridSize, updatedCells, updatedCellsCount);
            updatedCellsCountArr[ind] = updatedCellsCount;

            cellsInfo_listArr = CellInfoUtility.InjectCellInfoArray(ind, cellsInfo_listArr, fullGridSize, cellsInfo_list, cellsInfoCount);
            cellsInfoArr = CellInfoUtility.InjectCellInfoArray(ind, cellsInfoArr, fullGridSizeCI, cellsInfo, fullGridSizeCI);

            //ill kill myself jeigu kazaks cia blogai
            //i'll need to copy back cellsInfo_list, CellsInfo, newCells, ref newCellsCount, updatedCells, ref updatedCellsCount
        }

        public static void UpdateGridCellState2(int i, ref NativeArray<CellInfo> newCellsArr, ref NativeArray<int> newCellsCountArr, ref NativeArray<CellInfo> updatedCellsArr, ref NativeArray<int> updatedCellsCountArr,
                                        ref NativeArray<CellInfo> cellsInfo_listArr, NativeArray<int> cellsInfoCountArr, ref NativeArray<CellInfo> cellsInfoArr, NativeArray<GridInfo> gridInfoArr)
        {
            //copy from bigger arrays
            GridInfo gridInfo = gridInfoArr[i];
            int fullGridSize = gridInfo.GridSize * gridInfo.GridSize * gridInfo.GridSize;
            int fullGridSizeCI = gridInfo.GridSizeCI * gridInfo.GridSizeCI * gridInfo.GridSizeCI;

            int cellsInfoCount = cellsInfoCountArr[i];
            CellInfo[] cellsInfo_list = CellInfoUtility.ExtractCellInfoArray(i, cellsInfo_listArr, fullGridSize, cellsInfoCount);
            CellInfo[] cellsInfo = CellInfoUtility.ExtractCellInfoArray(i, cellsInfoArr, fullGridSizeCI, fullGridSizeCI);

            CellInfo[] newCells = new CellInfo[fullGridSize];
            int newCellsCount = 0;
            CellInfo[] updatedCells = new CellInfo[fullGridSize];
            int updatedCellsCount = 0;

            GridUtility.UpdateCells(cellsInfo_list, cellsInfoCount, cellsInfo, gridInfo,
                newCells, ref newCellsCount, updatedCells, ref updatedCellsCount);

            //copy to bigger arrays
            CellInfoUtility.InjectCellInfoArray(i, ref newCellsArr, fullGridSize, newCells, newCellsCount);
            newCellsCountArr[i] = newCellsCount;
            CellInfoUtility.InjectCellInfoArray(i, ref updatedCellsArr, fullGridSize, updatedCells, updatedCellsCount);
            updatedCellsCountArr[i] = updatedCellsCount;

            CellInfoUtility.InjectCellInfoArray(i, ref cellsInfo_listArr, fullGridSize, cellsInfo_list, cellsInfoCount);
            CellInfoUtility.InjectCellInfoArray(i, ref cellsInfoArr, fullGridSizeCI, cellsInfo, fullGridSizeCI);
        }



        public void CreateAndUpdateGridCells(int ind, ref CellInfo[] newCellsArr, ref int[] newCellsCountArr, ref CellInfo[] updatedCellsArr, ref int[] updatedCellsCountArr,
                                        ref CellInfo[] cellsInfo_listArr, ref CellInfo[] cellsInfoArr)
        {
            int fullGridSize = GridInfo.GridSize * GridInfo.GridSize * GridInfo.GridSize;
            int fullGridSizeCI = GridInfo.GridSizeCI * GridInfo.GridSizeCI * GridInfo.GridSizeCI;
            //copy cellsinfo list and the other
            CellsInfo_list = CellInfoUtility.ExtractCellInfoArray(ind, cellsInfo_listArr, fullGridSize, fullGridSize);
            CellsInfo = CellInfoUtility.ExtractCellInfoArray(ind, cellsInfoArr, fullGridSizeCI, fullGridSizeCI);

            //extract new and updated cells

            int newCellsCount = newCellsCountArr[ind];
            CellInfo[] newCells = CellInfoUtility.ExtractCellInfoArray(ind, newCellsArr, fullGridSize, newCellsCount);

            int updatedCellsCount = updatedCellsCountArr[ind];
            CellInfo[] updatedCells = CellInfoUtility.ExtractCellInfoArray(ind, updatedCellsArr, fullGridSize, updatedCellsCount);


            //create all new cells in environment
            CreateNewCells(newCells, newCellsCount);
            //update all cell objects
            UpdateCellObjects(updatedCells, updatedCellsCount, CellsInfo, GridInfo);
            //update neighboring cells 
            UpdateNeighborCellObjects(CellsInfo);
        }

        public void CreateAndUpdateGridCells(int ind, ref NativeArray<CellInfo> newCellsArr, ref NativeArray<int> newCellsCountArr, ref NativeArray<CellInfo> updatedCellsArr, ref NativeArray<int> updatedCellsCountArr,
                                        ref NativeArray<CellInfo> cellsInfo_listArr, ref NativeArray<CellInfo> cellsInfoArr)
        {
            int fullGridSize = GridInfo.GridSize * GridInfo.GridSize * GridInfo.GridSize;
            int fullGridSizeCI = GridInfo.GridSizeCI * GridInfo.GridSizeCI * GridInfo.GridSizeCI;
            //copy cellsinfo list and the other
            CellsInfo_list = CellInfoUtility.ExtractCellInfoArray(ind, cellsInfo_listArr, fullGridSize, fullGridSize);
            CellsInfo = CellInfoUtility.ExtractCellInfoArray(ind, cellsInfoArr, fullGridSizeCI, fullGridSizeCI);

            //extract new and updated cells

            int newCellsCount = newCellsCountArr[ind];
            CellInfo[] newCells = CellInfoUtility.ExtractCellInfoArray(ind, newCellsArr, fullGridSize, newCellsCount);

            int updatedCellsCount = updatedCellsCountArr[ind];
            CellInfo[] updatedCells = CellInfoUtility.ExtractCellInfoArray(ind, updatedCellsArr, fullGridSize, updatedCellsCount);

            //create all new cells in environment
            CreateNewCells(newCells, newCellsCount);
            //update all cell objects
            UpdateCellObjects(updatedCells, updatedCellsCount, CellsInfo, GridInfo);
            //update neighboring cells 
            UpdateNeighborCellObjects(CellsInfo);
            //active check
            if (newCellsCount == 0 && updatedCellsCount == 0)
                GridInfo.Active = false;
        }

        private void CreateNewCells(CellInfo[] newCells, int count)
        {
            for(int i=0; i<count; i++)
            {
                var cell = newCells[i];
                CreateCell(cell.X, cell.Y, cell.Z, cell.Volume);
            }   
        }

        private void UpdateCellObjects(CellInfo[] cellList, int count, CellInfo[] cells, GridInfo gridInfo)
        {
            for (int i = 0; i < count; i++)
            {
                var cell = cellList[i];
                int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
                int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
                int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;
                UpdateCellObject(CellInfoUtility.Get(x, y, z, GridSizeCI, cells));
            }
        }

        //can be more efficient
        private void UpdateNeighborCellObjects(CellInfo[] cells)
        {
            for (int i = 1; i < GridSizeCI - 1; i++)
                for (int j = 1; j < GridSizeCI - 1; j++)
                {
                    UpdateCellObject(0, j, i, cells);
                    UpdateCellObject(0, i, j, cells);
                    UpdateCellObject(i, j, 0, cells);
                    UpdateCellObject(j, i, 0, cells);
                    UpdateCellObject(j, 0, i, cells);
                    UpdateCellObject(i, 0, j, cells);

                    UpdateCellObject(GridSizeCI - 1, j, i, cells);
                    UpdateCellObject(GridSizeCI - 1, i, j, cells);
                    UpdateCellObject(i, j, GridSizeCI - 1, cells);
                    UpdateCellObject(j, i, GridSizeCI - 1, cells);
                    UpdateCellObject(j, GridSizeCI - 1, i, cells);
                    UpdateCellObject(i, GridSizeCI - 1, j, cells);
                }
        }

        private void UpdateCellObject(int x, int y, int z, CellInfo[] cells)
        {
            Cell cellObj = Manager.GetCell(x - OffsetCI, y - OffsetCI, z - OffsetCI, X, Y, Z);

            if (cellObj != null)
            {
                cellObj.Cellinfo = CellInfoUtility.Get(x, y, z,GridSizeCI, cells);
                //set to active
                cellObj.Grid.GridInfo.Active = true;
            }
        }

        private void UpdateCellObject(CellInfo cell)
        {
            int x = (int)cell.X + Offset - (X * GridSize);
            int y = (int)cell.Y + Offset - (Y * GridSize);
            int z = (int)cell.Z + Offset - (Z * GridSize);

            //update through manager in case the cell is in another grid
            Cell cellObj = Manager.GetCell(x, y, z, X, Y, Z);
            cellObj.Cellinfo = cell;
            //GetNeighboursInfo(cellObj);
            cellObj.RenderCell();
        }

        private void UpdateNeighborCellInfo(CellInfo[] cells)
        {
            for (int i = 1; i < GridSizeCI - 1; i++)
                for (int j = 1; j < GridSizeCI - 1; j++)
                {
                    UpdateCellInfo(0, j, i, cells);
                    UpdateCellInfo(0, i, j, cells);
                    UpdateCellInfo(i, j, 0, cells);
                    UpdateCellInfo(j, i, 0, cells);
                    UpdateCellInfo(j, 0, i, cells);
                    UpdateCellInfo(i, 0, j, cells);

                    UpdateCellInfo(GridSizeCI - 1, j, i, cells);
                    UpdateCellInfo(GridSizeCI - 1, i, j, cells);
                    UpdateCellInfo(i, j, GridSizeCI - 1, cells);
                    UpdateCellInfo(j, i, GridSizeCI - 1, cells);
                    UpdateCellInfo(j, GridSizeCI - 1, i, cells);
                    UpdateCellInfo(i, GridSizeCI - 1, j, cells);
                }
        }

        private void UpdateCellInfo(CellInfo[] cellList, int count, CellInfo[] cells, GridInfo gridInfo)
        {
            for(int i =0; i<count; i++)
            {
                var cell = cellList[i];
                int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
                int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
                int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;
                UpdateCellInfo(x, y, z, cells);
            }
        }

        private void UpdateCellInfo(int x, int y, int z, CellInfo[] cells)
        {
            Cell cellObj = Manager.GetCell(x - OffsetCI, y - OffsetCI, z - OffsetCI, X, Y, Z);

            if (cellObj != null)
            {
                CellInfoUtility.Put(cellObj.Cellinfo, x, y, z, GridSizeCI, cells);
            }
        }
        /*--------------------------------------------------------------------------------*/

        public Cell CreateCell(float x, float y, float z, int volume)
        {
            Grid grid = GetGrid((int)x, (int)y, (int)z);


            Cell cellScript = grid.CreateCellObject(x, y, z, volume);
            grid.PutIntoGrid(cellScript);
            grid.GetNeighboursInfo(cellScript);
            GridUtility.GetNeighboursInfo(cellScript.Cellinfo, CellsInfo, grid.GridInfo);
            //cellinfo
            grid.PutIntoInfoGrid(cellScript);
            //put only when created and does not include neighbor cells
            grid.PutIntoInfoList(cellScript);
            //put also neighboring cells? maybe not needed
            if (grid != this)
                this.PutIntoInfoGrid(cellScript);
            //grid.UpdateNeighboursInfo(cellScript);
            //GridUtility.UpdateNeighboursInfo(cellScript.Cellinfo, CellsInfo, grid.GridInfo);

            return cellScript;
        }

        private Cell CreateCellObject(float x, float y, float z, int volume)
        {
            GameObject newCell = Instantiate(Cube, transform);
            newCell.transform.position = new Vector3(x, y, z);
            Cell cellScript = newCell.GetComponent<Cell>();

            cellScript.Initiate();
            cellScript.Cellinfo.Volume = volume;

            return cellScript;
        }

        private Grid GetGrid(int Xorg, int Yorg, int Zorg)
        {
            int x = Xorg + Offset - (X * GridSize);
            int y = Yorg + Offset - (Y * GridSize);
            int z = Zorg + Offset - (Z * GridSize);

            if (x >= GridSize || x <= -1 ||
                y >= GridSize || y <= -1 ||
                z >= GridSize || z <= -1)
            {
                return Manager.GetGrid(x, y, z, X, Y, Z);
            }
            else
            {
                return this;
            }
        }

        public void PutIntoGrid(Cell cell)
        {
            int x = (int)cell.Cellinfo.X + Offset - (X * GridSize);
            int y = (int)cell.Cellinfo.Y + Offset - (Y * GridSize);
            int z = (int)cell.Cellinfo.Z + Offset - (Z * GridSize);

            Cells[x, y, z] = cell;
            Cells_list.Add(cell);

            cell.Grid = this;
        }

        public void PutIntoInfoGrid(Cell cell)
        {
            int x = (int)cell.Cellinfo.X + Offset - (X * GridSize);
            int y = (int)cell.Cellinfo.Y + Offset - (Y * GridSize);
            int z = (int)cell.Cellinfo.Z + Offset - (Z * GridSize);

            //cellInfo
            CellInfoUtility.Put(cell.Cellinfo, x + OffsetCI, y + OffsetCI, z + OffsetCI, GridSizeCI, CellsInfo);
        }

        public void PutIntoInfoList(Cell cell)
        {
            CellsInfo_list[CellsInfoCount] = cell.Cellinfo;
            CellsInfoCount++;
        }

        private void GetNeighboursInfo(Cell cell)
        {
            int x = (int)cell.Cellinfo.X + Offset - (X * GridSize);
            int y = (int)cell.Cellinfo.Y + Offset - (Y * GridSize);
            int z = (int)cell.Cellinfo.Z + Offset - (Z * GridSize);

            //update through manager in case the cell is in another grid
            Cell Front = Manager.GetCell(x + 1, y, z, X, Y, Z);
            Cell Right = Manager.GetCell(x, y, z - 1, X, Y, Z);
            Cell Back = Manager.GetCell(x - 1, y, z, X, Y, Z);
            Cell Left = Manager.GetCell(x, y, z + 1, X, Y, Z);
            Cell Bottom = Manager.GetCell(x, y - 1, z, X, Y, Z);
            Cell Top = Manager.GetCell(x, y + 1, z, X, Y, Z);

            GetNeighboursInfo(out cell.Cellinfo.FrontState, out cell.Cellinfo.FrontVolume, Front);
            GetNeighboursInfo(out cell.Cellinfo.RightState, out cell.Cellinfo.RightVolume, Right);
            GetNeighboursInfo(out cell.Cellinfo.BackState, out cell.Cellinfo.BackVolume, Back);
            GetNeighboursInfo(out cell.Cellinfo.LeftState, out cell.Cellinfo.LeftVolume, Left);
            GetNeighboursInfo(out cell.Cellinfo.BottomState, out cell.Cellinfo.BottomVolume, Bottom);
            GetNeighboursInfo(out cell.Cellinfo.TopState, out cell.Cellinfo.TopVolume, Top);

            //UpdateInfoGridWithCell(cell);
        }

        private void GetNeighboursInfo(out CellState state, out int volume, Cell cell)
        {
            if(cell == null)
            {
                state = CellState.None;
                volume = -1;
                return;
            }
            else
            {
                state = cell.Cellinfo.State;
                volume = cell.Cellinfo.Volume;
                //UpdateInfoGridWithCell(cell);
                return;
            }
        }

        //not used
        private void DeleteCell(Cell cell)
        {
            int X = (int)cell.Cellinfo.X + Offset;
            int Y = (int)cell.Cellinfo.Y + Offset;
            int Z = (int)cell.Cellinfo.Z + Offset;
            Cells[X, Y, Z] = null;
        }

        //not used
        private void ExpandGrid()
        {
            int addOffset = 50;
            Cell[,,] cells = new Cell[GridSize + addOffset, GridSize + addOffset, GridSize + addOffset];
            Array.Copy(Cells, 0, cells, addOffset, GridSize);

            Cells = cells;
            Offset += addOffset;
            GridSize += addOffset;
        }
    }
}
