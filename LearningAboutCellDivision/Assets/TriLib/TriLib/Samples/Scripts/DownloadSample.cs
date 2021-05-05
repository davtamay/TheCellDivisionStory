//#define TRILIB_USE_ZIP
#pragma warning disable 649
using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicerSamples;
using BzKovSoft.ObjectSlicer.EventHandlers;
#if TRILIB_USE_ZIP
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
using System.IO.Compression;
#else
using ICSharpCode.SharpZipLib.Zip;
#endif
#endif
namespace TriLib
{
    namespace Samples
    {
        public class DownloadSample : MonoBehaviour
        {
            //Here are our asset URLs
            public String_List url_List;
            //public string[] urls =
            //{
            //    "https://cdn.jsdelivr.net/gh/leation/mydata/Bee.glb",
            //    "http://ricardoreis.net/trilib/test2.zip",
            //    "http://ricardoreis.net/trilib/test3.zip",
            //    "http://ricardoreis.net/trilib/test1.3ds"
            //};


            //Stores a reference for file downloaders
            private UnityWebRequest[] _fileDownloaders;

            //Reference for the latest loaded GameObject
            private GameObject _loadedGameObject;

            // public List<GameObject> allGOLoaded;
            GameObject parentOfLoadedObj;
            //Start logic
            private IEnumerator Start()
            {
                //For each asset on the list, we create a slot for a new WWW instance
                _fileDownloaders = new UnityWebRequest[url_List.url_list.Count];

                yield return StartCoroutine(LoadAllURL_GO());

               
                ClientSpawnManager.Instance.isURL_LoadingFinished = true;

                //foreach (Transform item in parentOfLoadedObj.transform)
                //{
                //    item.gameObject.SetActive(false);
                //}
            }

            public IEnumerator LoadAllURL_GO()
            {
                parentOfLoadedObj = new GameObject("Loaded_Object_List");

                for (int i = 0; i < url_List.url_list.Count; i++)
                {
                    yield return StartCoroutine(LoadNetworkObjects(i));

                }

                //LOAD THEM EARLY SO NO Loadingtimes during runtime
                foreach (Transform item in parentOfLoadedObj.transform)
                    item.gameObject.SetActive(true);

                //HavetoIterateAndSetUp_ChildrenOfURLObjects here to maintain index order for ui setup
                for (int i = 0; i < goList.Count; i++)
                {
                    //only set up children if flag is on
                    if(!url_List.url_list[i].isWholeObject)
                    SetUp_RecursiveChild_RBAndColliders(goList[i].transform);
                }
          
                foreach (Transform item in parentOfLoadedObj.transform)
                    item.gameObject.SetActive(false);
                  
                
                //THIS RUNS AFTER SET UP HOPE THAT TURNING THEM OFF AND ON DOESNT MESS UP THE SCENE
                //FIXME        //without yield return? does it still load them up to network list? //eroor in network, you send it to list and then to network update list error?
                //Cant turn them off because they do not receive setup for some raseon
                yield return null;
            
            }

            void SetUp_RecursiveChild_RBAndColliders(Transform trans)
            {
              // CheckForIndividualFilterAndSkinn(trans.gameObject);

                foreach (Transform child in trans)
                {
                    CheckForIndividualFilterAndSkinn(child.gameObject);
                  

                    if (child.childCount > 0)
                    {
                        SetUp_RecursiveChild_RBAndColliders(child);
                    }
                }
            }

