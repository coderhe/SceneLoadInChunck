/*
 * PURPOSE : tools for path/string
 */

using System;
using System.IO;

namespace GameEngine
{
	public class GameEngineFileUtil
	{
        public const string msNameSpliter = "&";
        public const string msBackSlash = "/";
        
        public static string GetFileNameByPathName(string pathname)
        {
            if (String.IsNullOrEmpty(pathname))
                return string.Empty;
            
            string fullpath = pathname.Replace(msNameSpliter, msBackSlash);
            int index = fullpath.LastIndexOf(msBackSlash);
            if (-1 == index)
                return fullpath;

            if (index + 1 > fullpath.Length)
                return fullpath;

            return fullpath.Substring(index + 1);
        }
        
        //file only setting normal
        public static bool SetFileAttributes(string FileName)
        {
            FileInfo fileinfo = new FileInfo(FileName);
            if (fileinfo.Exists)
            {
                if ((fileinfo.Attributes & FileAttributes.ReadOnly) > 0)
                {
                    fileinfo.Attributes = FileAttributes.Normal;
                    return true;
                }
            }

            return false;
        }

        public static bool FileExists(string path)
	    {
	        return File.Exists(path);
	    }
    }
}