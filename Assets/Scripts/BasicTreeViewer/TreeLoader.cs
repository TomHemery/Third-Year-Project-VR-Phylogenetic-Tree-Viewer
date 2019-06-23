using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TreeLoader{

    static public Tree CurrentTree { get; private set; }
    static readonly string NEWICK_SYSTEM_DIRECTORY_PATH = "Assets/Resources/NewickTrees";
    public static readonly string NEWICK_RELATIVE_DIRECTORY_PATH = "NewickTrees";

    private static int numberOfTrees = -1;
    public static TextAsset[] TreeFiles { get; private set; }
    static public string CurrentTreeFileName { get; private set; }
    public static int CurrentTreeIndex { get; private set; } = 0;

    public static void _init () {
        if (TreeFiles == null) InitTreeFiles();
        CurrentTreeFileName = "RAxML_bestTree.clusterIa_16s";
        for (int i = 0; i < TreeFiles.Length; i++) {
            if (TreeFiles[i].name == CurrentTreeFileName) {
                CurrentTreeIndex = i;
                break;
            }
        }

        LoadTree(CurrentTreeIndex);        
        
    }

    private static void InitTreeFiles() {
        DirectoryInfo d = new DirectoryInfo(NEWICK_SYSTEM_DIRECTORY_PATH);
        List<TextAsset> allFiles = new List<TextAsset>(Resources.LoadAll<TextAsset>(NEWICK_RELATIVE_DIRECTORY_PATH));
        TextAsset file;
        for (int i = allFiles.Count - 1; i > 0; i--) {
            file = allFiles[i];
            if (file.name.Contains("heightmap"))
            {
                allFiles.Remove(file);
            }
        }
        TreeFiles = allFiles.ToArray();
    }

    public static int GetNumberOfTrees() {
        if (numberOfTrees >= 0) {
            return numberOfTrees;
        }
        else {
            if (TreeFiles == null) InitTreeFiles();
            numberOfTrees = TreeFiles.Length;
            return numberOfTrees;
        }
    }

    public static void LoadTree(int treeIndex) {
        if (treeIndex >= 0 && treeIndex < TreeFiles.Length)
        {
            CurrentTreeFileName = TreeFiles[treeIndex].name;
            CurrentTreeIndex = treeIndex;
            Node.clearMaxDepth();
            CurrentTree = NewickParser.Parse(NEWICK_RELATIVE_DIRECTORY_PATH + "/" + TreeFiles[treeIndex].name);
            if (CurrentTree != null)
            {
                DBQueries.ExtractMetaInformation(CurrentTree, CurrentTreeFileName + ".db");
            }
        }
    }
}