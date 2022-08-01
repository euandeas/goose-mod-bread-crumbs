﻿using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
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

        private SoundPlayer soundplayer;
        private Image img;

        private int tickCount;
        private string keyBind;
        private int imgWidth;
        private int imgHeight;
        
        void IMod.Init()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string PicFileName = Path.Combine(assemblyFolder, "crumbs.png");
            string SoundFileName = Path.Combine(assemblyFolder, "nom.wav");

            soundplayer = new SoundPlayer(SoundFileName);
            img = Image.FromFile(PicFileName);
            
            imgWidth = img.Width;
            imgHeight = img.Height;

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
                    }
                }
            }
            catch
            {
                using (StreamWriter streamWriter = File.Exists(path) ? File.AppendText(path) : File.CreateText(path))
                {
                    MessageBox.Show("Config.txt for BreadCrumbs was not found.\nMaking config file please restart", "Mod Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    streamWriter.WriteLine("KeyName=RShiftKey");
                }
            }
        }

        private void PreRenderEvent(GooseEntity goose, Graphics g)
        {
            if (feedOut)
            {
                g.DrawImage(img, pointOfCrumbs.X, pointOfCrumbs.Y, imgWidth, imgHeight);
            } 
        }

        public void PostTick(GooseEntity g)
        {
            if ((GetAsyncKeyState((Keys)Enum.Parse(typeof(Keys), keyBind, true)) != 0) && !feedOut)
            {
                feedOut = true;
                targetVector = new Vector2(Input.mouseX- (imgWidth / 4), Input.mouseY+(imgHeight / 4));
                pointOfCrumbs = new Point(Cursor.Position.X - (imgWidth / 2), Cursor.Position.Y - (imgHeight / 2));
                g.targetPos = targetVector;

                API.Goose.playHonckSound();
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
                        soundplayer.Play();

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
