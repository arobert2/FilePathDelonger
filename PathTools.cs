using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delimon.Win32.IO;

namespace FilePathDelonger
{
    public class PathTools
    {
        /// <summary>
        /// Signals the start of a file tree scan.
        /// </summary>
        public Action<object, EventArgs> ScanStarted { get; set; }
        /// <summary>
        /// Signals the end of a file tree scan.
        /// </summary>
        public Action<object, EventArgs> ScanEnded { get; set; }
        /// <summary>
        /// Signals the start of a file path check.
        /// </summary>
        public Action<object, EventArgs> PathCheckStarted { get; set; }
        /// <summary>
        /// Signals the end of a file path check.
        /// </summary>
        public Action<object, EventArgs> PathCheckEnded { get; set; }
        /// <summary>
        /// Signals that files will start being copied or moved.
        /// </summary>
        public Action<object, EventArgs> FixStart { get; set; }
        /// <summary>
        /// Signals the files have finished being copied or moved.
        /// </summary>
        public Action<object, EventArgs> FixEnd { get; set; }
        /// <summary>
        /// Signals a folder and it's contents were moved.
        /// </summary>
        public Action<object, EventArgs> ContentMoved { get; set; }
        /// <summary>
        /// Signals a folder and it's contents were copied.
        /// </summary>
        public Action<object, EventArgs> ContentCopied { get; set; }

        private int _count = 0;
        private List<FileTree> _cutpoint = new List<FileTree>();