            public List<GameObject> goList = new List<GameObject>();
            public IEnumerator LoadNetworkObjects(int index)
            {
                GameObject loadedGO = null;
                yield return StartCoroutine(LoadFileFrom_URL(url_List.url_list[index].url, value =>
                {
                    value.transform.position = url_List.url_list[index].position;
                    value.transform.rotation = Quaternion.Euler(url_List.url_list[index].euler_rotation);//url_List.url_list[index].rotation;
                    value.transform.localScale = new Vector3(url_List.url_list[index].scale, url_List.url_list[index].scale, url_List.url_list[index].scale);

                   
                    loadedGO = value;

                    value.GetComponent<Animation>().animatePhysics = true;

                    //SET UP COLLISIONS, RIGIDBODY AND REGISTER AND GIVE INDEX FOR UI SELECTION
                    //setting index gives it errors stays at 3 or 4
                    //  CheckForIndividualFilterAndSkinn(loadedGO);
                      ClientSpawnManager.Instance.LinkNewNetworkObject(loadedGO, index);
                    //set this to allow sliced objects to get this copy
                    loadedGO.GetComponent<Net_Register_GameObject>().isAutomatic_Register = true;


                    goList.Add(loadedGO);

                    //Check url list to see if we need to create a composite collider or individual collider for each
                    if (!url_List.url_list[index].isWholeObject)
                        CheckForIndividualFilterAndSkinn(loadedGO);
                    else
                        SetUpOneMainFilterAndSkin(loadedGO, index);

                  //  CheckForIndividualFilterAndSkinn(trans.gameObject);
                    loadedGO.transform.parent = parentOfLoadedObj.transform;

                    loadedGO.SetActive(false);

                }));
                //loadedGO.transform.parent.gameObject.SetActive(false);
                //register help? nope doesn help with nodes -> lego children
                //  yield return null;




            }

            public void SetUpOneMainFilterAndSkin(GameObject gameObjectToCheck, int indexForUISelection = -1)
            {
               
                BoxCollider tempCollider = default;

                tempCollider = gameObjectToCheck.gameObject.AddComponent<BoxCollider>();
                //      tempCollider.convex = false;
                if (gameObjectToCheck.transform.localScale.magnitude < 1)
                {
                    float sizeDif = 1 - url_List.url_list[indexForUISelection].scale;
                    tempCollider.size *= sizeDif/ url_List.url_list[indexForUISelection].scale; //gameObjectToCheck.transform.localScale;

                }//CHECK TO SEE IF OBJECT DOES HAVE A RIGID BODY 

                        Rigidbody tempRB = gameObjectToCheck.gameObject.AddComponent<Rigidbody>();

                        //IF NOT REGISTERED INITIATY (PARENT OBJECTS FOR UI LIST - THEN REGISTER THEM NOW CHILDREN OF URLOBJECTS NO NEED FOR URL INDIXES NOW DO URL THEN DO CHILD THEN DO SCENE REGISTER  URL OBJECTS -> CHILD OBJECTS OF URL -> SCENE OBJECTS
                        if (gameObjectToCheck.GetComponent<Net_Register_GameObject>() == null)
                            ClientSpawnManager.Instance.LinkNewNetworkObject(gameObjectToCheck, ClientSpawnManager.Instance._EntityID_To_NetObject.Count);
                        gameObjectToCheck.GetComponent<Net_Register_GameObject>().isAutomatic_Register = true;

                        //set this to allow sliced objects to get this copy
                        //   gameObjectToCheck.GetComponent<Net_Register_GameObject>().isAutomatic_Register = true;
                        //SLICING ITEMS
                        ObjectSlicerSample oSS = gameObjectToCheck.AddComponent<ObjectSlicerSample>();
                        // oSS.useLazyRunner = true;
                        oSS.defaultSliceMaterial = defaultMaterialForSlice;
                        gameObjectToCheck.AddComponent<KnifeSliceableAsync>();
                        gameObjectToCheck.AddComponent<BzSmoothDepenetration>();
                        //

                        tempRB.isKinematic = true;
                        tempRB.useGravity = false;

                        tempRB.velocity = Vector3.zero;
                        gameObjectToCheck.gameObject.tag = "Interactable";
                    
                
                //catch { }



            }

