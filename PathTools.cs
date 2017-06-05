using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delimon.Win32.IO;
using System.Collections;
using System.Threading;

namespace FilePathDelonger
{
    public static class ExtensionMethods
    { 
        /// <summary>
        /// Adds a timestamp to a string if that FileSystem object already exists at the selected path.
        /// </summary>
        /// <param name="path">Path to object</param>
        /// <param name="outpath">Where you want to make sure it doesn't exist</param>
        /// <returns>string modified to not match</returns>
        public static string EnforceNoMatch(this string path, string outpath)
        {
            string checkfsobjname = Path.GetFileName(path);                             //get File System object name
            string checkfsobjnamenoext = "";                                            //System object name without extension
            string checkfsext = "";                                                     //System object extension
            string checkfspath = Path.GetDirectoryName(path);                           //get File System object directory path  
            bool fileexists = File.Exists(outpath.CapPath() + checkfsobjname);          //check to see if a file exists
            bool folderexists = Directory.Exists(outpath.CapPath() + checkfsobjname);   //check to see if folder exists

            //If the file doesn't exist just return the original file name.
            if (!fileexists || !folderexists)
                return path;                                                            //return original path

            //Check to see if this file has an extension.
            if (checkfsobjname.Contains("."))
            {
                checkfsobjnamenoext = Path.GetFileNameWithoutExtension(checkfsobjname); //Get filename without an extension
                checkfsext = Path.GetExtension(checkfsobjname);                         //Get filename extension
            }
            else
                checkfsobjnamenoext = checkfsobjname;                                   //If no extension, just make noextension variable = name.

            return checkfspath.CapPath() + checkfsobjnamenoext.AppendTimeStamp() + checkfsext;    //return timestamped path                                                                
        }
        /// <summary>
        /// Appends a Time Stamp on a string if needed.
        /// </summary>
        /// <param name="st">String to change</param>
        /// <returns>Modified string</returns>
        public static string AppendTimeStamp(this string st)
        {
            return st + DateTime.Now.ToString("yyyyMMddHHmmssfff");     //Add a timestamp to the folder
        }
        /// <summary>
        /// Adds a \ on the end of a string if needed.
        /// </summary>
        /// <param name="st">String to change</param>
        /// <returns>Modified string</returns>
        public static string CapPath(this string st)
        {
            if (st[st.Length - 1] == System.IO.Path.DirectorySeparatorChar) //If the last character is a \
                return st;                                                  //return the original string
            return st + System.IO.Path.DirectorySeparatorChar;              //return a string with the \ added to the end.
        }
        /// <summary>
        /// Get the file name from a string[] list of file paths.
        /// </summary>
        /// <param name="files">Files to chop</param>
        /// <returns>filename</returns>
        public static string[] ChopPath(this string[] files)
        {
            List<string> chops = new List<string>();
            foreach (string s in files)
                chops.Add(Path.GetFileName(s));
            return chops.ToArray();
        }
        /// <summary>
        /// Log information to a location
        /// </summary>
        /// <param name="path">Location of log file</param>
        /// <param name="text">text to log</param>
        public static void LogMove(this string path, string text)
        {
            if (!File.Exists(path))     //If the log file doesn't exist
                File.Create(path);      //Create a new log file

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path, true))  //create a new StreamWriter
            {
                string write = "[".AppendTimeStamp() + "] " + text;                     //Add time stamp to log entry.
                sw.WriteLine(write);                                                    //Write log
                sw.Close();                                                             //Close file
            }
        }
    }

    public class FileTree
    {
        /// <summary>
        /// Full path to directory
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Directory name
        /// </summary>
        public string Directory { get; set; }
        /// <summary>
        /// Sub directories
        /// </summary>
        public List<FileTree> Directories { get; set; } = new List<FileTree>();
        /// <summary>
        /// Files in directory
        /// </summary>
        public List<string> Files { get; set; } = new List<string>();
        /// <summary>
        /// If this path is over the 260 limit.
        /// </summary>
        public bool AtLimit { get; set; } = false;
    }
}
