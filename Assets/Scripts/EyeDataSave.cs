using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using ViveSR.anipal.Eye;
using System;
using System.Xml.Linq;
using UnityEditor;

public class EyeDataSave : MonoBehaviour
{
    public int LengthOfRay = 50;
    [SerializeField] private LineRenderer GazeRayRenderer;
    private static EyeData_v2 eyeData = new EyeData_v2();
    private bool eye_callback_registered = false;
    RaycastHit hit;

    private Vector3 iniPos;
    private Vector3 iniVel;
    private Vector3 nowPos;
    private Vector3 nowVel;

    //private float nowSpeed;
    private Vector3 nowAcc;
    //private float nowAccRate;

    private Vector3 iniAng;
    private Vector3 iniPal;
    private Vector3 nowAng;
    private Vector3 nowPal;

    // private float nowAngRate;
    private Vector3 nowAngleAcc;
    //private float nowAngleAccRate;

    private float conDis;

    private Vector3 iniGazePos;
    private Vector3 nowGazePos;
    private Vector3 iniGazePosVel;
    private Vector3 nowGazePosVel;
    private Vector3 nowGazePosAcc;

    private Vector2 nowGazePos2D;

    private StreamWriter sw;

    public float iniTime;

    private Matrix4x4 vpMatrix;

    private float farDis;

    private float pupilDiameterLeft, pupilDiameterRight;
    public Vector2 pupilPositionLeft, pupilPositionRight;
    private float eyeOpenLeft, eyeOpenRight;

    // private int tris;

    private void Start()
    {
        string path = "Eye_" + DateTime.UtcNow.ToString("dd_mm_yyyy_hh_mm_ss") + ".csv";

        sw = new StreamWriter(path, false, Encoding.UTF8);
        sw.WriteLine(
            "time, gaze.x, gaze.y, gaze.z, gaze2D.x, gaze2D.y, conDis, pdl, pdr, ppl.x, ppl.y, ppr.x, ppr.y, opl, opr");

        iniPos = transform.position;
        iniVel = new Vector3(0, 0, 0);
        //nowSpeed = 0;
        nowAcc = new Vector3(0, 0, 0);
        //nowAccRate = 0;

        iniAng = transform.eulerAngles;
        iniPal = new Vector3(0, 0, 0);
        //nowAngRate = 0;
        nowAngleAcc = new Vector3(0, 0, 0);
        //nowAngleAccRate = 0;

        iniGazePos = new Vector3(0, 0, 1);
        iniGazePosVel = new Vector3(0, 0, 0);
        nowGazePosVel = new Vector3(0, 0, 0);
        nowGazePosAcc = new Vector3(0, 0, 0);

        conDis = 0;

        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        Assert.IsNotNull(GazeRayRenderer);

        iniTime = Time.time;

        //Matrix4x4 viewMatrix = mc.worldToCameraMatrix;
        //Matrix4x4 projMatrix = mc.projectionMatrix;
        //vpMatrix = projMatrix * viewMatrix;

        farDis = Camera.main.farClipPlane;
    }

