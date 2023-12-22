using UnityEngine;

namespace GameEngine
{
    public class SceneChunkDebug : MonoBehaviour
    {
        /// <summary>
        /// 地块大小
        /// </summary>
        public int ChunkSize = 20;
        /// <summary>
        /// 地块层数
        /// </summary>
        public int ChunkLayer = 20;
        /// <summary>
        /// 
        /// </summary>
        public GameObject DebugGo = null;
        /// <summary>
        /// 是否显示地块
        /// </summary>
        public bool ShowChunkDebug = true;

        private void OnDrawGizmos()
        {
            DrawChunkDebugInfo();            
            DrawDebugGoBound();
        }

        private void DrawChunkDebugInfo()
        {
            if (!ShowChunkDebug)
                return;

            for (int i = 0; i < SceneChunkUtil.GetChunkTotalCount(ChunkLayer); i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(SceneChunkUtil.GetChunkCenterPos(i, ChunkSize), Vector3.one * ChunkSize);
            }
        }
        
        private void DrawDebugGoBound()
        {
            if (DebugGo == null)
                return;
            
            Renderer[] childRenders = GameTools.GetComponentsInChildren<Renderer>(DebugGo, true);
            Vector3 center = Vector3.zero;
            foreach (Renderer render in childRenders)
            {
                center += render.bounds.center;
            }
            
            center /= childRenders.Length;
            Bounds partBounds = new Bounds(center, Vector3.zero);
            foreach (Renderer render in childRenders)
            {
                partBounds.Encapsulate(render.bounds);
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(partBounds.center, partBounds.size);
        }
    }
}