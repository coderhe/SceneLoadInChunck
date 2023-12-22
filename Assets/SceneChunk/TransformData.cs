using UnityEngine;
using System.Collections.Generic;

namespace GameEngine
{
     public class TransformData
    {
        /// <summary>
        /// 子节点数据集合
        /// </summary>
        private List<TransformData> mChilds = new List<TransformData>();

        /// <summary>
        /// 索引
        /// </summary>
        private int mIndex = -1;
        public int Index
        {
            get { return mIndex; }
        }

        /// <summary>
        /// 坐标位置
        /// </summary>
        private Vector3 mPosition = Vector3.zero;
        public Vector3 Position
        {
            get { return mPosition; }
        }

        /// <summary>
        /// 欧拉角
        /// </summary>
        private Vector3 mEulerAngles = Vector3.zero;
        public Vector3 EulerAngles
        {
            get {return mEulerAngles; }
        }

        /// <summary>
        /// 大小
        /// </summary>
        private Vector3 mLocalScale = Vector3.zero;
        public Vector3 LocalScale
        {
            get { return mLocalScale; }
        }

        /// <summary>
        /// 父节点数据
        /// </summary>
        private TransformData mParent;
        public TransformData Parent
        {
            get { return mParent; }
        }

        /// <summary>
        /// 子节点数量
        /// </summary>
        private int mChildCount = 0;
        public int ChildCount
        {
            get { return mChildCount; }
        }
        
        public TransformData(TransformData parent)
        {
            mParent = parent;
        }

        public void AddChild(TransformData td)
        {
            mChilds.Add(td);
        }
        
        public void LoadTransformData(GameBinaryFile bf)
        {
            mIndex = bf.ReadInt32();
            mPosition = EditSLBinary.LoadVector3(bf);
            mEulerAngles = EditSLBinary.LoadVector3(bf);
            mLocalScale = EditSLBinary.LoadVector3(bf);
            mChildCount = bf.ReadInt32();
        }

        public void LoadTransformData(string data)
        {
            //去除结尾的"\r"
            data = data.Substring(0, data.Length - 1);
            string[] contents = data.Split(SceneChunkDef.ITEM_SEPARATOR);
            if (contents[0].Split(SceneChunkDef.RESOURCE_SEPARATOR).Length <= 2)
                mChildCount = 1;

            if (contents.Length == 4)
            {
                mPosition = _StringToVec3(contents[1]);
                mEulerAngles = _StringToVec3(contents[2]);
                mLocalScale = _StringToVec3(contents[3]);
            }
        }

        private Vector3 _StringToVec3(string data)
        {
            string[] contents = data.Split(",");
            if (contents.Length != 3)
                return Vector3.zero;

            string content = contents[0].Substring(1);
            float x, y, z = .0f;
            float.TryParse(content, out x);
            float.TryParse(contents[1], out y);
            content = contents[2].Substring(0, contents[2].Length - 1);
            float.TryParse(content, out z);
            return new Vector3(x, y, z);
        }

        public override bool Equals(object obj)
        {
            TransformData tfd = obj as TransformData;
            if (tfd == null)
                return false;            
            
            if (mPosition != tfd.Position || mEulerAngles != tfd.EulerAngles || mLocalScale != tfd.LocalScale)
                return false;            

            if (mChildCount != tfd.ChildCount)
                return false;            
                
            return true;
        }
    }
}