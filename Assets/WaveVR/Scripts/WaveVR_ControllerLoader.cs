using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using wvr;
using WaveVR_Log;
using System;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(WaveVR_ControllerLoader))]
public class WaveVR_ControllerLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaveVR_ControllerLoader myScript = target as WaveVR_ControllerLoader;

        myScript.WhichHand = (WaveVR_ControllerLoader.ControllerHand)EditorGUILayout.EnumPopup ("Type", myScript.WhichHand);
        myScript.ControllerComponents = (WaveVR_ControllerLoader.CComponent)EditorGUILayout.EnumPopup ("Controller Components", myScript.ControllerComponents);

        myScript.TrackPosition = EditorGUILayout.Toggle ("Track Position", myScript.TrackPosition);
        if (true == myScript.TrackPosition)
        {
            myScript.SimulationOption = (WVR_SimulationOption)EditorGUILayout.EnumPopup ("    Simulate Position", myScript.SimulationOption);
            if (myScript.SimulationOption == WVR_SimulationOption.ForceSimulation || myScript.SimulationOption == WVR_SimulationOption.WhenNoPosition)
            {
                myScript.FollowHead = (bool)EditorGUILayout.Toggle ("        Follow Head", myScript.FollowHead);
            }
        }

        myScript.TrackRotation = EditorGUILayout.Toggle ("Track Rotation", myScript.TrackRotation);

        EditorGUILayout.LabelField("Indication feature");
        myScript.overwriteIndicatorSettings = EditorGUILayout.Toggle("Overwrite Indicator Settings", myScript.overwriteIndicatorSettings);
        if (true == myScript.overwriteIndicatorSettings)
        {
            myScript.showIndicator = EditorGUILayout.Toggle("Show Indicator", myScript.showIndicator);
            if (true == myScript.showIndicator)
            {
                myScript.hideIndicatorByRoll = EditorGUILayout.Toggle("Hide Indicator when roll angle > 90 ", myScript.hideIndicatorByRoll);
                myScript.showIndicatorAngle = EditorGUILayout.FloatField("Show When Angle > ", myScript.showIndicatorAngle);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Line customization");
                myScript.lineLength = EditorGUILayout.FloatField("Line Length", myScript.lineLength);
                myScript.lineStartWidth = EditorGUILayout.FloatField("Line Start Width", myScript.lineStartWidth);
                myScript.lineEndWidth = EditorGUILayout.FloatField("Line End Width", myScript.lineEndWidth);
                myScript.lineColor = EditorGUILayout.ColorField("Line Color", myScript.lineColor);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Text customization");
                myScript.textCharacterSize = EditorGUILayout.FloatField("Text Character Size", myScript.textCharacterSize);
                myScript.zhCharactarSize = EditorGUILayout.FloatField("Chinese Character Size", myScript.zhCharactarSize);
                myScript.textFontSize = EditorGUILayout.IntField("Text Font Size", myScript.textFontSize);
                myScript.textColor = EditorGUILayout.ColorField("Text Color", myScript.textColor);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Key indication");
                var list = myScript.buttonIndicationList;

                int newCount = Mathf.Max(0, EditorGUILayout.IntField("Button indicator size", list.Count));

                while (newCount < list.Count)
                    list.RemoveAt(list.Count - 1);
                while (newCount > list.Count)
                    list.Add(new ButtonIndication());

                for (int i = 0; i < list.Count; i++)
                {
                    EditorGUILayout.LabelField("Button indication " + i);
                    myScript.buttonIndicationList[i].keyType = (ButtonIndication.KeyIndicator)EditorGUILayout.EnumPopup("Key Type", myScript.buttonIndicationList[i].keyType);
                    myScript.buttonIndicationList[i].alignment = (ButtonIndication.Alignment)EditorGUILayout.EnumPopup("Alignment", myScript.buttonIndicationList[i].alignment);
                    myScript.buttonIndicationList[i].indicationOffset = EditorGUILayout.Vector3Field("Indication offset", myScript.buttonIndicationList[i].indicationOffset);
                    myScript.buttonIndicationList[i].indicationText = EditorGUILayout.TextField("Indication text", myScript.buttonIndicationList[i].indicationText);
                    myScript.buttonIndicationList[i].followButtonRotation = EditorGUILayout.Toggle("Follow button rotation", myScript.buttonIndicationList[i].followButtonRotation);
                    EditorGUILayout.Space();
                }
            }
        }

        myScript.adaptiveLoading = EditorGUILayout.Toggle("Adaptive loading", myScript.adaptiveLoading);

        if (GUI.changed)
            EditorUtility.SetDirty ((WaveVR_ControllerLoader)target);
    }
}
#endif

