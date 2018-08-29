using System.Diagnostics;

namespace scch
{
    public static class Offsets
    {
        public static string pat_EntityList = "42 18 3B C7";
        public static long dw_EntityList = 0x0;

        public static string pat_LocalPlayer = "8D 34 85 ? ? ? ? 89 15";
        public static long dw_LocalPlayer = 0x0;

        public static string pat_ViewMatrix = "0F 10 05 ? ? ? ? 8D 85 ? ? ? ? B9";
        public static long dw_ViewMatrix = 0x0;

        public static long dwRadarBase  =  0x0 ;
        public static long dwRadarBasePointer = 0x0;
        //client_panorama.dll+dwSensitivityPtr+0x24 灵敏度字符串
        public static long dwSensitivityPtr = 0x0;
        public static long dwClientState = 0x0;

        public static long m_vecViewOffset = 0x0;
        public static long m_dwBoneMatrix = 0x0;
        public static long m_iFOVStart = 0x0;
        public static long dwClientState_ViewAngles = 0x0;
        public static long m_iCrosshairId = 0x0;
      
        public static long oHealth = 0x0;
        public static long oVecOrigin = 0x0;
        public static long oTeam = 0x0;
        public static float mouseSensitivity;
        public static long oDormant = 0xE9;
        public static long m_viewPunchAngle = 0x0;

        public static long dwClientState_Map = 0x0;
        public static long dwClientState_State = 0x0;
        public static long dwClientState_MapDirectory = 0x0;
        public static long dwGameDir = 0x0;
        public static long dwGlowObjectManager = 0x0;
        public static long GlowObjectManager = 0x0;
        public static long m_iGlowIndex = 0x0;

        public static string path;

        public static long m_dwRadarBasePointer = 0x6c;

        public static void GetPatterns()
        {
            //Thanks to Traxin from GH for the sig scan func.
            //Creds to Zaczero from MPGH because i stole these sigs from his post.
            ProcessModule client = Engine.mem.GetModule("client_panorama.dll");

            //entityList - "42 18 3B C7", 0xB, client.dll
            //Offsets.dw_EntityList = 0xB + (long)Engine.mem.PatternScanMod(client, Offsets.pat_EntityList);
            Offsets.dw_EntityList += (int)client.BaseAddress;
            // Offsets.dw_EntityList = Engine.mem.ReadInt32(Offsets.dw_EntityList);

            //localPlayer - "8D 34 85 ? ? ? ? 89 15", 0x3, 0x4, client.dll
            //Offsets.dw_LocalPlayer = 0x3 + (long)Engine.mem.PatternScanMod(client, Offsets.pat_LocalPlayer);
            Offsets.dw_LocalPlayer += (int)client.BaseAddress;
            // Offsets.dw_LocalPlayer = Engine.mem.ReadInt32(Offsets.dw_LocalPlayer);
            // Offsets.dw_LocalPlayer = Engine.mem.ReadInt32(Offsets.dw_LocalPlayer + 4);

            //viewMatrix - "0F 10 05 ? ? ? ? 8D 85 ? ? ? ? B9", 0x3, 0xB0, client.dll
            //Offsets.dw_ViewMatrix = 0x3 + (long)Engine.mem.PatternScanMod(client, Offsets.pat_ViewMatrix);
            Offsets.dw_ViewMatrix += (int)client.BaseAddress;
            // Offsets.dw_ViewMatrix = 0xB0 + Engine.mem.ReadInt32(Offsets.dw_ViewMatrix);

            Offsets.dwRadarBase += (int)client.BaseAddress;

            Offsets.dwRadarBasePointer = Engine.mem.ReadInt32(Engine.mem.ReadInt32(Offsets.dwRadarBase) + Offsets.m_dwRadarBasePointer);

            Offsets.dwClientState = Engine.mem.ReadInt32(Engine.ENGINE + Offsets.dwClientState);

            mouseSensitivity = float.Parse(Engine.mem.ReadStringAscii(Engine.mem.ReadInt32((int)client.BaseAddress + Offsets.dwSensitivityPtr + 0x24), 9));

            path = Engine.mem.ReadStringAscii(Engine.ENGINE+ dwGameDir, 120)+"\\";

            dwClientState_State += Offsets.dwClientState;

            GlowObjectManager = Engine.mem.ReadInt32((int)client.BaseAddress + dwGlowObjectManager);
        }
    }
}
