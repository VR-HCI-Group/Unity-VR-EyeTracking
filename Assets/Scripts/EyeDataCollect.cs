using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.IO;
using ViveSR.anipal.Eye;
public class EyeDataCollect : MonoBehaviour
{
    public Camera cam;//vr相机
    public int LengthOfRay = 25;
    // [SerializeField] private LineRenderer GazeRayRenderer;
    private static EyeData_v2 eyeData = new EyeData_v2();
    private bool eye_callback_registered = false;
    //此处为增加的变量，定义好需要采集的数据，后续如果需要可以继续添加
    //瞳孔数据
    private float pupilDiameterLeft, pupilDiameterRight;
    private Vector2 pupilPositionLeft, pupilPositionRight;
    //开合度数据
    private float eyeOpenLeft, eyeOpenRight;
    //注视数据
    public Vector2 Angular = new Vector2();//角度
    public Vector2 Point = new Vector2();//坐标
    Vector3 normalizedGazeDirection = new Vector3(0.0f, 0.0f, 1.0f);
    // Start is called before the first frame update
    //保存数据
    private string EyeDataFolder="./GazeRecordings/" ;
    public string filename;
    string separator = "\t";
    void Start()
    {
        string datetimeString = System.DateTime.Now.ToString("yy-MM-dd-HH-mm-ss");
        if (!Directory.Exists(EyeDataFolder))
        {
            Directory.CreateDirectory(EyeDataFolder);
        }
        filename = EyeDataFolder + "GazeData" + datetimeString+".txt";//这里是把数据存在该目录下的txt文件中
        string titlehead = "time" + separator + "GazeDirectionx" + separator + "GazeDirectiony" + separator + "GazePositionx" + separator + "GazePositiony" +
                           separator + "leftOpenness" + separator + "rightOpenness" + separator + "pupilDiameterLeft" +separator + "pupilDiameterRight" 
                           +separator + "pupilPositionLeft" +separator + "pupilPositionRight" +"\n";
        System.IO.File.AppendAllText(filename, titlehead);
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }
        // Assert.IsNotNull(GazeRayRenderer);
    }

    // Update is called once per frame
    void Update()
    {
        System.TimeSpan timeSpan = System.DateTime.Now - new System.DateTime(1970, 1, 1, 0, 0, 0);
        long timeStamp = (long)timeSpan.TotalMilliseconds - 8 * 60 * 60 * 1000;
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;

        if (eye_callback_registered)
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData)) { }
            else return;
        }
        else
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
            else return;
        }
        
        Vector3 GazeDirectionCombined = cam.transform.TransformDirection(GazeDirectionCombinedLocal);
        // GazeRayRenderer.SetPosition(0, Camera.main.transform.position - Camera.main.transform.up * 0.05f);
        // GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
        //pupil
        //pupil diameter 瞳孔的直径
        pupilDiameterLeft = eyeData.verbose_data.left.pupil_diameter_mm;
        pupilDiameterRight = eyeData.verbose_data.right.pupil_diameter_mm;
        //pupil positions 瞳孔位置
        //pupil_position_in_sensor_area手册里写的是The normalized position of a pupil in [0,1]，给坐标归一化了
        pupilPositionLeft = eyeData.verbose_data.left.pupil_position_in_sensor_area;
        pupilPositionRight = eyeData.verbose_data.right.pupil_position_in_sensor_area;
        //eye open 睁眼
        //eye_openness手册里写的是A value representing how open the eye is,也就是睁眼程度，从输出来看是在0-1之间，也归一化了
        eyeOpenLeft = eyeData.verbose_data.left.eye_openness;
        eyeOpenRight = eyeData.verbose_data.right.eye_openness;
        //gaze
        //注视方向与位置
        normalizedGazeDirection = GazeDirectionCombinedLocal.normalized;//local
        Angular = GetAngularFromDirection(normalizedGazeDirection);
        Point = GetPoint(GazeDirectionCombinedLocal);
        
        string info_gaze = timeStamp.ToString()+ separator + Angular[0].ToString("f2") + separator + Angular[1].ToString("f2") + separator 
                           + Point[0].ToString("f2") + separator + Point[1].ToString("f2") + separator + eyeOpenLeft.ToString("f2") + separator 
                           + eyeOpenRight.ToString("f2") + pupilDiameterLeft.ToString("f2") + pupilDiameterRight.ToString("f2") 
                           + pupilPositionLeft.ToString("f2") + pupilPositionRight.ToString("f2")+ "\n";
        Debug.Log("info_gaze" + info_gaze);
        System.IO.File.AppendAllText(filename, info_gaze);
    }
    private Vector2 GetAngularFromDirection(Vector3 direction)
    {
        float longitudeOffset = Mathf.PI / 2;
        float x = direction.x;
        float y = direction.y;
        float z = direction.z;
        float xz = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(z, 2));
        float gazeLongitude = (Mathf.Atan2(x, z) ) / Mathf.PI * 180;
        float gazeLatitude = Mathf.Atan2(y, xz) / Mathf.PI * 180;

        return new Vector2(gazeLongitude, gazeLatitude);
    }
    private Vector2 GetPoint(Vector3 GazeDirectionCombinedLocal)
    {
        float gaze_x = GazeDirectionCombinedLocal.x;
        float gaze_y = GazeDirectionCombinedLocal.y;
        float gaze_z = GazeDirectionCombinedLocal.z;
        float tanHalfVerticalFov = Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2.0f);
        float tanHalfHorizontalFov = tanHalfVerticalFov * cam.aspect;
        Vector2 gazeData = Vector2.zero;
        gazeData.x = (gaze_x / gaze_z) / tanHalfHorizontalFov;
        gazeData.y = (gaze_y / gaze_z) / tanHalfVerticalFov;
        gazeData = (gazeData + Vector2.one) / 2.0f;
        return gazeData;
    }
    private void Release()
    {
        if (eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
    }
    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;
    }
}
