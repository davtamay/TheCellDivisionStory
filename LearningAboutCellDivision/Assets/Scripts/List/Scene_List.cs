using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEditor;

[System.Serializable]
public struct Scene_Reference
{
    public string name;
    public int sceneIndex;

}


[CreateAssetMenu(fileName = "Scene_List", menuName = "new_Scene_List", order = 0)]
public class Scene_List : ScriptableObject
{
    public List<Object> scene_list;
    public List<string> scene_StringList;
  //  public List<string> string_list;
    public List<Scene_Reference> _scene_Reference_List;




    //public ValueTuple valueTuple

    //[AddComponentMenu("REFRESH SCENE LIST")]
    public void OnValidate()
    {
    //    _scene_Reference_List.Clear();
    //    scene_StringList.Clear();

        foreach (var item in scene_list)
        {
            if (!scene_StringList.Contains(item.name))
            {
                _scene_Reference_List.Add(new Scene_Reference
                {
                    name = item.name,
                    sceneIndex = scene_StringList.Count
                });
            }
        }
      
        //    scene_StringList.Add(item.name);

        //    //SceneManager.LoadSceneAsync(3 + i, LoadSceneMode.Additive);
        //    //List<EditorBuilderSettingsScene> scenes = new List<EditorBuilderSettingsScene>();
        //    ////have some sort of for loop to cycle through scenes...
        //    //string pathToScene = UnityEditor.AssetDatabase.GetAssetPath(_sceneList.[i]);
        //    //UnityEditor.EditorBuildSettingsScene Ed_scene = new EditorBuildSettingsScene(pathToScene, true);
        //    //scenes.Add(Ed_scene);
        //    ////later on...
        //    //EditorBuildSettings.scenes = scenes.ToArray();

        //}


        //foreach (var item in scene_list)
        //{
        //    if (!scene_StringList.Contains(item.name))
        //    {
        //        _scene_Reference_List.Add(new Scene_Reference
        //        {
        //            name = item.name,
        //            sceneIndex = scene_StringList.Count
        //        });
        //        scene_StringList.Add(item.name);
        //    }

        //}

    }
    public void OnDestroy()
    {
        scene_StringList.Clear();
    }
    //[System.Serializable]
    //public struct URL_Content
    //{
    //    public string name;
    //    public string url;
    //    public float scale;
    //    public Vector3 position;
    //  //  public Quaternion rotation;
    //    public Vector3 euler_rotation;

    //    public bool isInNetwork;


    //}
    //public void DownloadURLIndex(TriLib.Samples.DownloadSample downloadSample, int index)
    //{
    //    downloadSample.LoadFileFrom_URL(string_list[index]);
    //}
}