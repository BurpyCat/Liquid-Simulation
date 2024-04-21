using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using VoxelWater;

namespace VoxelWater
{
    public struct UpdateGridsParallel : IJobFor
    {
        public int gridSizeFull;
        public int gridSizeFullCI;
        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> newCellsArr;
        [NativeDisableParallelForRestriction] public NativeArray<int> newCellsCountArr;
        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> updatedCellsArr;
        [NativeDisableParallelForRestriction] public NativeArray<int> updatedCellsCountArr;

        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> cellsInfo_listArr;
        [NativeDisableParallelForRestriction] public NativeArray<int> cellsInfoCountArr;
        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> cellsInfoArr;
        [NativeDisableParallelForRestriction] public NativeArray<GridInfo> gridInfoArr;
        public void Execute(int i)
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
    }
    
    static public class GridUtility
    {
        static public void UpdateCells(CellInfo[] cellsList, int count, CellInfo[] cells, GridInfo gridInfo, CellInfo[] newCells, ref int newCount, CellInfo[] updatedCells, ref int updatedCount)
        {
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
                foreach(var cell in newCellsTemp)
                {
                    newCells[newCount] = cell;
                    newCount= 1+newCount;
                }
                //newCells.AddRange(newCellsTemp);
                updatedCells[updatedCount]=newCell;
                updatedCount=1+ updatedCount;

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
