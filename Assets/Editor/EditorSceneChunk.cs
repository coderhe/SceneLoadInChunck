using GameEngine;
using System.Collections.Generic;

public class EditorSceneChunk : SceneChunk<EditorScenePart>
{
    
    public List<EditorScenePart> SceneParts
    {
        get
        {
            return mSceneParts;
        }
    }

    public EditorSceneChunk(int chunkSize)
    {
        SetChunkSize(chunkSize);
    }
    
    public override void AddScenePartInfo(EditorScenePart sp)
    {
        if (!mSceneParts.Contains(sp))
            mSceneParts.Add(sp);        
    }
    
    public void SaveData(GameBinaryFile bf)
    {
        if (bf == null || bf.Writer == null)
            return;
        
        bf.Writer.Write(mChunkIndex);
        bf.Writer.Write(mSceneParts.Count);
        for (int i = 0; i < mSceneParts.Count; i++)
        {
            mSceneParts[i].SaveData(bf);
        }
    }
}