using System;
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
        private string keyBind;
        private int imageSize;
        void IMod.Init()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string PicFileName = Path.Combine(assemblyFolder, "crumbs.png");
            SoundFileName = Path.Combine(assemblyFolder, "nom.wav");
            theImage = Image.FromFile(PicFileName);

            CheckConfig(assemblyFolder);

            InjectionPoints.PostTickEvent += PostTick;
            InjectionPoints.PreRenderEvent += PreRenderEvent;
        }

        private void CheckConfig(string assemFolder)
        {
            string path = Path.Combine(assemFolder, "Config.txt");
            try
            {
                using (TextReader textReader = new StreamReader(new FileStream(path, FileMode.Open)))
                {
                    string text;
                    while ((text = textReader.ReadLine()) != null)
                    {
                        if (text.StartsWith("KeyName"))
                        {
                            int num = text.IndexOf("=") + 1;
                            keyBind = text.Substring(num, text.Length - num).Trim();
                        }
                        if (text.StartsWith("ImageSize"))
                        {
                            int num = text.IndexOf("=") + 1;
                            imageSize = Convert.ToInt32(text.Substring(num, text.Length - num).Trim());
                        }
                    }
                }
            }
            catch
            {
                using (StreamWriter streamWriter = File.Exists(path) ? File.AppendText(path) : File.CreateText(path))
                {
                    MessageBox.Show("Config.txt for BreadCrumbs was not found.\nMaking config file please restart", "Mod Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    streamWriter.WriteLine("KeyName=RShiftKey");
                    streamWriter.WriteLine("ImageSize=80");
                }
            }
        }

        private void PreRenderEvent(GooseEntity goose, Graphics g)
        {
            if (feedOut)
            {
                g.DrawImage(theImage, pointOfCrumbs.X, pointOfCrumbs.Y, imageSize, imageSize);
            } 
        }

        public void PostTick(GooseEntity g)
        {
            if ((GetAsyncKeyState((Keys)Enum.Parse(typeof(Keys), keyBind, true)) != 0) && !feedOut)
            {
                feedOut = true;
                targetVector = new Vector2(Input.mouseX- (imageSize / 4), Input.mouseY+(imageSize / 4));
                pointOfCrumbs = new Point(Cursor.Position.X - (imageSize / 2), Cursor.Position.Y - (imageSize / 2));

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
