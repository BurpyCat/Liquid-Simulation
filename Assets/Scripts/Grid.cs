using System.Collections.Generic;
using UnityEngine;
using System;
using System.Dynamic;
using UnityEngine.UIElements;

namespace VoxelWater
{
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

        void Update()
        {

            //UpdateCells();
            UpdateCellsInfo(CellsInfo_list);
            //UpdateCellsInfo(CellsInfo_list, CellsInfo, GridSizeCI);
        }
 
        private void UpdateCells()
        {
            //Debug.Log(CellsInfo_list.Count);
            /*
            for (int y = 0; y < GridSizeCI; y++)
            {
                for (int x = 0; x < GridSizeCI; x++)
                {
                    string line = "";
                    for (int z = 0; z < GridSizeCI; z++)
                    {
                        if (CellsInfo[x, y, z].State == CellState.None)
                            line += "X";
                        else
                            line += CellsInfo[x, y, z].Volume.ToString();
                    }
                    Debug.Log(line);
                }
               Debug.Log("");
            }
            */

            //have an array of cellInfo
            //and maybe print it for small scale demo
            int count = Cells_list.Count;
            List<Cell> removeList = new List<Cell>();
            for (int i = 0; i < count; i++)
            {
                UpdateCell(Cells_list[i]);
                if (Cells_list[i].Cellinfo.State == CellState.Still && Cells_list[i].Cellinfo.OldState == CellState.Still)
                    removeList.Add(Cells_list[i]);

            }

            foreach (Cell cell in removeList)
            {
                //Cells_list.Remove(cell);
            }        
        }

        private void AddCellInfo()
        {

        }

        private void UpdateCellsInfo(List<CellInfo> cellsInfo)
        {
            UpdateAllCellInfo(CellsInfo);
            //need a list or array of cells that are around the grid, but they are not updated
            List<CellInfo> perimeterCells = new List<CellInfo>();
            List <CellInfo> newCells = new List<CellInfo>();
            List<CellInfo> updatedCells = new List<CellInfo>();

            //go through cell array and the array has neighbouring cells but the array only goes through length
            //tbh i can remove this array use list and copy coordinates from list
            
            //for now this will increase with new cells
            int count = CellsInfo_list.Count;
            for (int i=0; i < count; i++)
            {
                //update local cells info
                cellsInfo[i] = GetCellInfo(cellsInfo[i], CellsInfo);

                //if empty, skip
                if (cellsInfo[i].State == CellState.None)
                    continue;
                //get info from neighbors
                cellsInfo[i] = GetNeighboursInfo(cellsInfo[i], CellsInfo);
                //set state
                cellsInfo[i] = CellUtility.SetState(cellsInfo[i]);

                //check if states activation is needed
                //if (cellsInfo[i].OldState == cellsInfo[i].State &&
                //(cellsInfo[i].State == CellState.Still || cellsInfo[i].State == CellState.Empty))
                //{
                //    UpdateInfoGrid(cellsInfo[i], cells);
                //    continue;
                //}
                //activate state
                Debug.Log("old="+cellsInfo[i].Volume);
                cellsInfo[i] = CellUtility.ActivateStateInfo(cellsInfo[i], this);
                Debug.Log("new=" + cellsInfo[i].Volume);
                Debug.Log("");
                //get ONLY new created cells info
                cellsInfo[i] = GetNewNeighboursInfo(cellsInfo[i], CellsInfo);
                //update neighbors
                UpdateNeighboursInfo(cellsInfo[i], ref CellsInfo);
                
                //update info grid locally
                UpdateInfoGrid(cellsInfo[i], ref CellsInfo);
            }

            //update all cell objects (even neighboring)
            UpdateAllCellObjects(CellsInfo);
            for (int y = 0; y < GridSizeCI; y++)
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
        }

