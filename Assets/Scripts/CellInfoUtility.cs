using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using VoxelWater;

public static class CellInfoUtility
{
    public static CellInfo Get( int x, int y, int z, int size, CellInfo[] grid)
    {
        if (x >= size || y >= size || z >= size ||
            x < 0 || y < 0 || z < 0)
            return CellUtility.EmptyCell;

        //i = x + width* y + width* height*z;
        int index = x + size * y + size * size * z;
        return grid[index];
    }
    public static CellInfo[] Put(CellInfo cellinfo, int x, int y, int z, int size, CellInfo[] grid)
    {
        if (x >= size || y >= size || z >= size ||
            x < 0 || y < 0 || z < 0)
            return grid;

        //i = x + width* y + width* height*z;
        int index = x + size * y + size * size * z;
        grid[index] = cellinfo;
        return grid;
    }
    public static CellInfo[] Create(int size)
    {
        return new CellInfo[size * size * size];
    }
}