            public void CheckForIndividualFilterAndSkinn(GameObject gameObjectToCheck, int indexForUISelection = -1)
            {

                bool hasMeshFilter = false;
                MeshFilter meshF = default;

                bool hasSkinnedMR = false;
                SkinnedMeshRenderer skinned = default;

                try
                {
                    meshF = gameObjectToCheck.GetComponent<MeshFilter>();
                    if (meshF != null)
                        hasMeshFilter = true;
                }
                catch
                { }
                try
                {
                    skinned = gameObjectToCheck.GetComponent<SkinnedMeshRenderer>();

                    if (skinned != null)
                        hasSkinnedMR = true;
                }
                catch { }

                try
                {

                    if (hasMeshFilter || hasSkinnedMR)
                    {
                        MeshCollider tempCollider = default;

                        if (hasMeshFilter)
                        {

                            //if (tempCollider.sharedMesh == null)
                            //{
                                tempCollider = gameObjectToCheck.gameObject.AddComponent<MeshCollider>();
                                tempCollider.convex = false;
                                tempCollider.sharedMesh = meshF.sharedMesh;
                               
                          //  }
                        }
                        else if (hasSkinnedMR)
                        {
                            if (skinned.rootBone) {

                                gameObjectToCheck = skinned.rootBone.gameObject;

                                tempCollider = gameObjectToCheck.gameObject.AddComponent<MeshCollider>();
                                tempCollider.convex = false;
                                tempCollider.sharedMesh = skinned.sharedMesh;
                           


                            }

                        }

                        //CHECK TO SEE IF OBJECT DOES HAVE A RIGID BODY 

                        Rigidbody tempRB = gameObjectToCheck.gameObject.AddComponent<Rigidbody>();
                   
                        //IF NOT REGISTERED INITIATY (PARENT OBJECTS FOR UI LIST - THEN REGISTER THEM NOW CHILDREN OF URLOBJECTS NO NEED FOR URL INDIXES NOW DO URL THEN DO CHILD THEN DO SCENE REGISTER  URL OBJECTS -> CHILD OBJECTS OF URL -> SCENE OBJECTS
                        if (gameObjectToCheck.GetComponent<Net_Register_GameObject>() == null)
                       ClientSpawnManager.Instance.LinkNewNetworkObject(gameObjectToCheck, ClientSpawnManager.Instance._EntityID_To_NetObject.Count);
                        gameObjectToCheck.GetComponent<Net_Register_GameObject>().isAutomatic_Register = true;

                        //set this to allow sliced objects to get this copy
                     //   gameObjectToCheck.GetComponent<Net_Register_GameObject>().isAutomatic_Register = true;
                        //SLICING ITEMS
                        ObjectSlicerSample oSS = gameObjectToCheck.AddComponent<ObjectSlicerSample>();
                       // oSS.useLazyRunner = true;
                        oSS.defaultSliceMaterial = defaultMaterialForSlice;
                        gameObjectToCheck.AddComponent<KnifeSliceableAsync>();
                        gameObjectToCheck.AddComponent<BzSmoothDepenetration>();
                        //

                        tempRB.isKinematic = true;
                        tempRB.useGravity = false;

                        tempRB.velocity = Vector3.zero;
                        gameObjectToCheck.gameObject.tag = "Interactable";
                    }
                }
                catch { }



            }
            public Material defaultMaterialForSlice;
            public IEnumerator LoadFileFrom_URL(string url, System.Action<GameObject> result)
            {
                for (var i = 0; i < url_List.url_list.Count; i++)
                {
                    if (url != url_List.url_list[i].url)
                        continue;

                    //Gets current WWW instance (if avaliable)
                    var fileDownloader = _fileDownloaders[i];

                    //Checks if current file downloader exists
                    if (fileDownloader == null)
                    {
                        //When clicking the "Load" button

                        //Gets current file name, extension, local path and local filename
                        var fileName = FileUtils.GetFilenameWithoutExtension(url);
                        var fileExtension = FileUtils.GetFileExtension(url);
                        var localFilePath = string.Format("{0}/{1}", Application.persistentDataPath, fileName);
                        var localFilename = string.Format("{0}/{1}{2}", localFilePath, fileName, fileExtension);

                        //Checks if local path exists, which indicates the file has been downloaded
                        if (Directory.Exists(localFilePath))
                        {
                            //Loads local file
                            result(LoadFile(fileExtension, localFilename));
                            yield return null;
                        }
                        else
                        {
                            //If local path doesn't exists, download the file and create the local folder
                            yield return StartCoroutine(DownloadFile(url, i, fileExtension, localFilePath, localFilename, result));
                        }
                        //  }
                    }


                }
            }


            //Searches inside a path and returns the first path of an asset loadable by TriLib
            private string GetReadableAssetPath(string path)
            {
                var supportedExtensions = AssetLoaderBase.GetSupportedFileExtensions();
                foreach (var file in Directory.GetFiles(path))
                {
                    var fileExtension = FileUtils.GetFileExtension(file);
                    if (supportedExtensions.Contains("*" + fileExtension + ";"))
                    {
                        return file;
                    }
                }

                foreach (var directory in Directory.GetDirectories(path))
                {
                    var assetPath = GetReadableAssetPath(directory);
                    if (assetPath != null)
                    {
                        return assetPath;
                    }
                }

                return null;
            }

