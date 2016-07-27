using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using UnityEngine.SceneManagement; 
using System.Linq;
using Assets.BeMoBI.Paradigms.SearchAndFind;

namespace Assets.EditorExtensions.BeMoBI.Paradigms.SearchAndFind
{
    public class ScriptBatch
    {

        static string executableName = "SearchAndFind";
        static string dataFolderName = executableName + "_Data";

        static string sep = "/";
        static string assets = "Assets";
        static string path = "";

        [MenuItem("BeMoBI/Compile SearchAndFind")]
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
            
            path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", System.Environment.CurrentDirectory, "");
            string[] levels = new string[] { "Assets/Paradigms/SearchAndFind.unity" };
            
            var targetExecutable = path + sep + executableName + ".exe";
            
            BuildPipeline.BuildPlayer(levels, targetExecutable, BuildTarget.StandaloneWindows64, BuildOptions.None);

            var nlogConfigFile = "NLog.config";

            var appConfigFileName = "AppConfig.json";

            var phasespaceConfigName = "phasespace.json";

            // Copy a file from the project folder to the build folder, alongside the built game.
            copy(nlogConfigFile);

            copy(appConfigFileName);

            copy(phasespaceConfigName);

            var rigidBodyfiles = Directory.GetFiles(assets, "*.rb");

            foreach (var item in rigidBodyfiles)
            {
                var fileName = Path.GetFileName(item);
                copy(fileName);
            }

            var configFiles = Directory.GetFiles(assets, "*_Config.json");

            foreach (var item in configFiles)
            {
                var fileName = Path.GetFileName(item);
                copy(fileName);
            }

            // open the target directory
            Process.Start(path);
        }

        static void copy(string fileName)
        {
            var source = assets + sep + fileName;

            var target = path + sep + dataFolderName + sep + fileName;

            if(!File.Exists(target))
                FileUtil.CopyFileOrDirectory(source,target);
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