        /// <summary>
        /// Build a FileTree from the selected location.
        /// </summary>
        /// <param name="path">Path to start with</param>
        /// <returns>FileTree</returns>
        [Obsolete("BuildTree(string path) is deprecated, please use ParsePath(string path, string pathto)")]
        public FileTree BuildTree(string path)
        {
            FileTree DirectoryTree = new FileTree();                    //Create a tree to hold data
            string[] dir;                                               //Subfolder paths
            //Ignore access denied folders.
            try
            {
                dir = Directory.GetDirectories(path);                   //Get child folders
                DirectoryTree.Files.AddRange(Directory.GetFiles(path)); //Get child files             
            }
            catch (UnauthorizedAccessException uae)
            {
                return null;                                            //If not possible return null.
            }
            DirectoryTree.Path = path.CapPath();                                  //Set Path in FileTree object
            DirectoryTree.Directory = Path.GetFileName(path);           //Set directory name.
            //Scan child directories
            foreach (string d in dir)
            {
                FileTree subtree = BuildTree(d);                        //Create a new FileTree from the sub directory information.
                if (subtree != null)                                    //Ensure that it was able to.
                    DirectoryTree.Directories.Add(subtree);             //Add subtree to the main FileTree Directories property.
            }
            return DirectoryTree;                                       //Return new FileTree
        }
        /// <summary>
        /// Generate a TreeData object that holds the root FileTree, directory count, and location that need to be moved.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pathto"></param>
        /// <returns></returns>
        public TreeData ParsePath(string path, string pathto)
        {
            ScanStarted?.Invoke(this, new EventArgs());
            TreeData treedata = new TreeData();
            treedata.FileTree = BuildTree(path, pathto, "");
            treedata.Count = _count;
            treedata.PathBreakPoints = _cutpoint.ToArray();
            ScanEnded?.Invoke(this, new EventArgs());
            return treedata;
        }
        /// <summary>
        /// Build a FileTree from the selected location.
        /// </summary>
        /// <param name="path">Path to start with</param>
        /// <returns>FileTree</returns>
        private FileTree BuildTree(string path, string pathto, string lastcut)
        {
            int charlimit = 240 - pathto.Length;                        //maximum path size.
            string lc = lastcut;                                        //last place that was over the limit
            _count++;

            FileTree DirectoryTree = new FileTree();                    //Create a tree to hold data
            string[] dir = GetFolders(path);                            //Subfolder paths
            DirectoryTree.Files.AddRange(Directory.GetFiles(path));     //Get child files             
            DirectoryTree.Path = path.CapPath();                        //Set Path in FileTree object
            DirectoryTree.Directory = Path.GetFileName(path);           //Set directory name.

            if(CheckPathLength(path, pathto, lastcut))                  //Check to see if it's over the limit and adjust
            {
                lc = path;                                              //Set lastcut to current path.
                _cutpoint.Add(DirectoryTree);                           //Add path to cut list.
            }

            foreach (string d in dir)                                   //Scan child directories
            {
                FileTree subtree = BuildTree(d, pathto, lc);            //Create a new FileTree from the sub directory information.
                if (subtree != null)                                    //Ensure that it was able to.
                    DirectoryTree.Directories.Add(subtree);             //Add subtree to the main FileTree Directories property.
            }
            return DirectoryTree;                                       //Return new FileTree
        }
        /// <summary>
        /// Move branches of file tree to destination.
        /// </summary>
        /// <param name="td"></param>
        /// <param name="dest"></param>
        public void MoveFiles(TreeData td, string dest)
        {
            FixStart?.Invoke(this, new EventArgs());
            for (int i = td.PathBreakPoints.Length; i > 0; i-- )                            //Count backwards so you don't move folders before they are scheduled.
            {
                ContentMoved?.Invoke(this, new EventArgs());
                Move(td.PathBreakPoints[i].Path, dest);                                          //call the move function
                //log to destination\FolderLog.txt
                (Path.GetFileName(td.PathBreakPoints[i].Path) + " has been moved from " + td.PathBreakPoints[i] + " to the backup destiontion " + dest).LogMove(dest.CapPath() + Path.GetFileName(td.PathBreakPoints[i].Path)); 
            }
            FixStart?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Sets AtLimit bool in FileTree object
        /// </summary>
        /// <param name="ft">FileTree</param>
        /// <param name="path">Path to check</param>
        [Obsolete("MarkCuts(FileTree ft, string pathto, string lastcut) is deprecated. This function has been rolled into PrasePath(string path, string pathto)")]
        public FileTree MarkCuts(FileTree ft, string pathto, string lastcut)
        {
            int charlimit = 240 - pathto.Length;                    //maximum path size.
            string lc = lastcut;                                    //Last cut for determining where to cut nust (path length - lastcutlength - destinationlength = maximum path size)
            FileTree newTree = new FileTree()                       //Create a new tree that is identical to the previous except no child directores.
            {
                Path = ft.Path,
                Directory = ft.Directory,
                Files = ft.Files
            };
            //If path is greater than charlimit
            if (ft.Path.Length - lc.Length > charlimit)
            {
                newTree.AtLimit = true;                             //Mark the tree as a cut.
                lc = ft.Path;                                       //update last cut with this path
            }
            //Check sub directories for cut mark.
            foreach (FileTree f in ft.Directories)
                newTree.Directories.Add(MarkCuts(f, pathto, lc));   //Run method against subtrees, add result to newTree sub directory (FileTree.Directories property)            
            return newTree;                                         //return updated tree.
        }
        /// <summary>
        /// Move files and folders that exceed limit
        /// </summary>
        /// <param name="ft">Root FileTree</param>
        /// <param name="path">Path to move files to.</param>
        [Obsolete("MoveFiles(FileTree ft, string path) is deprecated, use MoveFiles(TreeData td, string dest")]
        public void MoveFiles(FileTree ft, string path)
        {
            foreach (FileTree f in ft.Directories)      //Go to end of branch before continuing.
                MoveFiles(f, path);                     //restart loop

            foreach (string f in ft.Files)              //look through files
                if (f.Length - path.Length >= 240)      //Check to see if their path exceeds the limit
                    Move(f, path);                      //Move file

            if (ft.AtLimit)                             //If marked for cut
                Move(ft.Path, path);                    //Move folder 
        }
        /// <summary>
        /// Copy the files from the File Tree that are over the limit to a specific location
        /// </summary>
        /// <param name="td">TreeData containing the FileTree, count, and breakpoints.</param>
        /// <param name="outpath">Path to send files.</param>
        public void CopyFiles(TreeData td, string outpath)
        {
            FixStart?.Invoke(this, new EventArgs());
            CopySection(td.FileTree, outpath);
            foreach (FileTree path in td.PathBreakPoints)
                CopySection(path, outpath);
            FixEnd?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Copy the data between 2 points marked over the limit.
        /// </summary>
        /// <param name="ft">FileTree to start with</param>
        /// <param name="outpath">output path</param>
        private void CopySection(FileTree ft, string outpath)
        {
            ContentCopied?.Invoke(this, new EventArgs());
            string op = outpath.CapPath() + ft.Directory;           //Where these files will go

            if (!Directory.Exists(op))                              //If directory at outpath doesn't exist
                Directory.CreateDirectory(op);                      //Create directory
            else
                Directory.CreateDirectory(op.AppendTimeStamp());    //Otherwise create directory with timestamp.

            foreach (string file in ft.Files)                       //Copy every file but do not overwrite.
                File.Copy(file, outpath, false);

            foreach (FileTree subdir in ft.Directories)             //Continue this until the next folder is marked to cut as well.
                if (!subdir.AtLimit)
                    CopySection(subdir, op);
        }
        /// <summary>
        /// Move a file or folder from one location to another without worrying about overwrite.
        /// </summary>
        /// <param name="inputpath">Object you wish to move</param>
        /// <param name="outputpath">Where you want to move it.</param>
        public void Move(string inputpath, string outputpath)
        {
            string inp = inputpath.EnforceNoMatch(outputpath);                                          //adds a timestamp if the file or folder already exists at the desired move path.
            string cpobj = Path.GetFileName(inp);                                                       //FileSystem object name without path.
            string movefrom = Path.GetDirectoryName(inputpath);                                         //Path where FileSystem object comes from

            Directory.Move(inputpath, outputpath.CapPath() + cpobj);                                    //Move the file/folder to the specified location
            (cpobj + " was moved from " + movefrom).LogMove(outputpath.CapPath() + "FolderLog.txt");    //Log the move 
        }
        /// <summary>
        /// Check the path length
        /// </summary>
        /// <param name="ipath">input path</param>
        /// <param name="opath">output path</param>
        /// <param name="lastpathlength">last length a cut was taken.</param>
        /// <returns>Is it over length</returns>
        private bool CheckPathLength (string ipath, string opath, string lastpathlength)
        {
            return (opath.Length - ipath.Length - lastpathlength.Length) > 240 ? true : false;
        }
        /// <summary>
        /// Get files safely
        /// </summary>
        /// <param name="path">Path to get files from</param>
        /// <returns>files</returns>
        private string[] GetFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path); //Get child files  
            }
            catch (UnauthorizedAccessException uae)
            {
                return null;                    //If not possible return null.
            }
        }
        /// <summary>
        /// Get directories safely.
        /// </summary>
        /// <param name="path">path to directory</param>
        /// <returns>directories</returns>
        private string[] GetFolders(string path)
        {
            try
            {
                return Directory.GetDirectories(path);                   //Get child folders         
            }
            catch (UnauthorizedAccessException uae)
            {
                return null;                                            //If not possible return null.
            }
        }
    }

    public class TreeData
    {
        public FileTree FileTree { get; set; }
        public int Count { get; set; }
        public FileTree[] PathBreakPoints { get; set; }
    }
}