public class WaveVR_ControllerLoader : MonoBehaviour {
    private static string LOG_TAG = "WaveVR_ControllerLoader";
    private void PrintDebugLog(string msg)
    {
        #if UNITY_EDITOR
        Debug.Log(LOG_TAG + "  Hand: " + WhichHand + ", " + msg);
        #endif
        Log.d (LOG_TAG, "Hand: " + WhichHand + ", " + msg);
    }

    private void PrintInfoLog(string msg)
    {
        #if UNITY_EDITOR
        PrintDebugLog(msg);
        #endif
        Log.i (LOG_TAG, "Hand: " + WhichHand + ", " + msg);
    }

    private void PrintWarningLog(string msg)
    {
#if UNITY_EDITOR
        PrintDebugLog(msg);
#endif
        Log.w(LOG_TAG, "Hand: " + WhichHand + ", " + msg);
    }

    public enum ControllerHand
    {
        Controller_Right,
        Controller_Left
    };

    public enum CComponent
    {
        One_Bone,
        Multi_Component
    };

    public enum CTrackingSpace
    {
        REAL_POSITION_ONLY,
        FAKE_POSITION_ONLY,
        AUTO_POSITION_ONLY,
        ROTATION_ONLY,
        ROTATION_AND_REAL_POSITION,
        ROTATION_AND_FAKE_POSITION,
        ROTATION_AND_AUTO_POSITION,
        CTS_SYSTEM
    };

    [Header("Loading options")]
    public ControllerHand WhichHand = ControllerHand.Controller_Right;
    public CComponent ControllerComponents = CComponent.Multi_Component;
    public bool TrackPosition = true;
    public WVR_SimulationOption SimulationOption = WVR_SimulationOption.WhenNoPosition;
    public bool FollowHead = false;
    public bool TrackRotation = true;

    [Header("Indication feature")]
    public bool overwriteIndicatorSettings = true;
    public bool showIndicator = false;
    public bool hideIndicatorByRoll = true;

    [Range(0, 90.0f)]
    public float showIndicatorAngle = 30.0f;

    [Header("Line customization")]
    [Range(0.01f, 0.1f)]
    public float lineLength = 0.03f;
    [Range(0.0001f, 0.1f)]
    public float lineStartWidth = 0.0004f;
    [Range(0.0001f, 0.1f)]
    public float lineEndWidth = 0.0004f;
    public Color lineColor = Color.white;

    [Header("Text customization")]
    [Range(0.01f, 0.2f)]
    public float textCharacterSize = 0.08f;
    [Range(0.01f, 0.2f)]
    public float zhCharactarSize = 0.07f;
    [Range(50, 200)]
    public int textFontSize = 100;
    public Color textColor = Color.white;

    [Header("Indications")]
    public List<ButtonIndication> buttonIndicationList = new List<ButtonIndication>();

    public bool adaptiveLoading = true;

    private GameObject controllerPrefab = null;
    private GameObject originalControllerPrefab = null;
    private string controllerFileName = "";
    private string controllerModelFoler = "Controller/";
    private string genericControllerFileName = "Generic_";
    private List<AssetBundle> loadedAssetBundle = new List<AssetBundle>();

    private WVR_DeviceType deviceType = WVR_DeviceType.WVR_DeviceType_Controller_Right;
    private bool connected = false;
#if UNITY_EDITOR
    public delegate void ControllerModelLoaded(GameObject go);
    public static event ControllerModelLoaded onControllerModelLoaded = null;
#endif

    void OnEnable()
    {
        controllerPrefab = null;
        controllerFileName = "";
        genericControllerFileName = "Generic_";
        if (WhichHand == ControllerHand.Controller_Right)
        {
            deviceType = WVR_DeviceType.WVR_DeviceType_Controller_Right;
        }
        else
        {
            deviceType = WVR_DeviceType.WVR_DeviceType_Controller_Left;
        }
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            WVR_DeviceType _type = WaveVR_Controller.Input(this.deviceType).DeviceType;
            onLoadController(_type);
            return;
        }
#endif

