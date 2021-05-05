using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetUp_ButtonURLs : MonoBehaviour
{
    public GameObject _buttonTemplate;
    

    public Transform transformToPlaceButtonUnder;

    public bool isURLButtonList = true;
    public String_List URL_List;

    public bool isEnvironmentButtonList = false;
    public Scene_List _sceneList;

    List<Button> allButtons = new List<Button>();
    IEnumerator Start()
    {
        if (isURLButtonList)
        {
            if (!transformToPlaceButtonUnder)
                transformToPlaceButtonUnder = transform;

            List<GameObject> buttonLinks = new List<GameObject>();

            for (int i = 0; i < URL_List.url_list.Count; i++)
            {
                GameObject temp = Instantiate(_buttonTemplate, transformToPlaceButtonUnder);

                Button tempButton = temp.GetComponentInChildren<Button>(true);

                SetButtonDelegateURL(tempButton, i);
                Text tempText = temp.GetComponentInChildren<Text>(true);
                tempText.text = URL_List.url_list[i].name;

              //  temp.SetActive(false);
                buttonLinks.Add(temp);
            }

             yield return new WaitUntil(() => ClientSpawnManager.Instance.isURL_LoadingFinished);
           
        }
        else if(isEnvironmentButtonList){

            if (!transformToPlaceButtonUnder)
                transformToPlaceButtonUnder = transform;

            List<GameObject> buttonLinks = new List<GameObject>();

            // for (int i = 0; i < _sceneList.scene_StringList.Count; i++)
            for (int i = 0; i < _sceneList._scene_Reference_List.Count; i++)
            {
                GameObject temp = Instantiate(_buttonTemplate, transformToPlaceButtonUnder);

                Button tempButton = temp.GetComponentInChildren<Button>(true);

                //Accounting for the scenes already present

                //  SceneManager.sceneList.scene_list[i]
                // SetButtonDelegate_Scene(tempButton, 3 + i);
                SetButtonDelegate_Scene(tempButton, _sceneList._scene_Reference_List[i]);
                Text tempText = temp.GetComponentInChildren<Text>(true);

                //  Scene scene = _sceneList.scene_list[i];

                tempText.text = _sceneList._scene_Reference_List[i].name;// scene_list[i].name;//scenes[i].name;

              //  temp.SetActive(false);
                buttonLinks.Add(temp);
                allButtons.Add(tempButton);

            }
        }
    }
    public void SetButtonDelegateURL(Button button, int index)
    {
        ClientSpawnManager.Instance._assetButtonRegisterList.Add(button);

        button.onClick.AddListener(delegate {
            ClientSpawnManager.Instance.On_Select_Asset_Refence_Button(index, button);
        });
    }

    public void SetButtonDelegate_Scene(Button button, Scene_Reference sceneRef)
    {
        ClientSpawnManager.Instance._sceneButtonRegisterList.Add(button);

        button.onClick.AddListener(delegate {
            foreach (Button but in allButtons)
            {
                but.interactable = true;
            };
        });

        button.onClick.AddListener(delegate {
            ClientSpawnManager.Instance.On_Select_Scene_Refence_Button(sceneRef, button);
        });

      
    }

}
