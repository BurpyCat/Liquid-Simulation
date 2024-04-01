using UnityEngine;
using System;

namespace VoxelWater
{
    public enum CellState
    {
        None,
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

    [Serializable]
    public struct CellInfo : IEquatable<CellInfo>
    {
        // main info
        public float X;
        public float Y;
        public float Z;
        public int Volume;
        //grid number?
        //public int GridNumber;
        public CellState State;
        public CellState OldState;

        // Neighboring cells
        //have CellStates instead
        //that are manually updates outside of thread?
        public CellState TopState;
        public CellState BottomState;
        public CellState RightState;
        public CellState LeftState;
        public CellState FrontState;
        public CellState BackState;

        //neighbour volume
        public int TopVolume;
        public int BottomVolume;
        public int RightVolume;
        public int LeftVolume;
        public int FrontVolume;
        public int BackVolume;

        public override bool Equals(object? obj) => obj is CellInfo other && this.Equals(other);

        public bool Equals(CellInfo cell) => X == cell.X && Y == cell.Y && Z == cell.Z &&
                                            Volume == cell.Volume && State == cell.State && OldState == cell.OldState &&
                                            TopState == cell.TopState && TopVolume == cell.TopVolume &&
                                            BottomState == cell.BottomState && BottomVolume == cell.BottomVolume &&
                                            RightState == cell.RightState && RightVolume == cell.RightVolume &&
                                            LeftState == cell.LeftState && LeftVolume == cell.LeftVolume &&
                                            FrontState == cell.FrontState && FrontVolume == cell.FrontVolume &&
                                            BackState == cell.BackState && BackVolume == cell.BackVolume;


        public override int GetHashCode() => (X, Y, Z).GetHashCode();

        public static bool operator ==(CellInfo cell1, CellInfo cell2) => cell1.Equals(cell2);

        public static bool operator !=(CellInfo cell1, CellInfo cell2) => !(cell1 == cell2);
    }

    public class Cell : MonoBehaviour, IEquatable<Cell>
    {
        public CellInfo Cellinfo;

        //??
        public GameObject GridObject;

        //special state enable
        public bool CreateWater = false;
        public bool RemoveWater = false;

        //grid number?
        //remove
        public Grid Grid;

        // Neighboring cells
        //have CellStates instead
        //that are manually updates outside of thread?      
        //remove
        /*
        public Cell Top;
        public Cell Bottom;
        public Cell Right;
        public Cell Left;
        public Cell Front;
        public Cell Back;  
        */

        //extra
        public MeshRenderer Mesh;

        //materials
        public Renderer RendererMaterial;
        public Material NormalMaterial;
        public Material PressuredMaterial;
        public Material ShallowMaterial;

        //diagostic
        public Diagnostic Diagnostics;

        private void Start()
        {
            Initiate();
        }

        public bool Equals(Cell other)
        {
            if(other == null) return false;
            if(other == this) return true;
            return false;
        }

        public void Initiate()
        {
            //diagnostics
            /*
            if(Diagnostics == null)
            {
                Diagnostics = GameObject.Find("Diagnostic").GetComponent<Diagnostic>();
                Diagnostics.IncreaseCellCount();
            }
            */

            if (GridObject != null)
                Grid = GridObject.GetComponent<Grid>();
            Mesh = GetComponent<MeshRenderer>();
            RendererMaterial = GetComponent<Renderer>();
            Cellinfo.X = transform.position.x;
            Cellinfo.Y = transform.position.y;
            Cellinfo.Z = transform.position.z;

            if (RemoveWater)
            {
                Cellinfo.State = CellState.Remove;
            }
            else if (CreateWater)
            {
                Cellinfo.State = CellState.Create;
                Cellinfo.Volume = 10;
                Grid.PutIntoGrid(this);
                Grid.PutIntoInfoList(this);
                Grid.PutIntoInfoGrid(this);
            }
            else
            {
                Cellinfo.State = CellState.Flow;
                Cellinfo.OldState = CellState.Flow;
            }

            ChangeMaterial();
        }

        //move to Grid
        /*
        public void StartProcess()
        {
            //if (Cellinfo.State == CellState.Flow || Cellinfo.State != CellState.Fall)
            //{
                Grid.GetNeighboursInfo(this);
                //Grid.UpdateNeighboursInfo(this);
                
            //}

            
            if (!RemoveWater && !CreateWater) CellUtility.SetState(ref Cellinfo);
            

            CellUtility.ActivateState(ref Cellinfo, Grid);

            //if (Cellinfo.State != CellState.Still)
            //{
                Grid.UpdateNeighboursInfo(this);
                //Grid.UpdateNeighboursInfo(this);
            //}
        }
        */

        public void RenderCell()
        {
            RenderMesh();
            ChangeMaterial();
        }

        private void ChangeMaterial()
        {
            switch (Cellinfo.State)
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
        
        private void RenderMesh()
        {
            if(Cellinfo.State == CellState.Empty || Cellinfo.Volume == 0)
            {
                Mesh.enabled = false;
            }
            else
                Mesh.enabled = true;
        }
    }
}
