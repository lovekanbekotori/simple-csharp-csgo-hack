using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace scch
{

    public partial class Form1 : Form
    {
        public const string WINDOW_NAME = "Counter-Strike: Global Offensive";
        IntPtr handle = FindWindow(null, WINDOW_NAME);
        //   static Bitmap bmp = new Bitmap(Engine.rect.left, Engine.rect.top); //创建画布
        Graphics g;// = Graphics.FromImage(bmp);
        Pen boxPen = new Pen(Color.Red, 2);
        Pen healthPen = new Pen(Color.Black, 1);

        Font menuFont = new Font("simfang.ttf", 12, FontStyle.Bold);//Arial
        Font debugFont = new Font("simfang.ttf", 12, FontStyle.Bold);
        Thread tb = new Thread(Triggerbot);
        Thread ab = new Thread(Aimbot.AAimbot);
        //everything is declared globally because i made this in a hurry. (and it is a very small project)
        //realistically this could be MUCH cleaner by sperating all of these and the global variables in Engine.cs into other classes
        

        public string SDrawBox;
        public string SDrawLine;
        public string SDrawHealth;
        public string SER;
        public string SEG;
        public string SEB;
        public string SDebug;
        public string SShowName;
        public string STriggerbot;
        public string SShotdelay;
        public string STriggerDownTime;
        public string SShowDis;
        public string SAimbot;
        public string SFov;
        public string SAimKey;
        public string SDrowBone;
        public string SSmooth;
        public string SAimParts;
        public string SAimAndFire;
        public string SSaveConfig;
        public string SVersion;
        public string SNewVersion;
        public string SGLOW;
        public string[] SAimName = new string[4];

        
        //i have the esp adjust itself according to this y offset. If playing in borderless windowed 0 is fine.
        //However, if the use is in regular windowed mode, there is a title bar at the top which isnt calculated any where.
        //and again, because i am lazy i dont feel like spending a long time detecting wether the game is in windowed or borderless.
        //so i let the user fix it themselves by pressing ctrl up or down. #lazy
        public int yOffset = 0;

        //i don't declare this in the watermark func itself because i call that function constantly to update the position.
        //therefore declaring it there would just constantly reset it to -8000. another lazily decalred global xD
        public int watermarkPosition = -8000;

        //MultiThreaded to prevent some lag. although GDI is laggy af regardless lol.
        // Thread T_CheckPatterns = new Thread(checkPatterns);
        Thread T_ReaderThread = new Thread(readerFunc); //刷新玩家信息

        #region imports
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out Engine.RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        [DllImport("user32.dll")]
        static extern short GetKeyState(System.Windows.Forms.Keys vKey);
        [DllImport("user32.dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern int PostMessage(IntPtr hWnd, int Msg, Keys wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(

         byte bVk,    //虚拟键值

         byte bScan,// 一般为0

         int dwFlags,  //这里是整数类型  0 为按下，2为释放

         int dwExtraInfo  //这里是整数类型 一般情况下设成为 0 
         );

        [DllImport("bsp.dll", EntryPoint = "bsp_is_visible")]
        static extern bool bsp_is_visible(Vector3 origin, Vector3 tagger);

        [DllImport("bsp.dll", EntryPoint = "bsp_init")]
        static extern void bsp_init();

        [DllImport("bsp.dll", EntryPoint = "bsp_get_info")]
        static extern IntPtr bsp_get_info();

        #endregion
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //want to see what is going on here?
            //check out https://www.youtube.com/watch?v=t1ErGj0YnaM
            Engine.rect = GetWindowRect(handle);
            this.BackColor = Color.Wheat;
            this.TransparencyKey = Color.Wheat;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(Engine.rect.right - Engine.rect.left, Engine.rect.bottom - Engine.rect.top);
            this.Left = Engine.rect.left;
            this.Top = Engine.rect.top;
            this.DoubleBuffered = true;

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            Engine.mem = new HexMem("csgo", HexMem.ProcessAccessFlags.VMRead| HexMem.ProcessAccessFlags.VMWrite| HexMem.ProcessAccessFlags.VMOperation);//HexMem.ProcessAccessFlags.VMRead | HexMem.ProcessAccessFlags.VMWrite
            Engine.CLIENT = Engine.mem.GetModuleAddress("client_panorama.dll");
            Engine.ENGINE = Engine.mem.GetModuleAddress("engine.dll");

            if (Engine.CLIENT <= 0)
            {
                Environment.Exit(-1);
            }

            this.TopMost = true;

            config.InitConfig();//初始化配置文件

            bsp_init();
            Offsets.GetPatterns(); //获取基址
            //T_CheckPatterns.Start();
            T_ReaderThread.Start();
            tb.Start();//triggerbot
            ab.Start();//自瞄
        }


        public static Engine.RECT GetWindowRect(IntPtr hWnd)
        {
            Engine.RECT result = new Engine.RECT();
            GetWindowRect(hWnd, out result);
            return result;
        }



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.TopMost = true;
            g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

            //ahhhhh the big function all the copy pasters want to see. i made it very simple. 
            //read carefully and you cant tell exactly what is going on.


            for (int i = 0; i < Engine.MAX_PLAYERS; i++)
            {
                if (Engine.enemy[i].health > 0 && !Engine.enemy[i].bDormant && Engine.enemy[i].isEnemy)
                {
                    if (Engine.WorldToScreen(Engine.enemy[i].position, out Engine.enemy[i].W2S_Feet) && Engine.WorldToScreen(Engine.enemy[i].positionHeight, out Engine.enemy[i].W2S_Height))
                    {
                        double temp1 = Math.Pow((Engine.enemy[i].W2S_Height.x - Engine.enemy[i].W2S_Feet.x), 2);
                        double temp2 = Math.Pow((Engine.enemy[i].W2S_Height.y - Engine.enemy[i].W2S_Feet.y), 2);
                        Engine.enemy[i].box_height = Convert.ToSingle(Math.Sqrt(temp1 + temp2));

                        if (Engine.enemy[i].box_height < 4)
                            Engine.enemy[i].box_height = 4;
                        if (Engine.enemy[i].box_height > 999)
                            Engine.enemy[i].box_height = 999;
                        Engine.enemy[i].box_width = Engine.enemy[i].box_height / 2;
                        if (Engine.enemy[i].box_width < 4)
                            Engine.enemy[i].box_width = 4;
                        if (Engine.enemy[i].box_width > 999)
                            Engine.enemy[i].box_width = 999;

                        Engine.enemy[i].box_height *= 1.17f;
                        Engine.enemy[i].box_width *= 1.17f;

                        Engine.enemy[i].box_x = Convert.ToInt32(Engine.enemy[i].W2S_Feet.x - (Engine.enemy[i].box_width / 2));
                        Engine.enemy[i].box_y = Convert.ToInt32(Engine.enemy[i].W2S_Feet.y - Engine.enemy[i].box_height);

                        if (config.bDrawBox)
                        {
                            DrawBox(i);
                            if (config.showname)
                                ShowName(i); //显示名字
                            if (config.bDrawLine)
                                DrawSnapLines(i);
                            if (config.bDrawHealth)
                                DrawHealth(i);
                            if (config.showdisc)
                                ShowDistance(i);
                            if (config.bDrowBone)
                                DrawBoneLine(i);
                        }

                        if (config.bGlow)
                            DrawGlow(i);


                        //   if (bsp_is_visible(Engine.LocalPlayer.position, Engine.enemy[i].positionHeight))
                        //     {
                        //     ShowVis(i);
                        //     }

                    }
                }

            }
            //drawWaterMark();
            if (config.debug)
                drawDebugInfo();
            if (config.bshowMenu)
                drawMenu();
            if (!config.bshowMenu)
                ShowVersion();
   
        }

        public void DrawGlow(int i)
        {
            Engine.mem.WriteFloat(Offsets.dwGlowObjectManager + Engine.enemy[i].iGlowIndex, config.color_espR / 255.0f); //r
            Engine.mem.WriteFloat(Offsets.dwGlowObjectManager + Engine.enemy[i].iGlowIndex + 0x4, config.color_espG / 255.0f);//g
            Engine.mem.WriteFloat(Offsets.dwGlowObjectManager + Engine.enemy[i].iGlowIndex + 0x8, config.color_espB / 255.0f);//b
            Engine.mem.WriteFloat(Offsets.dwGlowObjectManager + Engine.enemy[i].iGlowIndex + 0xc, 170 / 255.0f);//a
                Engine.mem.WriteBool(Offsets.dwGlowObjectManager + Engine.enemy[i].iGlowIndex + 0x20, true);
                Engine.mem.WriteBool(Offsets.dwGlowObjectManager + Engine.enemy[i].iGlowIndex + 0x21, false);
        }

        public void ShowVersion()
        {
            drawShadowString(config.Version <= config.VERSION ? SVersion + config.VERSION : SVersion + config.VERSION + SNewVersion + config.Version, debugFont, Brushes.Red, new PointF(1, 1));
        }

        //public void ShowVis(int i)
      //  {
      //      g.DrawString("可见", menuFont, Brushes.Red, new PointF(Engine.enemy[i].box_x, Engine.enemy[i].box_y - 18));
       // }



        public static int menu_selected = 0;
        public const int MENU_MAX_ITEMS = 20; //菜单数
        public const int MENU_TEXT_GAP = 23; //菜单背景间隔
        public static Point MENU_START_POS = new Point(8, 8);





        public void drawMenu()
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 32, 48, 24)), MENU_START_POS.X - 2, MENU_START_POS.Y - 2, 200, 480); //最后两个参数是菜单背景大小

            addMenutItem(SDrawBox + config.bDrawBox, 0);
            addMenutItem(SDrawLine + config.bDrawLine, 1);
            addMenutItem(SDrawHealth + config.bDrawHealth, 2);
            addMenutItem(SER + config.color_espR, 3);
            addMenutItem(SEG + config.color_espG, 4);
            addMenutItem(SEB + config.color_espB, 5);
            addMenutItem(SDebug + config.debug, 6);
            addMenutItem(SShowName + config.showname, 7);
            addMenutItem(STriggerbot + config.triggerbot, 8);
            addMenutItem(SShotdelay + config.delany + "ms", 9);
            addMenutItem(STriggerDownTime + config.downdelany + "ms", 10);
            addMenutItem(SShowDis + config.showdisc, 11);
            addMenutItem(SAimbot + config.bAimbot, 12);
            addMenutItem(SFov + config.iFov, 13);
            addMenutItem(SAimKey + config.bAimbotKey, 14);
            addMenutItem(SDrowBone + config.bDrowBone, 15);
            addMenutItem(SSmooth + config.fSmooth, 16);
            addMenutItem(SAimParts + SAimName[Engine.iAimBone], 17);
            addMenutItem(SAimAndFire + config.bAimTrigger, 18);
            addMenutItem(SSaveConfig, 19);
            addMenutItem(SGLOW + config.bGlow, 20);

        }

        public void SetupLanguage()
        {
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "zh-CN" || System.Threading.Thread.CurrentThread.CurrentCulture.Name == "Zh-TW")
            {
                SDrawBox             = "画方框         ";
                SDrawLine            = "画线           ";
                SDrawHealth          = "画血条         ";
                SER                  = "ESP Color R    ";
                SEG                  = "ESP Color G    ";
                SEB                  = "ESP Color B    ";
                SDebug               = "Debug          ";
                SShowName            = "显示名字       ";
                STriggerbot          = "自动扳机       ";
                SShotdelay           = "射击延迟       ";
                STriggerDownTime     = "扳机按下时间   ";
                SShowDis             = "显示距离       ";
                SAimbot              = "自动瞄准       ";
                SFov                 = "范围           ";
                SAimKey              = "自瞄键         ";
                SDrowBone            = "画骨骼         ";
                SSmooth              = "平滑           ";
                SAimParts            = "瞄准部位       ";
                SAimAndFire          = "瞄准后开火     ";
                SAimName[0]          = "头";
                SAimName[1]          = "脖子";
                SAimName[2]          = "胸口";
                SAimName[3]          = "肚子";
                SSaveConfig          = "保存配置";
                SVersion             = "当前版本 :";
                SNewVersion          = "发现新版本 :";
                SGLOW                = "人物发光       ";
            }
            else //default 默认
            {
                SDrawBox             = "DrowBOX         ";
                SDrawLine            = "DrowLine        ";
                SDrawHealth          = "DrowHealth      ";
                SER                  = "ESP Color R     ";
                SEG                  = "ESP Color G     ";
                SEB                  = "ESP Color B     ";
                SDebug               = "Debug           ";
                SShowName            = "ShowName        ";
                STriggerbot          = "TriggerBot      ";
                SShotdelay           = "Shoot Delay     ";
                STriggerDownTime     = "Trriger Pree Time";
                SShowDis             = "Show Distance   ";
                SAimbot              = "Aimbot          ";
                SFov                 = "Aim FOV         ";
                SAimKey              = "Aim Key         ";
                SDrowBone            = "Drow Skeleton   ";
                SSmooth              = "Smooth          ";
                SAimParts            = "Aim Parts       ";
                SAimAndFire          = "Aim And Fire    ";
                SAimName[0]          = "Head";
                SAimName[1]          = "Neck";
                SAimName[2]          = "Chest";
                SAimName[3]          = "Belly";
                SSaveConfig          = "Save Configuer";
                SVersion             = "Current Version :";
                SNewVersion          = "There is now a new version :";
                SGLOW                = "GLOW            ";
            }
        }

        public void DrawBoneLine(int i)
        { //shit skeleton
            int[] BoneListId = new int[16] {43,42,41,7,11,12,13,8,7,5,0,78,79,0,71,72};//骨骼id
            Vec3[] BoneList = new Vec3[16];
            Vec2[] src = new Vec2[16];
            for (int a = 0; a < 16; a++)
            {
                GetBone(i, BoneListId[a], out BoneList[a]);
                Engine.WorldToScreen(BoneList[a],out src[a]);
            }
            for(int p=0;p<6;p++)
            {
                g.DrawLine(boxPen, src[p].x, src[p].y, src[p + 1].x, src[p + 1].y);
            }
            for (int p = 7; p < 12; p++)
            {
                g.DrawLine(boxPen, src[p].x, src[p].y, src[p + 1].x, src[p + 1].y);
            }
            for (int p = 13; p < 15; p++)
            {
                g.DrawLine(boxPen, src[p].x, src[p].y, src[p + 1].x, src[p + 1].y);
            }

        }

        public void DrawBone(int i)
        {
            Vec3 bone;
            Vec2 src;
            src.x = 0;
            src.y = 0;
            for (int id = 0; id < 128; id++)
            {
                GetBone(i, id, out bone);
                Engine.WorldToScreen(bone,out src);
                drawShadowString(id.ToString(), debugFont, Brushes.AliceBlue, new PointF(src.x, src.y));
            }
        }

        public void GetBone(int i,int id,out Vec3 matrix)
        {
            matrix.x = Engine.mem.ReadFloat(Engine.enemy[i].BoneStart + 0x30 * id + 0xc);
            matrix.y = Engine.mem.ReadFloat(Engine.enemy[i].BoneStart + 0x30 * id + 0x1c);
            matrix.z = Engine.mem.ReadFloat(Engine.enemy[i].BoneStart + 0x30 * id + 0x2c);
        }

        public void drawDebugInfo()//This function only used for debuggin purposes... (obvioussly)
        {
            Vec2 evec;
            drawShadowString("LocalPlayer: " + Offsets.dw_LocalPlayer.ToString("X") + "     " + Engine.LocalPlayer.team.ToString(), debugFont, Brushes.AliceBlue, new PointF(300, 10));
            drawShadowString("Entity List: " + Offsets.dw_EntityList.ToString("X"), debugFont, Brushes.AliceBlue, new PointF(300, 25));
            drawShadowString("View Matrix : " + Offsets.dw_ViewMatrix.ToString("X"), debugFont, Brushes.AliceBlue, new PointF(300, 40));
            drawShadowString(Engine.LocalPlayer.vecViewOffset.ToString(), debugFont, Brushes.AliceBlue, new PointF(600, 40));
            drawShadowString("x :" + Engine.LocalPlayer.position.x + " y :" + Engine.LocalPlayer.position.x + " z :" + Engine.LocalPlayer.position.z, debugFont, Brushes.AliceBlue, new PointF(600, 60));
            drawShadowString("X : " + Engine.LocalPlayer.angles.x.ToString()+"Y  :"+Engine.LocalPlayer.angles.y.ToString(), debugFont, Brushes.AliceBlue, new PointF(300, 50));
          drawShadowString(Engine.LocalPlayer.punchAngle.x +"       "+Engine.LocalPlayer.punchAngle.y +"        "+ Engine.LocalPlayer.ViewPunchAngle.x+"        "+Engine.LocalPlayer.ViewPunchAngle.y, debugFont, Brushes.AliceBlue, new PointF(300, 60));//Marshal.PtrToStringAnsi(bsp_get_info())
                                                                                                                         //   drawShadowString(, debugFont, Brushes.AliceBlue, new PointF(300, 70));
           // bsp_get_info();
      //    drawShadowString(Engine.LocalPlayer.mapname, debugFont, Brushes.AliceBlue, new PointF(300, 85));
              for (int i = 1; i < Engine.MAX_PLAYERS; i++)
            {
                 if (Engine.enemy[i].health > 0 && !Engine.enemy[i].bDormant)
                {
                     Aimbot.MoveAngles(Engine.enemy[i].head, out evec);
              //   Aimbot.MoveToVec(evec);
                     drawShadowString(evec.x.ToString() + "  " + evec.y.ToString() , debugFont, Brushes.AliceBlue, new PointF(300, 60 + i * 15));
                    //       DrawBone(i);
                    DrawGlow(i);
                 }
             }
        }

        /*This is my first attempt at drawing a menu.
         * literally have never looked at another source of a menu
         * so i was going into this blind. really really basic
         * and i am sure there are much better ways to do this, but this
         * is what i came up with. it makes sense to me logically again, but
         * it may take some time to comprehend how it works. 
         * (it works well imo lol)
         */

 
        public void addMenutItem(string text, int i)
        {
            if (i == menu_selected)
                drawShadowString(text, menuFont, Brushes.CornflowerBlue, new PointF(MENU_START_POS.X, MENU_START_POS.Y + i * MENU_TEXT_GAP));
            else
                drawShadowString(text, menuFont, Brushes.AliceBlue, new PointF(MENU_START_POS.X, MENU_START_POS.Y + i * MENU_TEXT_GAP));
        }

       


        public static void Triggerbot()
        {
            IntPtr hwnd= FindWindow(null, WINDOW_NAME);
            for (; ; )
            {
                if (config.triggerbot)
                {
                    int id = Engine.mem.ReadInt32(Engine.mem.ReadInt32(Offsets.dw_LocalPlayer) + Offsets.m_iCrosshairId);
                    if (id > 0 && id < 64 && Engine.enemy[id - 1].isEnemy && ((GetAsyncKeyState(Keys.LButton) & 0x8000) != 0x8000))
                    {
                        Thread.Sleep(config.delany);
                        keybd_event(0x01, 0, 0, 0); //https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes 按键代码
                        Thread.Sleep(config.downdelany);
                        keybd_event(0x01, 0, 2, 0);
                    }

                }
                else
                {
                    Thread.Sleep(40);
                }
            }
        }
        public void ShowName(int i)
        {
            g.DrawString(Engine.enemy[i].name, menuFont, Brushes.Red, new PointF(Engine.enemy[i].box_x, Engine.enemy[i].box_y - 18));
        }

        public void ShowDistance(int i)
        {
            g.DrawString(Engine.enemy[i].distance.ToString() + " m", menuFont, Brushes.Red, new PointF(Engine.enemy[i].box_x, Engine.enemy[i].box_y - 36));
        }

        public void DrawHealth(int i)
        {
                int startGap = 100 - Engine.enemy[i].health;
                if (startGap > 0)
                {
                    startGap = (int)Engine.enemy[i].box_height - Engine.enemy[i].health * (int)Engine.enemy[i].box_height / 100;//WOW this took a long time!
                }
                g.DrawRectangle(healthPen, Engine.enemy[i].box_x - 6, Engine.enemy[i].box_y - 1, 3, Engine.enemy[i].box_height + 2);
                g.FillRectangle(Brushes.Chartreuse, Engine.enemy[i].box_x - 5, Engine.enemy[i].box_y + startGap, 2, Engine.enemy[i].box_height - startGap + 1);
        }

        public void DrawBox(int i)
        {
         //   g.DrawRectangle(boxPen, Engine.enemy[i].box_x, Engine.enemy[i].box_y, Engine.enemy[i].box_width, Engine.enemy[i].box_height);
            g.DrawRectangle(Engine.enemy[i].isVisible? new Pen(Color.GreenYellow):boxPen, Engine.enemy[i].box_x, Engine.enemy[i].box_y, Engine.enemy[i].box_width, Engine.enemy[i].box_height);
        }
        public void DrawSnapLines(int i)
        {
            g.DrawLine(boxPen, (Engine.rect.right - Engine.rect.left) / 2, Engine.rect.bottom - Engine.rect.top, Engine.enemy[i].W2S_Feet.x, Engine.enemy[i].W2S_Feet.y);
        }
        public void drawShadowString( string text, Font font, Brush brush, PointF pos)
        {
            g.DrawString(text, font, Brushes.Black, new PointF(pos.X + 1, pos.Y + 1));
            g.DrawString(text, font, brush, pos);
        }
        public void drawWaterMark()
        {
            drawShadowString("\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tDrop a thanks on my post on GH!\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\twww.guidedhacking.com\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\twww.youtube.com/HexMurder\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\tHexMurder's Free GDI ESP V2.5", menuFont, Brushes.DarkCyan, new PointF(watermarkPosition, 3));
            watermarkPosition++;
            if (watermarkPosition > 8920)
                watermarkPosition = -4000;
        }
        private void timerRefresh_Tick(object sender, EventArgs e)
        {
              this.Refresh();
            //  Engine.rect = GetWindowRect(handle); //更新窗口位置,没卵用,有Bug
            //  this.Top = Engine.rect.top + yOffset;
            //  this.Left = Engine.rect.left;
            //this.CreateGraphics().DrawImage(bmp, 0, 0);
        }
        private static void readerFunc() //刷新玩家信息
        {
            for(;;)
            {
                Engine.LocalPlayer.read();
                for (int i = 0; i < Engine.MAX_PLAYERS; i++)
                {
                    Engine.enemy[i].read(i);
                }
                Thread.Sleep(1);
            }
        }
        private void timerInput_Tick(object sender, EventArgs e)
        {
            bool btnUp = (GetAsyncKeyState(Keys.Up) & (1 << 15)) != 0;
            bool btnDown = (GetAsyncKeyState(Keys.Down) & (1 << 15)) != 0;
            bool btnLeft = (GetAsyncKeyState(Keys.Left) & (1 << 15)) != 0;
            bool btnRight = (GetAsyncKeyState(Keys.Right) & (1 << 15)) != 0;
            bool btnCtrl = (GetAsyncKeyState(Keys.LControlKey) & (1 << 15)) != 0;
            bool btnAlt = (GetAsyncKeyState(Keys.LMenu) & (1 << 15)) != 0;
            bool btnQ = (GetAsyncKeyState(Keys.Q) & (1 << 15)) != 0;
            bool btnInsert = (GetAsyncKeyState(Keys.Insert) & (1 << 15)) != 0;

            if(btnInsert)
            {
                config.bshowMenu = !config.bshowMenu;
                Thread.Sleep(120);
            }

            if (btnUp && btnCtrl)
            {
                yOffset -= 1;//按下向上箭头次数
                Thread.Sleep(120);
            }
            if (btnDown && btnCtrl)
            {
                yOffset += 1; //按下向下箭头次数
                Thread.Sleep(120);
            }
            if(btnCtrl && btnAlt && btnQ)
            {
                Environment.Exit(1);
            }
            if (config.bshowMenu)
            {
                if (btnDown && !btnCtrl)
                {
                    if (menu_selected < MENU_MAX_ITEMS)
                        menu_selected++;
                    else
                        menu_selected = 0;
                    Thread.Sleep(120);
                }
                if (btnUp && !btnCtrl)
                {
                    if (menu_selected > 0)
                        menu_selected--;
                    else
                        menu_selected = MENU_MAX_ITEMS;
                    Thread.Sleep(120);
                }
                if (btnRight) //按下小键盘右键时
                {
                    switch (menu_selected)
                    {
                        case 0:
                            config.bDrawBox = true;
                            break;
                        case 1:
                            config.bDrawLine = true;
                            break;
                        case 2:
                            config.bDrawHealth = true;
                            break;
                        case 3:
                            if(config.color_espR < 255)
                                config.color_espR += 1;
                            Thread.Sleep(5);
                            break;
                        case 4:
                            if (config.color_espG < 255)
                                config.color_espG += 1;
                            Thread.Sleep(5);
                            break;
                        case 5:
                            if (config.color_espB < 255)
                                config.color_espB += 1;
                            Thread.Sleep(5);
                            break;
                        case 6:
                            config.debug = true;
                            break;
                        case 7:
                            config.showname = true;
                            break;
                        case 8:
                            config.triggerbot = true;
                            break;
                        case 9:
                            config.delany += 1;
                            break;
                        case 10:
                            config.downdelany += 1;
                            break;
                        case 11:
                            config.showdisc = true;
                            break;
                        case 12:
                            config.bAimbot = true;
                            break;
                        case 13:
                            if (config.iFov < 180)
                                config.iFov += 1;
                            break;
                        case 14:
                            config.bAimbotKey = true;
                            break;
                        case 15:
                            config.bDrowBone = true;
                            break;
                        case 16:
                            config.fSmooth +=1;
                            break;
                        case 17:
                            if (Engine.iAimBone< 3)
                                Engine.iAimBone += 1;
                            break;
                        case 18:
                            config.bAimTrigger = true;
                            break;
                        case 19:
                            config.SaveConfiguer();
                            break;
                        case 20:
                            config.bGlow = true;
                            break;
                    }
                    boxPen.Color = Color.FromArgb(255, config.color_espR, config.color_espG, config.color_espB);
                    Thread.Sleep(120);
                }
                if (btnLeft) //按下小键盘左键时
                {
                    switch (menu_selected)
                    {
                        case 0:
                            config.bDrawBox = false;
                            break;
                        case 1:
                            config.bDrawLine = false;
                            break;
                        case 2:
                            config.bDrawHealth = false;
                            break;
                        case 3:
                            if (config.color_espR > 0)
                                config.color_espR -= 1;
                            Thread.Sleep(5);
                            break;
                        case 4:
                            if (config.color_espG > 0)
                                config.color_espG -= 1;
                            Thread.Sleep(5);
                            break;
                        case 5:
                            if (config.color_espB > 0)
                                config.color_espB -= 1;
                            Thread.Sleep(5);
                            break;
                        case 6:
                            config.debug = false;
                            break;
                        case 7:
                            config.showname = false;
                            break;
                        case 8:
                            config.triggerbot = false;
                            break;
                        case 9:
                            if (config.delany > 0)
                                config.delany -= 1;
                            break;
                        case 10:
                            config.downdelany -= 1;
                            break;
                        case 11:
                            config.showdisc = false;
                            break;
                        case 12:
                            config.bAimbot = false;
                            break;
                        case 13:
                            if (config.iFov > 0)
                                config.iFov -= 1;
                            break;
                        case 14:
                            config.bAimbotKey = false;
                            break;
                        case 15:
                            config.bDrowBone = false;
                            break;
                        case 16:
                            if(config.fSmooth >1)
                                config.fSmooth -=1;
                            break;
                        case 17:
                            if (Engine.iAimBone > 0)
                                Engine.iAimBone -= 1;
                            break;
                        case 18:
                            config.bAimTrigger = false;
                            break;
                        case 19:
                            break;
                        case 20:
                            config.bGlow = false;
                            break;
                    }
                    boxPen.Color = Color.FromArgb(255, config.color_espR, config.color_espG, config.color_espB);
                    Thread.Sleep(120);
                }
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {

        }
    }
}
