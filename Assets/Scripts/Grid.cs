using System.Collections.Generic;
using UnityEngine;
using System;
using System.Dynamic;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;

namespace VoxelWater
{
    [Serializable]
    public struct GridInfo
    {
        //first grid
        public int X;
        public int Y;
        public int Z;

        //(GridSize - 1) /2
        public int Offset;
        //nelyginiai!
        public int GridSize;
        //(GridManagerSize - 1) /2
        public int GridOffset;

        public int GridSizeCI;
        public int OffsetCI;
    }

    public class Grid : MonoBehaviour
    {
        private List<Cell> Cells_list;
        public Cell[,,] Cells;

        //cellinfo
        private List<CellInfo> CellsInfo_list;
        public CellInfo[,,] CellsInfo;
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
            CellsInfo = new CellInfo[GridSizeCI, GridSizeCI, GridSizeCI];
            CellsInfo_list = new List<CellInfo>();

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

            //if(X==0 && Y==0 && Z==0)
            //    Manager.PutIntoGridManager(X, Y, Z, this);

        }

        void Update()
        {

            //UpdateCells();
            UpdateCellsInfo(CellsInfo_list);
            //UpdateCellsInfo(CellsInfo_list, CellsInfo, GridSizeCI);
        }

        private void UpdateCellsInfo(List<CellInfo> cellsInfo)
        {
            //Debug.Log(CellsInfo_list.Count);
            //UpdateNeighborCellInfo(CellsInfo);
            
            UpdateAllCellInfo(CellsInfo);
            //need a list or array of cells that are around the grid, but they are not updated
            //List<CellInfo> perimeterCells = new List<CellInfo>();
            List <CellInfo> newCells = new List<CellInfo>();
            List<CellInfo> updatedCells = new List<CellInfo>();

            //go through cell array and the array has neighbouring cells but the array only goes through length
            //tbh i can remove this array use list and copy coordinates from list
            
            //for now this will increase with new cells
            int count = CellsInfo_list.Count;
            for (int i=0; i < count; i++)
            {
                
                List<CellInfo> newCellsTemp = new List<CellInfo>();
                //update local cells info
                cellsInfo[i] = GridUtility.GetCellInfo(cellsInfo[i], CellsInfo, GridInfo);

                //if empty, skip
                if (cellsInfo[i].State == CellState.None)
                    continue;
                //get info from neighbors
                cellsInfo[i] = GridUtility.GetNeighboursInfo(cellsInfo[i], CellsInfo, GridInfo);
                //set state
                Debug.Log(cellsInfo[i].State);
                cellsInfo[i] = CellUtility.SetState(cellsInfo[i]);
                Debug.Log(cellsInfo[i].State);

                //check if states activation is needed
                //if (cellsInfo[i].OldState == cellsInfo[i].State &&
                //(cellsInfo[i].State == CellState.Still || cellsInfo[i].State == CellState.Empty))
                //{
                //    UpdateInfoGrid(cellsInfo[i], cells);
                //    continue;
                //}
                //activate state
                //Debug.Log("old="+cellsInfo[i].Volume);
                cellsInfo[i] = CellUtility.ActivateStateInfo(cellsInfo[i], newCellsTemp);
                //check if any updating is needed
                //if(newCellsInfo == cellsInfo[i] && newCellsTemp.Count == 0)
                //{
                    //continue;
                //}
                //cellsInfo[i] = newCellsInfo;
                //put into array to not create duplicates
                foreach (var cell in newCellsTemp)
                {
                    GridUtility.UpdateInfoGrid(cell, CellsInfo, GridInfo);
                    //Debug.Log(cell.X + " " + cell.Y + " " + cell.Z);
                }

                //Debug.Log("new=" + cellsInfo[i].Volume);
                
                //get ONLY new created cells info
                cellsInfo[i] = GridUtility.GetNewNeighboursInfo(cellsInfo[i], CellsInfo, GridInfo);
                //update neighbors
                GridUtility.UpdateNeighboursInfo(cellsInfo[i], CellsInfo, GridInfo);

                //update info grid locally
                GridUtility.UpdateInfoGrid(cellsInfo[i], CellsInfo, GridInfo);
                Debug.Log(cellsInfo[i].State);
                //add to global list
                newCells.AddRange(newCellsTemp);
                updatedCells.Add(cellsInfo[i]);
            }

            //create all new cells in environment
            CreateNewCells(newCells);
            //update all cells!



            //update all cell objects (even neighboring)
            UpdateCellObjects(updatedCells, CellsInfo, GridInfo);
            /*for (int y = 0; y < GridSizeCI; y++)
            {
                for (int x = 0; x < GridSizeCI; x++)
                {
                    string line = "";
                    for (int z = 0; z < GridSizeCI; z++)
                    {
                        if (CellsInfo[x, y, z].State == CellState.None)
                            line += "X";
                        else
                            line += "O";
                    }
                    Debug.Log(line);
                }
                Debug.Log("");
            }
            */
        }

