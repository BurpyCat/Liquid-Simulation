using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Diagnostics;

namespace VoxelWater
{
    [Serializable]
    public struct GridInfo
    {


        public int X;
        public int Y;
        public int Z;

        //for parallel work
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
        public int ActiveCount;
    }

    public class Grid : MonoBehaviour
    {
        //position in environment
        public Vector3 Position => gameObject.transform.localPosition;
        //cell objects that are placed by user
        public GameObject[] PlacedCells;

        public Cell[,,] Cells;

        //includes current cells
        public int CellsInfoCount = 0;
        public CellInfo[] CellsInfo_list;

        //includes current cells AND neighbours
        public CellInfo[] CellsInfo;

        public GridManager Manager;
        public GameObject Cube;
        public GameObject GridPrefab;
        public int VolumeExcess = 0;

        //grid info
        public GridInfo GridInfo;

        public bool first = false;

        //colliders
        public bool[] Colliders;

        void Awake()
        {
            Initiate(0, 0, 0);
            if(first)
            {
                Manager.PutIntoGridManager(0, 0, 0, this);
            }
            
            foreach(var cell in PlacedCells)
            {
                cell.GetComponent<Cell>().Initiate();
            }
        }

        public void Initiate(int X, int Y, int Z)
        {
            Manager = GameObject.Find("GridManager").GetComponent<GridManager>();
            //fill gridInfo
            GridInfo.X = X;
            GridInfo.Y = Y;
            GridInfo.Z = Z;
            GridInfo.GridSize = Manager.GridSize;
            GridInfo.Offset = (GridInfo.GridSize - 1) / 2;
            GridInfo.GridOffset = Manager.GridOffset;
            GridInfo.GridSizeCI = GridInfo.GridSize + 2;
            GridInfo.OffsetCI = 1;
            GridInfo.Num = CalcGridNumber();
            GridInfo.Active = true;
            GridInfo.ActiveCount = 0;

            Cells = new Cell[GridInfo.GridSize, GridInfo.GridSize, GridInfo.GridSize];

            //cellinfo      
            CellsInfo = CellInfoUtility.Create(GridInfo.GridSizeCI);
            CellsInfo_list = new CellInfo[GridInfo.GridSize * GridInfo.GridSize * GridInfo.GridSize];
            CellsInfoCount = 0;

            Colliders = GridUtility.GenerateCollidersOptimized(GridInfo);
        }

        private int CalcGridNumber()
        {
            int x = (GridInfo.X + GridInfo.GridOffset) % 7;
            int y = (GridInfo.Y + GridInfo.GridOffset) % 7;
            int z = (GridInfo.Z + GridInfo.GridOffset) % 7;

            return (x + y * 3 + z * 3 * 3) % 7;
        }

        public void UpdateGridCellsInfo()
        {
            UpdateNeighborCellInfo(CellsInfo);
            UpdateCellInfo(CellsInfo_list, CellsInfoCount, CellsInfo, GridInfo);
        }

        public void CreateAndUpdateGridCells(GridInfo gridInfo, CellInfo[] cellsInfo_list, int cellsInfoCount, CellInfo[] cellsInfo, bool[] colliders,
                                                CellInfo[] newCells, int newCellsCount, CellInfo[] updatedCells, int updatedCellsCount)
        {
            int fullGridSize = GridInfo.GridSize * GridInfo.GridSize * GridInfo.GridSize;
            int fullGridSizeCI = GridInfo.GridSizeCI * GridInfo.GridSizeCI * GridInfo.GridSizeCI;

            //copy cellsinfo list and the other
            CellsInfo_list = cellsInfo_list;
            CellsInfo = cellsInfo;

            //create all new cells in environment
            CreateNewCells(newCells, newCellsCount);
            //update all cell objects
            int updated = UpdateCellObjectsInt(updatedCells, updatedCellsCount, CellsInfo, GridInfo);
            //update neighboring cells 
            UpdateNeighborCellObjects(CellsInfo);
            //active check
            if (newCellsCount == 0 && updated == 0)
            {
                //if (GridInfo.ActiveCount == 0)
                GridInfo.Active = false;
                //else
                //    GridInfo.ActiveCount++;
            }
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
                int x = cell.GetGridXCI(gridInfo);
                int y = cell.GetGridYCI(gridInfo);
                int z = cell.GetGridZCI(gridInfo);
                UpdateCellObject(CellInfoUtility.Get(x, y, z, GridInfo.GridSizeCI, cells));
            }
        }

