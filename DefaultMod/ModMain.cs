using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using GooseShared;
using SamEngine;

namespace BreadCrumbs
{
    public class ModEntryPoint : IMod
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        private bool feedOut = false;
        private Vector2 targetVector;
        private Point pointOfCrumbs;

        private string SoundFileName;
        private Image theImage;

        private int tickCount;

        void IMod.Init()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string PicFileName = Path.Combine(assemblyFolder, "crumbs.png");
            SoundFileName = Path.Combine(assemblyFolder, "nom.wav");
            theImage = Image.FromFile(PicFileName);

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
            if ((GetAsyncKeyState(Keys.RShiftKey) != 0) && !feedOut)
            {
                feedOut = true;
                targetVector = new Vector2(Input.mouseX-20, Input.mouseY+20);
                pointOfCrumbs = new Point(Cursor.Position.X - 42, Cursor.Position.Y - 42);

                API.Goose.playHonckSound();
                g.targetPos = targetVector;
                API.Goose.setTaskRoaming(g);
                API.Goose.setSpeed(g, GooseEntity.SpeedTiers.Charge);
            }
            if (feedOut)
            {
                if (API.Goose.isGooseAtTarget(g,10))
                {
                    tickCount++;
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
