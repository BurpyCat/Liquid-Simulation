using System.Collections.Generic;
using UnityEngine;

namespace VoxelWater
{  
    static public class GridUtility
    {
        static public void UpdateCells(CellInfo[] cellsList, int count, CellInfo[] cells, GridInfo gridInfo, CellInfo[] newCells, ref int newCount, CellInfo[] updatedCells, ref int updatedCount, bool[] colliders)
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
                newCell = CellUtility.SetState(newCell, colliders);

                //fix
                //check if states activation is needed

                if (newCell.OldState == newCell.State &&
                (newCell.State == CellState.Still || newCell.State == CellState.Empty))
                {
                    UpdateNeighboursInfo(newCell, cells, gridInfo);
                    UpdateInfoGrid(newCell, cells, gridInfo);
                    cellsList[i] = newCell;
                    updatedCells[updatedCount] = newCell;
                    updatedCount++;
                    continue;
                }
                
                //activate state
                newCell = CellUtility.ActivateStateInfo(newCell, newCellsTemp, colliders);
                //check if any updating is needed
                //this also has bugs
                if (newCell == cellsList[i] && newCellsTemp.Count == 0)
                {
                    UpdateNeighboursInfo(newCell, cells, gridInfo);
                    UpdateInfoGrid(newCell, cells, gridInfo);
                    cellsList[i] = newCell;
                    //updatedCells[updatedCount] = newCell;
                    //updatedCount++;
                    
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
                    newCount++;
                }

                updatedCells[updatedCount]=newCell;
                updatedCount++;

                cellsList[i] = newCell;
            }
        }
        static public void UpdateInfoGrid(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = cell.GetGridXCI(gridInfo);
            int y = cell.GetGridYCI(gridInfo);
            int z = cell.GetGridZCI(gridInfo);

            CellInfoUtility.Put(cell, x, y, z, gridInfo.GridSizeCI, cells);
        }

        static public CellInfo GetCellInfo(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = cell.GetGridXCI(gridInfo);
            int y = cell.GetGridYCI(gridInfo);
            int z = cell.GetGridZCI(gridInfo);

            return CellInfoUtility.Get(x, y, z, gridInfo.GridSizeCI, cells);
        }

        static public CellInfo GetNeighboursInfo(CellInfo cell, CellInfo[] cells, GridInfo gridInfo)
        {
            int x = cell.GetGridXCI(gridInfo);
            int y = cell.GetGridYCI(gridInfo);
            int z = cell.GetGridZCI(gridInfo);

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
            int x = cell.GetGridXCI(gridInfo);
            int y = cell.GetGridYCI(gridInfo);
            int z = cell.GetGridZCI(gridInfo);

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
            int x = cell.GetGridXCI(gridInfo);
            int y = cell.GetGridYCI(gridInfo);
            int z = cell.GetGridZCI(gridInfo);

            //update only volume
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

        //could make more efficient
        static public bool[] GenerateColliders(GridInfo grid)
        {
            //true 0,0,0 point of a grid
            int x0 = (grid.X * grid.GridSize) - grid.Offset;
            int y0 = (grid.Y * grid.GridSize) - grid.Offset;
            int z0 = (grid.Z * grid.GridSize) - grid.Offset;
            bool[] colliders = new bool[grid.GridSizeCI* grid.GridSizeCI* grid.GridSizeCI];

            for(int x = 0; x < grid.GridSize; x++)
            for(int y = 0; y < grid.GridSize; y++)
            for(int z = 0; z < grid.GridSize; z++)
            {
                CheckEveryDirectionColliders(colliders, grid.GridSizeCI, x+1, y+1, z+1, x0+x, y0+y, z0+z);
            }
            return colliders;
        }

        static private void CheckEveryDirectionColliders(bool[] colliders, int size, int x, int y, int z, float xpos, float ypos, float zpos)
        {
            CellInfoUtility.Put(ColliderExists(xpos, ypos, zpos, 1, 0, 0), x+1, y, z, size, colliders);
            CellInfoUtility.Put(ColliderExists(xpos, ypos, zpos, -1, 0, 0), x-1, y, z, size, colliders);
            CellInfoUtility.Put(ColliderExists(xpos, ypos, zpos, 0, 1, 0), x, y+1, z, size, colliders);
            CellInfoUtility.Put(ColliderExists(xpos, ypos, zpos, 0, -1, 0), x, y-1, z, size, colliders);
            CellInfoUtility.Put(ColliderExists(xpos, ypos, zpos, 0, 0, 1), x, y, z+1, size, colliders);
            CellInfoUtility.Put(ColliderExists(xpos, ypos, zpos, 0, 0, -1), x, y, z-1, size, colliders);
        }

        static private bool ColliderExists(float x, float y, float z, float xdir, float ydir, float zdir)
        {
            Vector3 currentPosition = new Vector3(x, y, z);
            Vector3 checkDirection = new Vector3(xdir, ydir, zdir);

            RaycastHit[] colliders = Physics.SphereCastAll(currentPosition, 0.25f, checkDirection, 1.20f);

            if (colliders.Length > 0)
            {
                return true;
            }

            return false;
        }
    }
}