            //Loads an existing local file
            private GameObject LoadFile(string fileExtension, string localFilename)
            {
                //Creates a new AssetLoader instance
                using (var assetLoader = new AssetLoader())
                {
                    //Checks if the URL is a ZIP file
                    if (fileExtension == ".zip")
                    {
#if TRILIB_USE_ZIP
                        var localFilePath = FileUtils.GetFileDirectory(localFilename);

                    //Gets the first asset loadable by TriLib on the folder
                    var assetPath = GetReadableAssetPath(localFilePath);
                    if (assetPath == null)
                    {
                        Debug.LogError("No TriLib readable file could be found on the given directory");
                        return null;
                    }

                    //Loads the found asset
                     return _loadedGameObject = assetLoader.LoadFromFile(assetPath);
                        //#else
                        //throw new Exception("Please enable TriLib ZIP loading");
#endif
                    }
                    else
                    {
                        //If the URL is not a ZIP file, loads the file inside the folder
                        return _loadedGameObject = assetLoader.LoadFromFile(localFilename);
                    }

                    return null;
                    //Move camera away to fit the loaded object in view
                    //  Camera.main.FitToBounds(_loadedGameObject.transform, 3f);
                }
            }

            //Downloads a file to a local path or extract all ZIP file contents to the local path in case of ZIP files, then loads the file
            private IEnumerator DownloadFile(string url, int index, string fileExtension, string localFilePath, string localFilename, System.Action<GameObject> result = null)
            {
                _fileDownloaders[index] = UnityWebRequest.Get(url);
                yield return _fileDownloaders[index].SendWebRequest();
                if (fileExtension == ".zip")
                {
#if TRILIB_USE_ZIP
                    using (var memoryStream = new MemoryStream(_fileDownloaders[index].downloadHandler.data))
                {
                    UnzipFromStream(memoryStream, localFilePath);
                }
#else
                    throw new Exception("Please enable TriLib ZIP loading");
#endif
                }

                Directory.CreateDirectory(localFilePath);
                File.WriteAllBytes(localFilename, _fileDownloaders[index].downloadHandler.data);

                if (result != null)
                {
                    result(LoadFile(fileExtension, localFilename));
                }
                else
                    LoadFile(fileExtension, localFilename);


                _fileDownloaders[index] = null;
            }

#if TRILIB_USE_ZIP
            //Helper function to extract all ZIP file contents to a local folder
            private void UnzipFromStream(Stream zipStream, string outFolder)
        {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
            var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Read);
            foreach (ZipArchiveEntry zipEntry in zipFile.Entries)
            {
                var zipFileStream = zipEntry.Open();
#else
                var zipFile = new ZipFile(zipStream);
            foreach (ZipEntry zipEntry in zipFile)
            {
                if (!zipEntry.IsFile)
                {
                    continue;
                }
                var zipFileStream = zipFile.GetInputStream(zipEntry);
#endif
                var entryFileName = zipEntry.Name;
                var buffer = new byte[4096];
                var fullZipToPath = Path.Combine(outFolder, entryFileName);
                var directoryName = Path.GetDirectoryName(fullZipToPath);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                var fileName = Path.GetFileName(fullZipToPath);
                if (fileName.Length == 0)
                {
                    continue;
                }
                using (var streamWriter = File.Create(fullZipToPath))
                {
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
                    zipFileStream.CopyTo(streamWriter);
#else
                    ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zipFileStream, streamWriter, buffer);
#endif
                }
            }
        }
#endif
        }
    }
}
#pragma warning restore 649


////#define TRILIB_USE_ZIP
//#pragma warning disable 649
//using System;
//using System.IO;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.Networking;
//using System.Collections.Generic;
//#if TRILIB_USE_ZIP
//#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
//using System.IO.Compression;
//#else
//using ICSharpCode.SharpZipLib.Zip;
//#endif
//#endif
//namespace TriLib
//{
//    namespace Samples
//    {
//        public class DownloadSample : MonoBehaviour
//        {
//            //Here are our asset URLs
//            public String_List url_List;
//            //public string[] urls =
//            //{
//            //    "https://cdn.jsdelivr.net/gh/leation/mydata/Bee.glb",
//            //    "http://ricardoreis.net/trilib/test2.zip",
//            //    "http://ricardoreis.net/trilib/test3.zip",
//            //    "http://ricardoreis.net/trilib/test1.3ds"
//            //};


