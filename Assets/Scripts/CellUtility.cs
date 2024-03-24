using UnityEngine;

namespace VoxelWater
{
    static public class CellUtility
    {
        static public void SetState(ref CellInfo Cellinfo)
        {
            int[] sides = getSideColliders(Cellinfo);
            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];

            bool down = ((Cellinfo.BottomState == CellState.None || Cellinfo.BottomState == CellState.Empty) && !ColliderExists(Cellinfo, 0, -1, 0));

            //Cell emptyNeighbour = GetEmptyNeighbour(ref Cellinfo, Front, Right, Back, Left, Bottom);
            bool surroundedByEmpty = SurroundedByEmpty(Cellinfo);

            if (surroundedByEmpty && Cellinfo.Volume == 0)
                Cellinfo.State = CellState.Destroy;
            else if (Cellinfo.Volume == 0)
                Cellinfo.State = CellState.Empty;
            else if (down)
                Cellinfo.State = CellState.Fall;
            //else if (emptyNeighbour != null && (Bottom==null || Bottom.State != CellState.Still))
            //{
            //    State = CellState.Pushed;
            //    //to not have stuck blocks
            //    if(OldState == CellState.Empty)
            //        State = CellState.Pressured;
            //} 
            else if (sum > 0 && Cellinfo.Volume > 1)
                Cellinfo.State = CellState.Flow;
            else if (Cellinfo.BottomState != CellState.None &&
                (Cellinfo.BottomState == CellState.Shallow))
                Cellinfo.State = CellState.Merge;
            else if (sum == 0 && Cellinfo.Volume == 1)
                Cellinfo.State = CellState.Still;
            else if (sum == 0 && Cellinfo.Volume > 1)
                Cellinfo.State = CellState.Pressured;
            else if (sum > 0 && Cellinfo.Volume == 1)
                Cellinfo.State = CellState.Shallow;
        }

        static public void ActivateState(ref CellInfo Cellinfo, Grid Grid, ref Cell Front, ref Cell Right, ref Cell Back, ref Cell Left, ref Cell Bottom)
        {
            switch (Cellinfo.State)
            {
                case CellState.Flow:
                    Flow(ref Cellinfo, Grid, ref Front, ref Right, ref Back, ref Left, ref Bottom);
                    break;
                case CellState.Pressured:
                    Pressured(ref Cellinfo);
                    break;
                case CellState.Shallow:
                    Shallow(ref Cellinfo);
                    break;
                case CellState.Fall:
                    Fall(ref Cellinfo, Grid, Bottom);
                    break;
                //case CellState.Pushed:
                //Pushed();
                //break;
                case CellState.Destroy:
                    Destroy();
                    break;
                case CellState.Merge:
                    Merge(ref Cellinfo, ref Bottom);
                    break;
                //case CellState.Remove:
                //Remove();
                //break;
                case CellState.Create:
                    Create(ref Cellinfo, Grid, ref Front, ref Right, ref Back, ref Left, ref Bottom, 5);
                    break;
                case CellState.Empty:
                    Destroy();
                    break;
            }
        }

        static public void Create(ref CellInfo Cellinfo, Grid Grid, ref Cell Front, ref Cell Right, ref Cell Back, ref Cell Left, ref Cell Bottom, int volume)
        {
            int[] sides = getSideColliders(Cellinfo);
            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
            if (sum == 0)
            {
                FlowAll(ref Front, ref Right, ref Back, ref Left, ref Bottom, volume);
            }
            else
                Flow(ref Cellinfo, Grid, ref Front, ref Right, ref Back, ref Left, ref Bottom);
        }

