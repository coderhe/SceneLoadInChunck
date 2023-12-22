using GameEngine;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EditorScenePart : ScenePart
{
    private Transform mRootTrans;
    private bool mIsEditorCreate = false;

    public EditorScenePart() { }
    public EditorScenePart(Transform rootTrans, int assetID)
    {
        mRootTrans = rootTrans;
        mAssetID = assetID;
        mIsEditorCreate = true;
    }

    public int AssetID
    {
        get { return mAssetID; }
    }

    public void LoadPart(GameObject goRoot, string prefabPath)
    {
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(prefabPath.Replace('&', '/'));
        if (obj == null)
            return;
        
        mPartGo = (GameObject)PrefabUtility.InstantiatePrefab(obj);
        mPartGo.transform.parent = goRoot.transform;
        SetPartPose();
    }
    
    public void SaveDebugData(GameTextFile file)
    {
        file.WriteLine("AssetID: " + mAssetID);
        Queue<Transform> transQueue = new Queue<Transform>();
        transQueue.Enqueue(mRootTrans);
        while (transQueue.Count > 0)
        {
            Transform tr = transQueue.Dequeue();
            string path = tr.GetFullHierarchyPath().Replace(mRootTrans.GetFullHierarchyPath(), "").Replace("/", "&");
            file.WriteLine((string.IsNullOrEmpty(path) ? "root" : path) + SceneChunkDef.ITEM_SEPARATOR + tr.position +
                           SceneChunkDef.ITEM_SEPARATOR + tr.eulerAngles + SceneChunkDef.ITEM_SEPARATOR + tr.localScale);

            for (int i = 0; i < tr.childCount; i++)
            {
                transQueue.Enqueue(tr.GetChild(i));
            }
        }
    }
    
    public void SaveData(GameBinaryFile bf)
    {
        bf.Writer.Write(AssetID);
        Queue<Transform> transQueue = new Queue<Transform>();
        transQueue.Enqueue(mRootTrans);
        int transIndex = 0;
        while (transQueue.Count > 0)
        {
            Transform tr = transQueue.Dequeue();
            bf.Writer.Write(transIndex++);
            EditSLBinary.SaveVector3(bf, tr.position);
            EditSLBinary.SaveVector3(bf, tr.eulerAngles);
            EditSLBinary.SaveVector3(bf, tr.localScale);
            bf.Writer.Write(tr.childCount);
            for (int i = 0; i < tr.childCount; i++)
            {
                transQueue.Enqueue(tr.GetChild(i));
            }
        }
    }
    
    public override bool Equals(object obj)
    {
        EditorScenePart sp = obj as EditorScenePart;
        if (sp == null)
            return false;
        
        if(mIsEditorCreate)
            return mRootTrans.GetFullHierarchyPath() == sp.mRootTrans.GetFullHierarchyPath();
        else
            return base.Equals(obj);        
    }
}