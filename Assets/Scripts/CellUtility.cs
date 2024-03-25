using UnityEngine;

namespace VoxelWater
{
    static public class CellUtility
    {
        static public void SetState(ref CellInfo cellinfo)
        {
            int[] sides = getSideColliders(cellinfo);
            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];

            bool down = ((cellinfo.BottomState == CellState.None || cellinfo.BottomState == CellState.Empty) && !ColliderExists(cellinfo, 0, -1, 0));

            //Cell emptyNeighbour = GetEmptyNeighbour(ref cellinfo, front, right, back, left, bottom);
            bool surroundedByEmpty = SurroundedByEmpty(cellinfo);

            if (surroundedByEmpty && cellinfo.Volume == 0)
                cellinfo.State = CellState.Destroy;
            else if (cellinfo.Volume == 0)
                cellinfo.State = CellState.Empty;
            else if (down)
                cellinfo.State = CellState.Fall;
            //else if (emptyNeighbour != null && (bottom==null || bottom.State != CellState.Still))
            //{
            //    State = CellState.Pushed;
            //    //to not have stuck blocks
            //    if(OldState == CellState.Empty)
            //        State = CellState.Pressured;
            //} 
            else if (sum > 0 && cellinfo.Volume > 1)
                cellinfo.State = CellState.Flow;
            else if (cellinfo.BottomState != CellState.None &&
                (cellinfo.BottomState == CellState.Shallow))
                cellinfo.State = CellState.Merge;
            else if (sum == 0 && cellinfo.Volume == 1)
                cellinfo.State = CellState.Still;
            else if (sum == 0 && cellinfo.Volume > 1)
                cellinfo.State = CellState.Pressured;
            else if (sum > 0 && cellinfo.Volume == 1)
                cellinfo.State = CellState.Shallow;
        }

        static public void ActivateState(ref CellInfo cellinfo, Grid grid)
        {
            switch (cellinfo.State)
            {
                case CellState.Flow:
                    Flow(ref cellinfo, grid);
                    break;
                case CellState.Pressured:
                    Pressured(ref cellinfo);
                    break;
                case CellState.Shallow:
                    Shallow(ref cellinfo);
                    break;
                case CellState.Fall:
                    Fall(ref cellinfo, grid);
                    break;
                //case CellState.Pushed:
                //Pushed();
                //break;
                case CellState.Destroy:
                    Destroy();
                    break;
                case CellState.Merge:
                    Merge(ref cellinfo);
                    break;
                //case CellState.Remove:
                //Remove();
                //break;
                case CellState.Create:
                    Create(ref cellinfo, grid, 5);
                    break;
                case CellState.Empty:
                    Destroy();
                    break;
            }
        }

        static public void Create(ref CellInfo cellinfo, Grid grid, int volume)
        {
            int[] sides = getSideColliders(cellinfo);
            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
            if (sum == 0)
            {
                FlowAll(ref cellinfo, volume);
            }
            else
                Flow(ref cellinfo, grid);
        }

