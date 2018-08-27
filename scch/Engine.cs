using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace scch
{
    public static class Engine
    {
        [DllImport("bsp.dll", EntryPoint = "bsp_parse_map", CharSet = CharSet.Ansi)]
        static extern bool bsp_parse_map([MarshalAs(UnmanagedType.LPStr)]string dir , [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport("bsp.dll", EntryPoint = "bsp_is_visible")]
        static extern bool bsp_is_visible(Vector3 origin, Vector3 tagger);


        public static int iAimBone = 0;
        public static int[] iAimBoneId = new int[4] { 8, 7, 5, 0 };
        public static int MAX_PLAYERS = 32;
        public static HexMem mem;
        public static long CLIENT;
        public static long ENGINE;
        public static Enemy[] enemy = new Enemy[32];
        public static RECT rect;

        public struct RECT
        {
            public int left, top, right, bottom;
        }

        public struct LocalPlayer
        {
            public static WorldToScreenMatrix_t WorldToScreenMatrix = new WorldToScreenMatrix_t();

            public static int BASE;
            public static int health;
            public static int team;
            public static int FOV;
            public static int BoneStart;
            public static int state;
            public static bool bDormant;
            public static bool isload=false;
            public static Vec3 position;
            public static Vec2 angles;
            public static Vec2 punchAngle;
            public static Vec2 ViewPunchAngle;
            public static Vec3 head;
            public static Vec3 vecViewOffset;
            public static string mapname="0";
            public static string oldmapname="1";

            public static void read()
            {
                BASE = mem.ReadInt32(Offsets.dw_LocalPlayer);
              //  health = mem.ReadInt32(BASE + Offsets.oHealth);
                team = mem.ReadInt32(BASE+Offsets.oTeam);
                bDormant = mem.ReadBool(BASE + Offsets.oDormant);
                position = mem.ReadVec3(BASE + Offsets.oVecOrigin);
                angles = mem.ReadVec2(Offsets.dwClientState + Offsets.dwClientState_ViewAngles);
                FOV = mem.ReadInt32(BASE + Offsets.m_iFOVStart);
                readW2S();
                vecViewOffset = mem.ReadVec3(BASE + Offsets.m_vecViewOffset);
                position.z += vecViewOffset.z;
                //    BoneStart = Engine.mem.ReadInt32(BASE + Offsets.m_dwBoneMatrix);
                //   head.x = mem.ReadFloat(BoneStart + 0x30 * 8 + 0xc); //ReadProcessMemory(hProcess, (LPVOID)(bbone + (0x30 * 10/*yourbone*/) + 0xC), &bone.x, sizeof(float), NULL);
                //    head.y = mem.ReadFloat(BoneStart + 0x30 * 8 + 0x1c);//ReadProcessMemory(hProcess, (LPVOID)(bbone + (0x30 * 10/*yourbone*/) + 0x1C), &bone.y, sizeof(float), NULL);
                //    head.z = mem.ReadFloat(BoneStart + 0x30 * 8 + 0x2c);//ReadProcessMemory(hProcess, (LPVOID)(bbone + (0x30 * 10/*yourbone*/) + 0x2C), &bone.z, sizeof(float), NULL);
                state = mem.ReadInt32(Offsets.dwClientState_State);
                punchAngle = mem.ReadVec2(BASE + Offsets.m_viewPunchAngle);
                ViewPunchAngle = mem.ReadVec2(BASE + Offsets.m_viewPunchAngle + 0xC);
                if (!isload && state == 6)
                {
                    mapname = mem.ReadStringAsciiLine(Offsets.dwClientState + Offsets.dwClientState_MapDirectory, 80);
                    //载入地图
                    if (oldmapname == mapname)
                    {
                        isload = true;
                    }
                    else
                    {
                        if (!bsp_parse_map(Offsets.path, mapname))
                           Application.Exit();
                        oldmapname = mapname;
                        isload = true;
                    }
                }
                else if (isload && state != 6)
                {
                    isload = false;
                }
            }


            public class WorldToScreenMatrix_t
            {
                public float[,] flMatrix = new float[4, 4];
            }
            public static void readW2S()
            {
                byte[] byteArray = new byte[64];
                byteArray = mem.ReadByteArray((Offsets.dw_ViewMatrix), 64);//reads 64 bytes at viewmatrix address. (float[4,4] = 64 bytes)
                int row = 0;
                int column = 0;
                int testVariable = 0;
                for (; row < 4; row++)
                {
                    for (; column < 4; column++)
                    {
                        WorldToScreenMatrix.flMatrix[row, column] = System.BitConverter.ToSingle(byteArray, testVariable);
                        testVariable += 4;
                    }
                    column = 0;
                }
            }
        }

        public struct Enemy
        {
            public long BASE;
            public int health;
            public int team;
            public int BoneStart;
            public bool bDormant;
            public int id;
            public string name;
            public Vec3 position;
            public Vec3 positionHeight; //this is used to calculate the correct height of the boxes later.
            public Vec2 W2S_Feet;
            public Vec2 W2S_Height;
            public Vec3 head;
            public Vec3 vecViewOffset;
            public float box_width;
            public float box_height;
            public float box_x;
            public float box_y;
            public bool isEnemy;
            public float distance;
            public bool isVisible;
            public int iGlowIndex;

            public void read(int i)
            {
                BASE = Engine.mem.ReadInt32(Offsets.dw_EntityList + i* 0x10);
                health = mem.ReadInt32(BASE + Offsets.oHealth);
                team = mem.ReadInt32(BASE + Offsets.oTeam);
                vecViewOffset = mem.ReadVec3(BASE + Offsets.m_vecViewOffset);
                if (team != LocalPlayer.team)
                    isEnemy = true;
                else
                    isEnemy = false;
                bDormant = mem.ReadBool(BASE + Offsets.oDormant);
                position = mem.ReadVec3(BASE + Offsets.oVecOrigin);
                if (!LocalPlayer.bDormant && !bDormant && health > 0)
                    distance = (float)Math.Sqrt(Math.Pow(position.x - LocalPlayer.position.x, 2) + Math.Pow(position.y - LocalPlayer.position.y, 2) + Math.Pow(position.z - LocalPlayer.position.z, 2)) * 0.0254f; //https://github.com/ValveSoftware/source-sdk-2013/blob/master/mp/src/public/vphysics_interface.h#L40 and  https://developer.valvesoftware.com/wiki/Dimensions
                positionHeight = position;
                positionHeight.z += vecViewOffset.z;
                id = i + 1;
                
                BoneStart = Engine.mem.ReadInt32(BASE + Offsets.m_dwBoneMatrix);
                head.x = mem.ReadFloat(BoneStart + 0x30 * iAimBoneId[iAimBone] + 0xc); //ReadProcessMemory(hProcess, (LPVOID)(bbone + (0x30 * 10/*yourbone*/) + 0xC), &bone.x, sizeof(float), NULL);
                head.y = mem.ReadFloat(BoneStart + 0x30 * iAimBoneId[iAimBone] + 0x1c);//ReadProcessMemory(hProcess, (LPVOID)(bbone + (0x30 * 10/*yourbone*/) + 0x1C), &bone.y, sizeof(float), NULL);
                head.z = mem.ReadFloat(BoneStart + 0x30 * iAimBoneId[iAimBone] + 0x2c); //;//ReadProcessMemory(hProcess, (LPVOID)(bbone + (0x30 * 10/*yourbone*/) + 0x2C), &bone.z, sizeof(float), NULL);

                name=mem.ReadStringUTF8(Offsets.dwRadarBasePointer + (0x168 * (id + 2) + 0x18),64);

                if (isEnemy && health > 0 && !bDormant)
                    isVisible = bsp_is_visible(Engine.LocalPlayer.position, positionHeight);
                else
                    isVisible = false;

                iGlowIndex = mem.ReadInt32(BASE + Offsets.m_iGlowIndex) * 0x38 + 0x4;

            }
        }

    public static bool WorldToScreen(Vec3 from, out Vec2 to)
        {
            //Thanks to Rake from GH for helping me with this, once upon a time. 
            //(never forget those who help you xD)
            float w = 0.0f;
            Vec3 clipCoords = new Vec3();
            clipCoords.x = LocalPlayer.WorldToScreenMatrix.flMatrix[0, 0] * from.x + LocalPlayer.WorldToScreenMatrix.flMatrix[0, 1] * from.y + LocalPlayer.WorldToScreenMatrix.flMatrix[0, 2] * from.z + LocalPlayer.WorldToScreenMatrix.flMatrix[0, 3];
            clipCoords.y = LocalPlayer.WorldToScreenMatrix.flMatrix[1, 0] * from.x + LocalPlayer.WorldToScreenMatrix.flMatrix[1, 1] * from.y + LocalPlayer.WorldToScreenMatrix.flMatrix[1, 2] * from.z + LocalPlayer.WorldToScreenMatrix.flMatrix[1, 3];
            w = LocalPlayer.WorldToScreenMatrix.flMatrix[3, 0] * from.x + LocalPlayer.WorldToScreenMatrix.flMatrix[3, 1] * from.y + LocalPlayer.WorldToScreenMatrix.flMatrix[3, 2] * from.z + LocalPlayer.WorldToScreenMatrix.flMatrix[3, 3];

            if (w < 0.1f)
            {
                to.x = 0;
                to.y = 0;
                return false;
            }

            Vec3 NDC = new Vec3();
            NDC.x = clipCoords.x / w;
            NDC.y = clipCoords.y / w;
            NDC.z = clipCoords.z / w;
            int width = (int)(rect.right - rect.left);
            int height = (int)(rect.bottom - rect.top);
            to.x = (width / 2 * NDC.x) + (NDC.x + width / 2);
            to.y = -(height / 2 * NDC.y) + (NDC.y + height / 2);
            return true;
        }

        public static void NormalizeAngle(ref Vec2 old)
        {
            if(old.x>89.0f)
                old.x = 89.0f;
            if (old.x < -89.0f)
                old.x = 89.0f;
            if (old.y > 180.0f)
                old.x = 180.0f;
            if (old.x < -180f)
                old.x = 180.0f;
        }

        public static Vec2 NormalizeAngle(Vec2 old)
        {
            if (old.x > 89.0f)
                old.x = 89.0f;
            if (old.x < -89.0f)
                old.x = 89.0f;
            if (old.y > 180.0f)
                old.x = 180.0f;
            if (old.x < -180f)
                old.x = 180.0f;
            return old;
        }

    }
}
