/*
 * PURPOSE : Tools for save/load binary data
 */
using UnityEngine;

namespace GameEngine
{
    public class EditSLBinary
    {
        public static void SaveColor(GameBinaryFile fs, Color col)
        {
            if (fs == null || fs.Writer == null)
                return;

            fs.Writer.Write(col.r);
            fs.Writer.Write(col.g);
            fs.Writer.Write(col.b);
            fs.Writer.Write(col.a);
        }

        public static Color LoadColor(GameBinaryFile fs)
        {
            Color col = Color.white;
            if (fs == null || fs.Reader == null)
                return col;

            col.r = fs.Reader.ReadSingle();
            col.g = fs.Reader.ReadSingle();
            col.b = fs.Reader.ReadSingle();
            col.a = fs.Reader.ReadSingle();

            return col;
        }

        public static void SaveVector3(GameBinaryFile fs, Vector3 vec)
        {
            if (fs == null || fs.Writer == null)
                return;

            fs.Writer.Write(vec.x);
            fs.Writer.Write(vec.y);
            fs.Writer.Write(vec.z);
        }

        public static Vector3 LoadVector3(GameBinaryFile fs)
        {
            Vector3 vec = Vector3.zero;
            if (fs == null || fs.Reader == null)
                return vec;

            vec.x = fs.Reader.ReadSingle();
            vec.y = fs.Reader.ReadSingle();
            vec.z = fs.Reader.ReadSingle();

            return vec;
        }

        public static void SaveString(GameBinaryFile fs, string str)
        {
            if (fs == null || fs.Writer == null)
                return;
            
            if(string.IsNullOrEmpty(str))
                str = string.Empty;            

            byte[] buf = fs.CurEncoding.GetBytes(str);
            fs.Writer.Write(buf.Length);
            if (buf.Length > 0)
            {
                fs.Writer.Write(buf);
            }
        }

        public static string LoadString(GameBinaryFile fs)
        {
            string str = string.Empty;
            if (fs == null || fs.Reader == null)
                return str;

            int len = fs.Reader.ReadInt32();
            if (len > 0)
            {
                byte[] buf = fs.Reader.ReadBytes(len);
                str = fs.CurEncoding.GetString(buf);
            }

            return str;
        }

        public static void SaveBool(GameBinaryFile fs, bool val)
        {
            if (fs == null || fs.Writer == null)
                return;

            fs.Writer.Write(val ? 1 : 0);
        }

        public static bool LoadBool(GameBinaryFile fs)
        {
            if (fs == null || fs.Reader == null)
                return false;

            return (fs.Reader.ReadInt32() != 0);
        }

        public static void SaveBounds(GameBinaryFile fs, Bounds bounds)
        {
            if (fs == null || fs.Writer == null)
                return;

            fs.Writer.Write(bounds.center.x);
            fs.Writer.Write(bounds.center.y);
            fs.Writer.Write(bounds.center.z);
            fs.Writer.Write(bounds.extents.x);
            fs.Writer.Write(bounds.extents.y);
            fs.Writer.Write(bounds.extents.z);
        }

        public static Bounds LoadBounds(GameBinaryFile fs)
        {
            Bounds bounds = new Bounds();
            if (fs == null || fs.Reader == null)
                return bounds;

            Vector3 center = Vector3.zero;
            Vector3 extents = Vector3.zero;

            center.x = fs.Reader.ReadSingle();
            center.y = fs.Reader.ReadSingle();
            center.z = fs.Reader.ReadSingle();
            extents.x = fs.Reader.ReadSingle();
            extents.y = fs.Reader.ReadSingle();
            extents.z = fs.Reader.ReadSingle();
            bounds.center = center;
            bounds.extents = extents;

            return bounds;
        }

        public static void SaveQuaternion(GameBinaryFile fs, Quaternion rot)
        {
            if (fs == null || fs.Writer == null)
                return;

            fs.Writer.Write(rot.x);
            fs.Writer.Write(rot.y);
            fs.Writer.Write(rot.z);
            fs.Writer.Write(rot.w);
        }

        public static Quaternion LoadQuaternion(GameBinaryFile fs)
        {
            Quaternion rot = Quaternion.identity;
            if (fs == null || fs.Reader == null)
                return rot;

            rot.x = fs.Reader.ReadSingle();
            rot.y = fs.Reader.ReadSingle();
            rot.z = fs.Reader.ReadSingle();
            rot.w = fs.Reader.ReadSingle();

            return rot;
        }
    }
}