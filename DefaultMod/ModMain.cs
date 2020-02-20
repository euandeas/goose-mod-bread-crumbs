using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
// 1. Added the "GooseModdingAPI" project as a reference.
// 2. Compile this.
// 3. Create a folder with this DLL in the root, and *no GooseModdingAPI DLL*
using GooseShared;
using SamEngine;

namespace BreadCrumbs
{
    public class ModEntryPoint : IMod
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        bool feedOut = false;
        Vector2 targetVector;
        Point pointOfCrumbs;

        string SoundFileName;
        Image theImage;

        int tickCount;
        // Gets called automatically, passes in a class that contains pointers to
        // useful functions we need to interface with the goose.
        void IMod.Init()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string PicFileName = Path.Combine(assemblyFolder, "crumbs.png");
            SoundFileName = Path.Combine(assemblyFolder, "nom.wav");
            theImage = Image.FromFile(PicFileName);

            // Subscribe to whatever events we want
            InjectionPoints.PostTickEvent += PostTick;
            InjectionPoints.PreRenderEvent += PreRenderEvent;
        }

        private void PreRenderEvent(GooseEntity goose, Graphics g)
        {
            if (feedOut)
            {
                g.DrawImage(theImage, pointOfCrumbs.X, pointOfCrumbs.Y, 80, 80);
            }
            
        }

        public void PostTick(GooseEntity g)
        {

            // Do whatever you want here.
            if ((GetAsyncKeyState(Keys.RShiftKey) != 0))
            {
                if (!feedOut)
                {
                    feedOut = true;
                    targetVector = new Vector2(Input.mouseX-20, Input.mouseY+20);
                    pointOfCrumbs = new Point(Cursor.Position.X - 42, Cursor.Position.Y - 42);

                    API.Goose.playHonckSound();
                    g.targetPos = targetVector;
                    API.Goose.setTaskRoaming(g);
                    API.Goose.setSpeed(g, GooseEntity.SpeedTiers.Charge);
                }
            }
            
            if (feedOut)
            {
                if (API.Goose.isGooseAtTarget(g,10))
                {
                    tickCount += 1;
                    g.direction = -20;
                    API.Goose.setSpeed(g, GooseEntity.SpeedTiers.Walk);
                    API.Goose.setTaskRoaming(g);

                    if (tickCount == 240)
                    {
                        new Thread(() => {
                            using (SoundPlayer player = new SoundPlayer(SoundFileName))
                            {
                                player.PlaySync();
                            }
                        }).Start(); 

                        feedOut = false;
                        tickCount = 0;
                        g.targetPos = targetVector + new Vector2(20, -20);
                    }   
                }
                else
                {
                    API.Goose.setTaskRoaming(g);
                    API.Goose.setSpeed(g, GooseEntity.SpeedTiers.Charge);
                    g.targetPos = targetVector;
                }
            }
        }
    }
}