        private int UpdateCellObjectsInt(CellInfo[] cellList, int count, CellInfo[] cells, GridInfo gridInfo)
        {
            int updatedCount = 0;
            for (int i = 0; i < count; i++)
            {
                var cell = cellList[i];
                //int xCI = cell.GetGridXCI(gridInfo);
                //int yCI = cell.GetGridYCI(gridInfo);
                //int zCI = cell.GetGridZCI(gridInfo);

                int x = cell.GetGridX(GridInfo);
                int y = cell.GetGridY(GridInfo);
                int z = cell.GetGridZ(GridInfo);

                //CellInfo cellCI = CellInfoUtility.Get(xCI, yCI, zCI, GridInfo.GridSizeCI, cells);
                //update through manager in case the cell is in another grid
                //CellInfo cellGrid = Manager.GetCell(x, y, z, GridInfo.X, GridInfo.Y, GridInfo.Z).Cellinfo;
                Cell cellObj = Manager.GetCell(x, y, z, GridInfo.X, GridInfo.Y, GridInfo.Z);
                

                if (cell != cellObj.Cellinfo)
                {
                    cellObj.Cellinfo = cell;
                    cellObj.RenderCell();
                    //UpdateCellObject(cellCI);
                    updatedCount++;
                }
            }
            return updatedCount;
        }

        private void UpdateNeighborCellObjects(CellInfo[] cells)
        {
            for (int i = 1; i < GridInfo.GridSizeCI - 1; i++)
                for (int j = 1; j < GridInfo.GridSizeCI - 1; j++)
                {
                    UpdateCellObject(0, j, i, cells);
                    UpdateCellObject(i, j, 0, cells);
                    UpdateCellObject(j, 0, i, cells);

                    UpdateCellObject(GridInfo.GridSizeCI - 1, j, i, cells);
                    UpdateCellObject(i, j, GridInfo.GridSizeCI - 1, cells);
                    UpdateCellObject(j, GridInfo.GridSizeCI - 1, i, cells);
                }
        }

        private void UpdateCellObject(int x, int y, int z, CellInfo[] cells)
        {
            
            CellInfo cellinfo = CellInfoUtility.Get(x, y, z, GridInfo.GridSizeCI, cells);
            if (cellinfo.State != CellState.None)
            {
                Cell cellObj = Manager.GetCell(x - GridInfo.OffsetCI, y - GridInfo.OffsetCI, z - GridInfo.OffsetCI, GridInfo.X, GridInfo.Y, GridInfo.Z);
                if (cellObj!= null && cellinfo != cellObj.Cellinfo)
                {
                    cellObj.Cellinfo = cellinfo;
                    //set to active
                    cellObj.Grid.GridInfo.Active = true;
                    //cellObj.Grid.GridInfo.ActiveCount = 0;
                }
            }
        }

        private void UpdateCellObject(CellInfo cell)
        {
            int x = cell.GetGridX(GridInfo);
            int y = cell.GetGridY(GridInfo);
            int z = cell.GetGridZ(GridInfo);

            //update through manager in case the cell is in another grid
            Cell cellObj = Manager.GetCell(x, y, z, GridInfo.X, GridInfo.Y, GridInfo.Z);
            cellObj.Cellinfo = cell;
            cellObj.RenderCell();
        }

        private void UpdateNeighborCellInfo(CellInfo[] cells)
        {
            for (int i = 1; i < GridInfo.GridSizeCI - 1; i++)
                for (int j = 1; j < GridInfo.GridSizeCI - 1; j++)
                {
                    UpdateCellInfo(0, j, i, cells);
                    UpdateCellInfo(i, j, 0, cells);
                    UpdateCellInfo(j, 0, i, cells);

                    UpdateCellInfo(GridInfo.GridSizeCI - 1, j, i, cells);
                    UpdateCellInfo(i, j, GridInfo.GridSizeCI - 1, cells);
                    UpdateCellInfo(j, GridInfo.GridSizeCI - 1, i, cells);
                }
        }