        WaveVR_Utils.Event.Listen(WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            return;
        }
#endif
        WaveVR_Utils.Event.Remove(WaveVR_Utils.Event.DEVICE_CONNECTED, onDeviceConnected);
    }
    // Use this for initialization
    void Start()
    {
        loadedAssetBundle.Clear();
        if (checkConnection () != connected)
            connected = !connected;

        if (connected)
        {
            WaveVR.Device _device = WaveVR.Instance.getDeviceByType (this.deviceType);
            onLoadController (_device.type);
        }

        WaveVR_EventSystemControllerProvider.Instance.MarkControllerLoader (deviceType, true);
    }

    private void onDeviceConnected(params object[] args)
    {
        bool _connected = false;
        WVR_DeviceType _type = this.deviceType;

        #if UNITY_EDITOR
        if (Application.isEditor)
        {
            _connected = WaveVR_Controller.Input (this.deviceType).connected;
            _type = WaveVR_Controller.Input(this.deviceType).DeviceType;
        }
        else
        #endif
        {
            WaveVR.Device _device = WaveVR.Instance.getDeviceByType (this.deviceType);
            _connected = _device.connected;
            _type = _device.type;
        }

        PrintDebugLog ("onDeviceConnected() " + _type + " is " + (_connected ? "connected" : "disconnected") + ", left-handed? " + WaveVR_Controller.IsLeftHanded);

        if (connected != _connected)
        {
            connected = _connected;
        }

        if (connected)
        {
            if (controllerPrefab == null) onLoadController (_type);
        }
    }

    private const string VRACTIVITY_CLASSNAME = "com.htc.vr.unity.WVRUnityVRActivity";
    private const string FILEUTILS_CLASSNAME = "com.htc.vr.unity.FileUtils";

    private void onLoadController(WVR_DeviceType type)
    {
        controllerFileName = "";
        controllerModelFoler = "Controller/";
        genericControllerFileName = "Generic_";

        // Make up file name
        // Rule =
        // ControllerModel_TrackingMethod_CComponent_Hand
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            genericControllerFileName = "Generic_";

            genericControllerFileName += "MC_";

            if (WhichHand == ControllerHand.Controller_Right)
            {
                genericControllerFileName += "R";
            }
            else
            {
                genericControllerFileName += "L";
            }

            originalControllerPrefab = Resources.Load(controllerModelFoler + genericControllerFileName) as GameObject;
            if (originalControllerPrefab == null)
            {
                PrintDebugLog("Cant load generic controller model, Please check file under Resources/" + controllerModelFoler + genericControllerFileName + ".prefab is exist!");
            }
            else
            {
                PrintDebugLog(genericControllerFileName + " controller model is found!");
                SetControllerOptions(originalControllerPrefab);
                controllerPrefab = Instantiate(originalControllerPrefab);
                controllerPrefab.transform.parent = this.transform.parent;

                PrintDebugLog("Controller model loaded");
                ApplyIndicatorParameters();
                if (onControllerModelLoaded != null)
                {
                    PrintDebugLog("trigger delegate");
                    onControllerModelLoaded(controllerPrefab);
                }

                WaveVR_EventSystemControllerProvider.Instance.SetControllerModel(deviceType, controllerPrefab);
            }
            return;
        }
