using Unity.Collections;
using Unity.Jobs;

namespace VoxelWater
{
    public struct UpdateGridsParallel : IJobFor
    {
        public int gridSizeFull;
        public int gridSizeFullCI;
        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> newCellsArr;
        [NativeDisableParallelForRestriction] public NativeArray<int> newCellsCountArr;
        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> updatedCellsArr;
        [NativeDisableParallelForRestriction] public NativeArray<int> updatedCellsCountArr;

        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> cellsInfo_listArr;
        [NativeDisableParallelForRestriction] public NativeArray<int> cellsInfoCountArr;
        [NativeDisableParallelForRestriction] public NativeArray<CellInfo> cellsInfoArr;
        [NativeDisableParallelForRestriction] public NativeArray<GridInfo> gridInfoArr;

        [NativeDisableParallelForRestriction] public NativeArray<bool> collidersArr;
        public void Execute(int i)
        {
            //copy from bigger arrays
            GridInfo gridInfo = gridInfoArr[i];
            if (!gridInfo.Active)
                return;

            int fullGridSize = gridInfo.GridSize * gridInfo.GridSize * gridInfo.GridSize;
            int fullGridSizeCI = gridInfo.GridSizeCI * gridInfo.GridSizeCI * gridInfo.GridSizeCI;

            int cellsInfoCount = cellsInfoCountArr[i];
            CellInfo[] cellsInfo_list = CellInfoUtility.ExtractCellInfoArray(i, cellsInfo_listArr, fullGridSize, cellsInfoCount);
            CellInfo[] cellsInfo = CellInfoUtility.ExtractCellInfoArray(i, cellsInfoArr, fullGridSizeCI, fullGridSizeCI);

            CellInfo[] newCells = new CellInfo[fullGridSize];
            int newCellsCount = 0;
            CellInfo[] updatedCells = new CellInfo[fullGridSize];
            int updatedCellsCount = 0;

            bool[] colliders = CellInfoUtility.ExtractBoolArray(i, collidersArr, fullGridSizeCI, fullGridSizeCI);

            GridUtility.UpdateCells(cellsInfo_list, cellsInfoCount, cellsInfo, gridInfo,
                newCells, ref newCellsCount, updatedCells, ref updatedCellsCount, colliders);

            //copy to bigger arrays
            CellInfoUtility.InjectCellInfoArray(i, ref newCellsArr, fullGridSize, newCells, newCellsCount);
            newCellsCountArr[i] = newCellsCount;
            CellInfoUtility.InjectCellInfoArray(i, ref updatedCellsArr, fullGridSize, updatedCells, updatedCellsCount);
            updatedCellsCountArr[i] = updatedCellsCount;

            CellInfoUtility.InjectCellInfoArray(i, ref cellsInfo_listArr, fullGridSize, cellsInfo_list, cellsInfoCount);
            CellInfoUtility.InjectCellInfoArray(i, ref cellsInfoArr, fullGridSizeCI, cellsInfo, fullGridSizeCI);
        }
    }
}