        private void UpdateCellInfo(CellInfo[] cellList, int count, CellInfo[] cells, GridInfo gridInfo)
        {
            for(int i =0; i<count; i++)
            {
                var cell = cellList[i];
                int x = cell.GetGridXCI(gridInfo);
                int y = cell.GetGridYCI(gridInfo);
                int z = cell.GetGridZCI(gridInfo);
                UpdateCellInfo(x, y, z, cells);
            }
        }

        private void UpdateCellInfo(int x, int y, int z, CellInfo[] cells)
        {
            Cell cellObj = Manager.GetCell(x - GridInfo.OffsetCI, y - GridInfo.OffsetCI, z - GridInfo.OffsetCI, GridInfo.X, GridInfo.Y, GridInfo.Z);

            if (cellObj != null)
            {
                CellInfoUtility.Put(cellObj.Cellinfo, x, y, z, GridInfo.GridSizeCI, cells);
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
            //put also neighboring cells
            if (grid != this)
                this.PutIntoInfoGrid(cellScript);

            return cellScript;
        }

        private Cell CreateCellObject(float x, float y, float z, int volume)
        {
            //maybe can optimize?
            GameObject newCell = Instantiate(Cube, transform);
            newCell.transform.position = new Vector3(x, y, z);
            Cell cellScript = newCell.GetComponent<Cell>();

            cellScript.Initiate();
            cellScript.FillCellInfo(GridInfo, x, y, z);
            cellScript.Cellinfo.Volume = volume;
            cellScript.RenderCell();

            return cellScript;
        }

        private Grid GetGrid(int Xorg, int Yorg, int Zorg)
        {
            int x = Xorg + GridInfo.Offset - (GridInfo.X * GridInfo.GridSize);
            int y = Yorg + GridInfo.Offset - (GridInfo.Y * GridInfo.GridSize);
            int z = Zorg + GridInfo.Offset - (GridInfo.Z * GridInfo.GridSize);

            if (x >= GridInfo.GridSize || x <= -1 ||
                y >= GridInfo.GridSize || y <= -1 ||
                z >= GridInfo.GridSize || z <= -1)
            {
                return Manager.GetGrid(x, y, z, GridInfo.X, GridInfo.Y, GridInfo.Z);
            }
            else
            {
                return this;
            }
        }

        public void PutIntoGrid(Cell cell)
        {
            int x = cell.Cellinfo.GetGridX(GridInfo);
            int y = cell.Cellinfo.GetGridY(GridInfo);
            int z = cell.Cellinfo.GetGridZ(GridInfo);

            Cells[x, y, z] = cell;

            cell.Grid = this;
        }

        public void PutIntoInfoGrid(Cell cell)
        {
            int x = cell.Cellinfo.GetGridXCI(GridInfo);
            int y = cell.Cellinfo.GetGridYCI(GridInfo);
            int z = cell.Cellinfo.GetGridZCI(GridInfo);

            //cellInfo
            CellInfoUtility.Put(cell.Cellinfo, x, y, z, GridInfo.GridSizeCI, CellsInfo);
        }

        public void PutIntoInfoList(Cell cell)
        {
            CellsInfo_list[CellsInfoCount] = cell.Cellinfo;
            CellsInfoCount++;
        }

        private void GetNeighboursInfo(Cell cell)
        {
            int x = cell.Cellinfo.GetGridX(GridInfo);
            int y = cell.Cellinfo.GetGridY(GridInfo);
            int z = cell.Cellinfo.GetGridZ(GridInfo);

            //update through manager in case the cell is in another grid
            Cell Front = Manager.GetCell(x + 1, y, z, GridInfo.X, GridInfo.Y, GridInfo.Z);
            Cell Right = Manager.GetCell(x, y, z - 1, GridInfo.X, GridInfo.Y, GridInfo.Z);
            Cell Back = Manager.GetCell(x - 1, y, z, GridInfo.X, GridInfo.Y, GridInfo.Z);
            Cell Left = Manager.GetCell(x, y, z + 1, GridInfo.X, GridInfo.Y, GridInfo.Z);
            Cell Bottom = Manager.GetCell(x, y - 1, z, GridInfo.X, GridInfo.Y, GridInfo.Z);
            Cell Top = Manager.GetCell(x, y + 1, z, GridInfo.X, GridInfo.Y, GridInfo.Z);

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
                return;
            }
        }

        //not used
        /*
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
        */
    }
}