#endif
        string parameterName = "GetRenderModelName";
        IntPtr ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);

        IntPtr ptrResult = Marshal.AllocHGlobal(64);
        uint resultVertLength = 64;

        Interop.WVR_GetParameters(type, ptrParameterName, ptrResult, resultVertLength);
        string renderModelName = Marshal.PtrToStringAnsi(ptrResult);

        int deviceIndex = -1;
        parameterName = "backdoor_get_device_index";
        ptrParameterName = Marshal.StringToHGlobalAnsi(parameterName);
        IntPtr ptrResultDeviceIndex = Marshal.AllocHGlobal(2);
        Interop.WVR_GetParameters(type, ptrParameterName, ptrResultDeviceIndex, 2);

        int _out = 0;
        bool _ret = int.TryParse (Marshal.PtrToStringAnsi (ptrResultDeviceIndex), out _out);
        if (_ret)
            deviceIndex = _out;

        PrintInfoLog("get controller id from runtime is " + renderModelName);

        controllerFileName += renderModelName;
        controllerFileName += "_";

        if (ControllerComponents == CComponent.Multi_Component)
        {
            controllerFileName += "MC_";
        }
        else
        {
            controllerFileName += "OB_";
        }

        if (WhichHand == ControllerHand.Controller_Right)
        {
            controllerFileName += "R";
        }
        else
        {
            controllerFileName += "L";
        }

        PrintInfoLog("controller file name is " + controllerFileName);
        var found = false;

        if (adaptiveLoading)
        {
            if (Interop.WVR_GetWaveRuntimeVersion() >= 2)
            {
                PrintInfoLog("Start adaptive loading");
                // try to adaptive loading
                bool folderPrepared = true;
                bool loadAssetBundles = false;

                // 1. check if there are assets in private folder
                string renderModelFolderPath = Application.temporaryCachePath + "/";
                string renderModelUnzipFolder = renderModelFolderPath + renderModelName + "/";
                string renderModelNamePath = renderModelFolderPath + renderModelName + "/Unity";

                // delete old asset
                if (Directory.Exists(renderModelNamePath))
                {
                    try
                    {
                        Directory.Delete(renderModelNamePath, true);
                    }
                    catch (Exception e)
                    {
                        PrintInfoLog("delete folder exception: " + e);
                        folderPrepared = false;
                    }
                }

                // unzip assets from runtime
                if (folderPrepared)
                {
                    PrintWarningLog(renderModelName + " assets, start to deploy");
                    loadAssetBundles = deployZIPFile(deviceIndex, renderModelUnzipFolder);
                }

                // load model from runtime
                if (loadAssetBundles)
                {
                    string UnityVersion = Application.unityVersion;
                    PrintInfoLog("Application built by Unity version : " + UnityVersion);

                    int assetVersion = checkAssetBundlesVersion(UnityVersion);

                    if (assetVersion == 1)
                    {
                        renderModelNamePath += "/5.6";
                    } else if (assetVersion == 2)
                    {
                        renderModelNamePath += "/2017.3";
                    }

                    // try root path
                    found = tryLoadModelFromRuntime(renderModelNamePath, controllerFileName);

                    /*
                    if (!found)
                    {
                        // try sub folder
                        var dirs = Directory.GetDirectories(renderModelNamePath);

                        foreach (string dir in dirs)
                        {
                            string subRenderModelPath = dir;
                            PrintInfoLog("Try sub folder: " + subRenderModelPath);
                            string subAssetBundle = subRenderModelPath + "/" + "Unity";
                            PrintInfoLog("file is " + subAssetBundle);

                            found = tryLoadModelFromRuntime(subRenderModelPath, controllerFileName);

                            if (found)
                                break;
                        }
                    }
                    */

                    // try to load generic from runtime
                    if (!found)
                    {
                        PrintInfoLog("Try to load generic controller model from runtime");
                        string tmpGeneric = genericControllerFileName;
                        if (WhichHand == ControllerHand.Controller_Right)
                        {
                            tmpGeneric += "MC_R";
                        }
                        else
                        {
                            tmpGeneric += "MC_L";
                        }
                        found = tryLoadModelFromRuntime(renderModelNamePath, tmpGeneric);
                    }
                }

                // load model from package
                if (!found)
                {
                    PrintWarningLog("Can not find controller model from runtime");
                    originalControllerPrefab = Resources.Load(controllerModelFoler + controllerFileName) as GameObject;
                    if (originalControllerPrefab == null)
                    {
                        Log.e(LOG_TAG, "Can't load preferred controller model from package: " + controllerFileName);
                    }
                    else
                    {
                        PrintInfoLog(controllerFileName + " controller model is found!");
                        found = true;
                    }
                }
            } else
            {
                PrintInfoLog("API Level(2) is larger than Runtime Version (" + Interop.WVR_GetWaveRuntimeVersion() + ")");
            }
        } else
        {
            PrintInfoLog("Start package resource loading");
            if (Interop.WVR_GetWaveRuntimeVersion() >= 2) {
                // load resource from package
                originalControllerPrefab = Resources.Load(controllerModelFoler + controllerFileName) as GameObject;
                if (originalControllerPrefab == null)
                {
                    Log.e(LOG_TAG, "Can't load preferred controller model: " + controllerFileName);
                }
                else
                {
                    PrintInfoLog(controllerFileName + " controller model is found!");
                    found = true;
                }
            } else
            {
                PrintInfoLog("API Level(2) is larger than Runtime Version (" + Interop.WVR_GetWaveRuntimeVersion() + "), use generic controller model!");
            }
        }

        // Nothing exist, load generic
        if (!found)
        {
            PrintInfoLog(controllerFileName + " controller model is not found from runtime and package!");

            originalControllerPrefab = loadGenericControllerModelFromPackage(genericControllerFileName);
            if (originalControllerPrefab == null)
            {
                Log.e(LOG_TAG, "Can't load generic controller model, Please check file under Resources/" + controllerModelFoler + genericControllerFileName + ".prefab is exist!");
            }
            else
            {
                PrintInfoLog(genericControllerFileName + " controller model is found!");
                found = true;
            }
        }

        if (found && (originalControllerPrefab != null))
        {
            PrintInfoLog("Instantiate controller model");
            SetControllerOptions(originalControllerPrefab);
            controllerPrefab = Instantiate(originalControllerPrefab);
            controllerPrefab.transform.parent = this.transform.parent;
            ApplyIndicatorParameters();

            WaveVR_Utils.Event.Send(WaveVR_Utils.Event.CONTROLLER_MODEL_LOADED, deviceType, controllerPrefab);
            WaveVR_EventSystemControllerProvider.Instance.SetControllerModel(deviceType, controllerPrefab);
        }

        if (adaptiveLoading)
        {
            PrintInfoLog("loadedAssetBundle length: " + loadedAssetBundle.Count);
            foreach (AssetBundle tmpAB in loadedAssetBundle)
            {
                tmpAB.Unload(false);
            }
            loadedAssetBundle.Clear();
        }
        Marshal.FreeHGlobal(ptrParameterName);
        Marshal.FreeHGlobal(ptrResult);
    }

    private bool tryLoadModelFromRuntime(string renderModelNamePath, string modelName)
    {
        string renderModelAssetBundle = renderModelNamePath + "/" + "Unity";
        PrintInfoLog("tryLoadModelFromRuntime, path is " + renderModelAssetBundle);
        // clear unused asset bundles
        foreach (AssetBundle tmpAB in loadedAssetBundle)
        {
            tmpAB.Unload(false);
        }
        loadedAssetBundle.Clear();
        // check root folder
        AssetBundle ab = AssetBundle.LoadFromFile(renderModelAssetBundle);
        if (ab != null)
        {
            loadedAssetBundle.Add(ab);
            AssetBundleManifest abm = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            if (abm != null)
            {
                PrintDebugLog(renderModelAssetBundle + " loaded");
                string[] assetsName = abm.GetAllAssetBundles();

                for (int i = 0; i < assetsName.Length; i++)
                {
                    string subRMAsset = renderModelNamePath + "/" + assetsName[i];
                    ab = AssetBundle.LoadFromFile(subRMAsset);

                    loadedAssetBundle.Add(ab);
                    PrintDebugLog(subRMAsset + " loaded");
                }
                PrintInfoLog("All asset Bundles loaded, start loading asset");
                originalControllerPrefab = ab.LoadAsset<GameObject>(modelName);

                if (originalControllerPrefab != null)
                {
                    PrintInfoLog("adaptive load controller model " + modelName + " success");
                    return true;
                }
            }
            else
            {
                PrintWarningLog("Can't find AssetBundleManifest!!");
            }
        }
        else
        {
            PrintWarningLog("Load " + renderModelAssetBundle + " failed");
        }
        PrintInfoLog("adaptive load controller model " + modelName + " from " + renderModelNamePath + " fail!");
        return false;
    }

    private bool deployZIPFile(int deviceIndex, string renderModelUnzipFolder)
    {
        AndroidJavaClass ajc = new AndroidJavaClass(VRACTIVITY_CLASSNAME);

        if (ajc == null || deviceIndex == -1)
        {
            PrintWarningLog("AndroidJavaClass vractivity is null, deviceIndex" + deviceIndex);
            return false;
        }
        else
        {
            AndroidJavaObject activity = ajc.CallStatic<AndroidJavaObject>("getInstance");
            if (activity != null)
            {
                AndroidJavaObject afd = activity.Call<AndroidJavaObject>("getControllerModelFileDescriptor", deviceIndex);
                if (afd != null)
                {
                    AndroidJavaObject fileUtisObject = new AndroidJavaObject(FILEUTILS_CLASSNAME, activity, afd);

                    if (fileUtisObject != null)
                    {
                        bool retUnzip = fileUtisObject.Call<bool>("doUnZIPAndDeploy", renderModelUnzipFolder);

                        if (!retUnzip)
                        {
                            PrintWarningLog("doUnZIPAndDeploy failed");
                        }
                        else
                        {
                            PrintInfoLog("doUnZIPAndDeploy success");
                            return true;
                        }
                    }
                    else
                    {
                        PrintWarningLog("fileUtisObject is null");
                    }
                }
                else
                {
                    PrintWarningLog("get fd failed");
                }
            }
            else
            {
                PrintWarningLog("getInstance failed");
            }
        }

        return false;
    }

    private int checkAssetBundlesVersion(string version)
    {
        if (version.StartsWith("5.6.3") || version.StartsWith("5.6.4") || version.StartsWith("5.6.5") || version.StartsWith("5.6.6") || version.StartsWith("2017.1") || version.StartsWith("2017.2"))
        {
            return 1;
        }

        if (version.StartsWith("2017.3") || version.StartsWith("2017.4") || version.StartsWith("2018.1"))
        {
            return 2;
        }

        return 0;
    }

    private GameObject loadGenericControllerModelFromPackage(string tmpGeneric)
    {
        if (WhichHand == ControllerHand.Controller_Right)
        {
            tmpGeneric += "MC_R";
        }
        else
        {
            tmpGeneric += "MC_L";
        }
        Log.w(LOG_TAG, "Can't find preferred controller model, load generic controller : " + tmpGeneric);
        if (adaptiveLoading) PrintInfoLog("Please update controller models from device service to have better experience!");
        return Resources.Load(controllerModelFoler + tmpGeneric) as GameObject;
    }

    private void SetControllerOptions(GameObject controller_prefab)
    {
        WaveVR_PoseTrackerManager _ptm = controller_prefab.GetComponent<WaveVR_PoseTrackerManager> ();
        if (_ptm != null)
        {
            _ptm.TrackPosition = TrackPosition;
            _ptm.SimulationOption = SimulationOption;
            _ptm.FollowHead = FollowHead;
            _ptm.TrackRotation = TrackRotation;
        }
    }

    // Update is called once per frame
    void Update () {
    }

    private bool checkConnection()
    {
        #if UNITY_EDITOR
        if (Application.isEditor)
        {
            return false;
        } else
        #endif
        {
            WaveVR.Device _device = WaveVR.Instance.getDeviceByType (this.deviceType);
            return _device.connected;
        }
    }

    private void ApplyIndicatorParameters()
    {
        if (!overwriteIndicatorSettings) return;
        WaveVR_ShowIndicator si = null;

        var ch = controllerPrefab.transform.childCount;
        bool found = false;

        for (int i = 0; i < ch; i++)
        {
            PrintInfoLog(controllerPrefab.transform.GetChild(i).gameObject.name);

            GameObject CM = controllerPrefab.transform.GetChild(i).gameObject;

            si = CM.GetComponentInChildren<WaveVR_ShowIndicator>();

            if (si != null)
            {
                found = true;
                break;
            }
        }


        if (found)
        {
            PrintInfoLog("WaveVR_ControllerLoader forced update WaveVR_ShowIndicator parameter!");
            si.showIndicator = this.showIndicator;

            if (showIndicator != true)
            {
                PrintInfoLog("WaveVR_ControllerLoader forced don't show WaveVR_ShowIndicator!");
                return;
            }
            si.showIndicator = this.showIndicator;
            si.showIndicatorAngle = showIndicatorAngle;
            si.hideIndicatorByRoll = hideIndicatorByRoll;
            si.lineColor = lineColor;
            si.lineEndWidth = lineEndWidth;
            si.lineStartWidth = lineStartWidth;
            si.lineLength = lineLength;
            si.textCharacterSize = textCharacterSize;
            si.zhCharactarSize = zhCharactarSize;
            si.textColor = textColor;
            si.textFontSize = textFontSize;

            if (buttonIndicationList.Count == 0)
            {
                PrintInfoLog("WaveVR_ControllerLoader uses controller model default button indication!");
                return;
            }
            PrintInfoLog("WaveVR_ControllerLoader uses customized button indication!");
            si.buttonIndicationList.Clear();
            foreach (ButtonIndication bi in buttonIndicationList)
            {
                PrintInfoLog("indication: "+ bi.indicationText);
                PrintInfoLog("alignment: " + bi.alignment);
                PrintInfoLog("offset: " + bi.indicationOffset);
                PrintInfoLog("keyType: " + bi.keyType);
                PrintInfoLog("followRotation: " + bi.followButtonRotation);

                si.buttonIndicationList.Add(bi);
            }
        } else
        {
            PrintInfoLog("Controller model doesn't support button indication feature!");
        }
    }
}
