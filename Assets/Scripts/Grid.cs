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

        //excess volume source
        public Dictionary<int, int> VolumeExcess2;

        void Awake()
        {
            Cells = new Cell[GridSize, GridSize, GridSize];
            VolumeExcess2 = new Dictionary<int, int>();
        }

        public Cell CreateCell(float x, float y, float z, int volume, int source)
        {
            GameObject newCell = Instantiate(Cube, transform);
            newCell.transform.position = new Vector3(x, y, z);
            Cell cellScript = newCell.GetComponent<Cell>();
            cellScript.Initiate();
            cellScript.Volume = volume;
            cellScript.Source = source;

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

            if(cell.State==CellState.Still)
                UpdateSources(cell);
        }

        private void UpdateSources(Cell cell)
        {
            //add shallow
            if (cell.Front != null &&
                (cell.Front.State == CellState.Still || cell.Front.State == CellState.Pushed || cell.Front.State == CellState.Shallow) &&
                cell.Source < cell.Front.Source)
                cell.Front.Source = cell.Source;
            
            if (cell.Right != null &&
                (cell.Right.State == CellState.Still || cell.Right.State == CellState.Pushed || cell.Right.State == CellState.Shallow) &&
                cell.Source < cell.Right.Source)
                cell.Right.Source = cell.Source;
            
            if (cell.Back != null &&
                (cell.Back.State == CellState.Still || cell.Back.State == CellState.Pushed || cell.Back.State == CellState.Shallow) &&
                cell.Source < cell.Back.Source)
                cell.Back.Source = cell.Source;
            
            if (cell.Left != null &&
                (cell.Left.State == CellState.Still || cell.Left.State == CellState.Pushed || cell.Left.State == CellState.Shallow) &&
                cell.Source < cell.Left.Source)
                cell.Left.Source = cell.Source;
            
            if (cell.Top != null &&
                (cell.Top.State == CellState.Still || cell.Top.State == CellState.Pushed || cell.Top.State == CellState.Shallow) &&
                cell.Source < cell.Top.Source)
                cell.Top.Source = cell.Source;

            if (cell.Bottom != null &&
                (cell.Bottom.State == CellState.Still || cell.Bottom.State == CellState.Pushed || cell.Bottom.State == CellState.Shallow) &&
                cell.Source < cell.Bottom.Source)
                cell.Bottom.Source = cell.Source;
        }

        public bool GiveVolume(int source, int volume)
        {
            //debug
            //source = -1;

            int value;
            if (VolumeExcess2.TryGetValue(source, out value))
            {
                //if (value == 1)
                //    return false;

                Debug.Log("Give exists "+source+" "+value);
                VolumeExcess2[source] += volume;
                return true;
            }
            else
            {
                Debug.Log("Give not " + source + " " + value);
                //VolumeExcess2.Add(source, volume);
                return false;
            }
        }

        public bool GetVolume(int source, int volume)
        {
            //debug
            //source = -1;

            int value;
            if (VolumeExcess2.TryGetValue(source, out value))
            {
                Debug.Log("Get exists " + source + " " + value);
                if (value > 0)
                {
                    VolumeExcess2[source] -= volume;
                    
                    if(VolumeExcess2[source]==0)
                        VolumeExcess2.Remove(source);
                    
                    return true;
                }
                else
                {
                    //VolumeExcess2.Remove(source);
                    return false;
                }
            }
            else
            {
                Debug.Log("Get not " + source + " " + value);
                VolumeExcess2.Add(source, 0);
                return false;
            }
        }

        public int GetSourceVolume(int source)
        {
            //debug
            //source = -1;

            int value;
            if (VolumeExcess2.TryGetValue(source, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
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
