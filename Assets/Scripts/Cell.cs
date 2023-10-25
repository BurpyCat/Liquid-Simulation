using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace VoxelWater
{
    public enum CellState
    {
        Flow, //when water has volume and can flow
        Still, //when water doesnt have volume and cant flow
        Pressured, //when water has volume, but cant flow
        Shallow, //when water doesnt have volume, but can flow
        Fall, //no collider or water under
        Empty, //volume is 0 and neighbouring blocks want to fill its place
        Pushed, //water is near an empty block that need sto be filled
        Destroy, //water is surrounded by empty blocks
        Merge //water on another water block ?and water not in excess?
    }

    public class Cell : MonoBehaviour
    {
        // main info
        public float X;
        public float Y;
        public float Z;
        public int Volume;
        public Grid Grid;
        public CellState State;
        public CellState OldState;

        // Neighboring cells
        public Cell Top;
        public Cell Bottom;
        public Cell Right;
        public Cell Left;
        public Cell Front;
        public Cell Back;

        //source
        public bool CheckedSource = false;
        public int Source = 0;

        //extra
        private int Delay = 0;
        public MeshRenderer Mesh;

        //debug
        public bool Rcoll;
        public bool Fcoll;
        public bool Bcoll;
        public bool Lcoll;
        public bool Dcoll;
        public bool Ucoll;

        public bool SurroundedEmpty;

        void Awake()
        {

        }

        private void Start()
        {
            //if spawned first
            Initiate();
        }

        // Update is called once per frame
        void Update()
        {
            if (Delay == 50)
            {
                StartProcess();
                Delay = 0;
            }
            else
                Delay++;
        }

        public void Initiate()
        {
            Grid = GameObject.Find("Grid").GetComponent<Grid>();
            Mesh = GetComponent<MeshRenderer>();
            X = transform.position.x;
            Y = transform.position.y;
            Z = transform.position.z;

            Grid.PutIntoGrid(this);
            Grid.UpdateNeighbours(this);

            State = CellState.Flow;
            OldState = CellState.Flow;
        }

        void StartProcess()
        {
            Grid.UpdateNeighbours(this);
            OldState = State;
            SetState();
            RenderMesh();

            switch (State)
            {
                case CellState.Flow:
                    Flow();
                    break;
                case CellState.Pressured:
                    Pressured();
                    break;
                case CellState.Shallow:
                    Shallow();
                    break;
                case CellState.Fall:
                    Fall();
                    break;
                case CellState.Pushed:
                    Pushed();
                    break;
                case CellState.Destroy:
                    Destroy();
                    break;
                case CellState.Merge:
                    Merge();
                    break;
            }

            Grid.UpdateNeighbours(this);
        }
        private void SetState()
        {
            Vector4 sides = getSideColliders();
            int sum = (int)sides[0] + (int)sides[1] + (int)sides[2] + (int)sides[3];
            
            bool down = (Bottom == null && !ColliderExists(0, -1, 0, out Dcoll));
            
            Cell emptyNeighbour = GetEmptyNeighbour();
            bool surroundedByEmpty = SurroundedByEmpty();
            SurroundedEmpty = SurroundedByEmpty();

            if (Volume == 0)
                State = CellState.Empty;
            else if (down)
                State = CellState.Fall;
            else if (emptyNeighbour != null && (Bottom==null || Bottom.State != CellState.Still))
            {
                State = CellState.Pushed;
                //to not have stuck blocks
                if(OldState == CellState.Empty)
                    State = CellState.Pressured;
            } 
            else if (sum > 0 && Volume > 1)
                State = CellState.Flow;
            else if (Bottom != null && 
                (Bottom.State == CellState.Still || Bottom.State == CellState.Shallow || Bottom.State == CellState.Pressured) && 
                Grid.GetSourceVolume(GetSource())==0)
                State = CellState.Merge;
            else if (sum == 0 && Volume == 1)
                State = CellState.Still;
            else if (sum == 0 && Volume > 1)
                State = CellState.Pressured;
            else if (sum > 0 && Volume == 1)
                State = CellState.Shallow;
        }

        private void Flow()
        {
            //flow to sides
            //(front, right, back, left)
            Vector4 sides = getSideColliders();

            int sum = (int)sides[0] + (int)sides[1] + (int)sides[2] + (int)sides[3];
            int volumeEach = 0;
            int oldresidue = 0;
            int residue = 0;

            if (sum != 0)
            {
                volumeEach = (Volume - 1) / sum;
                residue = (Volume - 1) % sum;
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
                    Front = Grid.CreateCell(X + 1, Y + 0, Z + 0, volume, Source + 1);
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
                    Right = Grid.CreateCell(X + 0, Y + 0, Z - 1, volume, Source + 2);
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
                    Back = Grid.CreateCell(X - 1, Y + 0, Z + 0, volume, Source + 3);
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
                    Left = Grid.CreateCell(X + 0, Y + 0, Z + 1, volume, Source + 4);
            }

            Volume = Volume - (sum * volumeEach + oldresidue);
        }

        private void Pressured()
        {
            int source = GetSource();
            GiveVolume(source, 1);
        }

        private void GiveVolume(int source, int volume)
        {
            //works only with volume 1
            if(Grid.GiveVolume(source, volume))
            {
                Volume -= volume;
            }
        }

        private void Shallow()
        {
            int source = GetSource();
            GetVolume(source, 1);
        }

        private void GetVolume(int source, int volume)
        {
            //works only with volume 1
            if (Grid.GetVolume(source, volume))
            {
                Volume+= volume;
            }
        }

        private void Fall()
        {
            Grid.CreateCell(X + 0, Y - 1, Z + 0, Volume, Source);
            Volume = 0;
            Mesh.enabled = false;
        }

        private void Pushed()
        {
            Cell emptyCell = GetEmptyNeighbour();

            emptyCell.Volume++;
            emptyCell.Mesh.enabled = true;

            Volume--;
            if (Volume == 0)
            {
                Mesh.enabled = false;
            }
        }

        private Cell GetEmptyNeighbourRandom()
        {
            int[] cells = new int[] { 0, 0, 0, 0, 0 };
            int sum = 0;

            if (Bottom != null && Bottom.State == CellState.Empty)
            {
                cells[0] = 1;
                sum++;
            }
            if (Front != null && Front.State == CellState.Empty)
            {
                cells[1] = 1;
                sum++;
            }
            if (Right != null && Right.State == CellState.Empty)
            {
                cells[2] = 1;
                sum++;
            }
            if (Back != null && Back.State == CellState.Empty)
            {
                cells[3] = 1;
                sum++;
            }
            if (Left != null && Left.State == CellState.Empty)
            {
                cells[4] = 1;
                sum++;
            }

            System.Random rnd = new System.Random();
            int num = rnd.Next(0, sum);

            int i = 0;
            int cellnum = 0;
            foreach(int cell in cells)
            {
                if (num == 0 && cell == 1)
                {
                    cellnum = i;
                    break;
                }
                else if (cell == 1)
                    num--;
                i++;
            }

            switch (cellnum)
            {
                case 0:
                    return Bottom;
                case 1:
                    return Front;
                case 2:
                    return Right;
                case 3:
                    return Back;
                case 4:
                    return Left;
            }

            return null;
        }

        private Cell GetEmptyNeighbour()
        {
            Cell emptyCell = null;
            if (Bottom != null && Bottom.State == CellState.Empty)
                emptyCell = Bottom;
            else if (Front != null && Front.State == CellState.Empty)
                emptyCell = Front;
            else if (Right != null && Right.State == CellState.Empty)
                emptyCell = Right;
            else if (Back != null && Back.State == CellState.Empty)
                emptyCell = Back;
            else if (Left != null && Left.State == CellState.Empty)
                emptyCell = Left;

            return emptyCell;
        }

        private bool SurroundedByEmpty()
        {
            if ((Bottom != null && Bottom.State != CellState.Empty))
                return false;
            if ((Front != null && Front.State != CellState.Empty))
                return false;
            if ((Right != null && Right.State != CellState.Empty))
                return false;
            if ((Back != null && Back.State != CellState.Empty))
                return false;
            if ((Left != null && Left.State != CellState.Empty))
                return false;
            if ((Top != null && Top.State != CellState.Empty))
                return false;
            //if ((Bottom != null && Bottom.State != CellState.Empty))
            //    return false;
            //if ((Top != null && Top.State != CellState.Empty))
            //    return false;

            return true;
        }

        private int GetSource()
        {
            if (Bottom != null && (Bottom.State == CellState.Still || Bottom.State == CellState.Pushed))
                return Bottom.Source;
            else if (Front != null && (Front.State == CellState.Still || Front.State == CellState.Pushed))
                return Front.Source;
            else if (Right != null && (Right.State == CellState.Still || Right.State == CellState.Pushed))
                return Right.Source;
            else if (Back != null && (Back.State == CellState.Still || Back.State == CellState.Pushed))
                return Back.Source;
            else if (Left != null && (Left.State == CellState.Still || Left.State == CellState.Pushed))
                return Left.Source;
            else if (Top != null && (Top.State == CellState.Still || Top.State == CellState.Pushed))
                return Top.Source;

            return Source;
        }

        private void Merge()
        {
            Bottom.Volume += Volume;
            Volume = 0;
        }

        private void Destroy()
        {
            Grid.VolumeExcess += Volume;
            DeleteCell();
        }
        private void DeleteCell()
        {
            Grid.DeleteCell(this);
            Destroy(gameObject);
        }

        private void RenderMesh()
        {
            if(State == CellState.Empty)
            {
                Mesh.enabled = false;
            }
            else
                Mesh.enabled = true;
        }

        private Vector4 getSideColliders()
        {
            Vector4 sides = Vector4.zero;
            //front
            if (Front == null && !ColliderExists(1, 0, 0, out Fcoll))
                sides += new Vector4(1, 0, 0, 0);
            //right
            if (Right == null && !ColliderExists(0, 0, -1, out Rcoll))
                sides += new Vector4(0, 1, 0, 0);
            //back
            if (Back == null && !ColliderExists(-1, 0, 0, out Bcoll))
                sides += new Vector4(0, 0, 1, 0);
            //left
            if (Left == null && !ColliderExists(0, 0, 1, out Lcoll))
                sides += new Vector4(0, 0, 0, 1);

            return sides;
        }

        private bool ColliderExists(float x, float y, float z, out bool collider)
        {
            X = transform.position.x;
            Y = transform.position.y;
            Z = transform.position.z;

            Vector3 currentPosition = new Vector3(X, Y, Z);
            Vector3 checkDirection = new Vector3(x, y, z);

            RaycastHit[] colliders = Physics.SphereCastAll(currentPosition, 0.25f, checkDirection, 1.20f);

            /*
            foreach(RaycastHit coll in colliders)
            {
                Debug.Log("+");
            }*/

            //Debug.Log(colliders.Length);
            if (colliders.Length > 0)
            {
                collider = true;
                return true;
            }

            collider = false;
            return false;
        }
    }
}
