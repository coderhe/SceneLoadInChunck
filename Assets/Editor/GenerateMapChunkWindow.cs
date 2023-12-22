using System;
using System.IO;
using GameEngine;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class GenerateMapChunkWindow : EditorWindow
{
    [MenuItem("SceneArtTools/地图资源分块数据", false)]
    public static void OpenGenerateMapChunkWindow()
    {
        EditorWindow.GetWindow<GenerateMapChunkWindow>().Show();
    }
    private GameObject mGoRoot = null;
    private Object[] mPrefabObjs = new Object[5];
    private int mChunkSize = 10;
    private int mGenerateChunkLayer = 2;
    
    //chunk 存储数据结构
    private List<string> mScenePartAssetPaths = new List<string>();
    private Dictionary<int, EditorSceneChunk> mSceneChunkDict = new Dictionary<int, EditorSceneChunk>();

    private bool mGenerateDebugInfo = false;
    private bool mShowTestCreateChunk = false;
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        mChunkSize = EditorGUILayout.IntField("地图块大小", mChunkSize);
#region 测试生成工具
        mShowTestCreateChunk = EditorGUILayout.Toggle("测试生成工具", mShowTestCreateChunk);
        if (mShowTestCreateChunk)
        {
            mGenerateChunkLayer = EditorGUILayout.IntField("生成层数", mGenerateChunkLayer);
            //自动化生成场景代码
            mGoRoot = (GameObject) EditorGUILayout.ObjectField("根节点", mGoRoot, typeof(GameObject));
            for (int i = 0; i < mPrefabObjs.Length; i++)
            {
                mPrefabObjs[i] = EditorGUILayout.ObjectField("chunk_" + i, mPrefabObjs[i], typeof(Object));
            }

            if (GUILayout.Button("生成地形资源/Generate Chunk Resource"))
            {
                if (mGoRoot != null)
                {
                    int chunkLayer = 0;
                    while (chunkLayer < mGenerateChunkLayer)
                    {
                        int count = SceneChunkUtil.GetLayerChunkCount(chunkLayer);
                        Vector3 beginPos = new Vector3(-mChunkSize, 0, mChunkSize) * chunkLayer;
                        Vector3 lastChunkPos = beginPos;
                        CreateChunk(beginPos, chunkLayer, 0);
                        for (int j = 1; j < count; j++)
                        {
                            Vector3 chunkPos = lastChunkPos;
                            if (j <= SceneChunkUtil.GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.TOP_RIGH))
                            {
                                chunkPos.x += mChunkSize;
                            }
                            else if (j <= SceneChunkUtil.GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_RIGHT))
                            {
                                chunkPos.z -= mChunkSize;
                            }
                            else if (j <= SceneChunkUtil.GetCornerLayerIndex(chunkLayer, CHUNK_CORNER.BOTTOM_LEFT))
                            {
                                chunkPos.x -= mChunkSize;
                            }
                            else
                            {
                                chunkPos.z += mChunkSize;
                            }
                        
                            CreateChunk(chunkPos, chunkLayer, j);
                            lastChunkPos = chunkPos;
                        }

                        chunkLayer++;
                    }
                }
            }
        }
