/*
 * PURPOSE : 场景部分信息--场景每一个prefab对应的实例
 */
using UnityEngine;
using System.Collections.Generic;

namespace GameEngine
{
    public class ScenePart
    {
        protected int mAssetID = -1;
        private TransformData mTransformData;
        private int mReference = 0;
        protected GameObject mPartGo;
        private List<TransformData> mAllTransformData;
        private float mDelTimeStamp = 0;
        
#region Scene Part Load And Release
        private bool IsLoaded()
        {
            return mPartGo != null;
        }
        
        public void LoadPart()
        {
            mReference++;
            if (mReference == 1)
            {
                SetActive(true);
                if (!IsLoaded())
                {
                    // string assetName = mScene.GetPartAssetPath(mAssetID);
                    // if (!UEEngineRoot.ResMgr.GetAsset(assetName, OnScenePartAssetLoad))
                    // {
                    //     UELogMan.LogError("Load Part Asset Error: " + assetName);
                    // }
                }
            }
        }

        public void SetActive(bool isActive)
        {
            if (mPartGo != null)
            {
                mPartGo.SetActive(isActive);
            }
        }

        public void ReleasePart()
        {
            mReference--;
            if (mReference == 0)
            {
                mDelTimeStamp = Time.realtimeSinceStartup;
                SetActive(false);
            }
        }
        
        public bool DeletePart(bool force = false)
        {
            if (!force && Time.realtimeSinceStartup - mDelTimeStamp < SceneChunkDef.DELETE_SCENE_PART_DELAY)
                return false;
            
            if (mPartGo != null)
            {
                GameObject.Destroy(mPartGo);
                mPartGo = null;
            }

            return true;
        }

        private bool CanInstantiate()
        {
            return mReference > 0;
        }

        private void OnScenePartAssetLoad(string path, object obj)
        {
            if (!CanInstantiate())
                return;            

            SetPartPose();
        }

        protected void SetPartPose()
        {
            Queue<Transform> transQueue = new Queue<Transform>();
            transQueue.Enqueue(mPartGo.transform);
            int transIndex = 0;
            while (transQueue.Count > 0)
            {
                Transform tr = transQueue.Dequeue();
                TransformData td = mAllTransformData[transIndex++];
                tr.position = td.Position;
                tr.eulerAngles = td.EulerAngles;
                tr.localScale = td.LocalScale;
                
                for (int i = 0; i < tr.childCount; i++)
                {
                    transQueue.Enqueue(tr.GetChild(i)); 
                }
            }
        }
#endregion
        
        public override bool Equals(object obj)
        {
            ScenePart sp = obj as ScenePart;
            if (sp == null)
            {
                return false;
            }
            
            if (!mAssetID.Equals(sp.mAssetID))
            {
                return false;
            }
            
            if (mAllTransformData == null || sp.mAllTransformData == null)
            {
                return false;
            }

            if (mAllTransformData.Count != sp.mAllTransformData.Count)
            {
                return false;
            }
            
            for (int i = 0; i < mAllTransformData.Count; i++)
            {
                if (!mAllTransformData[i].Equals(sp.mAllTransformData[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void LoadData(GameBinaryFile bf)
        {
            mAssetID = bf.ReadInt32();

            mTransformData = new TransformData(null);
            Queue<TransformData> tdQueue = new Queue<TransformData>();
            tdQueue.Enqueue(mTransformData);
            mAllTransformData = new List<TransformData>();
            while (tdQueue.Count > 0)
            {
                TransformData td = tdQueue.Dequeue();
                td.LoadTransformData(bf);
                mAllTransformData.Add(td);
                for (int i = 0; i < td.ChildCount; i++)
                {
                    TransformData child =  new TransformData(td);
                    td.AddChild(child);
                    tdQueue.Enqueue(child);
                }
            }
        }

        public void LoadData(string[] data, ref int index)
        {
            string sAssetId = data[index++].Replace("AssetID: ", "");
            int.TryParse(sAssetId, out mAssetID);

            mTransformData = new TransformData(null);
            Queue<TransformData> tdQueue = new Queue<TransformData>();
            tdQueue.Enqueue(mTransformData);
            mAllTransformData = new List<TransformData>();
            while (tdQueue.Count > 0)
            {
                TransformData td = tdQueue.Dequeue();
                td.LoadTransformData(data[index]);
                mAllTransformData.Add(td);
                for (int i = 0; i < td.ChildCount; i++)
                {
                    index++;
                    TransformData child = new TransformData(td);
                    td.AddChild(child);
                    tdQueue.Enqueue(child);
                }
            }

            index++;
        }
    }
}