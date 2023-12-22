using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameEngine
{
    public enum FILE_TYPE
    {
        BINARY = 0,
        TEXT,        
    }

    public enum OPEN_MODE
    {
        OPEN_READ,
        OPEN_WRITE,
        OPEN_WRITE_CREATE,
        OPEN_APPEND,
    }

	public abstract class GameFile   
	{
        public FILE_TYPE FileType { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public Encoding CurEncoding { get; set; }
        protected bool mOpened = false;
        protected Stream mStream = null;        

        public GameFile(FILE_TYPE ftype)
        {
            CurEncoding = Encoding.Default;
        }

        public GameFile(FILE_TYPE ftype, Encoding encode)
        {
            CurEncoding = encode;
        }

        public virtual bool Open(string path, OPEN_MODE mode)
        {
            if (mOpened)
                Close();

            FileMode filemode = GetFileMode(mode);
            FileAccess access = GetFileAccess(mode, filemode);
            if (access == FileAccess.Read)
            {
                if (!GameEngineFileUtil.FileExists(path))
                    return false;
            }
            else
            {
                if (GameEngineFileUtil.FileExists(path))
                    GameEngineFileUtil.SetFileAttributes(path);
            }

            try
            {
                mStream = new FileStream(path, filemode, access, FileShare.ReadWrite);
                if (mStream == null)
                    return false;

                FilePath = path;
                FileName = GameEngineFileUtil.GetFileNameByPathName(path);
                mOpened = true;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }

            return true;
        }

	    protected static FileAccess GetFileAccess(OPEN_MODE mode, FileMode filemode)
	    {
	        FileAccess access = (mode == OPEN_MODE.OPEN_READ) ? FileAccess.Read : FileAccess.ReadWrite;
	        if (filemode == FileMode.Append)
                access = FileAccess.Write;

	        return access;
	    }

	    protected static FileMode GetFileMode(OPEN_MODE mode)
	    {
	        FileMode filemode = FileMode.Open;
	        if (mode == OPEN_MODE.OPEN_WRITE)
	            filemode = FileMode.OpenOrCreate;	        
	        else if (mode == OPEN_MODE.OPEN_WRITE_CREATE)
	            filemode = FileMode.Create;	        
	        else if (mode == OPEN_MODE.OPEN_APPEND)
	            filemode = FileMode.Append;

	        return filemode;
        }

        public virtual bool OpenMem(byte[] buf, OPEN_MODE mode)
        {
            if (mOpened)
                Close();

            if(null == buf)
                return false;
            
            mStream = new MemoryStream(buf, mode == OPEN_MODE.OPEN_READ ? false : true);
            if (mStream == null)
                return false;

            mOpened = true;
            return true;
        }

        public virtual bool OpenRead(string path)
        {
            return Open(path, OPEN_MODE.OPEN_READ);
        }

        public virtual bool OpenWrite(string path,OPEN_MODE mode = OPEN_MODE.OPEN_WRITE)
        {
            return Open(path, mode);
        }

        public virtual bool OpenRead(byte[] mem)
        {
            return OpenMem(mem, OPEN_MODE.OPEN_READ);
        }

        public virtual bool OpenWriter(byte[] mem)
        {
            return OpenMem(mem, OPEN_MODE.OPEN_WRITE);
        }

        public virtual void Close()
        {
            if (!mOpened)
                return;

            if (mStream != null)
	        {
		        mStream.Close();
                mStream = null;
	        }

            mOpened = false;
            FileName = null;
            FilePath = null;
        }
	}

    public class GameBinaryFile : GameFile
    {
        public BinaryReader Reader { get; set; }
        public BinaryWriter Writer { get; set; }

        public GameBinaryFile() 
            : base(FILE_TYPE.BINARY)
        {
        }

        public GameBinaryFile(Encoding encode) 
            : base(FILE_TYPE.BINARY, encode)
        {
        }

        public override bool Open(string path, OPEN_MODE mode)
        {
            if (!base.Open(path, mode))
                return false;

            if (mode == OPEN_MODE.OPEN_READ)
                Reader = new BinaryReader(mStream, CurEncoding);
            else
                Writer = new BinaryWriter(mStream, CurEncoding);
            
            return true;
        }

        public override bool OpenMem(byte[] buf, OPEN_MODE mode)
        {
            if (!base.OpenMem(buf, mode))
                return false;

            if (mode == OPEN_MODE.OPEN_READ)
                Reader = new BinaryReader(mStream, CurEncoding);
            else
                Writer = new BinaryWriter(mStream, CurEncoding);

            return true;
        }

        public override void Close()
        {
            if (Writer != null)
                Writer.Close();

            if (Reader != null)
                Reader.Close();
            
            base.Close();
        }

        public bool ReadBool()
        {
            if (null == Reader)
                return false;
            
            return EditSLBinary.LoadBool(this);
        }

        public int ReadInt32()
        {
            if(null == Reader)
                return 0;            

            return Reader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            if (null == Reader)
                return 0;            

            return Reader.ReadUInt32();
        }

        public float ReadSingle()
        {
            if (null == Reader)
                return 0;            

            return Reader.ReadSingle();
        }

        public string ReadString()
        {
            if (null == Reader)
                return "";            

            return EditSLBinary.LoadString(this);
        }

        public Color ReadColor()
        {
            if (null == Reader)
                return Color.black;

            return EditSLBinary.LoadColor(this);
        }

        public Vector3 ReadVector3()
        {
            if (null == Reader)
                return Vector3.zero;

            return EditSLBinary.LoadVector3(this);
        }

        public Quaternion ReadQuaternion()
        {
            if(null == Reader)
                return Quaternion.identity;

            return EditSLBinary.LoadQuaternion(this);
        }

        public long GetLength()
        {
            if (null == Reader)
                return 0;

            return Reader.BaseStream.Length;
        }

        public long GetPosition()
        {
            if (null == Reader)
                return 0;

            return Reader.BaseStream.Position;
        }

        public long GetRemainSize()
        {
            return GetLength() - GetPosition();
        }
    }

    public class GameTextFile : GameFile
    {
        public StreamReader Reader { get; set; }
        public StreamWriter Writer { get; set; }

        public GameTextFile() 
            : base(FILE_TYPE.TEXT)
        {
        }

        public GameTextFile(Encoding encode)
            : base(FILE_TYPE.TEXT, encode)
        {
        }

        public override bool Open(string path, OPEN_MODE mode)
        {
            if (!base.Open(path, mode))
                return false;

            if (mode == OPEN_MODE.OPEN_READ)
                Reader = new StreamReader(mStream, CurEncoding);
            else
                Writer = new StreamWriter(mStream, CurEncoding);

            return true;
        }

        public override bool OpenMem(byte[] buf, OPEN_MODE mode)
        {
            if (!base.OpenMem(buf, mode))
                return false;

            if (mode == OPEN_MODE.OPEN_READ)
                Reader = new StreamReader(mStream, CurEncoding);
            else
                Writer = new StreamWriter(mStream, CurEncoding);
            
            return true;
        }

        public override void Close()
        {
            if (Writer != null)
                Writer.Close();            

            if (Reader != null)
                Reader.Close();
            
            base.Close();
        }

        public void WriteLine(string format)
        {
            Writer.WriteLine(format);
        }

        public string ReadLine(string format = null)
        {
            string result = "";
            try
            {
                result = Reader.ReadLine();
            }
            catch (Exception e)
            {
                result = "";
                Debug.LogException(e);
            }

            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(format))
                result = result.Replace(format, "");
            
            return result;
        }

        public bool SaveColor(Color color)
        {
            string str = "";
            str += color.r+"-";
            str += color.g + "-";
            str += color.b + "-";
            str += color.a;
            WriteLine(str);
            return true;
        }

        public Color ReadColor()
        {
            if (null == Reader)
                return Color.black;
            
            Color col = Color.white;
            string line = ReadLine();
            string[] strs = line.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if(null == strs || strs.Length < 4)
                return col;            

            col.r = float.Parse(strs[0]);
            col.g = float.Parse(strs[1]);
            col.b = float.Parse(strs[2]);
            col.a = float.Parse(strs[3]);

            return col;
        }

        public bool OpenReadFromResources(string filepath)
        {
            TextAsset asset = Resources.Load(filepath) as TextAsset;
            if (asset == null)
                return false;
            
            mStream = new MemoryStream(asset.bytes);
            if (mStream == null)
                return false;

            Reader = new StreamReader(mStream, CurEncoding);
            FilePath = filepath;
            FileName = "TODO: FileName";
            mOpened = true;
            return true;
        }
    }
}