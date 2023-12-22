using System;
using UnityEngine;

namespace GameEngine
{
    public enum CHUNK_CORNER
    {
        TOP_LEFT = 0,
        TOP_RIGH = 1,
        BOTTOM_RIGHT = 2,
        BOTTOM_LEFT = 3,
    }

    public static class SceneChunkUtil
    {
        public static string GetFullHierarchyPath(this Transform transform)
        {
            string path = "/" + transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = "/" + transform.name + path;
            }

            return path;
        }
        /*
         * 获取指定层的单边chunk数量
         */
        public static int GetLayerChunkRow(int chunkLayer)
        {
            return 2 * chunkLayer + 1;
        }

        /*
         * 获取指定层的chunk数量
         */
        public static int GetLayerChunkCount(int chunkLayer)
        {
            return 8 * chunkLayer;
        }

        /*
         * 获取指定的层，指定角的层级索引，从0开始
         */
        public static int GetCornerLayerIndex(int chunkLayer, CHUNK_CORNER conner)
        {
            int row = GetLayerChunkRow(chunkLayer);
            return row * (int)conner - (int)conner;
        }

        /*
         * 指定坐标是否处于chunk边上
         */
        public static bool IsChunkBorderline(Vector3 pos, int chunkSize)
        {
            float x = (pos.x - chunkSize / 2f);
            float z = pos.z - chunkSize / 2f;

            if (x % chunkSize == 0 || z % chunkSize == 0)
            {
                return true;
            }

            return false;
        }

        /*
         * 获取指定chunk的中心点坐标
         */
        public static Vector3 GetChunkCenterPos(int chunkIndex, int chunkSize)
        {
            int chunkLayer = 0, layerIndex = 0;
            while (chunkIndex > GetLayerChunkCount(chunkLayer))
            {
                chunkIndex -= GetLayerChunkCount(chunkLayer);
                layerIndex = chunkIndex - 1;
                chunkLayer++;
            }

            int halfRowCount = Mathf.CeilToInt(GetLayerChunkRow(chunkLayer) / 2f);
            int centerIndex = halfRowCount - 1;
            int x = 0, z = 0;
            if (layerIndex <= GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.TOP_RIGH))
            {
                x = layerIndex < centerIndex ? -(centerIndex - layerIndex) : layerIndex - centerIndex;
                z = chunkLayer;
            }
            else if (layerIndex <= GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_RIGHT))
            {
                centerIndex = GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.TOP_RIGH) + halfRowCount - 1;
                x = chunkLayer;
                z = layerIndex < centerIndex ? centerIndex - layerIndex : -(layerIndex - centerIndex);
            }
            else if (layerIndex <= GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_LEFT))
            {
                centerIndex = GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_RIGHT) + halfRowCount - 1;
                x = layerIndex < centerIndex ? centerIndex - layerIndex : -(layerIndex - centerIndex);
                z = -chunkLayer;
            }
            else
            {
                centerIndex = GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_LEFT) + halfRowCount - 1;
                x = -chunkLayer;
                z = layerIndex < centerIndex ? -(centerIndex - layerIndex) : (layerIndex - centerIndex);
            }

            return new Vector3(x * chunkSize, 0, z * chunkSize);
        }

        /*
         * 根据指定位置获取所属的chunk索引
         */
        public static int GetChunkIndex(Vector3 pos, int chunkSize)
        {
            int xDiff = (int)Mathf.Ceil((Mathf.Abs(pos.x) - chunkSize / 2f) / chunkSize);
            int zDiff = (int)Mathf.Ceil((Mathf.Abs(pos.z) - chunkSize / 2f) / chunkSize);
            int chunkLayer = Mathf.Max(xDiff, zDiff);
            int indexDiff = Math.Abs(zDiff - xDiff);
            int layerIndex = 0;

            //第二象限+Z+
            if (pos.x <= 0 && pos.z > 0)            
            {
                int cornerIndex = GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.TOP_LEFT);
                layerIndex = zDiff >= xDiff
                    ? cornerIndex + indexDiff
                    : GetLayerChunkCount(chunkLayer) - indexDiff;
            }
            //第一象限+X+
            else if (pos.x > 0 && pos.z >= 0) 
            {
                int cornerIndex = GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.TOP_RIGH);
                layerIndex = zDiff > xDiff
                    ? cornerIndex - indexDiff
                    : cornerIndex + indexDiff;
            }
            //第四象限
            else if (pos.x >= 0 && pos.z < 0) 
            {
                int cornerIndex = GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_RIGHT);
                layerIndex = xDiff > zDiff
                    ? cornerIndex - indexDiff
                    : cornerIndex + indexDiff;
            }
            //第三象限
            else if (pos.x < 0 && pos.z <= 0) 
            {
                int cornerIndex = GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_LEFT);
                layerIndex = zDiff > xDiff
                    ? cornerIndex - indexDiff
                    : cornerIndex + indexDiff;
            }

            return GetChunkIndex(chunkLayer, layerIndex);
        }

        /*
         * 根据chunk层和层内索引返回chunk索引
         */
        public static int GetChunkIndex(int chunkLayer, int LayerIndex = 0)
        {
            if (chunkLayer == 0)
            {
                return 0;
            }

            chunkLayer--;
            return GetChunkTotalCount(chunkLayer) + LayerIndex;
        }

        /*
         * 根据chunk层，获取总的chunk数量
         */
        public static int GetChunkTotalCount(int chunkLayer)
        {
            return chunkLayer * 8 + chunkLayer * (chunkLayer - 1) / 2 * 8 + 1;
        }
    }
}