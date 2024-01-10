using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;
using Unity.Jobs;
using UnityEditor.Experimental.GraphView;

namespace VoxelWater
{
    public enum CellState
    {
        Flow, //when water has volume and can flow
        Still, //when water doesnt have volume and cant flow
        Pressured, //when water has volume, but cant flow// currently not used
        Shallow, //when water doesnt have volume, but can flow
        Fall, //no collider or water under
        Empty, //volume is 0 and neighbouring blocks want to fill its place
        Pushed, //water is near an empty block that need sto be filled
        Destroy, //water is surrounded by empty blocks
        Merge, //water on another water block ?and water not in excess?

        Create, //block creates infinite other blocks
        Remove //block destroys surrounding blocks
    }

    public class Cell : MonoBehaviour, IEquatable<Cell>
    {
        public GameObject GridObject;

        public bool CreateWater = false;
        public bool RemoveWater = false;
        public int DelayTime = 40;

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

        //extra
        private int Delay = 0;
        public MeshRenderer Mesh;

        //materials
        public Renderer RendererMaterial;
        public Material NormalMaterial;
        public Material PressuredMaterial;
        public Material ShallowMaterial;

        //diagostic
        public Diagnostic Diagnostics;

        //debug
        public bool Rcoll;
        public bool Fcoll;
        public bool Bcoll;
        public bool Lcoll;
        public bool Dcoll;
        public bool Ucoll;

        public bool SurroundedEmpty;



        private void Start()
        {
            Initiate();
        }

        // Update is called once per frame
        /*
        void Update()
        {
            //if (Delay == DelayTime)
            //{
                StartProcess();
              //  Delay = 0;
            //}
            //else
            //    Delay++;
        }
        */

        public bool Equals(Cell other)
        {
            if(other == null) return false;
            if(other == this) return true;
            return false;
        }

        public void Initiate()
        {
            if(Diagnostics == null)
            {
                Diagnostics = GameObject.Find("Diagnostic").GetComponent<Diagnostic>();
                Diagnostics.IncreaseCellCount();
            }

            if (GridObject != null)
                Grid = GridObject.GetComponent<Grid>();
            Mesh = GetComponent<MeshRenderer>();
            RendererMaterial = GetComponent<Renderer>();
            X = transform.position.x;
            Y = transform.position.y;
            Z = transform.position.z;

            //Grid.PutIntoGrid(this);
            //Grid.UpdateNeighbours(this);

            if (RemoveWater)
            {
                State = CellState.Remove;
            }
            else if (CreateWater)
            {
                State = CellState.Create;
                Grid.PutIntoGrid(this);
            }
            else
            {
                State = CellState.Flow;
                OldState = CellState.Flow;
            }

            ChangeMaterial();
        }

        public IEnumerator StartProcess()
        {
            if (State == CellState.Flow || State != CellState.Fall)
            {
                Grid.UpdateNeighbours(this);
            }
            
            OldState = State;
            if (!RemoveWater && !CreateWater) SetState();
            RenderMesh();
            ChangeMaterial();

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
                case CellState.Remove:
                    Remove();
                    break;
                case CellState.Create:
                    Create(5);
                    break;
                case CellState.Empty:
                    Destroy();
                    break;
            }

            if (State != CellState.Still)
            {
                Grid.UpdateNeighbours(this);
            }

            yield return null;
        }

        public void StartProcessNoCoroutine()
        {
            if (State == CellState.Flow || State != CellState.Fall)
            {
                Grid.UpdateNeighbours(this);
            }

            OldState = State;
            if (!RemoveWater && !CreateWater) SetState();
            RenderMesh();
            ChangeMaterial();

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
                case CellState.Remove:
                    Remove();
                    break;
                case CellState.Create:
                    Create(5);
                    break;
                case CellState.Empty:
                    Destroy();
                    break;
            }

