using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace FilenameDelonger
{
    public class PathTools
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
        public FileTree MarkCuts(FileTree ft, string path, string lastcut, int charlimit)
        {
            string lc = lastcut;
            FileTree newTree = new FileTree() { Path = ft.Path, Directory = ft.Directory, Files = ft.Files };   //Create a new tree that is identical to the previous except no child directores.

            //If path is greater than charlimit
            if(path.Substring(lc.Length - 1).Length > charlimit)
            {
                //Mark 
                newTree.AtLimit = true;
                lc = lastcut;
            }

            foreach(FileTree f in ft.Directories)
            {
                FileTree subtree = MarkCuts(f, f.Path, lc, charlimit);
                if(subtree != null)
                    newTree.Directories.Add(subtree);
            }

            return newTree;
        }
        /// <summary>
        /// Move files and folders that exceed limit
        /// </summary>
        /// <param name="ft">Root FileTree</param>
        /// <param name="path">Path to move files to.</param>
        public void MoveFiles(FileTree ft, string path = @"C:\")
        {
            foreach (FileTree f in ft.Directories)
                MoveFiles(f, path);

            foreach(string f in ft.Files)
                if(f.Length > 150)
                {
                    File.Move(f, path);
                    LogMove(path + @"\FileLog.txt", Path.GetFileName(f) + " was moved from " + ft.Path);
                }


            if(ft.AtLimit)
            {
                Directory.Move(ft.Path, path);
                LogMove(path + @"\" + ft.Directory + "FolderLog.txt", ft.Directory + " was moved from " + ft.Path);
            }
        }

        private void LogMove(string path, string text)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(text);
            }
                
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