        private void UpdateAllCellObjects(CellInfo[,,] cells)
        {
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
                        Debug.Log($"x={x}, y={y}, z={z}");
                        Cell cellObj = Manager.GetCell(x-OffsetCI, y - OffsetCI, z - OffsetCI, X, Y, Z);
                        if(cellObj != null)
                        {
                            cells[x, y, z] = cellObj.Cellinfo;
                        }
                    }
                }
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
            cellObj.RenderCell();
        }

        private void UpdateInfoGrid(CellInfo cell, ref CellInfo[,,] cells)
        {
            int x = (int)cell.X + Offset - (X * GridSize);
            int y = (int)cell.Y + Offset - (Y * GridSize);
            int z = (int)cell.Z + Offset - (Z * GridSize);

            cells[x + OffsetCI, y + OffsetCI, z + OffsetCI] = cell;
        }

        private CellInfo GetCellInfo(CellInfo cell, CellInfo[,,] cells)
        {
            int x = (int)cell.X + Offset - (X * GridSize);
            int y = (int)cell.Y + Offset - (Y * GridSize);
            int z = (int)cell.Z + Offset - (Z * GridSize);

            return cells[x + OffsetCI, y + OffsetCI, z + OffsetCI];
        }

        private CellInfo GetNeighboursInfo(CellInfo cell, CellInfo[,,] cells)
        {
            int x = (int)cell.X + Offset - (X * GridSize) + OffsetCI;
            int y = (int)cell.Y + Offset - (Y * GridSize) + OffsetCI;
            int z = (int)cell.Z + Offset - (Z * GridSize) + OffsetCI;

            //update through manager in case the cell is in another grid
            /*CellInfo Front = GetCellInfoFromArray(x + 1, y, z, cellsInfoArr, size);
            CellInfo Right = GetCellInfoFromArray(x, y, z - 1, cellsInfoArr, size);
            CellInfo Back = GetCellInfoFromArray(x - 1, y, z, cellsInfoArr, size);
            CellInfo Left = GetCellInfoFromArray(x, y, z + 1, cellsInfoArr, size);
            CellInfo Bottom = GetCellInfoFromArray(x, y - 1, z, cellsInfoArr, size);
            CellInfo Top = GetCellInfoFromArray(x, y + 1, z, cellsInfoArr, size);
            */
            CellInfo Front = cells[x + 1, y, z];
            CellInfo Right = cells[x, y, z - 1];
            CellInfo Back = cells[x - 1, y, z];
            CellInfo Left = cells[x, y, z + 1];
            CellInfo Bottom = cells[x, y - 1, z];
            CellInfo Top = cells[x, y + 1, z];

            GetNeighboursInfo(out cell.FrontState, out cell.FrontVolume, Front);
            GetNeighboursInfo(out cell.RightState, out cell.RightVolume, Right);
            GetNeighboursInfo(out cell.BackState, out cell.BackVolume, Back);
            GetNeighboursInfo(out cell.LeftState, out cell.LeftVolume, Left);
            GetNeighboursInfo(out cell.BottomState, out cell.BottomVolume, Bottom);
            GetNeighboursInfo(out cell.TopState, out cell.TopVolume, Top);

            return cell;
        }

        private void GetNeighboursInfo(out CellState state, out int volume, CellInfo cell)
        {
            if (cell.State == CellState.None)
            {
                state = CellState.None;
                volume = -1;
                return;
            }
            else
            {
                state = cell.State;
                volume = cell.Volume;
                return;
            }
        }
        private CellInfo GetNewNeighboursInfo(CellInfo cell, CellInfo[,,] cells)
        {
            int x = (int)cell.X + Offset - (X * GridSize) + OffsetCI;
            int y = (int)cell.Y + Offset - (Y * GridSize) + OffsetCI;
            int z = (int)cell.Z + Offset - (Z * GridSize) + OffsetCI;

            //update through manager in case the cell is in another grid
            /*CellInfo Front = GetCellInfoFromArray(x + 1, y, z, cellsInfoArr, size);
            CellInfo Right = GetCellInfoFromArray(x, y, z - 1, cellsInfoArr, size);
            CellInfo Back = GetCellInfoFromArray(x - 1, y, z, cellsInfoArr, size);
            CellInfo Left = GetCellInfoFromArray(x, y, z + 1, cellsInfoArr, size);
            CellInfo Bottom = GetCellInfoFromArray(x, y - 1, z, cellsInfoArr, size);
            CellInfo Top = GetCellInfoFromArray(x, y + 1, z, cellsInfoArr, size);
            */
            CellInfo Front = cells[x + 1, y, z];
            CellInfo Right = cells[x, y, z - 1];
            CellInfo Back = cells[x - 1, y, z];
            CellInfo Left = cells[x, y, z + 1];
            CellInfo Bottom = cells[x, y - 1, z];
            CellInfo Top = cells[x, y + 1, z];

            GetNewNeighboursInfo(out cell.FrontState, out cell.FrontVolume, Front, cell.FrontState, cell.FrontVolume);
            GetNewNeighboursInfo(out cell.RightState, out cell.RightVolume, Right, cell.RightState, cell.RightVolume);
            GetNewNeighboursInfo(out cell.BackState, out cell.BackVolume, Back, cell.BackState, cell.BackVolume);
            GetNewNeighboursInfo(out cell.LeftState, out cell.LeftVolume, Left, cell.LeftState, cell.LeftVolume);
            GetNewNeighboursInfo(out cell.BottomState, out cell.BottomVolume, Bottom, cell.BottomState, cell.BottomVolume);
            GetNewNeighboursInfo(out cell.TopState, out cell.TopVolume, Top, cell.TopState, cell.TopVolume);

            return cell;
        }
        private void GetNewNeighboursInfo(out CellState newState, out int newVolume, CellInfo cell, CellState oldState, int oldVolume)
        {
            if (oldState == CellState.None && cell.State != CellState.None)
            {
                newState = cell.State;
                newVolume = cell.Volume;
                return;
            }
            else
            {
                newState = oldState;
                newVolume = oldVolume;
                return;
            }
        }

        private void UpdateNeighboursInfo(CellInfo cell, ref CellInfo[,,] cells)
        {
            int x = (int)cell.X + Offset - (X * GridSize) + OffsetCI;
            int y = (int)cell.Y + Offset - (Y * GridSize) + OffsetCI;
            int z = (int)cell.Z + Offset - (Z * GridSize) + OffsetCI;

            //update through manager in case the cell is in another grid
            /*CellInfo Front = cells[x + 1, y, z];
            CellInfo Right = cells[x, y, z - 1];
            CellInfo Back = cells[x - 1, y, z];
            CellInfo Left = cells[x, y, z + 1];
            CellInfo Bottom = cells[x, y - 1, z];
            CellInfo Top = cells[x, y + 1, z];*/

            //update only volume
            //might be a problem
            UpdateNeighboursInfo(cell.FrontVolume, ref cells[x + 1, y, z]);
            UpdateNeighboursInfo(cell.RightVolume, ref cells[x, y, z - 1]);
            UpdateNeighboursInfo(cell.BackVolume, ref cells[x - 1, y, z]);
            UpdateNeighboursInfo(cell.LeftVolume, ref cells[x, y, z + 1]);
            UpdateNeighboursInfo(cell.BottomVolume, ref cells[x, y - 1, z]);
            UpdateNeighboursInfo(cell.TopVolume, ref cells[x, y + 1, z]);
        }

        private void UpdateNeighboursInfo(int volume, ref CellInfo cell)
        {
            if (cell.State == CellState.None || volume == -1)
            {
                return;
            }
            else
            {
                cell.Volume = volume;
                return;
            }
        }
        /*--------------------------------------------------------------------------------*/

        //this goes after UpdateCellsInfo() and it updates cells with cells array (for now, later on it should be a list)
        private void UpdateCellsWithInfo()
        {

        }

        private void UpdateCell(Cell cell)
        {
            GetNeighboursInfo(cell);

            CellUtility.SetState(ref cell.Cellinfo);
            cell.RenderCell();
            CellUtility.ActivateState(ref cell.Cellinfo, this);

            UpdateNeighboursInfo(cell);
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

            //if(X==0 && Y==0 && Z==0)
            //    Manager.PutIntoGridManager(X, Y, Z, this);
        }

        public Cell CreateCell(float x, float y, float z, int volume)
        {
            Grid grid = GetGrid((int)x, (int)y, (int)z);


            Cell cellScript = grid.CreateCellObject(x, y, z, volume);
            grid.PutIntoGrid(cellScript);
            grid.GetNeighboursInfo(cellScript);
            grid.GetNeighboursInfo(cellScript.Cellinfo, CellsInfo);
            //cellinfo
            grid.PutIntoInfoGrid(cellScript);
            //put only when created and does not include neighbor cells
            grid.PutIntoInfoList(cellScript);
            //put also neighboring cells? maybe not needed
            if (grid != this)
                this.PutIntoInfoGrid(cellScript);
            grid.UpdateNeighboursInfo(cellScript);
            grid.UpdateNeighboursInfo(cellScript.Cellinfo, ref CellsInfo);

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

        //might need to cghange with updatedCell new
        

        private CellInfo GetCellInfoFromArray(int x, int y, int z, CellInfo[,,] cellsInfoArr, int size)
        {
            if(x < size && y < size && z < size && x >= 0 && y >= 0 && z >= 0)
            {
                return cellsInfoArr[x, y, z];
            }
            return new CellInfo();
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