            if (State != CellState.Still)
            {
                Grid.UpdateNeighbours(this);
            }
        }

        public void StartProcess2()
        {
            if (State == CellState.Flow || State != CellState.Fall)
            {
                Grid.UpdateNeighbours(this);
            }
            
            OldState = State;
            if (!RemoveWater && !CreateWater) SetState();
            
            //RenderMesh();
            
            //ChangeMaterial();
            
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
                    //Destroy();
                    break;
                case CellState.Merge:
                    //Merge();
                    break;
                case CellState.Remove:
                    //Remove();
                    break;
                case CellState.Create:
                    Create(5);
                    break;
                case CellState.Empty:
                    //Destroy();
                    break;
            }

            if (State != CellState.Still)
            {
                Grid.UpdateNeighbours(this);
            }
            
        }

        private void Create(int volume)
        {
            int[] sides = getSideColliders();
            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
            if (sum == 0)
            {
                FlowAll(volume);
            }
            else
                Flow(false);
        }

        private void Remove()
        {
            if (Top != null)
                Top.Volume = 0;
            if (Bottom != null)
                Bottom.Volume = 0;
            if (Right != null)
                Right.Volume = 0;
            if (Left != null)
                Left.Volume = 0;
            if (Front != null)
                Front.Volume = 0;
            if (Back != null)
                Back.Volume = 0;
        }

        private void ChangeMaterial()
        {
            switch (State)
            {
                case CellState.Shallow:
                    RendererMaterial.material = ShallowMaterial;
                    break;
                case CellState.Fall:
                    RendererMaterial.material = ShallowMaterial;
                    break;
                case CellState.Flow:
                    RendererMaterial.material = ShallowMaterial;
                    break;
                case CellState.Pressured:
                    RendererMaterial.material = PressuredMaterial;
                    break;
                default:
                    RendererMaterial.material = NormalMaterial;
                    break;
            }
        }

        private void SetState()
        {
            int[] sides = getSideColliders();
            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];

            bool down = ((Bottom == null || Bottom.State == CellState.Empty) && !ColliderExists(0, -1, 0, out Dcoll));
            
            Cell emptyNeighbour = GetEmptyNeighbour();
            bool surroundedByEmpty = SurroundedByEmpty();
            SurroundedEmpty = SurroundedByEmpty();

            if(surroundedByEmpty && Volume == 0)
                State = CellState.Destroy;
            else if (Volume == 0)
                State = CellState.Empty;
            else if (down)
                State = CellState.Fall;
            //else if (emptyNeighbour != null && (Bottom==null || Bottom.State != CellState.Still))
            //{
            //    State = CellState.Pushed;
            //    //to not have stuck blocks
            //    if(OldState == CellState.Empty)
            //        State = CellState.Pressured;
            //} 
            else if (sum > 0 && Volume > 1)
                State = CellState.Flow;
            else if (Bottom != null && 
                (Bottom.State == CellState.Shallow))
                State = CellState.Merge;
            else if (sum == 0 && Volume == 1)
                State = CellState.Still;
            else if (sum == 0 && Volume > 1)
                State = CellState.Pressured;
            else if (sum > 0 && Volume == 1)
                State = CellState.Shallow;
        }

        private void Flow(bool decreaseVolume = true)
        {
            //flow to sides
            //(front, right, back, left)
            int[] sides = getSideColliders(); //five array

            int sum = sides[0] + sides[1] + sides[2] + sides[3] + sides[4];
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
                {
                    if (Front != null)
                        Front.Volume += volume;
                    else
                        Front = Grid.CreateCell(X + 1, Y + 0, Z + 0, volume);
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
                        Right.Volume += volume;
                    else
                        Right = Grid.CreateCell(X + 0, Y + 0, Z - 1, volume);
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
                        Back.Volume += volume;
                    else
                        Back = Grid.CreateCell(X - 1, Y + 0, Z + 0, volume);
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
                        Left.Volume += volume;
                    else
                        Left = Grid.CreateCell(X + 0, Y + 0, Z + 1, volume);
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
                        Bottom.Volume += volume;
                    else
                        Bottom = Grid.CreateCell(X + 0, Y - 1, Z + 0, volume);
                }
            }

            if (decreaseVolume)
                Volume = Volume - (sum * volumeEach + oldresidue);
        }

        private void Pressured()
        {
            GiveVolume(1);
        }

        private void GiveVolume(int volume)
        {
            //Grid.GiveVolume(volume);
            Volume -= volume;
        }

        private void Shallow()
        {
            GetVolume(1);
        }

        private void GetVolume(int volume)
        {
            //works only with volume 1
            //if (Grid.GetVolume(volume))
            //{
                Volume+= volume;
            //}
        }

        private void Fall()
        {
            if (Bottom == null)
                Grid.CreateCell(X + 0, Y - 1, Z + 0, Volume);
            else
                Bottom.Volume += Volume;
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

        private void FlowAll(int volume)
        {
            if (Bottom != null)
                Bottom.Volume+=volume;
            if (Front != null)
                Front.Volume += volume;
            if (Right != null)
                Right.Volume += volume;
            if (Back != null)
                Back.Volume += volume;
            if (Left != null)
                Left.Volume += volume;
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
            //Grid.DeleteCell(this);
            //Destroy(gameObject);
        }

        private void RenderMesh()
        {
            if(State == CellState.Empty || Volume == 0)
            {
                Mesh.enabled = false;
            }
            else
                Mesh.enabled = true;
        }

        private int[] getSideColliders()
        {
            int[] sides = { 0, 0, 0, 0, 0 };
            //front
            if ((Front == null || Front.State == CellState.Empty) && !ColliderExists(1, 0, 0, out Fcoll))
                sides[0] = 1;
            //right
            if ((Right == null || Right.State == CellState.Empty) && !ColliderExists(0, 0, -1, out Rcoll))
                sides[1] = 1;
            //back
            if ((Back == null || Back.State == CellState.Empty) && !ColliderExists(-1, 0, 0, out Bcoll))
                sides[2] = 1;
            //left
            if ((Left == null || Left.State == CellState.Empty) && !ColliderExists(0, 0, 1, out Lcoll))
                sides[3] = 1;
            //bottom
            if ((Bottom == null || Bottom.State == CellState.Empty) && !ColliderExists(0, -1, 0, out Bcoll))
                sides[4] = 1;

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
