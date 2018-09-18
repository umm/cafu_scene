using System;
using System.Collections.Generic;
using System.Linq;
using CAFU.Scene.Application;
using CAFU.Scene.Domain.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CAFU.Scene.Editor
{
    [InitializeOnLoad]
    public static class SceneContractLoader
    {
        private const string SystemSceneName = "System";
        private const string SystemScenePath = "Assets/Scenes/System.unity";
        private const string SystemControllerPropertyNameInitialSceneNameList = "InitialSceneNameList";
        private const string EditorPrefsKeySystemControllerProperty = "OriginalInitialSceneNameList";

        static SceneContractLoader()
        {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredEditMode && EditorPrefs.HasKey(EditorPrefsKeySystemControllerProperty))
                {
                    RestoreScenes();
                }

                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    LoadSystemScene();
                }
            };
        }

        private static void RestoreScenes()
        {
            var sceneContractState = JsonUtility.FromJson<SceneContractState>(EditorPrefs.GetString(EditorPrefsKeySystemControllerProperty));
            var systemScene = SceneManager.GetSceneByPath(SystemScenePath);
            var systemController = systemScene
                .GetRootGameObjects()
                .First(x => x.GetComponent<ISystemController>() != null)
                .GetComponent<ISystemController>();
            systemController.GetType().GetProperty(SystemControllerPropertyNameInitialSceneNameList)?.SetValue(systemController, sceneContractState.InitialSceneNameList);
            var openedSceneList = new List<UnityEngine.SceneManagement.Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                openedSceneList.Add(SceneManager.GetSceneAt(i));
            }

            var dummyScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            foreach (var scene in openedSceneList)
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            foreach (var openedScenePath in sceneContractState.OpenedScenePathList)
            {
                EditorSceneManager.OpenScene(openedScenePath, OpenSceneMode.Additive);
            }

            EditorSceneManager.CloseScene(dummyScene, true);
        }

        private static void LoadSystemScene()
        {
            // Do nothing if already loaded.
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == SystemSceneName)
                {
                    return;
                }
            }

            // Do nothing if `System' scene does not exists.
            var systemSceneAsset = AssetDatabase.LoadAssetAtPath<Object>(SystemScenePath);
            if (systemSceneAsset == null)
            {
                Debug.LogWarning("Could not find scene named `System' under `Assets/Scenes/'.");
                return;
            }

            // Search SceneStrategyList from all ScriptableObjects
            var sceneStrategyList = AssetDatabase
                .FindAssets("t:ScriptableObject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
                .OfType<ISceneStrategyList>()
                .FirstOrDefault();

            // Do nothing if `SceneStrategyList' does not exists.
            if (sceneStrategyList == null)
            {
                Debug.LogWarning("Could not find SceneStrategyList.asset in project.");
                return;
            }

            var openedSceneNameList = new List<string>();
            var openedScenePathList = new List<string>();
            var openedSceneList = new List<UnityEngine.SceneManagement.Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                openedSceneNameList.Add(SceneManager.GetSceneAt(i).name);
                openedScenePathList.Add(SceneManager.GetSceneAt(i).path);
                openedSceneList.Add(SceneManager.GetSceneAt(i));
            }

            var systemScene = EditorSceneManager.OpenScene(SystemScenePath, OpenSceneMode.Additive);
            var systemController = systemScene
                .GetRootGameObjects()
                .First(x => x.GetComponent<ISystemController>() != null)
                .GetComponent<ISystemController>();
            SceneManager.SetActiveScene(systemScene);

            // Store original `InitialSceneNameList' value
            var systemControllerProperty = new SceneContractState(systemController.InitialSceneNameList.ToList(), openedScenePathList);
            EditorPrefs.SetString(EditorPrefsKeySystemControllerProperty, JsonUtility.ToJson(systemControllerProperty));

            // Overwrite opened scene name list into `InitialSceneNameList'
            systemController.GetType().GetProperty(SystemControllerPropertyNameInitialSceneNameList)?.SetValue(systemController, openedSceneNameList);

            foreach (var scene in openedSceneList)
            {
                if (scene.name != SystemSceneName)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            EditorApplication.isPlaying = true;
        }
    }

    [Serializable]
    public struct SceneContractState
    {
        [SerializeField] private List<string> initialSceneNameList;

        public List<string> InitialSceneNameList => initialSceneNameList;

        [SerializeField] private List<string> openedScenePathList;
        public List<string> OpenedScenePathList => openedScenePathList;

        public SceneContractState(List<string> initialSceneNameList, List<string> openedScenePathList)
        {
            this.initialSceneNameList = initialSceneNameList;
            this.openedScenePathList = openedScenePathList;
        }
    }
}