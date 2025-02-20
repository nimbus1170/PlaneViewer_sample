﻿//
//
//
//---------------------------------------------------------------------------
using DSF_NET_Scene;

using System;
using System.Windows.Forms;
//---------------------------------------------------------------------------
namespace GeoViewer_sample
{
	//---------------------------------------------------------------------------
	public partial class GeoViewerControllerForm : Form
	{
		// ◆関係フォームの依存関係(作成順)のためコンストラクタで指定できないのでreadonlyやprivateにできない。
		public CGeoViewer Viewer;

		public GeoViewerControllerForm(in CGeoViewer viewer)
		{
			InitializeComponent();

			Viewer = viewer;
		}

		private void ObjXScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.ObjXScrollBar_Scroll().DrawScene();
		}

		private void ObjYScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.ObjYScrollBar_Scroll().DrawScene();
		}

		private void DistanceScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.DistanceScrollBar_Scroll().DrawScene();
		}

		private void AngleScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.AngleScrollBar_Scroll().DrawScene();
		}

		private void DirScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.DirScrollBar_Scroll().DrawScene();
		}

		private void ObserverXScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.ObserverXScrollBar_Scroll().DrawScene();
		}

		private void ObserverYScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.ObserverYScrollBar_Scroll().DrawScene();
		}

		private void ObserverAltitudeScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			Viewer?.ObserverAltitudeScrollBar_Scroll().DrawScene();
		}

		private void Height0Button_Click(object sender, EventArgs e)
		{
			//Viewer.
		}
	}
	//---------------------------------------------------------------------------
}