//            //Stores a reference for file downloaders
//            private UnityWebRequest[] _fileDownloaders;

//            //Reference for the latest loaded GameObject
//            private GameObject _loadedGameObject;

//           // public List<GameObject> allGOLoaded;
//            GameObject parentOfLoadedObj;
//            //Start logic
//            private IEnumerator Start()
//            {
//                //For each asset on the list, we create a slot for a new WWW instance
//                _fileDownloaders = new UnityWebRequest[url_List.url_list.Count];

//                yield return StartCoroutine(LoadAllURL_GO());

//                ClientSpawnManager.Instance.isURL_LoadingFinished = true;
//            }

//            public IEnumerator LoadAllURL_GO()
//            {
//                parentOfLoadedObj = new GameObject("Loaded_Object_List");

//                //WORKS ON WEBGL DOING MULTIPLE IS WHAT BRINGS ERROR
//                for (int i = 0; i < url_List.url_list.Count; i++)
//                {
//                    yield return StartCoroutine(LoadNetworkObjects(i));

//                    //   yield return new WaitForSeconds(2);
//                }
//            }

//            void SetUpRBAndColliders(Transform trans)
//            {
//                try
//                {
//                   // trans.gameObject.SetActive(true);

//                    if (trans.GetComponent<MeshFilter>() || trans.GetComponent<SkinnedMeshRenderer>() || trans.GetComponent<MeshRenderer>())
//                    {

//                        MeshCollider tempCollider = trans.gameObject.AddComponent<MeshCollider>();
//                        tempCollider.convex = true;

//                        Rigidbody tempRB = trans.gameObject.AddComponent<Rigidbody>();


//                        Net_Register_GameObject nRO = trans.gameObject.AddComponent<Net_Register_GameObject>();

//                        nRO.isAutomatic_Register = true;
//                       // trans.gameObject.SetActive(true);
//                        // nRO.gameObject.SetActive(true);
//                        //     nRO.gameObject.SetActive(false);


//                        tempRB.isKinematic = true;
//                        tempRB.useGravity = false;

//                        tempRB.velocity = Vector3.zero;
//                        trans.gameObject.tag = "Interactable";
//                    }
//                }
//                catch {  }

//                foreach (Transform child in trans)
//                {

//                    try
//                    {

//                        if (child.GetComponent<MeshFilter>() || child.GetComponent<SkinnedMeshRenderer>() || child.GetComponent<MeshRenderer>())
//                        {

//                            MeshCollider tempCollider = child.gameObject.AddComponent<MeshCollider>();
//                           tempCollider.convex = true;

//                            Rigidbody tempRB = child.gameObject.AddComponent<Rigidbody>();

//                            Net_Register_GameObject nRO = child.gameObject.AddComponent<Net_Register_GameObject>();

//                            nRO.isAutomatic_Register = true;



//                            tempRB.isKinematic = true;
//                            tempRB.useGravity = false;

//                            tempRB.velocity = Vector3.zero;
//                            child.gameObject.tag = "Interactable";
//                        }
//                    }
//                    catch { continue; }
//                   // Debug.Log(child.name);
//                    if (child.childCount > 0)
//                    {
//                        SetUpRBAndColliders(child);
//                    }
//                }
//            }
//            public IEnumerator LoadNetworkObjects(int index)
//            {
//                GameObject loadedGO = null;
//                yield return StartCoroutine(LoadFileFrom_URL(url_List.url_list[index].url, value =>
//                {
//                    value.transform.position = url_List.url_list[index].position;
//                    value.transform.rotation = Quaternion.Euler(url_List.url_list[index].euler_rotation);//url_List.url_list[index].rotation;
//                    value.transform.localScale = new Vector3(url_List.url_list[index].scale, url_List.url_list[index].scale, url_List.url_list[index].scale);

//                    loadedGO = value;

//                    value.GetComponent<Animation>().animatePhysics = true;


//                    foreach (Transform item in loadedGO.transform)
//                    {