        //remove grid and cell use
        static public void Flow(ref CellInfo Cellinfo, Grid Grid, ref Cell Front, ref Cell Right, ref Cell Back, ref Cell Left, ref Cell Bottom, bool decreaseVolume = true)
        {
            //flow to sides
            //(front, right, back, left)
            int[] sides = getSideColliders(Cellinfo); //five array

            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
            int volumeEach = 0;
            int oldresidue = 0;
            int residue = 0;

            if (sum != 0)
            {
                volumeEach = (Cellinfo.Volume - 1) / sum;
                residue = (Cellinfo.Volume - 1) % sum;
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
                    if (Front != null)
                        Front.Cellinfo.Volume += volume;
                    else
                        Front = Grid.CreateCell(Cellinfo.X + 1, Cellinfo.Y + 0, Cellinfo.Z + 0, volume);
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
                    if (Right != null)
                        Right.Cellinfo.Volume += volume;
                    else
                        Right = Grid.CreateCell(Cellinfo.X + 0, Cellinfo.Y + 0, Cellinfo.Z - 1, volume);
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
                    if (Back != null)
                        Back.Cellinfo.Volume += volume;
                    else
                        Back = Grid.CreateCell(Cellinfo.X - 1, Cellinfo.Y + 0, Cellinfo.Z + 0, volume);
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
                    if (Left != null)
                        Left.Cellinfo.Volume += volume;
                    else
                        Left = Grid.CreateCell(Cellinfo.X + 0, Cellinfo.Y + 0, Cellinfo.Z + 1, volume);
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
                    if (Bottom != null)
                        Bottom.Cellinfo.Volume += volume;
                    else
                        Bottom = Grid.CreateCell(Cellinfo.X + 0, Cellinfo.Y - 1, Cellinfo.Z + 0, volume);
                }
            }
            if (decreaseVolume)
                Cellinfo.Volume = Cellinfo.Volume - (sum * volumeEach + oldresidue);
        }
        //what does it do???
        //remove grid and cell use
        static private void FlowAll(ref Cell Front, ref Cell Right, ref Cell Back, ref Cell Left, ref Cell Bottom, int volume)
        {
            if (Bottom != null)
                Bottom.Cellinfo.Volume += volume;
            if (Front != null)
                Front.Cellinfo.Volume += volume;
            if (Right != null)
                Right.Cellinfo.Volume += volume;
            if (Back != null)
                Back.Cellinfo.Volume += volume;
            if (Left != null)
                Left.Cellinfo.Volume += volume;
        }

        static public int[] getSideColliders(CellInfo Cellinfo)
        {
            int[] sides = { 0, 0, 0, 0, 0 };
            //front
            if ((Cellinfo.FrontState == CellState.None || Cellinfo.FrontState == CellState.Empty) && !ColliderExists(Cellinfo, 1, 0, 0))
                sides[0] = 1;
            //right
            if ((Cellinfo.RightState == CellState.None || Cellinfo.RightState == CellState.Empty) && !ColliderExists(Cellinfo, 0, 0, -1))
                sides[1] = 1;
            //back
            if ((Cellinfo.BackState == CellState.None || Cellinfo.BackState == CellState.Empty) && !ColliderExists(Cellinfo, -1, 0, 0))
                sides[2] = 1;
            //left
            if ((Cellinfo.LeftState == CellState.None || Cellinfo.LeftState == CellState.Empty) && !ColliderExists(Cellinfo, 0, 0, 1))
                sides[3] = 1;
            //bottom
            if ((Cellinfo.BottomState == CellState.None || Cellinfo.BottomState == CellState.Empty) && !ColliderExists(Cellinfo, 0, -1, 0))
                sides[4] = 1;

            return sides;
        }

        static public bool ColliderExists(CellInfo Cellinfo, float x, float y, float z)
        {
            Vector3 currentPosition = new Vector3(Cellinfo.X, Cellinfo.Y, Cellinfo.Z);
            Vector3 checkDirection = new Vector3(x, y, z);

            RaycastHit[] colliders = Physics.SphereCastAll(currentPosition, 0.25f, checkDirection, 1.20f);

            if (colliders.Length > 0)
            {
                return true;
            }

            return false;
        }
        static public void Pressured(ref CellInfo Cellinfo)
        {
            GiveVolume(ref Cellinfo, 1);
        }

        static private void GiveVolume(ref CellInfo Cellinfo, int volume)
        {
            //Grid.GiveVolume(volume);
            Cellinfo.Volume -= volume;
        }

        static public void Shallow(ref CellInfo Cellinfo)
        {
            GetVolume(ref Cellinfo,1);
        }

        static private void GetVolume(ref CellInfo Cellinfo, int volume)
        {
            //works only with volume 1
            //if (Grid.GetVolume(volume))
            //{
            Cellinfo.Volume += volume;
            //}
        }

        //remove grid and bottom
        static public void Fall(ref CellInfo Cellinfo, Grid Grid, Cell Bottom)
        {
            if (Cellinfo.BottomState == CellState.None)
                Grid.CreateCell(Cellinfo.X + 0, Cellinfo.Y - 1, Cellinfo.Z + 0, Cellinfo.Volume);
            else
                Bottom.Cellinfo.Volume += Cellinfo.Volume;
            Cellinfo.Volume = 0;
        }

        //remove cell
        static public Cell GetEmptyNeighbour(ref CellInfo Cellinfo, Cell Front, Cell Right, Cell Back, Cell Left, Cell Bottom)
        {
            Cell emptyCell = null;
            if (Cellinfo.BottomState != CellState.None && Cellinfo.BottomState == CellState.Empty)
                emptyCell = Bottom;
            else if (Cellinfo.FrontState != CellState.None && Cellinfo.FrontState == CellState.Empty)
                emptyCell = Front;
            else if (Cellinfo.RightState != CellState.None && Cellinfo.RightState == CellState.Empty)
                emptyCell = Right;
            else if (Cellinfo.BackState != CellState.None && Cellinfo.BackState == CellState.Empty)
                emptyCell = Back;
            else if (Cellinfo.LeftState != CellState.None && Cellinfo.LeftState == CellState.Empty)
                emptyCell = Left;

            return emptyCell;
        }

        static public bool SurroundedByEmpty(CellInfo Cellinfo)
        {
            if ((Cellinfo.BottomState != CellState.None && Cellinfo.BottomState != CellState.Empty))
                return false;
            if ((Cellinfo.FrontState != CellState.None && Cellinfo.FrontState != CellState.Empty))
                return false;
            if ((Cellinfo.RightState != CellState.None && Cellinfo.RightState != CellState.Empty))
                return false;
            if ((Cellinfo.BackState != CellState.None && Cellinfo.BackState != CellState.Empty))
                return false;
            if ((Cellinfo.LeftState != CellState.None && Cellinfo.LeftState != CellState.Empty))
                return false;
            if ((Cellinfo.TopState != CellState.None && Cellinfo.TopState != CellState.Empty))
                return false;
            //if ((Bottom != null && Bottom.State != CellState.Empty))
            //    return false;
            //if ((Top != null && Top.State != CellState.Empty))
            //    return false;

            return true;
        }

        static public void Merge(ref CellInfo Cellinfo, ref Cell Bottom)
        {
            Bottom.Cellinfo.Volume += Cellinfo.Volume;
            Cellinfo.Volume = 0;
        }

        static public void Destroy()
        {
            //Grid.VolumeExcess += Cellinfo.Volume;
            //DeleteCell();
        }

        //not used
        /*
        private void Pushed()
        {
            Cell emptyCell = GetEmptyNeighbour();

            emptyCell.Cellinfo.Volume++;
            emptyCell.Mesh.enabled = true;

            Cellinfo.Volume--;
            if (Cellinfo.Volume == 0)
            {
                Mesh.enabled = false;
            }
        }
        */
        //not used
        /*
        private void DeleteCell()
        {
            //Grid.DeleteCell(this);
            //Destroy(gameObject);
        }
        */
        //not used
        /*
        private void Remove()
        {
            if (Top != null)
                Top.Cellinfo.Volume = 0;
            if (Bottom != null)
                Bottom.Cellinfo.Volume = 0;
            if (Right != null)
                Right.Cellinfo.Volume = 0;
            if (Left != null)
                Left.Cellinfo.Volume = 0;
            if (Front != null)
                Front.Cellinfo.Volume = 0;
            if (Back != null)
                Back.Cellinfo.Volume = 0;
        }
        */
    }
}
