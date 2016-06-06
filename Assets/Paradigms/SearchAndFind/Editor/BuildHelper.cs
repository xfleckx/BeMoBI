using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Diagnostics;
using System.IO;

namespace Assets.Editor.BeMoBI.Paradigms.SearchAndFind
{
    public class ScriptBatch
    {
        [MenuItem("BeMoBI/Combile SearchAndFind")]
        public static void Build_SearchAndFind()
        {
            /// a prototypical function to test automaticaly build scenes

            // Get filename.
            string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", System.Environment.CurrentDirectory, "");
            string[] levels = new string[] { "Assets/Paradigms/SearchAndFind.unity" };

            var executableName = "SearchAndFind";
            var dataFolderName = executableName + "_Data";
            var targetExecutable = path + "/" + executableName + ".exe";

            // Build player.
            BuildPipeline.BuildPlayer(levels, targetExecutable, BuildTarget.StandaloneWindows64, BuildOptions.None);

            var nlogConfigFile = "NLog.config";
            var configFileName = "SearchAndFind_Config.json";
            var appConfigFileName = "AppConfig.json";

            var sep = Path.DirectorySeparatorChar;
            var assets = "Assets";

            // Copy a file from the project folder to the build folder, alongside the built game.
            FileUtil.CopyFileOrDirectory(assets + sep + nlogConfigFile, path + sep + dataFolderName + sep + nlogConfigFile);
            FileUtil.CopyFileOrDirectory(assets + sep + configFileName, path + sep + dataFolderName + sep + configFileName);

            FileUtil.CopyFileOrDirectory(assets + sep + appConfigFileName, path + sep + dataFolderName + sep + appConfigFileName);
            // open the target directory
            Process.Start(path);

        }
    }
}