#endregion

        mGenerateDebugInfo = EditorGUILayout.Toggle("测试文件模式", mGenerateDebugInfo);
        GUILayout.Space(12f);
        if (!mShowTestCreateChunk && GUILayout.Button("GenerateSceneConfig"))
        {
            GameObject go = GameObject.Find(SceneManager.GetActiveScene().name);
            if (go != null)
            {
                Transform transRoot = go.transform;
                for (int j = 0; j < transRoot.childCount; j++)
                {
                    EditorUtility.DisplayProgressBar(string.Format("处理chunk信息({0}/{1})", j, transRoot.childCount),
                        string.Format("处理chunk信息: {0}", transRoot.GetChild(j).name), j / transRoot.childCount);

                    GeneratePartChunkRef(transRoot.GetChild(j));
                }

                EditorUtility.DisplayProgressBar("TIP", "Generating scene config..........", 1f);
                WriteMapInfoToFile();
                ClearCacheMapChunkInfo();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("TIP", "Generate scene config success", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("TIP", "Generate scene config Error，Can't Find the root: " + SceneManager.GetActiveScene().name, "OK");
            }
        }

        GUILayout.Space(12f);
        if (!mShowTestCreateChunk && GUILayout.Button("根据SceneConfig创建Scene"))
        {
            List<string> scenePartPathIDString = new List<string>();
            string dataFolderPath = Path.Combine(Path.GetDirectoryName(SceneManager.GetActiveScene().path), "..", SceneChunkDef.SAVE_MAP_INFO_FOLDER_NAME);
            if (mGenerateDebugInfo)
            {
#region Read Debug File
                string debugFolderPath = Path.Combine(dataFolderPath, "Debug");
                string assetIDPath = Path.Combine(debugFolderPath, SceneChunkDef.DEBUG_ASSETID_FILE_NAME);
                string sceneInfoPath = Path.Combine(debugFolderPath, SceneChunkDef.DEBUG_SCENEINFO_FILE_NAME);
                if (!Directory.Exists(debugFolderPath))
                {
                    Directory.CreateDirectory(debugFolderPath);
                    EditorUtility.DisplayDialog("TIP", "Read file error，can't find the Debug file!", "OK");
                    return;
                }

                if (GameEngineFileUtil.FileExists(assetIDPath))
                {
                    FileInfo saveFileInfo = new FileInfo(assetIDPath);
                    if (saveFileInfo.IsReadOnly)
                    {
                        File.SetAttributes(assetIDPath, FileAttributes.Normal);
                    }

                    GameTextFile file = new GameTextFile(System.Text.Encoding.Unicode);
                    if (file.OpenRead(assetIDPath))
                    {
                        string contents = file.Reader.ReadToEnd();
                        string[] lines = contents.Split("\n");
                        for (int i = 0; i < lines.Length; ++i)
                        {
                            string line = lines[i].Replace(string.Format("{0} = ", i), "");
                            if(line.Length > 0)
                            {
                                //去除结尾的"\r"
                                line = line.Substring(0, line.Length - 1);
                                scenePartPathIDString.Add(line);
                            }                            
                        }
                    }
                    else
                    {
                        Debug.Log("Read Asset_ID File Error! No Such File Path: " + assetIDPath);
                    }

                    file.Close();
                }

                if (GameEngineFileUtil.FileExists(sceneInfoPath))
                {
                    FileInfo saveFileInfo = new FileInfo(sceneInfoPath);
                    if (saveFileInfo.IsReadOnly)
                    {
                        File.SetAttributes(sceneInfoPath, FileAttributes.Normal);
                    }

                    GameObject rootGo = GameObject.Find(SceneManager.GetActiveScene().name);
                    if (rootGo == null)
                        rootGo = new GameObject(SceneManager.GetActiveScene().name);

                    GameTextFile file = new GameTextFile(System.Text.Encoding.Unicode);
                    List<EditorScenePart> temp = new List<EditorScenePart>();
                    if (file.OpenRead(sceneInfoPath))
                    {
                        int index = 0;
                        string contents = file.Reader.ReadToEnd();
                        string[] lines = contents.Split("\n");
                        string skybox = lines[index].Replace("SkyMaterial: ", "");
                        Debug.LogError("skybox = " + skybox);
                        index++;
                        string lightmaps = lines[index].Replace("LightmapsCount: ", "");
                        int lightMapDataCount = 0;
                        int.TryParse(lightmaps, out lightMapDataCount);
                        for (int i = 0; i < lightMapDataCount; i++)
                        {
                            index++;
                            string prefixStr = string.Format("Lightmap[{0}]", i);
                            string lightmapColor = lines[index++].Replace(prefixStr + "_Color: ", "");
                            string lightmapDir = lines[index++].Replace(prefixStr + "_Dir: ", "");
                            string shadowMask = lines[index].Replace(prefixStr + "_shadowMask: ", "");
                        }

                        string chunkSize = lines[++index].Replace("ChunkSize: ", "");
                        int.TryParse(chunkSize, out mChunkSize);

                        string sChunkCount = lines[++index].Replace("ChunkCount: ", "");
                        index++;
                        int chuckCount, curSpCount = 0;
                        int.TryParse(sChunkCount, out chuckCount);
                        for (int i = 0; i < chuckCount; ++i)
                        {
                            string spCount = lines[index + 1].Replace("ScenePartsCount: ", "");
                            int.TryParse(spCount, out curSpCount);

                            EditorSceneChunk sc = new EditorSceneChunk(mChunkSize);
                            sc.LoadData(lines, ref index, curSpCount);

                            foreach (EditorScenePart sp in sc.SceneParts)
                            {
                                if (!temp.Contains(sp))
                                {
                                    sp.LoadPart(rootGo, scenePartPathIDString[sp.AssetID]);
                                    temp.Add(sp);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Read Scene_Info Error! No Such File Path: " + sceneInfoPath);
                    }

                    file.Close();
                }
#endregion
            }
            else
            {
#region Read Binary File
                string assetIDPath = Path.Combine(dataFolderPath, SceneChunkDef.ASSETID_FILE_NAME);
                string sceneInfoPath = Path.Combine(dataFolderPath, SceneChunkDef.SCENEINFO_FILE_NAME);
                if (GameEngineFileUtil.FileExists(assetIDPath))
                {
                    FileInfo saveFileInfo = new FileInfo(assetIDPath);
                    if (saveFileInfo.IsReadOnly)
                    {
                        File.SetAttributes(assetIDPath, FileAttributes.Normal);
                    }
                }

                if (GameEngineFileUtil.FileExists(sceneInfoPath))
                {
                    FileInfo saveFileInfo = new FileInfo(sceneInfoPath);
                    if (saveFileInfo.IsReadOnly)
                    {
                        File.SetAttributes(sceneInfoPath, FileAttributes.Normal);
                    }
                }

                try
                {
                    GameBinaryFile bf = new GameBinaryFile(System.Text.Encoding.Unicode);
                    if (bf.OpenRead(assetIDPath))
                    {
                        int count = bf.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            scenePartPathIDString.Add(bf.ReadString());
                        }
                    }
                    else
                    {
                        Debug.Log("Read Asset_ID File Error! No Such File Path: " + assetIDPath);
                    }

                    bf.Close();
                }
                catch (Exception)
                {
                    EditorUtility.DisplayDialog("ERROR", "Read Asset_ID File Error!", "OK");
                }

                try
                {
                    GameObject rootGo = GameObject.Find(SceneManager.GetActiveScene().name);
                    if (rootGo == null)
                        rootGo = new GameObject(SceneManager.GetActiveScene().name);

                    GameBinaryFile bf = new GameBinaryFile(System.Text.Encoding.Unicode);
                    List<EditorScenePart> temp = new List<EditorScenePart>();
                    if (bf.OpenRead(sceneInfoPath))
                    {
                        string skybox = EditSLBinary.LoadString(bf);
                        Debug.LogError("skybox = " + skybox);
                        int lightMapDataCount = bf.ReadInt32();
                        for (int i = 0; i < lightMapDataCount; i++)
                        {
                            string lightmapColor = EditSLBinary.LoadString(bf); // lightmapColor
                            string lightmapDir = EditSLBinary.LoadString(bf);   // lightmapDir
                            string shadowMask = EditSLBinary.LoadString(bf);    // shadowMask
                        }

                        mChunkSize = bf.ReadInt32();
                        int count = bf.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            EditorSceneChunk sc = new EditorSceneChunk(mChunkSize);
                            sc.LoadData(bf);

                            foreach (EditorScenePart sp in sc.SceneParts)
                            {
                                if (!temp.Contains(sp))
                                {
                                    sp.LoadPart(rootGo, scenePartPathIDString[sp.AssetID]);
                                    temp.Add(sp);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Read Scene_Info Error! No Such File Path: " + sceneInfoPath);
                    }

                    bf.Close();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    EditorUtility.DisplayDialog("ERROR", "Read Scene_Info File Error!", "OK");
                }
            }
#endregion
        }

        EditorGUILayout.EndVertical();
    }

    private void ClearCacheMapChunkInfo()
    {
        mSceneChunkDict.Clear();
        mScenePartAssetPaths.Clear();
    }
    
    private void GeneratePartChunkRef(Transform partTrans)
    {
        if (!PrefabUtility.IsAnyPrefabInstanceRoot(partTrans.gameObject))
            return;        
        
        Renderer[] childRenders = GameTools.GetComponentsInChildren<Renderer>(partTrans.gameObject, true);
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
        
        Vector3 min = partBounds.min;
        Vector3 max = partBounds.max;
        Vector3 min2Max = max - min;

        int xDiffMax = Mathf.CeilToInt(Mathf.Abs(min2Max.x) / mChunkSize);
        int zDiffMax = Mathf.CeilToInt(Mathf.Abs(min2Max.z) / mChunkSize);
        int assetID = GetScenePartAssetID(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(partTrans));
        EditorScenePart sp = new EditorScenePart(partTrans, assetID);
        
        Vector3 currPos = min;
        for (int xDiff = 0; xDiff <= xDiffMax; xDiff++)
        {
            for (int zDiff = 0; zDiff <= zDiffMax; zDiff++)
            {
                if (SceneChunkUtil.IsChunkBorderline(currPos, mChunkSize))
                {
                    Vector3 topRight = new Vector3(1, 0, 1);
                    AddScenePartInfo(SceneChunkUtil.GetChunkIndex(currPos + topRight, mChunkSize), sp);   
                    AddScenePartInfo(SceneChunkUtil.GetChunkIndex(currPos - topRight, mChunkSize), sp);   
                    Vector3 topLeft = new Vector3(-1, 0, 1);
                    AddScenePartInfo(SceneChunkUtil.GetChunkIndex(currPos + topLeft, mChunkSize), sp);  
                    AddScenePartInfo(SceneChunkUtil.GetChunkIndex(currPos - topLeft, mChunkSize), sp);  
                }
                else
                {
                    AddScenePartInfo(SceneChunkUtil.GetChunkIndex(currPos, mChunkSize), sp);   
                }
                currPos.z = currPos.z + (min2Max.z / Mathf.Abs(min2Max.z)) * mChunkSize;
            }
            currPos.x = currPos.x + (min2Max.x / Mathf.Abs(min2Max.x)) * mChunkSize;
            currPos.z = min.z;
        }
    }

    private void AddScenePartInfo(int chunkIndex, EditorScenePart sp)
    {
        EditorSceneChunk chunk = null;
        if (!mSceneChunkDict.TryGetValue(chunkIndex, out chunk))
        {
            chunk = new EditorSceneChunk(mChunkSize);
            chunk.SetChunkIndex(chunkIndex);
            mSceneChunkDict.Add(chunkIndex, chunk);
        }
        chunk.AddScenePartInfo(sp);
    }
    
    private int GetScenePartAssetID(string assetPath)
    {
        assetPath = assetPath.Replace('/', '&');
        if (mScenePartAssetPaths.Count == 0)
        {
            mScenePartAssetPaths.Add("");
        }
        if (!mScenePartAssetPaths.Contains(assetPath))
        {
            mScenePartAssetPaths.Add(assetPath);
            return mScenePartAssetPaths.Count - 1;
        }
        else
        {
            return mScenePartAssetPaths.IndexOf(assetPath);
        }
    }

    private GameObject CreateChunk(Vector3 pos, int chunkLayer, int layerIndex)
    {
        Object createObj = mPrefabObjs[UnityEngine.Random.Range(0, mPrefabObjs.Length)];
        GameObject go = PrefabUtility.InstantiatePrefab(createObj, mGoRoot.transform) as GameObject;
        go.transform.localPosition = pos;
        go.name = string.Format("chunk_{0}_{1}_{2}", chunkLayer, layerIndex, pos);
        Transform indexTrans = go.transform.Find("Canvas/Index");
        if (indexTrans != null)
        {
            indexTrans.GetComponent<Text>().text = SceneChunkUtil.GetChunkIndex(chunkLayer, layerIndex) + "";
        }
        return go;
    }

    private string FixPath(string path)
    {
        return path.Replace('/', '&');
    }

    private void WriteMapInfoToFile()
    {
        string saveFolderPath = Path.Combine(Path.GetDirectoryName(SceneManager.GetActiveScene().path), "..", SceneChunkDef.SAVE_MAP_INFO_FOLDER_NAME);
        if(mGenerateDebugInfo)
        {
#region Write Debug File
            string debugFolderPath = Path.Combine(saveFolderPath, "Debug");
            string assetIDPath = Path.Combine(debugFolderPath, SceneChunkDef.DEBUG_ASSETID_FILE_NAME);
            string sceneInfoPath = Path.Combine(debugFolderPath, SceneChunkDef.DEBUG_SCENEINFO_FILE_NAME);
            if (!Directory.Exists(debugFolderPath))
            {
                Directory.CreateDirectory(debugFolderPath);
            }

            if (GameEngineFileUtil.FileExists(assetIDPath))
            {
                FileInfo saveFileInfo = new FileInfo(assetIDPath);
                if (saveFileInfo.IsReadOnly)
                {
                    File.SetAttributes(assetIDPath, FileAttributes.Normal);
                }
            }

            if (GameEngineFileUtil.FileExists(sceneInfoPath))
            {
                FileInfo saveFileInfo = new FileInfo(sceneInfoPath);
                if (saveFileInfo.IsReadOnly)
                {
                    File.SetAttributes(sceneInfoPath, FileAttributes.Normal);
                }
            }

            try
            {
                EditTextFile file = new EditTextFile(System.Text.Encoding.Unicode);
                if (file.OpenWrite(assetIDPath, OPEN_MODE.OPEN_WRITE_CREATE))
                {
                    for (int i = 0; i < mScenePartAssetPaths.Count; i++)
                    {
                        file.WriteLine(string.Format("{0} = {1}", i, mScenePartAssetPaths[i]));
                    }
                }
                else
                {
                    Debug.Log("Write Asset_ID Error! No Such File Path: " + assetIDPath);
                }

                file.Close();
            }
            catch (Exception)
            {
                EditorUtility.DisplayDialog("ERROR", "Save Asset_ID File Error!", "OK");
            }

            try
            {
                EditTextFile file = new EditTextFile(System.Text.Encoding.Unicode);
                if (file.OpenWrite(sceneInfoPath, OPEN_MODE.OPEN_WRITE_CREATE))
                {
                    file.WriteLine("SkyMaterial: " + FixPath(AssetDatabase.GetAssetPath(RenderSettings.skybox)));
                    file.WriteLine("LightmapsCount: " + LightmapSettings.lightmaps.Length);
                    for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
                    {
                        LightmapData lmd = LightmapSettings.lightmaps[i];
                        string prefixStr = string.Format("Lightmap[{0}]", i);
                        file.WriteLine(prefixStr + "_Color: " + FixPath(AssetDatabase.GetAssetPath(lmd.lightmapColor)));
                        file.WriteLine(prefixStr + "_Dir: " + FixPath(AssetDatabase.GetAssetPath(lmd.lightmapDir)));
                        file.WriteLine(prefixStr + "_shadowMask: " + FixPath(AssetDatabase.GetAssetPath(lmd.shadowMask)));
                    }

                    file.WriteLine("ChunkSize: " + mChunkSize);
                    Dictionary<int, EditorSceneChunk>.Enumerator enumerator = mSceneChunkDict.GetEnumerator();
                    file.WriteLine("ChunkCount: " + mSceneChunkDict.Keys.Count);
                    while (enumerator.MoveNext())
                    {
                        file.WriteLine("ChunkIndex: " + enumerator.Current.Key);
                        EditorSceneChunk sc = enumerator.Current.Value;
                        file.WriteLine("ScenePartsCount: " + sc.SceneParts.Count);
                        foreach (EditorScenePart sp in sc.SceneParts)
                        {
                            sp.SaveDebugData(file);
                        }
                    }
                }
                else
                {
                    Debug.Log("Write Scene_Info Error! No Such File Path: " + sceneInfoPath);
                }

                file.Close();
            }
            catch (Exception)
            {
                EditorUtility.DisplayDialog("ERROR", "Save Scene_Info File Error！", "OK");
            }

            return;
#endregion
        }

#region Write Binary File
        string bAssetIDPath = Path.Combine(saveFolderPath, SceneChunkDef.ASSETID_FILE_NAME);
        string bSceneInfoPath = Path.Combine(saveFolderPath, SceneChunkDef.SCENEINFO_FILE_NAME);
        if (GameEngineFileUtil.FileExists(bAssetIDPath))
        {
            FileInfo saveFileInfo = new FileInfo(bAssetIDPath);
            if (saveFileInfo.IsReadOnly)
            {
                File.SetAttributes(bAssetIDPath, FileAttributes.Normal);
            }
        }
        
        if (GameEngineFileUtil.FileExists(bSceneInfoPath))
        {
            FileInfo saveFileInfo = new FileInfo(bSceneInfoPath);
            if (saveFileInfo.IsReadOnly)
            {
                File.SetAttributes(bSceneInfoPath, FileAttributes.Normal);
            }
        }
        
        try
        {
            GameBinaryFile bf = new GameBinaryFile(System.Text.Encoding.Unicode);
            if (bf.OpenWrite(bAssetIDPath, OPEN_MODE.OPEN_WRITE_CREATE))
            {
                bf.Writer.Write(mScenePartAssetPaths.Count);
                for (int i = 0; i < mScenePartAssetPaths.Count; i++)
                {
                    EditSLBinary.SaveString(bf, mScenePartAssetPaths[i]);
                }
            }
            else
            {
                Debug.Log("Write Asset_ID Error! No Such File Path: " + bSceneInfoPath);
            }
        
            bf.Close();
        }
        catch (Exception)
        {
            EditorUtility.DisplayDialog("ERROR", "Save Asset_ID File Error!", "OK");
        }
        
        try
        {
            GameBinaryFile bf = new GameBinaryFile(System.Text.Encoding.Unicode);
            if (bf.OpenWrite(bSceneInfoPath, OPEN_MODE.OPEN_WRITE_CREATE))
            {
                EditSLBinary.SaveString(bf, FixPath(AssetDatabase.GetAssetPath(RenderSettings.skybox)));
                bf.Writer.Write(LightmapSettings.lightmaps.Length);
                for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
                {
                    LightmapData lmd = LightmapSettings.lightmaps[i];
                    EditSLBinary.SaveString(bf, FixPath(AssetDatabase.GetAssetPath(lmd.lightmapColor)));
                    EditSLBinary.SaveString(bf, FixPath(AssetDatabase.GetAssetPath(lmd.lightmapDir)));
                    EditSLBinary.SaveString(bf, FixPath(AssetDatabase.GetAssetPath(lmd.shadowMask)));
                }
                
                bf.Writer.Write(mChunkSize);
                Dictionary<int, EditorSceneChunk>.Enumerator enumerator = mSceneChunkDict.GetEnumerator();
                bf.Writer.Write(mSceneChunkDict.Keys.Count);
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.SaveData(bf);
                }
            }
            else
            {
                Debug.Log("Write Scene_Info Error! No Such File Path: " + bSceneInfoPath);
            }
        
            bf.Close();
        }
        catch (Exception)
        {
            EditorUtility.DisplayDialog("ERROR", "Save Scene_info File Error!", "OK");
        }
#endregion
    }
}