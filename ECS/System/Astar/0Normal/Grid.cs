using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
 
public class Grid<T>  where T:GridNode, new()
    {
        private int width;
        private int height;
        private int cellSize;
        
        private T [,] gridArray;
        private FixedVector3 originPosition;
        /// <summary>
        /// 新建一个网格系统
        /// </summary>
        /// <param name="width">一行有几个格子</param>
        /// <param name="height">一列有几个格子</param>
        /// <param name="cellSize">格子大小</param>
        /// <param name="originPosition">起始偏移量</param>
        public Grid(int width, int height, int cellSize, FixedVector3 originPosition ){
         
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;
            gridArray = new T[width, height];
            for (int x = 0; x < gridArray.GetLength(0); x++) {
                for (int y = 0; y < gridArray.GetLength(1); y++) {
                    gridArray[x, y] = new T();
                    gridArray[x, y].Init(x,y);
            }
        }
        }



        public FixedVector3 GetWorldPosition(int x,int z){
            return new FixedVector3(x,0,z) * cellSize;
        }
        public void GetXZ(FixedVector3 worldPosition, out int x, out int y){
            x = (worldPosition.x.round / cellSize).RawInt;
            y = (worldPosition.z.round / cellSize).RawInt;
        }

        public int GetWidth() => width;
        public int GetHeight() => height;
        public int GetCellSize() => cellSize;


        public T GetNode(int x, int z){
            if(x>=0 && z>=0 && x< width && z< height)
            {
                return gridArray[x,z];
            }
      
            return default(T);
            
        }
        public void SetNode(int x, int z, T value){
            if(x>=0 && z>=0 && x< width && z< height)
                gridArray[x,z] = value;
        }

        public void SetNode(FixedVector3 worldPosition, T value){
            int x, z;
            GetXZ(worldPosition, out x, out z);
            SetNode(x, z, value);

        }
    }
 