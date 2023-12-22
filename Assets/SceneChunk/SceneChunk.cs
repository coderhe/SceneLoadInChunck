/* 
 * PURPOSE: 地块信息--每个场景格子所储存的信息
 */
using UnityEngine;
using System.Collections.Generic;

namespace GameEngine
{
    public class SceneChunk<T> where T : ScenePart, new()
    {
        /// <summary>
        /// 地块大小
        /// </summary>
        private int mChunkSize = 0;
        /// <summary>
        /// 是否加载标志位
        /// </summary>
        private bool mIsLoaded = false;
        /// <summary>
        /// 场景地块容器
        /// </summary>
        protected List<T> mSceneParts = new List<T>();

        /// <summary>
        /// 地块索引
        /// </summary>
        protected int mChunkIndex = -1;
        public int ChunkIndex
        {
            get
            {
                return mChunkIndex;
            }
        }

        /// <summary>
        /// 中心点
        /// </summary>
        private Vector3 mCenterPos = Vector3.zero;
        public Vector3 CenterPos
        {
            get
            {
                return mCenterPos;
            }
        }

        public SceneChunk() {   }
                
        public SceneChunk(int chunkSize)
        {
            SetChunkSize(chunkSize);
        }
                
        protected void SetChunkSize(int chunkSize)
        {
            mChunkSize = chunkSize;
        }
        
        public void SetChunkIndex(int chunkIndex)
        {
            mChunkIndex = chunkIndex;
            mCenterPos = SceneChunkUtil.GetChunkCenterPos(mChunkIndex, mChunkSize);
        }
        
        public int DiffChunkLayer(Vector3 pos)
        {
            return Mathf.CeilToInt(Mathf.Max(Mathf.Abs(mCenterPos.x - pos.x) / mChunkSize, Mathf.Abs(mCenterPos.z - pos.z) / mChunkSize));
        }

        public virtual void AddScenePartInfo(T sp)
        {
            if (!mSceneParts.Contains(sp))
            {
                mSceneParts.Add(sp);   
            }
        }

        public void LoadChunk()
        {
            if (mIsLoaded)
                return;
            
            if (mSceneParts != null)
            {
                for (int i = 0; i < mSceneParts.Count; i++)
                {
                    mSceneParts[i].LoadPart();
                }
            }

            mIsLoaded = true;
        }

        public void ReleaseChunk()
        {
            if (!mIsLoaded)
                return;
            
            if (mSceneParts != null)
            {
                for (int i = 0; i < mSceneParts.Count; i++)
                {
                    mSceneParts[i].ReleasePart();
                }
            }

            mIsLoaded = false;
        }
        
        public void LoadData(GameBinaryFile bf)
        {
            if (bf == null || bf.Reader == null)
                return;

            SetChunkIndex(bf.ReadInt32());
            int partCount = bf.ReadInt32();
            for (int i = 0; i < partCount; i++)
            {
                T sp = new T();
                sp.LoadData(bf);
                AddScenePartInfo(sp);
            }
        }

        public void LoadData(string[] data, ref int index, int partCount)
        {
            if (data == null || data.Length <= 0)
                return;

            string sChunckIndex = data[index].Replace("ChunkIndex: ", "");
            int chunkIndex = 0;
            int.TryParse(sChunckIndex, out chunkIndex);
            SetChunkIndex(chunkIndex);
            index = index + 2;
            for (int i = 0; i < partCount; i++)
            {
                T sp = new T();
                sp.LoadData(data, ref index);
                AddScenePartInfo(sp);
            }
        }
    }
}