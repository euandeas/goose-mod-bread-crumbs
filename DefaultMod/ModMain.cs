using GooseShared;
using SamEngine;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System;

public class ModMain : IMod
{
	private bool feedOut;

	private Vector2 targetVector;

	private Point pointOfCrumbs;

	private string SoundFileName;

	private Image theImage;

	private int tickCount;

	private Config config = new Config();

	KeysConverter kc = new KeysConverter();

	[DllImport("user32.dll")]
	public static extern short GetAsyncKeyState(Keys vKey);

	private void checkConfig() {
		string path = "Assets\\Mods\\BreadCrumbs\\Config.txt";

		try {
			using (TextReader textReader = new StreamReader(new FileStream(path, FileMode.Open))) {
				string text;
				while ((text = textReader.ReadLine()) != null) {
					if (text.StartsWith("KeyName")) {
						int num = text.IndexOf("=") + 1;
						string keybind = text.Substring(num, text.Length - num).Trim();
						config.KeyName = keybind; 
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

	void IMod.Init()
	{
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		string filename = Path.Combine(directoryName, "crumbs.png");
		SoundFileName = Path.Combine(directoryName, "nom.wav");
		theImage = Image.FromFile(filename);
		checkConfig();
		InjectionPoints.PostTickEvent += new InjectionPoints.PostTickEventHandler(this.PostTick);
		InjectionPoints.PreRenderEvent += new InjectionPoints.PreRenderEventHandler(this.PreRenderEvent);
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
		
		if (GetAsyncKeyState((Keys)Enum.Parse(typeof(Keys), config.KeyName, true)) != 0 && !feedOut)
		{
			feedOut = true;
			targetVector = new Vector2((float)(Input.mouseX - 20), (float)(Input.mouseY + 20));
			pointOfCrumbs = new Point(Cursor.Position.X - 42, Cursor.Position.Y - 42);
			API.Goose.playHonckSound.Invoke();
			g.targetPos = targetVector;
			API.Goose.setTaskRoaming.Invoke(g);
			API.Goose.setSpeed.Invoke(g, (GooseEntity.SpeedTiers)2);
		}
		if (!feedOut)
		{
			return;
		}
		if (API.Goose.isGooseAtTarget.Invoke(g, 10f))
		{
			tickCount++;
			g.direction = -20f;
			API.Goose.setSpeed.Invoke(g, (GooseEntity.SpeedTiers)0);
			API.Goose.setTaskRoaming.Invoke(g);
			if (tickCount == 240)
			{
				new Thread((ThreadStart)delegate
				{
					using (SoundPlayer soundPlayer = new SoundPlayer(SoundFileName))
					{
						soundPlayer.PlaySync();
					}
				}).Start();
				feedOut = false;
				tickCount = 0;
				g.targetPos = targetVector + new Vector2(20f, -20f);
			}
		}
		else
		{
			API.Goose.setTaskRoaming.Invoke(g);
			API.Goose.setSpeed.Invoke(g, (GooseEntity.SpeedTiers)2);
			g.targetPos = targetVector;
		}
	}
}
