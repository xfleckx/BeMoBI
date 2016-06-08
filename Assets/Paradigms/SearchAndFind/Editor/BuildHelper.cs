using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using UnityEngine.SceneManagement; 
using System.Linq;
using Assets.BeMoBI.Paradigms.SearchAndFind;

namespace Assets.Editor.BeMoBI.Paradigms.SearchAndFind
{
    public class ScriptBatch
    {
        [MenuItem("BeMoBI/Combile SearchAndFind")]
        public static void Build_SearchAndFind()
        {
            /// a prototypical function to test automaticaly build scenes

            var activeScene = SceneManager.GetActiveScene();

            if (activeScene.HasNoParadigmController())
            {
                UnityEngine.Debug.LogError("It seems that the current Scene is not a SearchAndFind paradigm. Didn't find a ParadimController instance in the top level game objects.");
                return;
            }

            var paradigm = activeScene.GetParadigmController();

            paradigm.ClearConfiguration();
            
            GameObject.DestroyImmediate(paradigm.InstanceDefinition);

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
    
    static class ExtensionHelper
    {
        public static bool HasNoParadigmController(this Scene scene)
        {
            var allRootGameObjects = scene.GetRootGameObjects();

            var hasOne = allRootGameObjects.Any((go) => go.GetComponent<ParadigmController>() != null);
            
            return !hasOne;
        }

        public static ParadigmController GetParadigmController(this Scene scene)
        {
            var allRootGameObjects = scene.GetRootGameObjects();
            var result = allRootGameObjects.Where((go) => go.GetComponent<ParadigmController>() != null).Select((go) => go.GetComponent<ParadigmController>()).First();
            return result;
        }
    }

}