    private void Update()
    {
        //Debug.Log(eyeOpenLeft + "  " + eyeOpenRight);
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
        {
            print("SR no!");
        }
        else
        {
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
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData))
                {
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData))
                {
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal, eyeData))
                {
                }
                else return;
            }
            else
            {
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal))
                {
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal))
                {
                }
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal))
                {
                }
                else return;
            }

            Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);

            Vector3 targetWorldPos = Camera.main.transform.position + GazeDirectionCombined;
            Vector4 targetWorldPos4 = new Vector4(targetWorldPos.x, targetWorldPos.y, targetWorldPos.z, 1);
            Matrix4x4 viewMatrix = Camera.main.worldToCameraMatrix;
            Matrix4x4 projMatrix = Camera.main.projectionMatrix;
            vpMatrix = projMatrix * viewMatrix;
            Vector4 screenPos4 = vpMatrix * targetWorldPos4;
            nowGazePos2D = new Vector2(screenPos4.x / screenPos4.w, screenPos4.y / screenPos4.w);

            Vector3 originPos = Camera.main.transform.position;

            GazeRayRenderer.SetPosition(0, originPos - Camera.main.transform.up * 0.05f);
            GazeRayRenderer.SetPosition(1, Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);

            //Ray ray = Camera.main.ScreenPointToRay(Camera.main.transform.position + GazeDirectionCombined * LengthOfRay);
            Ray ray = new Ray(originPos, GazeDirectionCombined);

            if (Physics.Raycast(ray, out hit)) //如果碰撞检测到物体
            {
                //Debug.Log(hit.point);//打印鼠标点击到的物体名称
                //GazeRayRenderer.SetPosition(1, hit.point);
                conDis = Vector3.Distance(hit.point, originPos);
            }
            else
            {
                conDis = farDis;
            }

            //GZ
            float sqrt = MathF.Sqrt(MathF.Pow(GazeDirectionCombinedLocal.x, 2) + MathF.Pow(GazeDirectionCombinedLocal.y, 2) +
                                    MathF.Pow(GazeDirectionCombinedLocal.z, 2));
            nowGazePos = new Vector3(GazeDirectionCombinedLocal.x / sqrt, GazeDirectionCombinedLocal.y / sqrt, GazeDirectionCombinedLocal.z / sqrt);
            Vector3 gazeDiff = nowGazePos - iniGazePos;
            iniGazePos = nowGazePos;

            nowGazePosVel = gazeDiff / Time.deltaTime;
            //nowGazeRate = nowGazePosVel.magnitude;
            Vector3 gpVelDiff = nowGazePosVel - iniGazePosVel;
            iniGazePosVel = nowGazePosVel;

            nowGazePosAcc = gpVelDiff / Time.deltaTime;
            //nowGazeAccRate = nowAngleAcc.magnitude;
        }

        //LG
        nowPos = transform.position;
        Vector3 posDiff = nowPos - iniPos;
        iniPos = nowPos;

        nowVel = posDiff / Time.deltaTime;
        //nowSpeed = nowVel.magnitude;
        Vector3 velDiff = nowVel - iniVel;
        iniVel = nowVel;

        nowAcc = velDiff / Time.deltaTime;
        //nowAccRate = nowAcc.magnitude;

        //RT
        nowAng = transform.eulerAngles;
        Vector3 angDiff = nowAng - iniAng;
        iniAng = nowAng;

        nowPal = angDiff / Time.deltaTime;
        //nowAngRate = nowPal.magnitude;
        Vector3 palDiff = nowPal - iniPal;
        iniPal = nowPal;

        nowAngleAcc = palDiff / Time.deltaTime;
        //nowAngleAccRate = nowAngleAcc.magnitude;

        float nowTime = Time.time;
        DateTime nowUtcTime = DateTime.UtcNow;
        long nowTimeStan = ToUnixTimeByDateTime(nowUtcTime);
        float fr = 1 / Time.deltaTime;
        // print(fr);
        //print(nowTime);
        //string theText = string.Format("当前速度：{0}", nowVel);

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

        // tris = UnityStats.triangles;

        long timeget = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        string timenow = timeget.ToString() + "\t";

        //Debug.Log(nowGazePos);

        sw.WriteLine(
            $"{nowTimeStan}, {nowGazePos.x}, {nowGazePos.y}, {nowGazePos.z}, {nowGazePos2D.x}, {nowGazePos2D.y}, {conDis}, {pupilDiameterLeft}, " +
            $"{pupilDiameterRight}, {pupilPositionLeft.x}, {pupilPositionLeft.y}, {pupilPositionRight.x}, {pupilPositionRight.y}, {eyeOpenLeft}, {eyeOpenRight}");
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

    public static long ToUnixTimeByDateTime(DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
    }

    private void OnDisable()
    {
        sw.Close();
    }
}
