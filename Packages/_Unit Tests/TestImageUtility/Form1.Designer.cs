﻿namespace ImageUtility.UnitTests
{
	partial class TestForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.textBoxEXIF = new System.Windows.Forms.TextBox();
			this.tabControlTests = new System.Windows.Forms.TabControl();
			this.tabPageBuilding = new System.Windows.Forms.TabPage();
			this.buttonBuild4 = new System.Windows.Forms.Button();
			this.buttonBuild3 = new System.Windows.Forms.Button();
			this.buttonBuild2 = new System.Windows.Forms.Button();
			this.buttonBuild1 = new System.Windows.Forms.Button();
			this.panelBuild = new ImageUtility.UnitTests.PanelOutput(this.components);
			this.tabPageLoading = new System.Windows.Forms.TabPage();
			this.buttonLoad15 = new System.Windows.Forms.Button();
			this.buttonLoad14 = new System.Windows.Forms.Button();
			this.buttonLoad16 = new System.Windows.Forms.Button();
			this.buttonLoad13 = new System.Windows.Forms.Button();
			this.buttonLoad12 = new System.Windows.Forms.Button();
			this.buttonLoad11 = new System.Windows.Forms.Button();
			this.buttonLoad10 = new System.Windows.Forms.Button();
			this.buttonLoad9 = new System.Windows.Forms.Button();
			this.buttonLoad8 = new System.Windows.Forms.Button();
			this.buttonLoad7 = new System.Windows.Forms.Button();
			this.buttonLoad6 = new System.Windows.Forms.Button();
			this.buttonLoad5 = new System.Windows.Forms.Button();
			this.buttonLoad4 = new System.Windows.Forms.Button();
			this.buttonLoad3 = new System.Windows.Forms.Button();
			this.buttonLoad2 = new System.Windows.Forms.Button();
			this.buttonLoad1 = new System.Windows.Forms.Button();
			this.panelLoad = new ImageUtility.UnitTests.PanelOutput(this.components);
			this.tabPageLDR2HDR = new System.Windows.Forms.TabPage();
			this.radioButtonFilterCurveFitting = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterTent = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterGaussian = new System.Windows.Forms.RadioButton();
			this.radioButtonFilterNone = new System.Windows.Forms.RadioButton();
			this.buttonLDR2HDRRAW = new System.Windows.Forms.Button();
			this.buttonLDR3RAW = new System.Windows.Forms.Button();
			this.buttonLDR2HDRJPG = new System.Windows.Forms.Button();
			this.buttonLDR9JPG = new System.Windows.Forms.Button();
			this.buttonLDR5JPG = new System.Windows.Forms.Button();
			this.buttonLDR3JPG = new System.Windows.Forms.Button();
			this.textBoxHDR = new System.Windows.Forms.TextBox();
			this.panelOutputHDR = new ImageUtility.UnitTests.PanelOutput(this.components);
			this.tabPageColorProfiles = new System.Windows.Forms.TabPage();
			this.buttonProfile4 = new System.Windows.Forms.Button();
			this.buttonProfile3 = new System.Windows.Forms.Button();
			this.buttonProfile2 = new System.Windows.Forms.Button();
			this.buttonProfile1 = new System.Windows.Forms.Button();
			this.panelColorProfile = new ImageUtility.UnitTests.PanelOutput(this.components);
			this.tabPageDrawing = new System.Windows.Forms.TabPage();
			this.buttonDraw3 = new System.Windows.Forms.Button();
			this.buttonDraw2 = new System.Windows.Forms.Button();
			this.buttonDraw1 = new System.Windows.Forms.Button();
			this.panelDrawing = new ImageUtility.UnitTests.PanelOutput(this.components);
			this.tabControlTests.SuspendLayout();
			this.tabPageBuilding.SuspendLayout();
			this.tabPageLoading.SuspendLayout();
			this.tabPageLDR2HDR.SuspendLayout();
			this.tabPageColorProfiles.SuspendLayout();
			this.tabPageDrawing.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxEXIF
			// 
			this.textBoxEXIF.Location = new System.Drawing.Point(855, 71);
			this.textBoxEXIF.Multiline = true;
			this.textBoxEXIF.Name = "textBoxEXIF";
			this.textBoxEXIF.ReadOnly = true;
			this.textBoxEXIF.Size = new System.Drawing.Size(297, 568);
			this.textBoxEXIF.TabIndex = 1;
			// 
			// tabControlTests
			// 
			this.tabControlTests.Controls.Add(this.tabPageBuilding);
			this.tabControlTests.Controls.Add(this.tabPageLoading);
			this.tabControlTests.Controls.Add(this.tabPageLDR2HDR);
			this.tabControlTests.Controls.Add(this.tabPageColorProfiles);
			this.tabControlTests.Controls.Add(this.tabPageDrawing);
			this.tabControlTests.Location = new System.Drawing.Point(13, 13);
			this.tabControlTests.Name = "tabControlTests";
			this.tabControlTests.SelectedIndex = 0;
			this.tabControlTests.Size = new System.Drawing.Size(1170, 763);
			this.tabControlTests.TabIndex = 2;
			// 
			// tabPageBuilding
			// 
			this.tabPageBuilding.Controls.Add(this.buttonBuild4);
			this.tabPageBuilding.Controls.Add(this.buttonBuild3);
			this.tabPageBuilding.Controls.Add(this.buttonBuild2);
			this.tabPageBuilding.Controls.Add(this.buttonBuild1);
			this.tabPageBuilding.Controls.Add(this.panelBuild);
			this.tabPageBuilding.Location = new System.Drawing.Point(4, 22);
			this.tabPageBuilding.Name = "tabPageBuilding";
			this.tabPageBuilding.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageBuilding.Size = new System.Drawing.Size(1162, 737);
			this.tabPageBuilding.TabIndex = 1;
			this.tabPageBuilding.Text = "Building";
			this.tabPageBuilding.UseVisualStyleBackColor = true;
			// 
			// buttonBuild4
			// 
			this.buttonBuild4.Location = new System.Drawing.Point(396, 17);
			this.buttonBuild4.Name = "buttonBuild4";
			this.buttonBuild4.Size = new System.Drawing.Size(124, 23);
			this.buttonBuild4.TabIndex = 4;
			this.buttonBuild4.Text = "Stress Test";
			this.buttonBuild4.UseVisualStyleBackColor = true;
			this.buttonBuild4.Click += new System.EventHandler(this.buttonBuild4_Click);
			// 
			// buttonBuild3
			// 
			this.buttonBuild3.Location = new System.Drawing.Point(266, 17);
			this.buttonBuild3.Name = "buttonBuild3";
			this.buttonBuild3.Size = new System.Drawing.Size(124, 23);
			this.buttonBuild3.TabIndex = 4;
			this.buttonBuild3.Text = "Buddhabrot";
			this.buttonBuild3.UseVisualStyleBackColor = true;
			this.buttonBuild3.Click += new System.EventHandler(this.buttonBuild3_Click);
			// 
			// buttonBuild2
			// 
			this.buttonBuild2.Location = new System.Drawing.Point(136, 17);
			this.buttonBuild2.Name = "buttonBuild2";
			this.buttonBuild2.Size = new System.Drawing.Size(124, 23);
			this.buttonBuild2.TabIndex = 4;
			this.buttonBuild2.Text = "Fill Scanlines";
			this.buttonBuild2.UseVisualStyleBackColor = true;
			this.buttonBuild2.Click += new System.EventHandler(this.buttonBuild2_Click);
			// 
			// buttonBuild1
			// 
			this.buttonBuild1.Location = new System.Drawing.Point(6, 17);
			this.buttonBuild1.Name = "buttonBuild1";
			this.buttonBuild1.Size = new System.Drawing.Size(124, 23);
			this.buttonBuild1.TabIndex = 4;
			this.buttonBuild1.Text = "Fill Pixels";
			this.buttonBuild1.UseVisualStyleBackColor = true;
			this.buttonBuild1.Click += new System.EventHandler(this.buttonBuild1_Click);
			// 
			// panelBuild
			// 
			this.panelBuild.Bitmap = null;
			this.panelBuild.Location = new System.Drawing.Point(6, 86);
			this.panelBuild.Name = "panelBuild";
			this.panelBuild.Size = new System.Drawing.Size(842, 568);
			this.panelBuild.TabIndex = 3;
			// 
			// tabPageLoading
			// 
			this.tabPageLoading.Controls.Add(this.buttonLoad15);
			this.tabPageLoading.Controls.Add(this.buttonLoad14);
			this.tabPageLoading.Controls.Add(this.buttonLoad16);
			this.tabPageLoading.Controls.Add(this.buttonLoad13);
			this.tabPageLoading.Controls.Add(this.buttonLoad12);
			this.tabPageLoading.Controls.Add(this.buttonLoad11);
			this.tabPageLoading.Controls.Add(this.buttonLoad10);
			this.tabPageLoading.Controls.Add(this.buttonLoad9);
			this.tabPageLoading.Controls.Add(this.buttonLoad8);
			this.tabPageLoading.Controls.Add(this.buttonLoad7);
			this.tabPageLoading.Controls.Add(this.buttonLoad6);
			this.tabPageLoading.Controls.Add(this.buttonLoad5);
			this.tabPageLoading.Controls.Add(this.buttonLoad4);
			this.tabPageLoading.Controls.Add(this.buttonLoad3);
			this.tabPageLoading.Controls.Add(this.buttonLoad2);
			this.tabPageLoading.Controls.Add(this.buttonLoad1);
			this.tabPageLoading.Controls.Add(this.textBoxEXIF);
			this.tabPageLoading.Controls.Add(this.panelLoad);
			this.tabPageLoading.Location = new System.Drawing.Point(4, 22);
			this.tabPageLoading.Name = "tabPageLoading";
			this.tabPageLoading.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageLoading.Size = new System.Drawing.Size(1162, 737);
			this.tabPageLoading.TabIndex = 0;
			this.tabPageLoading.Text = "Loading";
			this.tabPageLoading.UseVisualStyleBackColor = true;
			// 
			// buttonLoad15
			// 
			this.buttonLoad15.Location = new System.Drawing.Point(386, 35);
			this.buttonLoad15.Name = "buttonLoad15";
			this.buttonLoad15.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad15.TabIndex = 2;
			this.buttonLoad15.Text = "EXR RGB32F";
			this.buttonLoad15.UseVisualStyleBackColor = true;
			this.buttonLoad15.Click += new System.EventHandler(this.buttonLoad15_Click);
			// 
			// buttonLoad14
			// 
			this.buttonLoad14.Location = new System.Drawing.Point(291, 35);
			this.buttonLoad14.Name = "buttonLoad14";
			this.buttonLoad14.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad14.TabIndex = 2;
			this.buttonLoad14.Text = "HDR RGBE";
			this.buttonLoad14.UseVisualStyleBackColor = true;
			this.buttonLoad14.Click += new System.EventHandler(this.buttonLoad14_Click);
			// 
			// buttonLoad16
			// 
			this.buttonLoad16.Location = new System.Drawing.Point(481, 35);
			this.buttonLoad16.Name = "buttonLoad16";
			this.buttonLoad16.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad16.TabIndex = 2;
			this.buttonLoad16.Text = "TIFF RGB16F";
			this.buttonLoad16.UseVisualStyleBackColor = true;
			this.buttonLoad16.Click += new System.EventHandler(this.buttonLoad16_Click);
			// 
			// buttonLoad13
			// 
			this.buttonLoad13.Location = new System.Drawing.Point(101, 35);
			this.buttonLoad13.Name = "buttonLoad13";
			this.buttonLoad13.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad13.TabIndex = 2;
			this.buttonLoad13.Text = "TIFF RGB16";
			this.buttonLoad13.UseVisualStyleBackColor = true;
			this.buttonLoad13.Click += new System.EventHandler(this.buttonLoad13_Click);
			// 
			// buttonLoad12
			// 
			this.buttonLoad12.Location = new System.Drawing.Point(6, 35);
			this.buttonLoad12.Name = "buttonLoad12";
			this.buttonLoad12.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad12.TabIndex = 2;
			this.buttonLoad12.Text = "TIFF RGB8";
			this.buttonLoad12.UseVisualStyleBackColor = true;
			this.buttonLoad12.Click += new System.EventHandler(this.buttonLoad12_Click);
			// 
			// buttonLoad11
			// 
			this.buttonLoad11.Location = new System.Drawing.Point(956, 6);
			this.buttonLoad11.Name = "buttonLoad11";
			this.buttonLoad11.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad11.TabIndex = 2;
			this.buttonLoad11.Text = "TGA RGBA8";
			this.buttonLoad11.UseVisualStyleBackColor = true;
			this.buttonLoad11.Click += new System.EventHandler(this.buttonLoad11_Click);
			// 
			// buttonLoad10
			// 
			this.buttonLoad10.Location = new System.Drawing.Point(861, 6);
			this.buttonLoad10.Name = "buttonLoad10";
			this.buttonLoad10.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad10.TabIndex = 2;
			this.buttonLoad10.Text = "TGA RGB8";
			this.buttonLoad10.UseVisualStyleBackColor = true;
			this.buttonLoad10.Click += new System.EventHandler(this.buttonLoad10_Click);
			// 
			// buttonLoad9
			// 
			this.buttonLoad9.Location = new System.Drawing.Point(766, 6);
			this.buttonLoad9.Name = "buttonLoad9";
			this.buttonLoad9.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad9.TabIndex = 2;
			this.buttonLoad9.Text = "PNG RGB16";
			this.buttonLoad9.UseVisualStyleBackColor = true;
			this.buttonLoad9.Click += new System.EventHandler(this.buttonLoad9_Click);
			// 
			// buttonLoad8
			// 
			this.buttonLoad8.Location = new System.Drawing.Point(671, 6);
			this.buttonLoad8.Name = "buttonLoad8";
			this.buttonLoad8.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad8.TabIndex = 2;
			this.buttonLoad8.Text = "PNG RGBA8";
			this.buttonLoad8.UseVisualStyleBackColor = true;
			this.buttonLoad8.Click += new System.EventHandler(this.buttonLoad8_Click);
			// 
			// buttonLoad7
			// 
			this.buttonLoad7.Location = new System.Drawing.Point(576, 6);
			this.buttonLoad7.Name = "buttonLoad7";
			this.buttonLoad7.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad7.TabIndex = 2;
			this.buttonLoad7.Text = "PNG RGB8";
			this.buttonLoad7.UseVisualStyleBackColor = true;
			this.buttonLoad7.Click += new System.EventHandler(this.buttonLoad7_Click);
			// 
			// buttonLoad6
			// 
			this.buttonLoad6.Location = new System.Drawing.Point(481, 6);
			this.buttonLoad6.Name = "buttonLoad6";
			this.buttonLoad6.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad6.TabIndex = 2;
			this.buttonLoad6.Text = "PNG R8P";
			this.buttonLoad6.UseVisualStyleBackColor = true;
			this.buttonLoad6.Click += new System.EventHandler(this.buttonLoad6_Click);
			// 
			// buttonLoad5
			// 
			this.buttonLoad5.Location = new System.Drawing.Point(386, 6);
			this.buttonLoad5.Name = "buttonLoad5";
			this.buttonLoad5.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad5.TabIndex = 2;
			this.buttonLoad5.Text = "JPG RGB8";
			this.buttonLoad5.UseVisualStyleBackColor = true;
			this.buttonLoad5.Click += new System.EventHandler(this.buttonLoad5_Click);
			// 
			// buttonLoad4
			// 
			this.buttonLoad4.Location = new System.Drawing.Point(291, 6);
			this.buttonLoad4.Name = "buttonLoad4";
			this.buttonLoad4.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad4.TabIndex = 2;
			this.buttonLoad4.Text = "JPG R8";
			this.buttonLoad4.UseVisualStyleBackColor = true;
			this.buttonLoad4.Click += new System.EventHandler(this.buttonLoad4_Click);
			// 
			// buttonLoad3
			// 
			this.buttonLoad3.Location = new System.Drawing.Point(196, 6);
			this.buttonLoad3.Name = "buttonLoad3";
			this.buttonLoad3.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad3.TabIndex = 2;
			this.buttonLoad3.Text = "GIF RGB8P";
			this.buttonLoad3.UseVisualStyleBackColor = true;
			this.buttonLoad3.Click += new System.EventHandler(this.buttonLoad3_Click);
			// 
			// buttonLoad2
			// 
			this.buttonLoad2.Location = new System.Drawing.Point(101, 6);
			this.buttonLoad2.Name = "buttonLoad2";
			this.buttonLoad2.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad2.TabIndex = 2;
			this.buttonLoad2.Text = "BMP RGBA8";
			this.buttonLoad2.UseVisualStyleBackColor = true;
			this.buttonLoad2.Click += new System.EventHandler(this.buttonLoad2_Click);
			// 
			// buttonLoad1
			// 
			this.buttonLoad1.Location = new System.Drawing.Point(6, 6);
			this.buttonLoad1.Name = "buttonLoad1";
			this.buttonLoad1.Size = new System.Drawing.Size(89, 23);
			this.buttonLoad1.TabIndex = 2;
			this.buttonLoad1.Text = "BMP RGB8";
			this.buttonLoad1.UseVisualStyleBackColor = true;
			this.buttonLoad1.Click += new System.EventHandler(this.buttonLoad1_Click);
			// 
			// panelLoad
			// 
			this.panelLoad.Bitmap = null;
			this.panelLoad.Location = new System.Drawing.Point(6, 71);
			this.panelLoad.Name = "panelLoad";
			this.panelLoad.Size = new System.Drawing.Size(842, 568);
			this.panelLoad.TabIndex = 0;
			// 
			// tabPageLDR2HDR
			// 
			this.tabPageLDR2HDR.Controls.Add(this.radioButtonFilterCurveFitting);
			this.tabPageLDR2HDR.Controls.Add(this.radioButtonFilterTent);
			this.tabPageLDR2HDR.Controls.Add(this.radioButtonFilterGaussian);
			this.tabPageLDR2HDR.Controls.Add(this.radioButtonFilterNone);
			this.tabPageLDR2HDR.Controls.Add(this.buttonLDR2HDRRAW);
			this.tabPageLDR2HDR.Controls.Add(this.buttonLDR3RAW);
			this.tabPageLDR2HDR.Controls.Add(this.buttonLDR2HDRJPG);
			this.tabPageLDR2HDR.Controls.Add(this.buttonLDR9JPG);
			this.tabPageLDR2HDR.Controls.Add(this.buttonLDR5JPG);
			this.tabPageLDR2HDR.Controls.Add(this.buttonLDR3JPG);
			this.tabPageLDR2HDR.Controls.Add(this.textBoxHDR);
			this.tabPageLDR2HDR.Controls.Add(this.panelOutputHDR);
			this.tabPageLDR2HDR.Location = new System.Drawing.Point(4, 22);
			this.tabPageLDR2HDR.Name = "tabPageLDR2HDR";
			this.tabPageLDR2HDR.Size = new System.Drawing.Size(1162, 737);
			this.tabPageLDR2HDR.TabIndex = 2;
			this.tabPageLDR2HDR.Text = "LDR->HDR";
			this.tabPageLDR2HDR.UseVisualStyleBackColor = true;
			// 
			// radioButtonFilterCurveFitting
			// 
			this.radioButtonFilterCurveFitting.AutoSize = true;
			this.radioButtonFilterCurveFitting.Location = new System.Drawing.Point(763, 40);
			this.radioButtonFilterCurveFitting.Name = "radioButtonFilterCurveFitting";
			this.radioButtonFilterCurveFitting.Size = new System.Drawing.Size(109, 17);
			this.radioButtonFilterCurveFitting.TabIndex = 5;
			this.radioButtonFilterCurveFitting.Text = "Filter Curve Fitting";
			this.radioButtonFilterCurveFitting.UseVisualStyleBackColor = true;
			// 
			// radioButtonFilterTent
			// 
			this.radioButtonFilterTent.AutoSize = true;
			this.radioButtonFilterTent.Location = new System.Drawing.Point(763, 17);
			this.radioButtonFilterTent.Name = "radioButtonFilterTent";
			this.radioButtonFilterTent.Size = new System.Drawing.Size(72, 17);
			this.radioButtonFilterTent.TabIndex = 5;
			this.radioButtonFilterTent.Text = "Filter Tent";
			this.radioButtonFilterTent.UseVisualStyleBackColor = true;
			// 
			// radioButtonFilterGaussian
			// 
			this.radioButtonFilterGaussian.AutoSize = true;
			this.radioButtonFilterGaussian.Checked = true;
			this.radioButtonFilterGaussian.Location = new System.Drawing.Point(663, 40);
			this.radioButtonFilterGaussian.Name = "radioButtonFilterGaussian";
			this.radioButtonFilterGaussian.Size = new System.Drawing.Size(94, 17);
			this.radioButtonFilterGaussian.TabIndex = 5;
			this.radioButtonFilterGaussian.TabStop = true;
			this.radioButtonFilterGaussian.Text = "Filter Gaussian";
			this.radioButtonFilterGaussian.UseVisualStyleBackColor = true;
			// 
			// radioButtonFilterNone
			// 
			this.radioButtonFilterNone.AutoSize = true;
			this.radioButtonFilterNone.Location = new System.Drawing.Point(663, 17);
			this.radioButtonFilterNone.Name = "radioButtonFilterNone";
			this.radioButtonFilterNone.Size = new System.Drawing.Size(81, 17);
			this.radioButtonFilterNone.TabIndex = 5;
			this.radioButtonFilterNone.Text = "Filter NONE";
			this.radioButtonFilterNone.UseVisualStyleBackColor = true;
			// 
			// buttonLDR2HDRRAW
			// 
			this.buttonLDR2HDRRAW.Location = new System.Drawing.Point(164, 43);
			this.buttonLDR2HDRRAW.Name = "buttonLDR2HDRRAW";
			this.buttonLDR2HDRRAW.Size = new System.Drawing.Size(150, 23);
			this.buttonLDR2HDRRAW.TabIndex = 4;
			this.buttonLDR2HDRRAW.Text = "LDR RAW -> HDR";
			this.buttonLDR2HDRRAW.UseVisualStyleBackColor = true;
			this.buttonLDR2HDRRAW.Click += new System.EventHandler(this.buttonLDR2HDRRAW_Click);
			// 
			// buttonLDR3RAW
			// 
			this.buttonLDR3RAW.Location = new System.Drawing.Point(476, 14);
			this.buttonLDR3RAW.Name = "buttonLDR3RAW";
			this.buttonLDR3RAW.Size = new System.Drawing.Size(150, 23);
			this.buttonLDR3RAW.TabIndex = 4;
			this.buttonLDR3RAW.Text = "3 LDR RAW -> Response";
			this.buttonLDR3RAW.UseVisualStyleBackColor = true;
			this.buttonLDR3RAW.Click += new System.EventHandler(this.buttonLDR3RAW_Click);
			// 
			// buttonLDR2HDRJPG
			// 
			this.buttonLDR2HDRJPG.Location = new System.Drawing.Point(8, 43);
			this.buttonLDR2HDRJPG.Name = "buttonLDR2HDRJPG";
			this.buttonLDR2HDRJPG.Size = new System.Drawing.Size(150, 23);
			this.buttonLDR2HDRJPG.TabIndex = 4;
			this.buttonLDR2HDRJPG.Text = "LDR JPEG -> HDR";
			this.buttonLDR2HDRJPG.UseVisualStyleBackColor = true;
			this.buttonLDR2HDRJPG.Click += new System.EventHandler(this.buttonLDR2HDRJPG_Click);
			// 
			// buttonLDR9JPG
			// 
			this.buttonLDR9JPG.Location = new System.Drawing.Point(320, 14);
			this.buttonLDR9JPG.Name = "buttonLDR9JPG";
			this.buttonLDR9JPG.Size = new System.Drawing.Size(150, 23);
			this.buttonLDR9JPG.TabIndex = 4;
			this.buttonLDR9JPG.Text = "9 LDR JPEG -> Response";
			this.buttonLDR9JPG.UseVisualStyleBackColor = true;
			this.buttonLDR9JPG.Click += new System.EventHandler(this.buttonLDR9JPG_Click);
			// 
			// buttonLDR5JPG
			// 
			this.buttonLDR5JPG.Location = new System.Drawing.Point(164, 14);
			this.buttonLDR5JPG.Name = "buttonLDR5JPG";
			this.buttonLDR5JPG.Size = new System.Drawing.Size(150, 23);
			this.buttonLDR5JPG.TabIndex = 4;
			this.buttonLDR5JPG.Text = "5 LDR JPEG -> Response";
			this.buttonLDR5JPG.UseVisualStyleBackColor = true;
			this.buttonLDR5JPG.Click += new System.EventHandler(this.buttonLDR5JPG_Click);
			// 
			// buttonLDR3JPG
			// 
			this.buttonLDR3JPG.Location = new System.Drawing.Point(8, 14);
			this.buttonLDR3JPG.Name = "buttonLDR3JPG";
			this.buttonLDR3JPG.Size = new System.Drawing.Size(150, 23);
			this.buttonLDR3JPG.TabIndex = 4;
			this.buttonLDR3JPG.Text = "3 LDR JPEG -> Response";
			this.buttonLDR3JPG.UseVisualStyleBackColor = true;
			this.buttonLDR3JPG.Click += new System.EventHandler(this.buttonLDR3JPG_Click);
			// 
			// textBoxHDR
			// 
			this.textBoxHDR.Location = new System.Drawing.Point(857, 84);
			this.textBoxHDR.Multiline = true;
			this.textBoxHDR.Name = "textBoxHDR";
			this.textBoxHDR.ReadOnly = true;
			this.textBoxHDR.Size = new System.Drawing.Size(297, 568);
			this.textBoxHDR.TabIndex = 3;
			// 
			// panelOutputHDR
			// 
			this.panelOutputHDR.Bitmap = null;
			this.panelOutputHDR.Location = new System.Drawing.Point(8, 84);
			this.panelOutputHDR.Name = "panelOutputHDR";
			this.panelOutputHDR.Size = new System.Drawing.Size(842, 568);
			this.panelOutputHDR.TabIndex = 2;
			// 
			// tabPageColorProfiles
			// 
			this.tabPageColorProfiles.Controls.Add(this.buttonProfile4);
			this.tabPageColorProfiles.Controls.Add(this.buttonProfile3);
			this.tabPageColorProfiles.Controls.Add(this.buttonProfile2);
			this.tabPageColorProfiles.Controls.Add(this.buttonProfile1);
			this.tabPageColorProfiles.Controls.Add(this.panelColorProfile);
			this.tabPageColorProfiles.Location = new System.Drawing.Point(4, 22);
			this.tabPageColorProfiles.Name = "tabPageColorProfiles";
			this.tabPageColorProfiles.Size = new System.Drawing.Size(1162, 737);
			this.tabPageColorProfiles.TabIndex = 3;
			this.tabPageColorProfiles.Text = "ColorProfiles";
			this.tabPageColorProfiles.UseVisualStyleBackColor = true;
			// 
			// buttonProfile4
			// 
			this.buttonProfile4.Location = new System.Drawing.Point(514, 20);
			this.buttonProfile4.Name = "buttonProfile4";
			this.buttonProfile4.Size = new System.Drawing.Size(160, 42);
			this.buttonProfile4.TabIndex = 4;
			this.buttonProfile4.Text = "Draw White Points Gradient (WB D65->D50)";
			this.buttonProfile4.UseVisualStyleBackColor = true;
			this.buttonProfile4.Click += new System.EventHandler(this.buttonProfile4_Click);
			// 
			// buttonProfile3
			// 
			this.buttonProfile3.Location = new System.Drawing.Point(348, 20);
			this.buttonProfile3.Name = "buttonProfile3";
			this.buttonProfile3.Size = new System.Drawing.Size(160, 42);
			this.buttonProfile3.TabIndex = 4;
			this.buttonProfile3.Text = "Draw White Points Gradient (WB D50->D65)";
			this.buttonProfile3.UseVisualStyleBackColor = true;
			this.buttonProfile3.Click += new System.EventHandler(this.buttonProfile3_Click);
			// 
			// buttonProfile2
			// 
			this.buttonProfile2.Location = new System.Drawing.Point(182, 20);
			this.buttonProfile2.Name = "buttonProfile2";
			this.buttonProfile2.Size = new System.Drawing.Size(160, 42);
			this.buttonProfile2.TabIndex = 4;
			this.buttonProfile2.Text = "Draw White Points Gradient (no white balance)";
			this.buttonProfile2.UseVisualStyleBackColor = true;
			this.buttonProfile2.Click += new System.EventHandler(this.buttonProfile2_Click);
			// 
			// buttonProfile1
			// 
			this.buttonProfile1.Location = new System.Drawing.Point(14, 20);
			this.buttonProfile1.Name = "buttonProfile1";
			this.buttonProfile1.Size = new System.Drawing.Size(162, 42);
			this.buttonProfile1.TabIndex = 4;
			this.buttonProfile1.Text = "Draw White Points Loci (red=spectrum method)";
			this.buttonProfile1.UseVisualStyleBackColor = true;
			this.buttonProfile1.Click += new System.EventHandler(this.buttonProfile1_Click);
			// 
			// panelColorProfile
			// 
			this.panelColorProfile.Bitmap = null;
			this.panelColorProfile.Location = new System.Drawing.Point(160, 84);
			this.panelColorProfile.Name = "panelColorProfile";
			this.panelColorProfile.Size = new System.Drawing.Size(842, 568);
			this.panelColorProfile.TabIndex = 3;
			// 
			// tabPageDrawing
			// 
			this.tabPageDrawing.Controls.Add(this.buttonDraw3);
			this.tabPageDrawing.Controls.Add(this.buttonDraw2);
			this.tabPageDrawing.Controls.Add(this.buttonDraw1);
			this.tabPageDrawing.Controls.Add(this.panelDrawing);
			this.tabPageDrawing.Location = new System.Drawing.Point(4, 22);
			this.tabPageDrawing.Name = "tabPageDrawing";
			this.tabPageDrawing.Size = new System.Drawing.Size(1162, 737);
			this.tabPageDrawing.TabIndex = 4;
			this.tabPageDrawing.Text = "Drawing";
			this.tabPageDrawing.UseVisualStyleBackColor = true;
			// 
			// buttonDraw3
			// 
			this.buttonDraw3.Location = new System.Drawing.Point(253, 21);
			this.buttonDraw3.Name = "buttonDraw3";
			this.buttonDraw3.Size = new System.Drawing.Size(119, 23);
			this.buttonDraw3.TabIndex = 5;
			this.buttonDraw3.Text = "Stress Test";
			this.buttonDraw3.UseVisualStyleBackColor = true;
			this.buttonDraw3.Click += new System.EventHandler(this.buttonDraw3_Click);
			// 
			// buttonDraw2
			// 
			this.buttonDraw2.Location = new System.Drawing.Point(128, 21);
			this.buttonDraw2.Name = "buttonDraw2";
			this.buttonDraw2.Size = new System.Drawing.Size(119, 23);
			this.buttonDraw2.TabIndex = 5;
			this.buttonDraw2.Text = "Simple Log Functions";
			this.buttonDraw2.UseVisualStyleBackColor = true;
			this.buttonDraw2.Click += new System.EventHandler(this.buttonDraw2_Click);
			// 
			// buttonDraw1
			// 
			this.buttonDraw1.Location = new System.Drawing.Point(3, 21);
			this.buttonDraw1.Name = "buttonDraw1";
			this.buttonDraw1.Size = new System.Drawing.Size(119, 23);
			this.buttonDraw1.TabIndex = 5;
			this.buttonDraw1.Text = "Simple Function";
			this.buttonDraw1.UseVisualStyleBackColor = true;
			this.buttonDraw1.Click += new System.EventHandler(this.buttonDraw1_Click);
			// 
			// panelDrawing
			// 
			this.panelDrawing.Bitmap = null;
			this.panelDrawing.Location = new System.Drawing.Point(160, 84);
			this.panelDrawing.Name = "panelDrawing";
			this.panelDrawing.Size = new System.Drawing.Size(842, 568);
			this.panelDrawing.TabIndex = 4;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1200, 788);
			this.Controls.Add(this.tabControlTests);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ImageUtilityLib - Unit Test";
			this.tabControlTests.ResumeLayout(false);
			this.tabPageBuilding.ResumeLayout(false);
			this.tabPageLoading.ResumeLayout(false);
			this.tabPageLoading.PerformLayout();
			this.tabPageLDR2HDR.ResumeLayout(false);
			this.tabPageLDR2HDR.PerformLayout();
			this.tabPageColorProfiles.ResumeLayout(false);
			this.tabPageDrawing.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private PanelOutput panelLoad;
		private System.Windows.Forms.TextBox textBoxEXIF;
		private System.Windows.Forms.TabControl tabControlTests;
		private System.Windows.Forms.TabPage tabPageLoading;
		private System.Windows.Forms.TabPage tabPageBuilding;
		private System.Windows.Forms.Button buttonLoad1;
		private System.Windows.Forms.TabPage tabPageLDR2HDR;
		private System.Windows.Forms.TabPage tabPageColorProfiles;
		private System.Windows.Forms.TabPage tabPageDrawing;
		private PanelOutput panelOutputHDR;
		private System.Windows.Forms.TextBox textBoxHDR;
		private System.Windows.Forms.Button buttonLDR2HDRRAW;
		private System.Windows.Forms.Button buttonLDR3RAW;
		private System.Windows.Forms.Button buttonLDR2HDRJPG;
		private System.Windows.Forms.Button buttonLDR3JPG;
		private System.Windows.Forms.Button buttonLDR9JPG;
		private System.Windows.Forms.Button buttonLDR5JPG;
		private System.Windows.Forms.RadioButton radioButtonFilterCurveFitting;
		private System.Windows.Forms.RadioButton radioButtonFilterTent;
		private System.Windows.Forms.RadioButton radioButtonFilterGaussian;
		private System.Windows.Forms.RadioButton radioButtonFilterNone;
		private System.Windows.Forms.Button buttonLoad6;
		private System.Windows.Forms.Button buttonLoad5;
		private System.Windows.Forms.Button buttonLoad4;
		private System.Windows.Forms.Button buttonLoad3;
		private System.Windows.Forms.Button buttonLoad2;
		private System.Windows.Forms.Button buttonLoad7;
		private System.Windows.Forms.Button buttonLoad8;
		private System.Windows.Forms.Button buttonLoad9;
		private System.Windows.Forms.Button buttonLoad10;
		private System.Windows.Forms.Button buttonLoad11;
		private System.Windows.Forms.Button buttonLoad12;
		private System.Windows.Forms.Button buttonLoad13;
		private System.Windows.Forms.Button buttonLoad14;
		private System.Windows.Forms.Button buttonLoad15;
		private System.Windows.Forms.Button buttonLoad16;
		private PanelOutput panelBuild;
		private System.Windows.Forms.Button buttonBuild4;
		private System.Windows.Forms.Button buttonBuild3;
		private System.Windows.Forms.Button buttonBuild2;
		private System.Windows.Forms.Button buttonBuild1;
		private PanelOutput panelColorProfile;
		private PanelOutput panelDrawing;
		private System.Windows.Forms.Button buttonDraw1;
		private System.Windows.Forms.Button buttonDraw2;
		private System.Windows.Forms.Button buttonProfile1;
		private System.Windows.Forms.Button buttonProfile2;
		private System.Windows.Forms.Button buttonProfile3;
		private System.Windows.Forms.Button buttonProfile4;
		private System.Windows.Forms.Button buttonDraw3;

	}
}

