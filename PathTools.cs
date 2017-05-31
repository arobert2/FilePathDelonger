﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace FilePathDelonger
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
        public static FileTree MarkCuts(FileTree ft, string pathto, string lastcut)
        {
            int charlimit = 260 - pathto.Length;    //maximum path size.
            string lc = lastcut;    //Last cut for determining where to cut nust (path length - lastcutlength - destinationlength = maximum path size)
            FileTree newTree = new FileTree() { Path = ft.Path, Directory = ft.Directory, Files = ft.Files };   //Create a new tree that is identical to the previous except no child directores.

            //If path is greater than charlimit
            if(ft.Path.Substring(lc.Length - 1).Length > charlimit)
            {
                //Mark the tree as a cut.
                newTree.AtLimit = true;
                //update last cut with this path
                lc = ft.Path;
            }

            //Check sub directories for cut mark.
            foreach(FileTree f in ft.Directories)
            {
                //Run method again with subtree of provided tree.
                FileTree subtree = MarkCuts(f, pathto, lc);
                //update newly created tree subdirectories with new cutmarked tree.
                newTree.Directories.Add(subtree);
            }
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
            // Go to end of tree.
            foreach (FileTree f in ft.Directories)
                MoveFiles(f, path);

            // check filename length
            foreach(string f in ft.Files)
                if(f.Length >= 260)
                {
                    File.Move(f, path);
                    LogMove(path + @"\FileLog.txt", Path.GetFileName(f) + " was moved from " + ft.Path);
                }

            // Check folder name length
            if(ft.AtLimit)
            {
                Directory.Move(ft.Path, path);
                LogMove(path + @"\" + ft.Directory + "FolderLog.txt", ft.Directory + " was moved from " + ft.Path);
            }
        }

        private static void LogMove(string path, string text)
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
