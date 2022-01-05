﻿//
//
//
//---------------------------------------------------------------------------
using DSF_NET_Geography;
using DSF_NET_Geometry;
using DSF_NET_Scene;

using System.Drawing;
using System.Windows.Forms;
//---------------------------------------------------------------------------
namespace PlaneViewer_sample
{
//---------------------------------------------------------------------------
public partial class PlaneViewerMainForm : Form
{
	void XYZPlaneViewerDrawShapes()
	{
		//--------------------------------------------------
		// ブロック１
	
		var block_1_pos = new CCoord(5000.0, 5000.0, 500.0);

		XYZPlaneViewer.AddPrimitive
			(new CGLBlock(1000.0, block_1_pos)
//				.SetTex("C:/DSF/SharedData/Images/blockface.bmp", 0);
				.SetTex("./Images/blockface.bmp", 0));

		//--------------------------------------------------
		// ブロック２
			
		var block_2_pos = new CCoord(10000.0, 5000.0, 500.0);

		XYZPlaneViewer.AddPrimitive
			(new CGLBlock(1000.0, block_2_pos)
//				.SetTex("C:/DSF/SharedData/Images/blockface.bmp", 0);
				.SetTex("./Images/blockface.bmp", 0));

		//--------------------------------------------------
		// 放物線

		// 分割数10、射角800ミル
		XYZPlaneViewer.AddPrimitive
			(new CGLParabola(10, block_1_pos, block_2_pos, new CMil(800.0))
				.SetColor(1.0f, 0.0f, 0.0f, 0.5f)
				.SetLineWidth(1.0f));
	}
}
//---------------------------------------------------------------------------
}
