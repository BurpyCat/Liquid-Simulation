using UnityEngine;

namespace VoxelWater
{
    public class GridManager : MonoBehaviour
    {
        Grid[,,] Grids;
        public GameObject GridPrefab;
        //(GridSize -1 )/2
        //nelyginis
        public int GridSize = 21;
        //(GridManagerSize -1 )/2
        public int GridOffset = 50;
        //nelyginis
        public int GridManagerSize = 101;

        void Awake()
        {
            Grids = new Grid[GridManagerSize, GridManagerSize, GridManagerSize];
            GridOffset = (GridManagerSize - 1) / 2;
        }

        public Grid GetGrid(int x, int y, int z, int Xorg, int Yorg, int Zorg)
        {
            int X = Xorg + GridOffset;
            int Y = Yorg + GridOffset;
            int Z = Zorg + GridOffset;

            if (x >= GridSize)
            {
                if (Grids[X + 1, Y, Z] == null)
                    CreateGrid(Xorg + 1, Yorg, Zorg);

                return Grids[X + 1, Y, Z];
            }
            else if (x <= -1)
            {
                if (Grids[X - 1, Y, Z] == null)
                    CreateGrid(Xorg - 1, Yorg, Zorg);

                return Grids[X - 1, Y, Z];
            }
            else if (y >= GridSize)
            {
                if (Grids[X, Y + 1, Z] == null)
                    CreateGrid(Xorg, Yorg + 1, Zorg);

                return Grids[X, Y + 1, Z];
            }
            else if (y <= -1)
            {
                if (Grids[X, Y - 1, Z] == null)
                    CreateGrid(Xorg, Yorg - 1, Zorg);

                return Grids[X, Y - 1, Z];
            }
            else if (z >= GridSize)
            {
                if (Grids[X, Y, Z + 1] == null)
                    CreateGrid(Xorg, Yorg, Zorg + 1);

                return Grids[X, Y, Z + 1];
            }
            else if (z <= -1)
            {
                if (Grids[X, Y, Z - 1] == null)
                    CreateGrid(Xorg, Yorg, Zorg - 1);

                return Grids[X, Y, Z - 1];
            }
            return null;
        }

        public Cell GetCell(int x, int y, int z, int Xorg, int Yorg, int Zorg)
        {
            int X = Xorg + GridOffset;
            int Y = Yorg + GridOffset;
            int Z = Zorg + GridOffset;

            if (x >= GridSize)
            {
                if (Grids[X + 1, Y, Z] == null)
                    return null;

                int newx = x - GridSize;
                //PutIntoGridInfo(Grids[X,Y,Z], Grids[X + 1, Y, Z].Cells[newx, y, z], x,y,z);
                return Grids[X + 1, Y, Z].Cells[newx, y,z];
            }
            else if (x <= -1)
            {
                if (Grids[X - 1, Y, Z] == null)
                    return null;
                
                int newx = x + GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X - 1, Y, Z].Cells[newx, y, z], x, y, z);
                return Grids[X - 1, Y, Z].Cells[newx, y, z];
            }
            else if (y >= GridSize)
            {
                if (Grids[X, Y + 1, Z] == null)
                    return null;

                int newy = y - GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y + 1, Z].Cells[x, newy, z], x, y, z);
                return Grids[X, Y + 1, Z].Cells[x, newy, z];
            }
            else if (y <= -1)
            {
                if (Grids[X, Y - 1, Z] == null)
                    return null;

                int newy = y + GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y - 1, Z].Cells[x, newy, z], x, y, z);
                return Grids[X, Y - 1, Z].Cells[x, newy, z];
            }
            else if (z >= GridSize)
            {
                if (Grids[X, Y, Z + 1] == null)
                {
                    return null;
                }

                int newz = z - GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y, Z + 1].Cells[x, y, newz], x, y, z);
                return Grids[X, Y, Z + 1].Cells[x, y, newz];
            }
            else if (z <= -1)
            {
                if (Grids[X, Y, Z - 1] == null)
                {
                    return null;
                }

                int newz = z + GridSize;
                //PutIntoGridInfo(Grids[X, Y, Z], Grids[X, Y, Z - 1].Cells[x, y, newz], x, y, z);
                return Grids[X, Y, Z - 1].Cells[x, y, newz];
            }
            return Grids[X, Y, Z].Cells[x, y, z];
        }

        //temporary
        public void PutIntoGridInfo(Grid grid, Cell cell, int x, int y, int z)
        {
            if (cell == null)
                return;
            else
            {
                CellInfoUtility.Put(cell.Cellinfo, x + 1, y + 1, z + 1, grid.GridSizeCI, grid.CellsInfo);
            }
        }

        public void CreateGrid(int X, int Y, int Z)
        {
            GameObject newGrid = Instantiate(GridPrefab, transform);
            newGrid.transform.position = new Vector3(X, Y, Z);

            Grid gridScript = newGrid.GetComponent<Grid>();
            gridScript.Initiate(X, Y, Z);

            PutIntoGridManager(X, Y, Z, gridScript);
        }

        public void PutIntoGridManager(int X, int Y, int Z, Grid grid)
        {
            //expansion?
            Grids[X+GridOffset, Y + GridOffset, Z + GridOffset] = grid;
        }
    }
}
