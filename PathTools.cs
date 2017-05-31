using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace FilePathDelonger
{
    public static class PathTools
    { 
        /// <summary>
        /// Build a FileTree from the selected location.
        /// </summary>
        /// <param name="path">Path to start with</param>
        /// <returns>FileTree</returns>
        public static FileTree BuildTree(string path)
        {
            FileTree DirectoryTree = new FileTree();                //root tree
            string[] dir;
            try
            {
                dir = Directory.GetDirectories(path);          //child directorier
                DirectoryTree.Files.AddRange(Directory.GetFiles(path)); //child files             
            }
            catch(UnauthorizedAccessException uae)
            {
                return null;
            }

            DirectoryTree.Path = path;
            DirectoryTree.Directory = path.Substring(path.LastIndexOf(@"\"));    //Path of directory

            //Scan child directories
            foreach (string d in dir)
            {
                FileTree subtree = BuildTree(d);
                if(subtree != null)
                    DirectoryTree.Directories.Add(BuildTree(d));
            }

            return DirectoryTree;
        }
        /// <summary>
        /// Chop the root path from the file
        /// </summary>
        /// <param name="files">Files to chop</param>
        /// <returns>filename</returns>
        private static string[] ChopPath(string[] files)
        {
            List<string> chops = new List<string>();
            foreach (string s in files)
                chops.Add(Path.GetFileName(s));
            return chops.ToArray();
        }
        /// <summary>
        /// Sets AtLimit bool in FileTree object
        /// </summary>
        /// <param name="ft">FileTree</param>
        /// <param name="path">Path to check</param>
        public static FileTree MarkCuts(FileTree ft, string pathto, string lastcut)
        {
            int charlimit = 260 - pathto.Length;    //maximum path size.
            string lc = lastcut;                    //Last cut for determining where to cut nust (path length - lastcutlength - destinationlength = maximum path size)
            FileTree newTree = new FileTree()       //Create a new tree that is identical to the previous except no child directores.
            {
                Path = ft.Path,
                Directory = ft.Directory,
                Files = ft.Files
            };   
            //If path is greater than charlimit
            if(ft.Path.Substring(lc.Length).Length > charlimit)
            {
                newTree.AtLimit = true;         //Mark the tree as a cut.
                lc = ft.Path;                   //update last cut with this path
            }
            //Check sub directories for cut mark.
            foreach(FileTree f in ft.Directories)
                newTree.Directories.Add(MarkCuts(f, pathto, lc));   //Run method against subtrees, add result to newTree sub directory (FileTree.Directories property)
            //return updated tree.
            return newTree;
        }
        /// <summary>
        /// Move files and folders that exceed limit
        /// </summary>
        /// <param name="ft">Root FileTree</param>
        /// <param name="path">Path to move files to.</param>
        public static void MoveFiles(FileTree ft, string path = @"C:\")
        {
            // Go to end of branch before continuing.
            foreach (FileTree f in ft.Directories) 
                MoveFiles(f, path);

            foreach (string f in ft.Files)              //look through files
                if (f.Length - path.Length >= 255)      //Check to see if their path exceeds the limit
                    Move(f, path);                      //Move file

            if (ft.AtLimit)             //If marked for cut
                Move(ft.Path, path);    //Move folder
                
        }
        /// <summary>
        /// Log information to a location
        /// </summary>
        /// <param name="path">Location of log file</param>
        /// <param name="text">text to log</param>
        private static void LogMove(string path, string text)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(text);
            }
                
        }
        /// <summary>
        /// Move a file or folder from one location to another without worrying about overwrite.
        /// </summary>
        /// <param name="inputpath">Object you wish to move</param>
        /// <param name="outputpath">Where you want to move it.</param>
        public static void Move(string inputpath, string outputpath)
        {
            string inp = inputpath.EnforceNoMatch(outputpath);          //adds a timestamp if the file or folder already exists at the desired move path.
            string cpobj = Path.GetFileName(inp);                       //FileSystem object name without path.
            string movefrom = Path.GetDirectoryName(inputpath);         //Path where FileSystem object comes from

            Directory.Move(inputpath, outputpath + cpobj);                                              //Move the file/folder to the specified location
            LogMove(outputpath.CapPath() + "FolderLog.txt", cpobj + " was moved from " + movefrom);    //Log the move 
        }
        /// <summary>
        /// Adds a timestamp to a string if that FileSystem object already exists at the selected path.
        /// </summary>
        /// <param name="path">Path to object</param>
        /// <param name="outpath">Where you want to make sure it doesn't exist</param>
        /// <returns>string modified to not match</returns>
        private static string EnforceNoMatch(this string path, string outpath)
        {
            FileAttributes fa = File.GetAttributes(path);                               //get file attributes
            string checkfsobjname = Path.GetFileNameWithoutExtension(path);             //get File System object name
            string checkfsext = Path.GetExtension(path);                                //get File System object extension
            string checkfspath = Path.GetDirectoryName(path);                           //get File System object directory path  
            bool fileexists = File.Exists(outpath.CapPath() + checkfsobjname);          //check to see if a file exists
            bool folderexists = Directory.Exists(outpath.CapPath() + checkfsobjname);   //check to see if folder exists

            if (fileexists || folderexists)    
                return checkfspath + checkfsobjname.AppendTimeStamp() + checkfsext;      //return timestamped path
            return path;                                                                 //return original path
        }
        /// <summary>
        /// Appends a Time Stamp on a string if needed.
        /// </summary>
        /// <param name="st">String to change</param>
        /// <returns>Modified string</returns>
        private static string AppendTimeStamp(this string st)
        {
            return st + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
        /// <summary>
        /// Adds a \ on the end of a string if needed.
        /// </summary>
        /// <param name="st">String to change</param>
        /// <returns>Modified string</returns>
        private static string CapPath(this string st)
        {
            if (st[st.Length - 1] == Path.DirectorySeparatorChar)
                return st;
            return st + Path.DirectorySeparatorChar;
        }
    }

    public class FileTree
    {
        public string Path { get; set; }
        public string Directory { get; set; }
        public List<FileTree> Directories { get; set; } = new List<FileTree>();
        public List<string> Files { get; set; } = new List<string>();
        public bool AtLimit { get; set; } = false;
    }
}
