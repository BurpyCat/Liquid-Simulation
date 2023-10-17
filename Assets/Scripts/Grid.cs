using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelWater
{
    public class Grid : MonoBehaviour
    {
        Cell[,,] cells;
        public GameObject Cube;
        public int VolumeExcess;
        // Start is called before the first frame update
        void Awake()
        {
            cells = new Cell[100, 100, 100];
            VolumeExcess = 0;
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
            cells[(int)cell.Xgrid, (int)cell.Ygrid, (int)cell.Zgrid] = cell;
        }

        public void UpdateNeighbours(Cell cell)
        {
            int X = (int)cell.Xgrid;
            int Y = (int)cell.Ygrid;
            int Z = (int)cell.Zgrid;
            //front
            cell.Front = cells[X + 1, Y, Z];
            //right
            cell.Right = cells[X, Y, Z - 1];
            //back
            cell.Back = cells[X - 1, Y, Z];
            //left
            cell.Left = cells[X, Y, Z + 1];
            //bottom
            cell.Bottom = cells[X, Y-1, Z];
        }

        public void DeleteCell(Cell cell)
        {
            int X = (int)cell.Xgrid;
            int Y = (int)cell.Ygrid;
            int Z = (int)cell.Zgrid;
            cells[X, Y, Z] = null;
        }
    }
}
