using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace scch
{
    public static class config
    {
        public static int VERSION = 2;

        public static int Version;
      //  public static int OffsetVersion;
        public static bool triggerbot = false;
        public static bool bshowMenu = true;
        public static bool bDrawBox = false;
        public static bool bDrawLine = false;
        public static bool bDrawHealth = false;
        public static bool bAimbot = false;
        public static bool showname = false;
        public static bool debug = false;
        public static bool showdisc = false;
        public static bool bAimbotKey = true;
        public static bool bDrowBone = false;
        public static bool bAimTrigger = true;
        public static bool bGlow = false;
        public static int fSmooth = 5;


        public static int color_espR = 255;
        public static int color_espG = 0;
        public static int color_espB = 0;

        public static int iFov = 20;
        public static int delany = 10;
        public static int downdelany = 5;







        public static string ConfigFileName = "cs.json";
        public static string OffsetFileName = "csgo_panorama.json";
        public static string JsonDllName = "Newtonsoft.Json.dll";
        public static string BspDllName = "bsp.dll";
        public static string OffsetUrl = "https://raw.githubusercontent.com/nrife/csoffset/master/csgo_panorama.json";
        public static void InitConfig()
        {
            //检查文件
            if (!File.Exists(JsonDllName))
            {
                byte[] Save = global::scch.Properties.Resources.Newtonsoft_Json;//资源文件
                FileStream fsObj = new FileStream(JsonDllName, FileMode.CreateNew);
                fsObj.Write(Save, 0, Save.Length);
                fsObj.Close();
                fsObj.Dispose();
            }

            if (!File.Exists(BspDllName))
            {
                byte[] Save = global::scch.Properties.Resources.bsp;//资源文件
                FileStream fsObj = new FileStream(BspDllName, FileMode.CreateNew);
                fsObj.Write(Save, 0, Save.Length);
                fsObj.Close();
                fsObj.Dispose();
            }

       //     if (File.Exists(OffsetFileName)) 
        //    {
        //        ReadOffset();
        //        CheckUpdate();
        //    }
        //    else
        //    {
         //       string file;
         //       GetNetworkFileString(OffsetUrl,out file);
         //       byte[] Save = Encoding.Default.GetBytes(file);
         //       FileStream fsObj = new FileStream(OffsetFileName, FileMode.CreateNew);
          //      fsObj.Write(Save, 0, Save.Length);
         //       fsObj.Close();
         //       fsObj.Dispose();
        //        ReadOffset();
        //    }
            if (File.Exists(ConfigFileName))
            {
                ReadConfiguer();
            }
            else
            {

            }

            ReadOffset(GetNetworkFileString(OffsetUrl));
        }
        //保存配置
        public static void SaveConfiguer()
        {
            StringWriter sw = new StringWriter();
            JsonWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("triggerbot");
            writer.WriteValue(triggerbot.ToString());
            writer.WritePropertyName("bshowMenu");
            writer.WriteValue(bshowMenu.ToString());
            writer.WritePropertyName("bDrawBox");
            writer.WriteValue(bDrawBox.ToString());
            writer.WritePropertyName("bDrawLine");
            writer.WriteValue(bDrawLine.ToString());
            writer.WritePropertyName("bDrawHealth");
            writer.WriteValue(bDrawHealth.ToString());
            writer.WritePropertyName("bAimbot");
            writer.WriteValue(bAimbot.ToString());
            writer.WritePropertyName("showname");
            writer.WriteValue(showname.ToString());
            writer.WritePropertyName("debug");
            writer.WriteValue(debug.ToString());
            writer.WritePropertyName("showdisc");
            writer.WriteValue(showdisc.ToString());
            writer.WritePropertyName("bAimbotKey");
            writer.WriteValue(bAimbotKey.ToString());
            writer.WritePropertyName("bDrowBone");
            writer.WriteValue(bDrowBone.ToString());
            writer.WritePropertyName("bAimTrigger");
            writer.WriteValue(bAimTrigger.ToString());
            writer.WritePropertyName("fSmooth");
            writer.WriteValue(fSmooth.ToString());
            writer.WritePropertyName("color_espR");
            writer.WriteValue(color_espR.ToString());
            writer.WritePropertyName("color_espG");
            writer.WriteValue(color_espG.ToString());
            writer.WritePropertyName("color_espB");
            writer.WriteValue(color_espB.ToString());
            writer.WritePropertyName("iFov");
            writer.WriteValue(iFov.ToString());
            writer.WritePropertyName("delany");
            writer.WriteValue(delany.ToString());
            writer.WritePropertyName("downdelany");
            writer.WriteValue(downdelany.ToString());
            writer.WritePropertyName("iAimBone");
            writer.WriteValue(Engine.iAimBone.ToString());
            writer.WritePropertyName("bGlow");
            writer.WriteValue(bGlow.ToString());

            writer.WriteEndObject();
            writer.Flush();

            string jsonText = sw.GetStringBuilder().ToString();

            byte[] Save = Encoding.Default.GetBytes(jsonText);
            FileStream fsObj = new FileStream(ConfigFileName, FileMode.Create);
            fsObj.Write(Save, 0, Save.Length);
            fsObj.Close();
            fsObj.Dispose();
        }
        //检查更新
        //public static void CheckUpdate()
        //{
          //  string JsonText=GetNetworkFileString(OffsetUrl);

            //     JObject jo = (JObject)JsonConvert.DeserializeObject(JsonText);
            //     if (OffsetVersion < (Convert.ToInt32(jo["Version"].ToString())))
            //     {
            //          byte[] Save = Encoding.Default.GetBytes(JsonText);
            //          FileStream fsObj = new FileStream(OffsetFileName, FileMode.Create);
            //          fsObj.Write(Save, 0, Save.Length);
            //          fsObj.Close();
            //          fsObj.Dispose();
            //          ReadOffset();
            //       }

      //  }
        //读取配置
        public static void ReadConfiguer()
        {
            string JsonText;
            byte[] buffer = new byte[1024 * 1024 * 2];
            FileStream file = new FileStream(ConfigFileName, FileMode.Open);
            int r = file.Read(buffer, 0, buffer.Length);
            JsonText = Encoding.Default.GetString(buffer, 0, r);//读取2M的文件
            file.Close();
            file.Dispose();


            JObject jo = (JObject)JsonConvert.DeserializeObject(JsonText);

            triggerbot = Convert.ToBoolean(jo["triggerbot"].ToString());
            bshowMenu = Convert.ToBoolean(jo["bshowMenu"].ToString());
            bDrawBox = Convert.ToBoolean(jo["bDrawBox"].ToString());
            bDrawLine = Convert.ToBoolean(jo["bDrawLine"].ToString());
            bDrawHealth = Convert.ToBoolean(jo["bDrawHealth"].ToString());
            bAimbot = Convert.ToBoolean(jo["bAimbot"].ToString());
            showname = Convert.ToBoolean(jo["showname"].ToString());
            debug = Convert.ToBoolean(jo["debug"].ToString());
            showdisc = Convert.ToBoolean(jo["showdisc"].ToString());
            bAimbotKey = Convert.ToBoolean(jo["bAimbotKey"].ToString());
            bDrowBone = Convert.ToBoolean(jo["bDrowBone"].ToString());
            bAimTrigger = Convert.ToBoolean(jo["bAimTrigger"].ToString());
            fSmooth = Convert.ToInt32(jo["fSmooth"].ToString());
            color_espR = Convert.ToInt32(jo["color_espR"].ToString());
            color_espG = Convert.ToInt32(jo["color_espG"].ToString());
            color_espB = Convert.ToInt32(jo["color_espB"].ToString());
            iFov = Convert.ToInt32(jo["iFov"].ToString());
            delany = Convert.ToInt32(jo["delany"].ToString());
            downdelany = Convert.ToInt32(jo["downdelany"].ToString());
            Engine.iAimBone= Convert.ToInt32(jo["iAimBone"].ToString());
            bGlow = Convert.ToBoolean(jo["bGlow"].ToString());
        }

        // 读取偏移
        public static void ReadOffset(string JsonText)
        {
      //      string JsonText;
      //      byte[] buffer = new byte[1024 * 1024 * 2];
      //      FileStream file = new FileStream(OffsetFileName, FileMode.Open);
      //      int r = file.Read(buffer, 0, buffer.Length);
      //      JsonText = Encoding.Default.GetString(buffer, 0, r);//读取2M的文件
     //       file.Close();
      //      file.Dispose();


            JObject jo = (JObject)JsonConvert.DeserializeObject(JsonText);

            //OffsetVersion = Convert.ToInt32(jo["OffsetVersion"].ToString());
             Version = Convert.ToInt32(jo["Version"].ToString());
             Offsets.dwClientState = Convert.ToInt32(jo["signatures"]["dwClientState"].ToString());
             Offsets.dwClientState_ViewAngles = Convert.ToInt32(jo["signatures"]["dwClientState_ViewAngles"].ToString());
             Offsets.dwRadarBase = Convert.ToInt32(jo["signatures"]["dwRadarBase"].ToString());
             Offsets.dwSensitivityPtr = Convert.ToInt32(jo["signatures"]["dwSensitivityPtr"].ToString());
             Offsets.dw_EntityList = Convert.ToInt32(jo["signatures"]["dwEntityList"].ToString());
             Offsets.dw_LocalPlayer = Convert.ToInt32(jo["signatures"]["dwLocalPlayer"].ToString());
             Offsets.dw_ViewMatrix = Convert.ToInt32(jo["signatures"]["dwViewMatrix"].ToString());
             Offsets.dwClientState_MapDirectory = Convert.ToInt32(jo["signatures"]["dwClientState_MapDirectory"].ToString());
             Offsets.dwClientState_Map = Convert.ToInt32(jo["signatures"]["dwClientState_Map"].ToString());
             Offsets.dwClientState_State = Convert.ToInt32(jo["signatures"]["dwClientState_State"].ToString());
             Offsets.dwGameDir = Convert.ToInt32(jo["signatures"]["dwGameDir"].ToString());
            Offsets.dwGlowObjectManager = Convert.ToInt32(jo["signatures"]["dwGlowObjectManager"].ToString());
            Offsets.m_dwBoneMatrix = Convert.ToInt32(jo["netvars"]["m_dwBoneMatrix"].ToString());
             Offsets.m_iCrosshairId = Convert.ToInt32(jo["netvars"]["m_iCrosshairId"].ToString());
             Offsets.m_iFOVStart = Convert.ToInt32(jo["netvars"]["m_iFOVStart"].ToString());
             Offsets.m_vecViewOffset = Convert.ToInt32(jo["netvars"]["m_vecViewOffset"].ToString());
             Offsets.m_viewPunchAngle= Convert.ToInt32(jo["netvars"]["m_viewPunchAngle"].ToString());
             Offsets.oHealth = Convert.ToInt32(jo["netvars"]["m_iHealth"].ToString());
             Offsets.oTeam = Convert.ToInt32(jo["netvars"]["m_iTeamNum"].ToString());
             Offsets.oVecOrigin = Convert.ToInt32(jo["netvars"]["m_vecOrigin"].ToString());
            Offsets.m_iGlowIndex = Convert.ToInt32(jo["netvars"]["m_iGlowIndex"].ToString());





        }

        // 获取网页保存为字符串
        public static string GetNetworkFileString(string url)
        {
            string str;
            int size;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            byte[] bArr = new byte[1024];
            str = "";
            while ((size = responseStream.Read(bArr, 0, (int)bArr.Length)) > 0)
            {
                str += Encoding.Default.GetString(bArr, 0, size);
            }
            response.Close();
            responseStream.Close(); //关闭文件
            return str;
        }

    }
}
