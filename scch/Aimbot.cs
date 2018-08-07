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

    public static class Aimbot
    {
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public extern static void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, ulong dwExtraInfo);
        //https://msdn.microsoft.com/en-us/library/windows/desktop/ms646260(v=vs.85).aspx 函数定义

        [DllImport("bsp.dll", EntryPoint = "bsp_parse_map")]
        static extern bool bsp_parse_map(string path, string filename);

        [DllImport("bsp.dll", EntryPoint = "bsp_is_visible")]
        static extern bool bsp_is_visible(Vector3 origin, Vector3 tagger);

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(

         byte bVk,    //虚拟键值

         byte bScan,// 一般为0

         int dwFlags,  //这里是整数类型  0 为按下，2为释放

         int dwExtraInfo  //这里是整数类型 一般情况下设成为 0 
         );
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        [DllImport("user32.dll")]
        static extern short GetKeyState(System.Windows.Forms.Keys vKey);








        public static void AAimbot()
        {
            Vec2 evec;
            Form1 f = new Form1();
            int id;
            for (; ; )
            {
                if (config.bAimbot)
                {
                    for (int i = 1; i < Engine.MAX_PLAYERS; i++)
                    {
                        if (Engine.enemy[i].health > 0 && !Engine.enemy[i].bDormant && Engine.enemy[i].isEnemy && Engine.enemy[i].isVisible)
                        {
                            MoveAngles(Engine.enemy[i].head, out evec);
                            if ((Engine.LocalPlayer.angles.y -evec.y < config.iFov && -(Engine.LocalPlayer.angles.y - evec.y) < config.iFov) && (Engine.LocalPlayer.angles.x -evec.x < config.iFov && -(Engine.LocalPlayer.angles.x - evec.x) < config.iFov) && (!config.bAimbotKey || ((GetAsyncKeyState(Keys.LButton) & 0x8000) == 0x8000)))
                            {
                                id = Engine.mem.ReadInt32(Engine.mem.ReadInt32(Offsets.dw_LocalPlayer) + Offsets.m_iCrosshairId);
                                if (id > 0 && id < 64 && Engine.enemy[id - 1].isEnemy)//&& ((GetAsyncKeyState(Keys.LButton) & 0x8000) != 0x8000)
                                {
                                    Thread.Sleep(5);
                                }
                           //     MoveToVec(evec);
                                // for (; ((evec.y > 0.5 || (-evec.y) > 0.5)) || ((-evec.x) > 0.5 || evec.x > 0.5);) 相对位置
                                // {
                                // smooth ?
                                for (int p=1; p<config.fSmooth; p++)
                                {
                                    if (Engine.enemy[i].health == 0 || Engine.enemy[i].bDormant || !Engine.enemy[i].isVisible)
                                        break;
                                    MoveAngles(Engine.enemy[i].head, out evec);
                                    //   MoveToVec((evec / config.fSmooth) * p);
                                    //  Vec2 ne = ;

                                    MoveToVec(Engine.NormalizeAngle((Engine.LocalPlayer.angles - Engine.LocalPlayer.ViewPunchAngle * 2.0f) + (evec - Engine.LocalPlayer.angles) / (config.fSmooth - p)));
                                    

                                    Thread.Sleep(10);
                                }

                             //   MoveToVec(Engine.NormalizeAngle(Engine.LocalPlayer.angles - Engine.LocalPlayer.ViewPunchAngle * 1.9f));

                                //     f.drawShadowString(i + " "+evec.x+"  "+evec.y, f.debugFont, Brushes.AliceBlue, new PointF(400, 40));
                                if (Engine.enemy[i].health == 0 || Engine.enemy[i].bDormant)
                                    break;
                                //  }
                                //  id = Engine.mem.ReadInt32(Engine.mem.ReadInt32(Offsets.dw_LocalPlayer) + Offsets.m_iCrosshairId);
                                //   if (id > 0 && id < 64 && Engine.enemy[id - 1].isEnemy && ((GetAsyncKeyState(Keys.LButton) & 0x8000) != 0x8000))//
                                //   {
                                if ((GetAsyncKeyState(Keys.LButton) & 0x8000) != 0x8000)
                                {
                                    keybd_event(0x01, 0, 0, 0); //https://docs.microsoft.com/en-us/windows/desktop/inputdev/virtual-key-codes 按键代码
                                    Thread.Sleep(config.downdelany);
                                    keybd_event(0x01, 0, 2, 0);
                                }
                            //    }
                                Thread.Sleep(10);
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(40);
                }
            }
        }

        public static void MoveAngles(Vec3 enemyvec, out Vec2 move)
        {
            // move.y = Engine.LocalPlayer.angles.y-(float)(180 * Math.Atan2(enemyvec.y - Engine.LocalPlayer.head.y, enemyvec.x - Engine.LocalPlayer.head.x) / Math.PI);


            //  move.y = Engine.LocalPlayer.angles.y - (float)(180 * Math.Atan2(enemyvec.y - Engine.LocalPlayer.position.y, enemyvec.x - Engine.LocalPlayer.position.x) / Math.PI);
            // move.x = -Engine.LocalPlayer.angles.x - (float)(180 * Math.Atan2(enemyvec.z - Engine.LocalPlayer.position.z, Math.Sqrt(((enemyvec.x - Engine.LocalPlayer.position.x) * (enemyvec.x - Engine.LocalPlayer.position.x)) + ((enemyvec.y - Engine.LocalPlayer.position.y) * (enemyvec.y - Engine.LocalPlayer.position.y)))) / Math.PI);
            //计算相对角度

            move.y = (float)(Math.Atan2(enemyvec.y - Engine.LocalPlayer.position.y, enemyvec.x - Engine.LocalPlayer.position.x) * 57.295779513082f);
            move.x = -(float)(Math.Atan2(enemyvec.z - Engine.LocalPlayer.position.z, Math.Sqrt(((enemyvec.x - Engine.LocalPlayer.position.x) * (enemyvec.x - Engine.LocalPlayer.position.x)) + ((enemyvec.y - Engine.LocalPlayer.position.y) * (enemyvec.y - Engine.LocalPlayer.position.y)))) * 57.295779513082f);
            //绝对角度

            // move.x =-(Engine.LocalPlayer.angles.x -(float)(180 * Math.Atan2(enemyvec.z - Engine.LocalPlayer.head.z, Math.Sqrt(((enemyvec.x - Engine.LocalPlayer.head.x) * (enemyvec.x - Engine.LocalPlayer.head.x)) + ((enemyvec.y - Engine.LocalPlayer.head.y) * (enemyvec.y - Engine.LocalPlayer.head.y)) + ((enemyvec.z - Engine.LocalPlayer.head.z) * (enemyvec.z - Engine.LocalPlayer.head.z)))) / Math.PI));
            //move.x = -(Engine.LocalPlayer.angles.x - (float)(180 * Math.Atan2(enemyvec.z - Engine.LocalPlayer.position.z, Math.Sqrt(((enemyvec.x - Engine.LocalPlayer.position.x) * (enemyvec.x - Engine.LocalPlayer.position.x)) + ((enemyvec.y - Engine.LocalPlayer.position.y) * (enemyvec.y - Engine.LocalPlayer.position.y)) + ((enemyvec.z - Engine.LocalPlayer.position.z) * (enemyvec.z - Engine.LocalPlayer.position.z)))) / Math.PI));

            if (move.y > 180)
            {
                move.y = -(360 - move.y);
                return;
            }
            else if (move.y < -180)
            {
                move.y = -(360 + move.y);
                return;
            }
        }


        public static void MoveToVec(Vec2 src)
        {
            //src=src - Engine.LocalPlayer.ViewPunchAngle;//-Engine.LocalPlayer.punchAngle
            if (!Engine.mem.WriteFloat(Offsets.dwClientState + Offsets.dwClientState_ViewAngles, src.x))
                return;
            if (!Engine.mem.WriteFloat(Offsets.dwClientState + Offsets.dwClientState_ViewAngles + 0x4, src.y))
            return ;

           // int x = (Engine.rect.right - Engine.rect.left) / Engine.LocalPlayer.FOV;
           // int y = (Engine.rect.bottom - Engine.rect.top) / Engine.LocalPlayer.FOV;
            //mouse_event(0x0001, (int)(y * src.y), (int)(x * src.x), 0, 0);/// mouseSensitivity
           // mouse_event(0x0001, (int)(y * src.y)*(65536/ (Engine.rect.right - Engine.rect.left)), (int)(x * src.x) * (65536 / (Engine.rect.bottom - Engine.rect.top)), 0, 0);
        }

    }
}
