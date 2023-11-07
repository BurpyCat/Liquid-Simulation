using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;

namespace VoxelWater
{
    public class Grid : MonoBehaviour
    {
        Cell[,,] Cells;
        public GameObject Cube;
        public int VolumeExcess = 0;
        public int Offset = 50;
        public int GridSize = 100;

        void Awake()
        {
            Cells = new Cell[GridSize, GridSize, GridSize];
        }

        public Cell CreateCell(float x, float y, float z, int volume)
        {
            GameObject newCell = Instantiate(Cube, transform);
            newCell.transform.position = new Vector3(x, y, z);
            Cell cellScript = newCell.GetComponent<Cell>();
            cellScript.Initiate();
            cellScript.Volume = volume;

            return cellScript;
        }

        public void PutIntoGrid(Cell cell)
        {
            int X = (int)cell.X + Offset;
            int Y = (int)cell.Y + Offset;
            int Z = (int)cell.Z + Offset;

            //might change to more efficient grid size assessment
            while (X >= GridSize || X <= 0 ||
                Y >= GridSize || Y <= 0 ||
                Z >= GridSize || Z <= 0)
            {
                ExpandGrid();
                X = (int)cell.X + Offset;
                Y = (int)cell.Y + Offset;
                Z = (int)cell.Z + Offset;
            }

            Cells[X, Y, Z] = cell;
        }

        public void UpdateNeighbours(Cell cell)
        {
            int X = (int)cell.X + Offset;
            int Y = (int)cell.Y + Offset;
            int Z = (int)cell.Z + Offset;
            //front
            cell.Front = Cells[X + 1, Y, Z];
            //right
            cell.Right = Cells[X, Y, Z - 1];
            //back
            cell.Back = Cells[X - 1, Y, Z];
            //left
            cell.Left = Cells[X, Y, Z + 1];
            //bottom
            cell.Bottom = Cells[X, Y-1, Z];
            //top
            cell.Top = Cells[X, Y + 1, Z];
        }

        public void GiveVolume(int volume)
        {
            VolumeExcess += volume;
        }

        public bool GetVolume(int volume)
        {
            if(VolumeExcess - volume >= 0)
            {
                VolumeExcess -= volume;
                return true;
            }
            return false;
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
