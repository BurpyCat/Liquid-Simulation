using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor.PackageManager;
using UnityEngine;
using VoxelWater;

namespace VoxelWater
{
    /*
    public struct UpdateCellsParallel : IJobFor
    {
        //public NativeArray<CellInfo> Value;
        //TO-DO flatten multi dimensional array...
        //i = x + width* y + width* height*z;
       // x = i % width;
       // y = (i / width)%height;
       // z = i / (width* height);
        public List<CellInfo> cellsList;
        public CellInfo[,,] cells;
        public GridInfo gridInfo;
        //these are size^3
        public List<CellInfo> newCells;
        //these might be size+1^3 but perhaps size^3 will be good
        public List<CellInfo> updatedCells;
        public void Execute(int i)
        {
            List<CellInfo> newCellsTemp = new List<CellInfo>();
            CellInfo newCell = cellsList[i];
            //update local cells info
            newCell = GridUtility.GetCellInfo(newCell, cells, gridInfo);

            //if empty, skip
            if (newCell.State == CellState.None)
                return;
            //get info from neighbors
            newCell = GridUtility.GetNeighboursInfo(newCell, cells, gridInfo);
            //set state
            newCell = CellUtility.SetState(newCell);

            //check if states activation is needed
            if (newCell.OldState == newCell.State &&
            (newCell.State == CellState.Still || newCell.State == CellState.Empty))
            {
                GridUtility.UpdateInfoGrid(newCell, cells, gridInfo);
                cellsList[i] = newCell;
                return;
            }
            //activate state
            newCell = CellUtility.ActivateStateInfo(newCell, newCellsTemp);
            //check if any updating is needed
            if (newCell == cellsList[i] && newCellsTemp.Count == 0)
            {
                return;
            }

            //put into array to not create duplicates
            foreach (var cell in newCellsTemp)
            {
                GridUtility.UpdateInfoGrid(cell, cells, gridInfo);
            }

            //get ONLY new created cells info
            newCell = GridUtility.GetNewNeighboursInfo(newCell, cells, gridInfo);
            //update neighbors
            GridUtility.UpdateNeighboursInfo(newCell, cells, gridInfo);
            //update info grid locally
            GridUtility.UpdateInfoGrid(newCell, cells, gridInfo);
            //add to global list
            newCells.AddRange(newCellsTemp);
            updatedCells.Add(newCell);
            cellsList[i] = newCell;
        }
    }
    */
    static public class GridUtility
    {
        static public void UpdateCells(List<CellInfo> cellsList, CellInfo[] cells, GridInfo gridInfo, List<CellInfo> newCells, List<CellInfo> updatedCells)
        {
            int count = cellsList.Count;
            for (int i = 0; i < count; i++)
            {
                List<CellInfo> newCellsTemp = new List<CellInfo>();
                CellInfo newCell = cellsList[i];
                //update local cells info
                newCell = GetCellInfo(newCell, cells, gridInfo);

                //if empty, skip
                if (newCell.State == CellState.None)
                    continue;
                //get info from neighbors
                newCell = GetNeighboursInfo(newCell, cells, gridInfo);
                //set state
                newCell = CellUtility.SetState(newCell);

                //check if states activation is needed
                if (newCell.OldState == newCell.State &&
                (newCell.State == CellState.Still || newCell.State == CellState.Empty))
                {
                    UpdateInfoGrid(newCell, cells, gridInfo);
                    cellsList[i] = newCell;
                    continue;
                }
                //activate state
                newCell = CellUtility.ActivateStateInfo(newCell, newCellsTemp);
                //check if any updating is needed
                if (newCell == cellsList[i] && newCellsTemp.Count == 0)
                {
                    continue;
                }

                //put into array to not create duplicates
                foreach (var cell in newCellsTemp)
                {
                    UpdateInfoGrid(cell, cells, gridInfo);
                }

                //get ONLY new created cells info
                newCell = GetNewNeighboursInfo(newCell, cells, gridInfo);
                //update neighbors
                UpdateNeighboursInfo(newCell, cells, gridInfo);
                //update info grid locally
                UpdateInfoGrid(newCell, cells, gridInfo);
                //add to global list
                newCells.AddRange(newCellsTemp);
                updatedCells.Add(newCell);
                cellsList[i] = newCell;
            }
        }
        static public void UpdateInfoGrid(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
            int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
            int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;

            CellInfoUtility.Put(cell, x, y, z, gridInfo.GridSizeCI, cells);
        }

        static public CellInfo GetCellInfo(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
            int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
            int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;

            return CellInfoUtility.Get(x, y, z, gridInfo.GridSizeCI, cells);
        }

