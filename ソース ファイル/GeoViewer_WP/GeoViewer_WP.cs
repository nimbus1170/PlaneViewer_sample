﻿//
// GeoViewer_WP.cs
// 地形ビューア(ワールドピクセル単位)
//
//---------------------------------------------------------------------------
using DSF_NET_Geography;
using DSF_NET_Geometry;
using DSF_NET_Map;
using DSF_NET_Scene;

using static DSF_NET_Geography.Convert_LgLt_WPInt;
using static DSF_NET_Geography.Convert_LgLt_WP;
using static DSF_NET_Geography.XMapTile;
using static DSF_NET_TacticalDrawing.XMLReader;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using static System.Convert;
using static System.Drawing.Drawing2D.DashStyle;
//---------------------------------------------------------------------------
namespace PlaneViewer_sample
{
//---------------------------------------------------------------------------
public partial class PlaneViewerMainForm : Form
{
	// 地表面プリミティブ作成でWP単位の座標を得るため。
	// ◆GeoViewerDrawShapes_LgLtにWPの要素が出てきている。
	int PolygonZoomLevel;

	void Run_GeoViewer_WP()
	{
		Profiler.Lap("run Viewer_WP");

		//--------------------------------------------------
		// ◆設定をハードコードする場合
		{
/*			var title = "糸島半島～福岡市";
			var ct = new CLgLt(new CLg(130.2), new CLt(33.6));
			var polygon_zoom_level = 10;
			var img_zoom_level = 15;
*/		}

		//--------------------------------------------------
		// 設定を設定ファイルで与える場合
		// 座標範囲s_lgltとe_lgltを含むタイルを連結表示する。
		// ●１個タイルの頂点数は256x256なので、view_zoom_levelでポリゴンサイズが決まる。
		// ◆設定未定義については例外が出る。
		// →▼設定誤りに対しては詳細なメッセージを表示するようにしたい。

		var cfg_xml = new XmlDocument();

		cfg_xml.Load(CfgFileName);

		var plane_viewer_cfg_xml = cfg_xml.SelectSingleNode("PlaneViewerCfg");

		var map_cfg_xml = plane_viewer_cfg_xml.SelectSingleNode("MapCfg");

		var title =	map_cfg_xml.Attributes["Title"].InnerText;

	//	var polygon_zoom_level = ToInt32(map_cfg_xml.Attributes["PolygonZoomLevel"].InnerText);
		PolygonZoomLevel   = ToInt32(map_cfg_xml.Attributes["PolygonZoomLevel"].InnerText);
		var img_zoom_level = ToInt32(map_cfg_xml.Attributes["ImageZoomLevel"  ].InnerText);

		// クランプ前の開始・終了経緯度座標
		// ◆プレーンをWP単位にクランプするので、クランプ前のものを使用しないようにする。
		var s_lglt_0 = ReadLgLt(map_cfg_xml.SelectSingleNode("Start"));
		var e_lglt_0 = ReadLgLt(map_cfg_xml.SelectSingleNode("End"  ));
		
		var map_data_fld = plane_viewer_cfg_xml.SelectSingleNode("MapData").Attributes["Folder"].InnerText;

		var gsi_img_tile_fld = plane_viewer_cfg_xml.SelectSingleNode("GSIImageTiles").Attributes["Folder"].InnerText;
		var gsi_img_tile_ext = plane_viewer_cfg_xml.SelectSingleNode("GSIImageTiles").Attributes["Ext"	 ].InnerText;

		var gsi_ev_tile_fld = plane_viewer_cfg_xml.SelectSingleNode("GSIElevationTiles").Attributes["Folder"].InnerText;

		var gsi_geoid_model_file = plane_viewer_cfg_xml.SelectSingleNode("GSIGeoidModel").Attributes["File"].InnerText;

		var grid_cfg_xml = plane_viewer_cfg_xml.SelectSingleNode("Grid");

		var grid_font_size = ToInt32(grid_cfg_xml.Attributes["FontSize"].InnerText);

		var grid_ol_cfg = grid_cfg_xml.SelectSingleNode("GridOverlay");

		//--------------------------------------------------

		Text = title;

		//--------------------------------------------------
		// 0 タイルをダウンロードする。
		// ◆GeoViewer_Tileと比較して、正しいか？

		Profiler.Lap("download tiles");

		// ◆ここはクランプ前で良いのか？
		DownloadGSITiles(s_lglt_0, e_lglt_0, img_zoom_level, map_data_fld);

		Profiler.Lap("- tiles downloaded");

		//--------------------------------------------------
		// 0 開始・終了座標について、経緯度座標をクランプしてWP座標を作成するとともに、WP座標にクランプされた経緯度座標を作成する。

		// ◆緯度方向を逆転させる。緯度方向の逆転を一般化するには範囲にする必要があるか。
		var s_wp = new CWPInt(ToWPIntX(PolygonZoomLevel, s_lglt_0.Lg), ToWPIntY(PolygonZoomLevel, e_lglt_0.Lt));
		var e_wp = new CWPInt(ToWPIntX(PolygonZoomLevel, e_lglt_0.Lg), ToWPIntY(PolygonZoomLevel, s_lglt_0.Lt));

		// ◆南北は逆転している。WPの南北逆転を一律に変換できないか？範囲クラスにするべきか。
		var s_lglt = new CLgLt(ToLg(s_wp.X), ToLt(e_wp.Y));
		var e_lglt = new CLgLt(ToLg(e_wp.X), ToLt(s_wp.Y));

		//--------------------------------------------------
		// 1 標高地図データを作成する。

		Profiler.Lap("build elevation map data");

		// 開始・終了座標は標高データのズームレベルの新規インスタンスにする。
		// ◆標高データのズームレベルは取り敢えず14のみ。
		var ev_map_data = new CElevationMapData_GSI_DEM_PNG
			(gsi_ev_tile_fld,
			 GetTile(new CWPInt(s_wp, 14)),
			 GetTile(new CWPInt(e_wp, 14)));

		Profiler.Lap("- elevation map data built");

		// 高度クラスに標高地図データを設定することにより、座標オブジェクトに標高が自動設定される。
		CAltitude.SetElevationMapData(ev_map_data);

		//--------------------------------------------------
		// 2 ジオイドデータを作成する。

		Profiler.Lap("build geoid map data");

		// ◆例外ではなくジオイドを無視するようにしろ。
	//	if(!(File.Exists(gsi_geoid_model_file))) throw new Exception("geoid model file not found");

		var geoid_map_data = new CGSIGeoidMapData(gsi_geoid_model_file);

		Profiler.Lap("- geoid map data built");

		// 高度クラスにジオイドデータを設定することにより、座標オブジェクトにジオイド高が自動設定される。
		// ◆WP版では高度クラスに直接反映しない。
		CAltitude.SetGeoidMapData(geoid_map_data);

		//--------------------------------------------------
		// 3 地図画像データを作成する。

		// 地図画像データのズームレベルにするので新規インスタンスにする。
		var img_s_wp = new CWPInt(s_wp, img_zoom_level);
		var img_e_wp = new CWPInt(e_wp, img_zoom_level);

		//--------------------------------------------------
		// 3.1 地図画像を作成する。

		Profiler.Lap("build map image");

		var map_img = GSIImageTile.MakeMapImageFromGSITiles(gsi_img_tile_fld, gsi_img_tile_ext, img_s_wp, img_e_wp);

		Profiler.Lap("- map image built");

		//--------------------------------------------------
		// 3.2 グリッドを描画する。
		
		// グリッドオーバレイを作成するようになっていなければ地図に描画する。
		if(grid_ol_cfg == null)
		{
			DrawLgLtGrid(map_img, s_lglt, e_lglt, grid_font_size);
			DrawUTMGrid (map_img, s_lglt, e_lglt, grid_font_size);
		}

		//--------------------------------------------------
		// 3.3 画像地図データを作成する。

		Profiler.Lap("build image map data");

		var img_map_data = new CImageMapData_WP(map_img, img_s_wp, img_e_wp);

		Profiler.Lap("- image map data built");

		// ◆テスト
		{
/*			// ◆地図画像は作成できていると思われる。グリッドが足りないように見えるが、タイルから切り出す前だからか？そうでもない？
			var map_img_test_form = new MapImageTestForm();
			map_img_test_form.pictureBox1.Image = map_img;
			map_img_test_form.Show();
*/		}

		//--------------------------------------------------
		// 4 ビューアフォームを作成する。

		Profiler.Lap("build viewer form");

		// ◆viewer_form.Viewerはnullであり、後で設定する。
		var viewer_form = new GeoViewViewerForm_WP();

		Profiler.Lap("- viewer form built");

		viewer_form.Text = title;

		// シーンの作成状況等を表示するため先に表示しておく。
		// ◆シーン作成状況の表示は整理されていないのでシーンの作成状況は実質的に表示できない。
		//  (途中で作成状況の表示(進行)が止まる。)
		viewer_form.Show();

		//--------------------------------------------------
		// 5 ビューアパラメータを作成する。← 1,2,3,4

		var viewer_params = new CViewerParams_WP();

		{
			viewer_params.viewer_control = viewer_form.PictureBox;
			viewer_params.s_wp = s_wp;
			viewer_params.e_wp = e_wp;
			viewer_params.ev_map_data = ev_map_data;
			viewer_params.geoid_map_data = geoid_map_data;
			viewer_params.img_map_data = img_map_data;
			viewer_params.options = "view_tri_polygons";
		}

		//--------------------------------------------------
		// 6 コントローラフォームを作成する。

		Profiler.Lap("build controller form");

		// ◆controller_form.Viewerはnullであり、後で設定する。
		var controller_form = new GeoViewerControllerForm_LgLt(GeoViewer_LgLt);

		Profiler.Lap("- controller form built");

		controller_form.Text = title;

		controller_form.Show();

		//--------------------------------------------------
		// 7 コントローラパラメータを作成する。← 6

		var controller_parts = new CControllerParts();

		{
			controller_parts.ObjXTextBox   = controller_form.ObjXTextBox;
			controller_parts.ObjXScrollBar = controller_form.ObjXScrollBar;
			controller_parts.MaxObjXLabel  = controller_form.MaxObjXLabel;
			controller_parts.MinObjXLabel  = controller_form.MinObjXLabel;

			controller_parts.ObjYTextBox   = controller_form.ObjYTextBox;
			controller_parts.ObjYScrollBar = controller_form.ObjYScrollBar;
			controller_parts.MaxObjYLabel  = controller_form.MaxObjYLabel;
			controller_parts.MinObjYLabel  = controller_form.MinObjYLabel;

			controller_parts.DirScrollBar = controller_form.DirScrollBar;
			controller_parts.DirTextBox   = controller_form.DirTextBox;

			controller_parts.DistanceScrollBar = controller_form.DistanceScrollBar;
			controller_parts.DistanceTextBox   = controller_form.DistanceTextBox;

			controller_parts.AngleScrollBar = controller_form.AngleScrollBar;
			controller_parts.AngleTextBox	= controller_form.AngleTextBox;

			controller_parts.ObserverXScrollBar = controller_form.ObserverXScrollBar;
			controller_parts.ObserverXTextBox	= controller_form.ObserverXTextBox;
			controller_parts.MaxObserverXLabel  = controller_form.MaxObserverXLabel;
			controller_parts.MinObserverXLabel  = controller_form.MinObserverXLabel;

			controller_parts.ObserverYScrollBar = controller_form.ObserverYScrollBar;
			controller_parts.ObserverYTextBox	= controller_form.ObserverYTextBox;
			controller_parts.MaxObserverYLabel  = controller_form.MaxObserverYLabel;
			controller_parts.MinObserverYLabel  = controller_form.MinObserverYLabel;

			controller_parts.ObserverAltitudeScrollBar = controller_form.ObserverAltitudeScrollBar;
			controller_parts.ObserverAltitudeTextBox   = controller_form.ObserverAltitudeTextBox;
		}

		//--------------------------------------------------
		// 8 表示設定を作成する。
		// ◆表示設定フォームにはビューアを設定するためビューア作成後に作成する。

		var scene_cfg = new CSceneCfg
			(0.6f, // 環境光反射係数 [0,1]
		 	 0.5f, // 鏡面反射係数   [0,1]
			 64,   // ハイライト     [0,128]
			 DShadingMode.SHADING_MAPPING, // ◆SHADING_FLATとかだと例外が出る。
			 DFogMode.FOG_NO,
			 3000f); // 視程(m)

		//--------------------------------------------------
		// 9 ビューアを作成する。← 5,7,8
		// ▼ここの設定順序には依存関係があるので、整理してライブラリに収めろ。ユーザプログラミングに晒すな。
		// →◆ユーザプログラミングでフォーム(コントロール)が作成されるので、その部分は別になるが。

		Profiler.Lap("build viewer");

		GeoViewer_WP = new CGeoViewer_WP(viewer_params, scene_cfg, controller_parts, Info, Profiler);

		Profiler.Lap("- viewer built");

		//--------------------------------------------------
		// 10 ビューアフォームとコントローラフォームにビューアを設定する。← 9

		viewer_form	   .Viewer =
		controller_form.Viewer = GeoViewer_WP;

		//--------------------------------------------------
		// 11 表示設定フォームを作成する。← 9
		// ◆コンストラクタで対象ビューアを設定しているためここで作成する必要がある。
		// →◆コントローラフォームとビューアは双方向なのでビューアにコントロールを設定するためにビューアより先に
		// 　　作成する必要があり、そのため、コントローラフォームには後でビューアを設定しなければならない。
		// 　　他方、設定フォームはビューア作成時には不要なので、ビューア作成後に設定フォームを作成し、その際に
		// 　　ビューアを設定すれば良いのでここだけにある。
		// 　　→◆設定フォームの初期値はビューアから取得しており、ビューアへは表示設定パラメータで与えている。
		// 　　　　ビューアへの表示設定は、ビューアを経由して設定フォームに設定されているが、設定フォームからの
		// 　　　　設定に統一すべきではないか？

		var cfg_form = new GeoViewerCfgForm(GeoViewer_WP);

		cfg_form.Text = title;

		cfg_form.Show();

		//--------------------------------------------------
		// 12 シーンを描画する。← 9

		Profiler.Lap("create scene");

		GeoViewer_WP.CreateScene();

		Profiler.Lap("- scene created");

		//--------------------------------------------------
		// 13 図形を描画する。← 9

		Profiler.Lap("draw shapes");

		GeoViewerDrawShapes_LgLt(GeoViewer_WP);

		Profiler.Lap("- shapes drawn");

		//--------------------------------------------------
		// 14 オーバレイプレーンを描画する。← 9

		//--------------------------------------------------
		// 14.1 地図を半透明にして重ねてみる。
		if(false)
		{
			Profiler.Lap("draw map overlay");
			GeoViewer_WP.AddOverlayPlane(img_map_data, 1000.0, 0.5f);
			Profiler.Lap("- map overlay drawn");
		}

		//--------------------------------------------------
		// 14.2 グリッドをオーバレイに描画する。

		if(grid_ol_cfg != null)
		{
			Profiler.Lap("draw grid overlay");

			// オーバレイのサイズの基準(小さい辺をこのサイズにする。)
			var ol_size = ToInt32(grid_ol_cfg.Attributes["Size"].InnerText);
			int ol_w = (map_img.Height > map_img.Width )? ol_size: (ol_size * map_img.Width  / map_img.Height);
			int ol_h = (map_img.Width  > map_img.Height)? ol_size: (ol_size * map_img.Height / map_img.Width ); 

			// ◆テクスチャサイズは任意だが、フォントサイズ等がテクスチャサイズに引きずられる。GraphicsUnit.Pixel以外でも。
			var grid_map_img = new Bitmap(ol_w, ol_h);

			DrawLgLtGrid(grid_map_img, s_lglt, e_lglt, grid_font_size);
			DrawUTMGrid (grid_map_img, s_lglt, e_lglt, grid_font_size);

			// 地表面プレーンからの高さ
		 	var ol_offset = ToDouble(grid_ol_cfg.Attributes["Offset"].InnerText);

			GeoViewer_WP.AddOverlayPlane
				(new CImageMapData_WP(grid_map_img, img_s_wp, img_e_wp),
				 ol_offset,
				 1.0f); // 透明度

			Profiler.Lap("- grid overlay drawn");
		}

		//--------------------------------------------------
		// 14.3 部分的にオーバレイプレーンを重ねてみる。
		if(true)
		{
			Profiler.Lap("draw overlay on Mt.Kayasan");

			// オーバレイのサイズの基準(小さい辺をこのサイズにする。)
			int ol_size = 1000;
			int ol_w = (map_img.Height > map_img.Width )? ol_size: (ol_size * map_img.Width  / map_img.Height);
			int ol_h = (map_img.Width  > map_img.Height)? ol_size: (ol_size * map_img.Height / map_img.Width ); 

			// 可也山
			var ol_s_lg = new CLg(new CDMS(130,  9, 0.0).Value); var ol_s_lt = new CLt(new CDMS( 33, 34, 0.0).Value);
			var ol_e_lg = new CLg(new CDMS(130, 10, 0.0).Value); var ol_e_lt = new CLt(new CDMS( 33, 35, 0.0).Value);

			// ◆オーバレイの範囲は南北逆転
			var ol = GeoViewer_WP.MakeOverlay
				(ToWPInt(PolygonZoomLevel, new CLgLt(ol_s_lg, ol_e_lt)),
				 ToWPInt(PolygonZoomLevel, new CLgLt(ol_e_lg, ol_s_lt)),
				 ol_w, ol_h);

			var p_from = ol.ToPointOnOverlay(ToWP(PolygonZoomLevel, new CLgLt(ol_s_lg, ol_s_lt)));
			var p_to   = ol.ToPointOnOverlay(ToWP(PolygonZoomLevel, new CLgLt(ol_e_lg, ol_e_lt)));

			var g = ol.GetGraphics();

			g.DrawRectangle(new Pen(Color.Red, 5.0f), 0, 0, ol_w, ol_h);

			g.DrawLine(new Pen(Color.Red, 5.0f), p_from, p_to);

			g.Dispose();
		
			GeoViewer_WP.AddOverlayPlane
				(ol,
				 200.0, // 地表面プレーンからの高さ
				 0.5f); // 透明度

			 Profiler.Lap("- overlay on Mt.Kayasan drawn");
		}

		//--------------------------------------------------

		Viewer = GeoViewer_WP;

		//--------------------------------------------------

		DisplayLog(s_lglt, e_lglt);
	}
}
//---------------------------------------------------------------------------
}
