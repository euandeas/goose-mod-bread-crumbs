using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        int targetX;
        int targetY;

        Form f;
        // Gets called automatically, passes in a class that contains pointers to
        // useful functions we need to interface with the goose.
        void IMod.Init()
        {
            // Subscribe to whatever events we want
            InjectionPoints.PostTickEvent += PostTick;
        }

        public void PostTick(GooseEntity g)
        {
            // Do whatever you want here.
            if (GetAsyncKeyState(Keys.T) != 0)
            {
                if (!feedOut)
                {
                    int mouseX = Cursor.Position.X;
                    int mouseY = Cursor.Position.Y;
                    targetX = Input.mouseX;
                    targetY = Input.mouseY;

                    CreateForm();
                    f.BringToFront();
                    f.DesktopLocation = new Point(mouseX - 42, mouseY - 42);
                    f.Controls.Add(new PictureBox() { ImageLocation = @"crumbs.jpg", SizeMode = PictureBoxSizeMode.AutoSize });
                    f.Show();
                    g.targetPos = new Vector2(targetX,targetY);
                    feedOut = true;
                }
            }
            
            if (feedOut)
            {
                g.targetPos = new Vector2(targetX, targetY);

                if (IsGooseOnFood(g))
                {
                    f.Dispose();
                    feedOut = false;
                }
            }

        }

        private void CreateForm()
        {
            f = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                Size = new Size(85, 85),
                StartPosition = FormStartPosition.Manual,
                MinimumSize = new Size(80, 80)
            };
        }

        public bool IsGooseOnFood(GooseEntity g)
        {
            float xDif = g.position.x - g.targetPos.x;
            float yDif = g.position.y - g.targetPos.y;

            using (StreamWriter sr = new StreamWriter("CDriveDirs.txt"))
            {
                sr.WriteLine(xDif);
                sr.WriteLine(yDif);
                sr.WriteLine("");
            }


            if (((-50 < xDif) && (xDif < 50)) && ((-50 < yDif) && (yDif < 50)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