//                        SetUpRBAndColliders(item);

//                    }

//                    ClientSpawnManager.Instance.LinkNewNetworkObject(loadedGO, index);
//                    loadedGO.SetActive(true);


//                    loadedGO.transform.parent = parentOfLoadedObj.transform;
//                }));


//            }

//            public IEnumerator LoadFileFrom_URL(string url, System.Action<GameObject> result)
//            {
//                for (var i = 0; i < url_List.url_list.Count; i++)
//                {
//                    if (url != url_List.url_list[i].url)
//                        continue;

//                    //Gets current WWW instance (if avaliable)
//                    var fileDownloader = _fileDownloaders[i];

//                    //Checks if current file downloader exists
//                    if (fileDownloader == null)
//                    {
//                        //When clicking the "Load" button

//                        //Gets current file name, extension, local path and local filename
//                        var fileName = FileUtils.GetFilenameWithoutExtension(url);
//                        var fileExtension = FileUtils.GetFileExtension(url);
//                        var localFilePath = string.Format("{0}/{1}", Application.persistentDataPath, fileName);
//                        var localFilename = string.Format("{0}/{1}{2}", localFilePath, fileName, fileExtension);

//                        //Checks if local path exists, which indicates the file has been downloaded
//                        if (Directory.Exists(localFilePath))
//                        {
//                            //Loads local file
//                            result(LoadFile(fileExtension, localFilename));
//                            yield return null;
//                        }
//                        else
//                        {
//                            //If local path doesn't exists, download the file and create the local folder
//                            yield return StartCoroutine(DownloadFile(url, i, fileExtension, localFilePath, localFilename, result));
//                        }
//                        //  }
//                    }


//                }
//            }
//            // GUI logic
//            //private void OnGUI()
//            //{
//            //    //Loop thru all avaliable URLs
//            //    for (var i = 0; i < url_List.url_list.Count; i++)
//            //    {
//            //        //Gets current URL
//            //        var url = url_List.url_list[i].url;

//            //        //Gets current WWW instance (if avaliable)
//            //        var fileDownloader = _fileDownloaders[i];

//            //        GUILayout.BeginHorizontal();

//            //        //Shows the URL on GUI
//            //        GUILayout.Label(url);

//            //        //Checks if current file downloader exists
//            //        if (fileDownloader == null)
//            //        {
//            //            //When clicking the "Load" button
//            //            if (GUILayout.Button("Load"))
//            //            {
//            //                //Destroys the latest loaded GameObject, if avaliable
//            //                if (_loadedGameObject != null)
//            //                {
//            //                    Destroy(_loadedGameObject);
//            //                }

//            //                //Gets current file name, extension, local path and local filename
//            //                var fileName = FileUtils.GetFilenameWithoutExtension(url);
//            //                var fileExtension = FileUtils.GetFileExtension(url);
//            //                var localFilePath = string.Format("{0}/{1}", Application.persistentDataPath, fileName);
//            //                var localFilename = string.Format("{0}/{1}{2}", localFilePath, fileName, fileExtension);

//            //                //Checks if local path exists, which indicates the file has been downloaded
//            //                if (Directory.Exists(localFilePath))
//            //                {
//            //                    //Loads local file
//            //                    LoadFile(fileExtension, localFilename);
//            //                }
//            //                else
//            //                {
//            //                    //If local path doesn't exists, download the file and create the local folder
//            //                    StartCoroutine(DownloadFile(url, i, fileExtension, localFilePath, localFilename));
//            //                }
//            //            }
//            //        }
//            //        else
//            //        {
//            //            //Shows the current file download progress
//            //            GUILayout.Label(string.Format("Downloaded {0:P2}", fileDownloader.downloadedBytes == 0 ? 0f : fileDownloader.downloadProgress));
//            //        }

//            //        GUILayout.EndHorizontal();
//            //    }
//            //}

//            //Searches inside a path and returns the first path of an asset loadable by TriLib
//            private string GetReadableAssetPath(string path)
//            {
//                var supportedExtensions = AssetLoaderBase.GetSupportedFileExtensions();
//                foreach (var file in Directory.GetFiles(path))
//                {
//                    var fileExtension = FileUtils.GetFileExtension(file);
//                    if (supportedExtensions.Contains("*" + fileExtension + ";"))
//                    {
//                        return file;
//                    }
//                }

