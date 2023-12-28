using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;
using Unity.Jobs;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace VoxelWater
{
    public class Grid : MonoBehaviour
    {
        public Cell[,,] Cells;
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


        void Awake()
        {
            Initiate(0, 0, 0);
        }

        public void Initiate(int X, int Y, int Z)
        {
            Manager = GameObject.Find("GridManager").GetComponent<GridManager>();
            GridSize = Manager.GridSize;
            Offset = (GridSize - 1) / 2;
            GridOffset = Manager.GridOffset;
            Cells = new Cell[GridSize, GridSize, GridSize];

            this.X = X; 
            this.Y = Y; 
            this.Z = Z;

            //Manager.PutIntoGridManager(X, Y, Z, this);
        }

        public Cell CreateCell(float x, float y, float z, int volume)
        {
            Grid grid = GetGrid((int)x, (int)y, (int)z);


            Cell cellScript = grid.CreateCellObject(x, y, z, volume);
            grid.PutIntoGrid(cellScript);
            grid.UpdateNeighbours(cellScript);

            return cellScript;
        }

        public Cell CreateCellObject(float x, float y, float z, int volume)
        {
            GameObject newCell = Instantiate(Cube, transform);
            newCell.transform.position = new Vector3(x, y, z);
            Cell cellScript = newCell.GetComponent<Cell>();

            cellScript.Initiate();
            cellScript.Volume = volume;

            return cellScript;
        }

        //maybe change X Y and Z
        public Grid GetGrid(int Xorg, int Yorg, int Zorg)
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
            int x = (int)cell.X + Offset - (X * GridSize);
            int y = (int)cell.Y + Offset - (Y * GridSize);
            int z = (int)cell.Z + Offset - (Z * GridSize);

            Cells[x, y, z] = cell;
            cell.Grid = this;
        }

        public void UpdateNeighbours(Cell cell)
        {
            int x = (int)cell.X + Offset - (X * GridSize);
            int y = (int)cell.Y + Offset - (Y * GridSize);
            int z = (int)cell.Z + Offset - (Z * GridSize);
            
            //front
            if (x + 1 >= GridSize)
                cell.Front = Manager.UpdateNeighbours(x + 1, y, z, X, Y, Z);
            else
                cell.Front = Cells[x + 1, y, z];
            //right
            if (z - 1 <= -1)
                cell.Right = Manager.UpdateNeighbours(x, y, z - 1, X, Y, Z);
            else
                cell.Right = Cells[x, y, z - 1];
            //back
            if (x-1 <= -1)
                cell.Back = Manager.UpdateNeighbours(x - 1, y, z, X, Y, Z);
            else
                cell.Back = Cells[x - 1, y, z];
            //left 
            if (z+1 >= GridSize)
                cell.Left = Manager.UpdateNeighbours(x, y, z + 1, X, Y, Z);
            else
                cell.Left = Cells[x, y, z + 1];     
            //bottom
            if (y - 1 <= -1)
                cell.Bottom = Manager.UpdateNeighbours(x, y - 1, z, X, Y, Z);
            else
                cell.Bottom = Cells[x, y - 1, z];
            //top
            if (y + 1 >= GridSize)
                cell.Top = Manager.UpdateNeighbours(x, y + 1, z, X, Y, Z);
            else
                cell.Top = Cells[x, y + 1, z];
        }

        public void DeleteCell(Cell cell)
        {
            int X = (int)cell.X + Offset;
            int Y = (int)cell.Y + Offset;
            int Z = (int)cell.Z + Offset;
            Cells[X, Y, Z] = null;
        }

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
