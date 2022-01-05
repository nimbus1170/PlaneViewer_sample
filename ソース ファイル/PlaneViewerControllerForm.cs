﻿//
//
//
//---------------------------------------------------------------------------
using DSF_NET_Scene;

using System;
using System.Windows.Forms;
//---------------------------------------------------------------------------
namespace PlaneViewer_sample
{
//---------------------------------------------------------------------------
public partial class PlaneViewerControllerForm : Form
{
	public CPlaneViewer Viewer;

	public PlaneViewerControllerForm(CPlaneViewer viewer)
	{
		InitializeComponent();
		Viewer = viewer;
	}

	private void ObjXScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.ObjXScrollBar_Scroll();
	}

	private void ObjYScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.ObjYScrollBar_Scroll();
	}

	private void DistanceScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.DistanceScrollBar_Scroll();
	}

	private void AngleScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.AngleScrollBar_Scroll();
	}

	private void DirScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.DirScrollBar_Scroll();
	}

	private void ObserverXScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.ObserverXScrollBar_Scroll();
	}

	private void ObserverYScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.ObserverYScrollBar_Scroll();
	}

	private void ObserverAltitudeScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		Viewer?.ObserverAltitudeScrollBar_Scroll();
	}

	private void Height0Button_Click(object sender, EventArgs e)
	{
		//Viewer.
	}
}
//---------------------------------------------------------------------------
}