        static public void Flow(ref CellInfo cellinfo, Grid grid, bool decreaseVolume = true)
        {
            //flow to sides
            //(front, right, back, left)
            int[] sides = getSideColliders(cellinfo); //five array

            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
            int volumeEach = 0;
            int oldresidue = 0;
            int residue = 0;

            if (sum != 0)
            {
                volumeEach = (cellinfo.Volume - 1) / sum;
                residue = (cellinfo.Volume - 1) % sum;
                oldresidue = residue;
            }

            //front
            if (sides[0] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (cellinfo.FrontState != CellState.None)
                        cellinfo.FrontVolume += volume;
                    else
                        grid.CreateCell(cellinfo.X + 1, cellinfo.Y + 0, cellinfo.Z + 0, volume);
                }
            }
            //right
            if (sides[1] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (cellinfo.RightState != CellState.None)
                        cellinfo.RightVolume += volume;
                    else
                        grid.CreateCell(cellinfo.X + 0, cellinfo.Y + 0, cellinfo.Z - 1, volume);
                }
            }
            //back
            if (sides[2] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (cellinfo.BackState != CellState.None)
                        cellinfo.BackVolume += volume;
                    else
                        grid.CreateCell(cellinfo.X - 1, cellinfo.Y + 0, cellinfo.Z + 0, volume);
                }
            }
            //left
            if (sides[3] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (cellinfo.LeftState != CellState.None)
                        cellinfo.LeftVolume += volume;
                    else
                        grid.CreateCell(cellinfo.X + 0, cellinfo.Y + 0, cellinfo.Z + 1, volume);
                }
            }

            //bottom
            if (sides[4] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (cellinfo.BottomState != CellState.None)
                        cellinfo.BottomVolume += volume;
                    else
                        grid.CreateCell(cellinfo.X + 0, cellinfo.Y - 1, cellinfo.Z + 0, volume);
                }
            }
            if (decreaseVolume)
                cellinfo.Volume = cellinfo.Volume - (sum * volumeEach + oldresidue);
        }

        /*
        static public void Create(ref CellInfo cellinfo, Grid grid, ref Cell front, ref Cell right, ref Cell back, ref Cell left, ref Cell bottom, int volume)
        {
            int[] sides = getSideColliders(cellinfo);
            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
            if (sum == 0)
            {
                FlowAll(ref cellinfo, volume);
            }
            else
                Flow(ref cellinfo, grid, ref front, ref right, ref back, ref left, ref bottom);
        }

        //remove grid and cell use
        static public void Flow(ref CellInfo cellinfo, Grid grid, ref Cell front, ref Cell right, ref Cell back, ref Cell left, ref Cell bottom, bool decreaseVolume = true)
        {
            //flow to sides
            //(front, right, back, left)
            int[] sides = getSideColliders(cellinfo); //five array

            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
            int volumeEach = 0;
            int oldresidue = 0;
            int residue = 0;

            if (sum != 0)
            {
                volumeEach = (cellinfo.Volume - 1) / sum;
                residue = (cellinfo.Volume - 1) % sum;
                oldresidue = residue;
            }

            //front
            if (sides[0] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (front != null)
                        front.Cellinfo.Volume += volume;
                    else
                        front = grid.CreateCell(cellinfo.X + 1, cellinfo.Y + 0, cellinfo.Z + 0, volume);
                }
            }
            //right
            if (sides[1] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (right != null)
                        right.Cellinfo.Volume += volume;
                    else
                        right = grid.CreateCell(cellinfo.X + 0, cellinfo.Y + 0, cellinfo.Z - 1, volume);
                }
            }
            //back
            if (sides[2] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (back != null)
                        back.Cellinfo.Volume += volume;
                    else
                        back = grid.CreateCell(cellinfo.X - 1, cellinfo.Y + 0, cellinfo.Z + 0, volume);
                }
            }
            //left
            if (sides[3] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (left != null)
                        left.Cellinfo.Volume += volume;
                    else
                        left = grid.CreateCell(cellinfo.X + 0, cellinfo.Y + 0, cellinfo.Z + 1, volume);
                }
            }

            //bottom
            if (sides[4] == 1)
            {
                int volume = volumeEach;
                if (residue != 0)
                {
                    --residue;
                    volume += 1;
                }
                if (volume > 0)
                {
                    if (bottom != null)
                        bottom.Cellinfo.Volume += volume;
                    else
                        bottom = grid.CreateCell(cellinfo.X + 0, cellinfo.Y - 1, cellinfo.Z + 0, volume);
                }
            }
            if (decreaseVolume)
                cellinfo.Volume = cellinfo.Volume - (sum * volumeEach + oldresidue);
        }
        */

        //remove grid and cell use
        static private void FlowAll(ref CellInfo cellinfo, int volume)
        {
            if (cellinfo.BottomState != CellState.None)
                cellinfo.BottomVolume += volume;
            if (cellinfo.FrontState != CellState.None)
                cellinfo.FrontVolume += volume;
            if (cellinfo.RightState != CellState.None)
                cellinfo.RightVolume += volume;
            if (cellinfo.BackState != CellState.None)
                cellinfo.BackVolume += volume;
            if (cellinfo.LeftState != CellState.None)
                cellinfo.LeftVolume += volume;
        }

        static public int[] getSideColliders(CellInfo cellinfo)
        {
            int[] sides = { 0, 0, 0, 0, 0 };
            //front
            if ((cellinfo.FrontState == CellState.None || cellinfo.FrontState == CellState.Empty) && !ColliderExists(cellinfo, 1, 0, 0))
                sides[0] = 1;
            //right
            if ((cellinfo.RightState == CellState.None || cellinfo.RightState == CellState.Empty) && !ColliderExists(cellinfo, 0, 0, -1))
                sides[1] = 1;
            //back
            if ((cellinfo.BackState == CellState.None || cellinfo.BackState == CellState.Empty) && !ColliderExists(cellinfo, -1, 0, 0))
                sides[2] = 1;
            //left
            if ((cellinfo.LeftState == CellState.None || cellinfo.LeftState == CellState.Empty) && !ColliderExists(cellinfo, 0, 0, 1))
                sides[3] = 1;
            //bottom
            if ((cellinfo.BottomState == CellState.None || cellinfo.BottomState == CellState.Empty) && !ColliderExists(cellinfo, 0, -1, 0))
                sides[4] = 1;

            return sides;
        }

        static public bool ColliderExists(CellInfo cellinfo, float x, float y, float z)
        {
            Vector3 currentPosition = new Vector3(cellinfo.X, cellinfo.Y, cellinfo.Z);
            Vector3 checkDirection = new Vector3(x, y, z);

            RaycastHit[] colliders = Physics.SphereCastAll(currentPosition, 0.25f, checkDirection, 1.20f);

            if (colliders.Length > 0)
            {
                return true;
            }

            return false;
        }
        static public void Pressured(ref CellInfo cellinfo)
        {
            GiveVolume(ref cellinfo, 1);
        }

        static private void GiveVolume(ref CellInfo cellinfo, int volume)
        {
            //grid.GiveVolume(volume);
            cellinfo.Volume -= volume;
        }

        static public void Shallow(ref CellInfo cellinfo)
        {
            GetVolume(ref cellinfo,1);
        }

        static private void GetVolume(ref CellInfo cellinfo, int volume)
        {
            //works only with volume 1
            //if (grid.GetVolume(volume))
            //{
            cellinfo.Volume += volume;
            //}
        }

        //remove grid and bottom
        static public void Fall(ref CellInfo cellinfo, Grid grid)
        {
            if (cellinfo.BottomState == CellState.None)
                grid.CreateCell(cellinfo.X + 0, cellinfo.Y - 1, cellinfo.Z + 0, cellinfo.Volume);
            else
                cellinfo.BottomVolume += cellinfo.Volume;
            cellinfo.Volume = 0;
        }

        //remove cell
        /*
        static public Cell GetEmptyNeighbour(ref CellInfo cellinfo, Cell front, Cell right, Cell back, Cell left, Cell bottom)
        {
            Cell emptyCell = null;
            if (cellinfo.BottomState != CellState.None && cellinfo.BottomState == CellState.Empty)
                emptyCell = bottom;
            else if (cellinfo.FrontState != CellState.None && cellinfo.FrontState == CellState.Empty)
                emptyCell = front;
            else if (cellinfo.RightState != CellState.None && cellinfo.RightState == CellState.Empty)
                emptyCell = right;
            else if (cellinfo.BackState != CellState.None && cellinfo.BackState == CellState.Empty)
                emptyCell = back;
            else if (cellinfo.LeftState != CellState.None && cellinfo.LeftState == CellState.Empty)
                emptyCell = left;

            return emptyCell;
        }
        */

        static public bool SurroundedByEmpty(CellInfo cellinfo)
        {
            if ((cellinfo.BottomState != CellState.None && cellinfo.BottomState != CellState.Empty))
                return false;
            if ((cellinfo.FrontState != CellState.None && cellinfo.FrontState != CellState.Empty))
                return false;
            if ((cellinfo.RightState != CellState.None && cellinfo.RightState != CellState.Empty))
                return false;
            if ((cellinfo.BackState != CellState.None && cellinfo.BackState != CellState.Empty))
                return false;
            if ((cellinfo.LeftState != CellState.None && cellinfo.LeftState != CellState.Empty))
                return false;
            if ((cellinfo.TopState != CellState.None && cellinfo.TopState != CellState.Empty))
                return false;
            //if ((bottom != null && bottom.State != CellState.Empty))
            //    return false;
            //if ((top != null && top.State != CellState.Empty))
            //    return false;

            return true;
        }

        static public void Merge(ref CellInfo cellinfo)
        {
            cellinfo.BottomVolume += cellinfo.Volume;
            cellinfo.Volume = 0;
        }

        static public void Destroy()
        {
            //grid.VolumeExcess += cellinfo.Volume;
            //DeleteCell();
        }

        //not used
        /*
        private void Pushed()
        {
            Cell emptyCell = GetEmptyNeighbour();

            emptyCell.cellinfo.Volume++;
            emptyCell.Mesh.enabled = true;

            cellinfo.Volume--;
            if (cellinfo.Volume == 0)
            {
                Mesh.enabled = false;
            }
        }
        */
        //not used
        /*
        private void DeleteCell()
        {
            //grid.DeleteCell(this);
            //Destroy(gameObject);
        }
        */
        //not used
        /*
        private void Remove()
        {
            if (top != null)
                top.cellinfo.Volume = 0;
            if (bottom != null)
                bottom.cellinfo.Volume = 0;
            if (right != null)
                right.cellinfo.Volume = 0;
            if (left != null)
                left.cellinfo.Volume = 0;
            if (front != null)
                front.cellinfo.Volume = 0;
            if (back != null)
                back.cellinfo.Volume = 0;
        }
        */
    }
}