        static public CellInfo GetNeighboursInfo(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
            int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
            int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;

            CellInfo Front = CellInfoUtility.Get(x + 1, y, z, gridInfo.GridSizeCI, cells);
            CellInfo Right = CellInfoUtility.Get(x, y, z - 1, gridInfo.GridSizeCI, cells);
            CellInfo Back = CellInfoUtility.Get(x - 1, y, z, gridInfo.GridSizeCI, cells);
            CellInfo Left = CellInfoUtility.Get(x, y, z + 1, gridInfo.GridSizeCI, cells);
            CellInfo Bottom = CellInfoUtility.Get(x, y - 1, z, gridInfo.GridSizeCI, cells);
            CellInfo Top = CellInfoUtility.Get(x, y + 1, z, gridInfo.GridSizeCI, cells);

            GetNeighboursInfo(out cell.FrontState, out cell.FrontVolume, Front);
            GetNeighboursInfo(out cell.RightState, out cell.RightVolume, Right);
            GetNeighboursInfo(out cell.BackState, out cell.BackVolume, Back);
            GetNeighboursInfo(out cell.LeftState, out cell.LeftVolume, Left);
            GetNeighboursInfo(out cell.BottomState, out cell.BottomVolume, Bottom);
            GetNeighboursInfo(out cell.TopState, out cell.TopVolume, Top);

            return cell;
        }

        static private void GetNeighboursInfo(out CellState state, out int volume, CellInfo cell)
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
        static public CellInfo GetNewNeighboursInfo(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
            int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
            int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;

            CellInfo Front = CellInfoUtility.Get(x + 1, y, z, gridInfo.GridSizeCI, cells);
            CellInfo Right = CellInfoUtility.Get(x, y, z - 1, gridInfo.GridSizeCI, cells);
            CellInfo Back = CellInfoUtility.Get(x - 1, y, z, gridInfo.GridSizeCI, cells);
            CellInfo Left = CellInfoUtility.Get(x, y, z + 1, gridInfo.GridSizeCI, cells);
            CellInfo Bottom = CellInfoUtility.Get(x, y - 1, z, gridInfo.GridSizeCI, cells);
            CellInfo Top = CellInfoUtility.Get(x, y + 1, z, gridInfo.GridSizeCI, cells);

            GetNewNeighboursInfo(out cell.FrontState, out cell.FrontVolume, Front, cell.FrontState, cell.FrontVolume);
            GetNewNeighboursInfo(out cell.RightState, out cell.RightVolume, Right, cell.RightState, cell.RightVolume);
            GetNewNeighboursInfo(out cell.BackState, out cell.BackVolume, Back, cell.BackState, cell.BackVolume);
            GetNewNeighboursInfo(out cell.LeftState, out cell.LeftVolume, Left, cell.LeftState, cell.LeftVolume);
            GetNewNeighboursInfo(out cell.BottomState, out cell.BottomVolume, Bottom, cell.BottomState, cell.BottomVolume);
            GetNewNeighboursInfo(out cell.TopState, out cell.TopVolume, Top, cell.TopState, cell.TopVolume);

            return cell;
        }
        static private void GetNewNeighboursInfo(out CellState newState, out int newVolume, CellInfo cell, CellState oldState, int oldVolume)
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

        static public void UpdateNeighboursInfo(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = (int)cell.X + gridInfo.Offset - (gridInfo.X * gridInfo.GridSize) + gridInfo.OffsetCI;
            int y = (int)cell.Y + gridInfo.Offset - (gridInfo.Y * gridInfo.GridSize) + gridInfo.OffsetCI;
            int z = (int)cell.Z + gridInfo.Offset - (gridInfo.Z * gridInfo.GridSize) + gridInfo.OffsetCI;

            //update only volume
            //might be a problem
            CellInfo Front = UpdateNeighboursInfo(cell.FrontVolume, CellInfoUtility.Get(x + 1, y, z, gridInfo.GridSizeCI, cells));
            CellInfo Right = UpdateNeighboursInfo(cell.RightVolume, CellInfoUtility.Get(x, y, z - 1, gridInfo.GridSizeCI, cells));
            CellInfo Back = UpdateNeighboursInfo(cell.BackVolume, CellInfoUtility.Get(x - 1, y, z, gridInfo.GridSizeCI, cells));
            CellInfo Left = UpdateNeighboursInfo(cell.LeftVolume, CellInfoUtility.Get(x, y, z + 1, gridInfo.GridSizeCI, cells));
            CellInfo Bottom = UpdateNeighboursInfo(cell.BottomVolume, CellInfoUtility.Get(x, y - 1, z, gridInfo.GridSizeCI, cells));
            CellInfo Top = UpdateNeighboursInfo(cell.TopVolume, CellInfoUtility.Get(x, y + 1, z, gridInfo.GridSizeCI, cells));

            CellInfoUtility.Put(Front, x + 1, y, z, gridInfo.GridSizeCI, cells);
            CellInfoUtility.Put(Right, x, y, z - 1, gridInfo.GridSizeCI, cells);
            CellInfoUtility.Put(Back, x - 1, y, z, gridInfo.GridSizeCI, cells);
            CellInfoUtility.Put(Left, x, y, z + 1, gridInfo.GridSizeCI, cells);
            CellInfoUtility.Put(Bottom, x, y - 1, z, gridInfo.GridSizeCI, cells);
            CellInfoUtility.Put(Top, x, y + 1, z, gridInfo.GridSizeCI, cells);
        }

        static private CellInfo UpdateNeighboursInfo(int volume, CellInfo cell)
        {
            if (cell.State == CellState.None || volume == -1)
            {
                return cell;
            }
            else
            {
                cell.Volume = volume;
                return cell;
            }
        }
    }
}
