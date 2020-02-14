using System;
using System.Collections.Generic;
using System.Drawing;
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
        // Gets called automatically, passes in a class that contains pointers to
        // useful functions we need to interface with the goose.
        void IMod.Init()
        {
            // Subscribe to whatever events we want
            InjectionPoints.PostTickEvent += PostTick;
            InjectionPoints.PostRenderEvent += postRender;
        }

        private void postRender(GooseEntity goose, Graphics g)
        {
            // Do whatever you want here.
            if (GetAsyncKeyState(Keys.T) != 0)
            {
                if (!feedOut)
                {
                    var f = new Form
                    {
                        FormBorderStyle = FormBorderStyle.None,
                        Size = new System.Drawing.Size(85, 85),
                        Location = new System.Drawing.Point(200, 200)
                    };
                    f.Controls.Add(new PictureBox() { ImageLocation = @"crumbs.png", SizeMode = PictureBoxSizeMode.AutoSize });
                    f.Show();
                    feedOut = true;
                }
            }
        }

        public void PostTick(GooseEntity g)
        {

        }
    }
}