//                foreach (var directory in Directory.GetDirectories(path))
//                {
//                    var assetPath = GetReadableAssetPath(directory);
//                    if (assetPath != null)
//                    {
//                        return assetPath;
//                    }
//                }

//                return null;
//            }

//            //Loads an existing local file
//            private GameObject LoadFile(string fileExtension, string localFilename)
//            {
//                //Creates a new AssetLoader instance
//                using (var assetLoader = new AssetLoader())
//                {
//                    //Checks if the URL is a ZIP file
//                    if (fileExtension == ".zip")
//                    {
//#if TRILIB_USE_ZIP
//                        var localFilePath = FileUtils.GetFileDirectory(localFilename);

//                    //Gets the first asset loadable by TriLib on the folder
//                    var assetPath = GetReadableAssetPath(localFilePath);
//                    if (assetPath == null)
//                    {
//                        Debug.LogError("No TriLib readable file could be found on the given directory");
//                        return null;
//                    }

//                    //Loads the found asset
//                     return _loadedGameObject = assetLoader.LoadFromFile(assetPath);
//                        //#else
//                        //throw new Exception("Please enable TriLib ZIP loading");
//#endif
//                    }
//                    else
//                    {
//                        //If the URL is not a ZIP file, loads the file inside the folder
//                        return _loadedGameObject = assetLoader.LoadFromFile(localFilename);
//                    }

//                    return null;
//                    //Move camera away to fit the loaded object in view
//                    //  Camera.main.FitToBounds(_loadedGameObject.transform, 3f);
//                }
//            }

//            //Downloads a file to a local path or extract all ZIP file contents to the local path in case of ZIP files, then loads the file
//            private IEnumerator DownloadFile(string url, int index, string fileExtension, string localFilePath, string localFilename, System.Action<GameObject> result = null)
//            {
//                _fileDownloaders[index] = UnityWebRequest.Get(url);
//                yield return _fileDownloaders[index].SendWebRequest();
//                if (fileExtension == ".zip")
//                {
//#if TRILIB_USE_ZIP
//                    using (var memoryStream = new MemoryStream(_fileDownloaders[index].downloadHandler.data))
//                {
//                    UnzipFromStream(memoryStream, localFilePath);
//                }
//#else
//                    throw new Exception("Please enable TriLib ZIP loading");
//#endif
//                }

//                Directory.CreateDirectory(localFilePath);
//                File.WriteAllBytes(localFilename, _fileDownloaders[index].downloadHandler.data);

//                if (result != null)
//                {
//                    result(LoadFile(fileExtension, localFilename));
//                }
//                else
//                    LoadFile(fileExtension, localFilename);


//                _fileDownloaders[index] = null;
//            }

//#if TRILIB_USE_ZIP
//            //Helper function to extract all ZIP file contents to a local folder
//            private void UnzipFromStream(Stream zipStream, string outFolder)
//        {
//#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
//            var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Read);
//            foreach (ZipArchiveEntry zipEntry in zipFile.Entries)
//            {
//                var zipFileStream = zipEntry.Open();
//#else
//                var zipFile = new ZipFile(zipStream);
//            foreach (ZipEntry zipEntry in zipFile)
//            {
//                if (!zipEntry.IsFile)
//                {
//                    continue;
//                }
//                var zipFileStream = zipFile.GetInputStream(zipEntry);
//#endif
//                var entryFileName = zipEntry.Name;
//                var buffer = new byte[4096];
//                var fullZipToPath = Path.Combine(outFolder, entryFileName);
//                var directoryName = Path.GetDirectoryName(fullZipToPath);
//                if (!string.IsNullOrEmpty(directoryName))
//                {
//                    Directory.CreateDirectory(directoryName);
//                }
//                var fileName = Path.GetFileName(fullZipToPath);
//                if (fileName.Length == 0)
//                {
//                    continue;
//                }
//                using (var streamWriter = File.Create(fullZipToPath))
//                {
//#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
//                    zipFileStream.CopyTo(streamWriter);
//#else
//                    ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(zipFileStream, streamWriter, buffer);
//#endif
//                }
//            }
//        }
//#endif
//        }
//    }
//}
//#pragma warning restore 649