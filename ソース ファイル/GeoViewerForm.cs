﻿//
//
//
//---------------------------------------------------------------------------
using DSF_NET_Scene;

using System;
using System.Drawing;
using System.Windows.Forms;
//---------------------------------------------------------------------------
namespace GeoViewer_sample
{
//---------------------------------------------------------------------------
public abstract partial class GeoViewerForm : Form
{
	// ◆関係フォームの依存関係(作成順)のためコンストラクタで指定できないのでreadonlyやprivateにできない。
	public CGeoViewer Viewer = null;

	public GeoViewerForm()
	{
		InitializeComponent();

		// マウスホイールの回転で近づく・遠ざかるプロシージャを追加する。
		// コーディングでの追加が必要
		PictureBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(PictureBox_MouseWheel);
	}

	private void PictureBox_Resize(object sender, EventArgs e)
	{
		Viewer?.Resize(PictureBox.Width, PictureBox.Height);
	}

	private void PictureBox_Paint(object sender, PaintEventArgs e)
	{
		Viewer?.DrawScene();
	}

	private void PictureBox_MouseDown(object sender, MouseEventArgs e)
	{
		switch(e.Button)
		{
			case MouseButtons.Middle:
				contextMenuStrip1.Show(Left + e.X, Top + e.Y);
				break;

			default:
				Viewer?.MouseDown(e);
				break;
		}
	}

	private void PictureBox_MouseMove(object sender, MouseEventArgs e)
	{
		Viewer?.MouseMove(e);
		ShowObjInfo();
	}

	private void PictureBox_MouseUp(object sender, MouseEventArgs e)
	{
		Viewer?.MouseUp(e);
	}

	private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
	{
		Viewer?.DistFB(-e.Delta * SystemInformation.MouseWheelScrollLines / 60); // ◆移動量は目分量	
	}

	private void PlaneViewerForm_LgLt_KeyPress(object sender, KeyPressEventArgs e)
	{
		if(e.KeyChar == 'w')
			Viewer?.MoveFB(10);
		else if(e.KeyChar == 's')
			Viewer?.MoveFB(-10);
		else
			return;
	}

	private void PlaneViewerForm_LgLt_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		switch(e.KeyCode)
		{
			case Keys.Up   : Viewer?.LookUD( 10); break;
			case Keys.Down : Viewer?.LookUD(-10); break;
			case Keys.Right: Viewer?.TurnRL(-10); break;
			case Keys.Left : Viewer?.TurnRL( 10); break;
		}
	}

	private void MarkerToolStripMenuItem_Click(Object sender, EventArgs e)
	{
		var item = (ToolStripMenuItem)sender;

		item.Checked = !(item.Checked);	

		Viewer?
			.SetMarkerMode(item.Checked? true: false)
			.DrawScene();
	}

	public abstract void ShowObjInfo();
}
//---------------------------------------------------------------------------
}
