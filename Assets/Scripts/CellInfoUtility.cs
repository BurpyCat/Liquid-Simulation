using Unity.Collections;
using VoxelWater;

public static class CellInfoUtility
{
    //2d flattened array to 1d, native to normal
    static public bool[] ExtractBoolArray(int ind, NativeArray<bool> array, int lengthArray, int count)
    {
        int index = ind * lengthArray;
        bool[] newArr = new bool[count];
        for (int i = 0; i < count; i++)
        {
            newArr[i] = array[index + i];
        }
        return newArr;
    }
    static public CellInfo[] ExtractCellInfoArray(int ind, NativeArray<CellInfo> array, int lengthArray, int count)
    {
        int index = ind * lengthArray;
        CellInfo[] newArr = new CellInfo[count];
        for (int i = 0; i < count; i++)
        {
            newArr[i] = array[index + i];
        }
        return newArr;
    }
    static public void InjectCellInfoArray(int ind, ref NativeArray<CellInfo> orgArray, int orgLengthArray, CellInfo[] array, int lengthArray)
    {
        int index = ind * orgLengthArray;
        for (int i = 0; i < lengthArray; i++)
        {
            orgArray[index + i] = array[i];
        }
    }
    //2d flattened array to 1d
    static public CellInfo[] ExtractCellInfoArray(int ind, CellInfo[] array, int lengthArray, int count)
    {
        int index = ind * lengthArray;
        CellInfo[] newArr = new CellInfo[count];
        for (int i = 0; i < count; i++)
        {
            newArr[i] = array[index+i];
        }
        return newArr;
    }
    static public CellInfo[] InjectCellInfoArray(int ind, CellInfo[] orgArray, int orgLengthArray, CellInfo[] array, int lengthArray)
    {
        int index = ind * orgLengthArray;
        for (int i = 0; i < lengthArray; i++)
        {
            orgArray[index+i] = array[i];
        }
        return orgArray;
    }

    //3d flattened array to 1d
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
    public static bool Get(int x, int y, int z, int size, bool[] grid)
    {
        if (x >= size || y >= size || z >= size ||
            x < 0 || y < 0 || z < 0)
            return false;

        //i = x + width* y + width* height*z;
        int index = x + size * y + size * size * z;
        return grid[index];
    }
    public static bool[] Put(bool cellinfo, int x, int y, int z, int size, bool[] grid)
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