        private void CreateNewCells(List<CellInfo> newCells)
        {
            foreach(var cell in newCells)
            {
                CreateCell(cell.X, cell.Y, cell.Z, cell.Volume);
            }   
        }

        private void UpdateCellObjects(List<CellInfo> cellList, CellInfo[,,] cells, GridInfo gridInfo)
        {
            foreach (var cell in cellList)
            {
                int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
                int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
                int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;
                UpdateCellObject(cells[x, y, z]);
            }
            /*
            for (int x = 0; x < GridSizeCI; x++)
            {
                for (int y = 0; y < GridSizeCI; y++)
                {
                    for (int z = 0; z < GridSizeCI; z++)
                    {
                        //Debug.Log($"x={x}, y={y}, z={z}");
                        if (cells[x, y, z].State != CellState.None)
                        {
                            UpdateCellObject(cells[x, y, z]);
                        }
                    }
                }
            }
            */

        }

        //or only neighbors
        //neighbors are not corner cells!!!!!
        private void UpdateAllCellInfo(CellInfo[,,] cells)
        {
            for (int x = 0; x < GridSizeCI; x++)
            {
                for (int y = 0; y < GridSizeCI; y++)
                {
                    for (int z = 0; z < GridSizeCI; z++)
                    {
                        //exclude corner cells
                        if(x==0 || x== GridSizeCI - 1)
                        {
                            if (y == 0 || y == GridSizeCI-1)
                                continue;
                            if (z == 0 || z == GridSizeCI - 1)
                                continue;
                        }
                        if (y == 0 || y == GridSizeCI - 1)
                        {
                            if (x == 0 || x == GridSizeCI - 1)
                                continue;
                            if (z == 0 || z == GridSizeCI - 1)
                                continue;
                        }
                        if (z == 0 || z == GridSizeCI - 1)
                        {
                            if (y == 0 || y == GridSizeCI - 1)
                                continue;
                            if (x == 0 || x == GridSizeCI - 1)
                                continue;
                        }
                        //Debug.Log($"x={x}, y={y}, z={z}");
                        Cell cellObj = Manager.GetCell(x-OffsetCI, y - OffsetCI, z - OffsetCI, X, Y, Z);
                        if(cellObj != null)
                        {
                            cells[x, y, z] = cellObj.Cellinfo;
                        }
                    }
                }
            }
        }
        /*
        private void UpdateGridCellInfo(List<CellInfo> cellsList, CellInfo[,,] cells)
        {
            foreach (var cell in cellsList)
            {
                UpdateCellInfo();
            }
        }
        */

        private void UpdateNeighborCellInfo(CellInfo[,,] cells)
        {
            for (int i = 1; i < GridSizeCI-1; i++)
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

        private void UpdateCellInfo(int x, int y, int z, CellInfo[,,] cells)
        {
            Cell cellObj = Manager.GetCell(x - OffsetCI, y - OffsetCI, z - OffsetCI, X, Y, Z);
            
            if (cellObj != null)
            {
                cells[x, y, z] = cellObj.Cellinfo;
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
            CellsInfo[x + OffsetCI, y + OffsetCI, z + OffsetCI] = cell.Cellinfo;
            //CellsInfo_list.Add(cell.Cellinfo);
        }

        public void PutIntoInfoList(Cell cell)
        {
            int x = (int)cell.Cellinfo.X + Offset - (X * GridSize);
            int y = (int)cell.Cellinfo.Y + Offset - (Y * GridSize);
            int z = (int)cell.Cellinfo.Z + Offset - (Z * GridSize);

            //cellInfo
            //CellsInfo_list.Insert(0, cell.Cellinfo);
            CellsInfo_list.Add(cell.Cellinfo);
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

        //updates cell grid
        //updates cellinfo grid

        private void UpdateNeighboursInfo(Cell cell)
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

            UpdateNeighboursInfo(cell.Cellinfo.FrontVolume, Front);
            UpdateNeighboursInfo(cell.Cellinfo.RightVolume, Right);
            UpdateNeighboursInfo(cell.Cellinfo.BackVolume, Back);
            UpdateNeighboursInfo(cell.Cellinfo.LeftVolume, Left);
            UpdateNeighboursInfo(cell.Cellinfo.BottomVolume, Bottom);
            UpdateNeighboursInfo(cell.Cellinfo.TopVolume, Top);
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

        private void UpdateNeighboursInfo(int volume, Cell cell)
        {
            if (cell == null || volume == -1)
            {
                return;
            }
            else
            {
                cell.Cellinfo.Volume = volume;
                UpdateInfoGridWithCell(cell);
                return;
            }
        }

        private void UpdateInfoGridWithCell(Cell cell)
        {
            int x = (int)cell.Cellinfo.X + Offset - (X * GridSize);
            int y = (int)cell.Cellinfo.Y + Offset - (Y * GridSize);
            int z = (int)cell.Cellinfo.Z + Offset - (Z * GridSize);

            CellsInfo[x + OffsetCI, y + OffsetCI, z + OffsetCI] = cell.Cellinfo;
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
