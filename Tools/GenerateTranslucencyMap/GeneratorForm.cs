﻿//////////////////////////////////////////////////////////////////////////
// Implements the Directional Translucency Map from Habel "Physically Based Real-Time Translucency for Leaves" (2007)
// Source: https://www.cg.tuwien.ac.at/research/publications/2007/Habel_2007_RTT/
// 
// The idea is to precompute 3 translucency values for 3 principal directions encoded by the HL2 basis.
// 
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GenerateTranslucencyMap
{
	public partial class GeneratorForm : Form
	{
		#region CONSTANTS

		private const int		MAX_THREADS = 1024;			// Maximum threads run by the compute shader

		private const int		BILATERAL_PROGRESS = 50;	// Bilateral filtering is considered as this % of the total task (bilateral is quite long so I decided it was equivalent to 50% of the complete computation task)
		private const int		MAX_LINES = 16;				// Process at most that amount of lines of a 4096x4096 image for a single dispatch

		#endregion

		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBInput {
			public uint						_Width;
			public uint						_Height;
			public float					_TexelSize_mm;		// Texel size in millimeters
			public float					_Thickness_mm;		// Thickness map max encoded displacement in millimeters
			public uint						_KernelSize;		// Size of the convolution kernel
			public float					_Sigma_a;			// Absorption coefficient (mm^-1)
			public float					_Sigma_s;			// Scattering coefficient (mm^-1)
			public float					_g;					// Scattering anisotropy (mean cosine of scattering phase function)
			public RendererManaged.float3	_Light;				// Light direction (in tangent space)
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBFilter {
			public UInt32	Y0;					// Index of the texture line we're processing
			public float	Radius;				// Radius of the bilateral filter
			public float	Tolerance;			// Range tolerance of the bilateral filter
			public UInt32	Tile;				// Tiling flag
		}

		#endregion

		#region FIELDS

		private RegistryKey						m_AppKey;
		private string							m_ApplicationPath;

		private System.IO.FileInfo				m_SourceFileName = null;
		private int								W, H;
		private ImageUtility.Bitmap				m_BitmapSourceThickness = null;
		private ImageUtility.Bitmap				m_BitmapSourceNormal = null;
		private ImageUtility.Bitmap				m_BitmapSourceAlbedo = null;
		private ImageUtility.Bitmap				m_BitmapSourceTransmittance = null;

		private RendererManaged.Device			m_Device = new RendererManaged.Device();
		private RendererManaged.Texture2D		m_TextureSourceThickness = null;
		private RendererManaged.Texture2D		m_TextureSourceNormal = null;
		private RendererManaged.Texture2D		m_TextureSourceAlbedo = null;
		private RendererManaged.Texture2D		m_TextureSourceTransmittance = null;
		private RendererManaged.Texture2D		m_TextureTarget0 = null;
		private RendererManaged.Texture2D		m_TextureTarget1 = null;
		private RendererManaged.Texture2D		m_TextureTarget_CPU = null;

		// Directional Translucency Map Generation
		private RendererManaged.ConstantBuffer<CBInput>						m_CB_Input;
		private RendererManaged.StructuredBuffer<RendererManaged.float3>	m_SB_Rays = null;
		private RendererManaged.ComputeShader								m_CS_GenerateVisibilityMap = null;
		private RendererManaged.ComputeShader								m_CS_GenerateTranslucencyMap = null;

		// Visibility map generation


		// Bilateral filtering pre-processing
		private RendererManaged.ConstantBuffer<CBFilter>	m_CB_Filter;
		private RendererManaged.ComputeShader	m_CS_BilateralFilter = null;

		private ImageUtility.ColorProfile		m_LinearProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR );
		private ImageUtility.ColorProfile		m_sRGBProfile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );
		private ImageUtility.Bitmap				m_BitmapResult = null;

		#endregion

		#region METHODS

		public unsafe GeneratorForm()
		{
			InitializeComponent();

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\TranslucencyMapGenerator" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );
		}

		protected override void  OnLoad(EventArgs e)
		{
 			base.OnLoad(e);

			try {
				m_Device.Init( imagePanelResult0.Handle, false, true );

				// Create our compute shaders
#if DEBUG
				m_CS_BilateralFilter = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/BilateralFiltering.hlsl" ) ), "CS", null );
				m_CS_GenerateVisibilityMap = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/GenerateVisibilityMap.hlsl" ) ), "CS", null );
				m_CS_GenerateTranslucencyMap = new RendererManaged.ComputeShader( m_Device, new RendererManaged.ShaderFile( new System.IO.FileInfo( "./Shaders/GenerateTranslucencyMap.hlsl" ) ), "CS", null );
#else
				m_CS_BilateralFilter = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/BilateralFiltering.fxbin" ), "CS" );
				m_CS_GenerateVisibilityMap = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/GenerateVisibilityMap.fxbin" ), "CS" );
				m_CS_GenerateTranslucencyMap = RendererManaged.ComputeShader.CreateFromBinaryBlob( m_Device, new System.IO.FileInfo( "./Shaders/Binary/GenerateTranslucencyMap.fxbin" ), "CS" );
#endif

				// Create our constant buffers
				m_CB_Filter = new RendererManaged.ConstantBuffer<CBFilter>( m_Device, 0 );
				m_CB_Input = new RendererManaged.ConstantBuffer<CBInput>( m_Device, 0 );

				// Create our structured buffer containing the rays
				m_SB_Rays = new RendererManaged.StructuredBuffer<RendererManaged.float3>( m_Device, 3 * MAX_THREADS, true );
				integerTrackbarControlRaysCount_SliderDragStop( integerTrackbarControlRaysCount, 0 );

//				LoadHeightMap( new System.IO.FileInfo( "eye_generic_01_disp.png" ) );
//				LoadHeightMap( new System.IO.FileInfo( "10 - Smooth.jpg" ) );

			} catch ( Exception _e ) {
				MessageBox( "Failed to create DX11 device and default shaders:\r\n", _e );
				Close();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();

				try {
					m_CS_GenerateTranslucencyMap.Dispose();
					m_CS_BilateralFilter.Dispose();

					m_SB_Rays.Dispose();
					m_CB_Filter.Dispose();
					m_CB_Input.Dispose();

					if ( m_TextureTarget_CPU != null )
						m_TextureTarget_CPU.Dispose();
					if ( m_TextureTarget1 != null )
						m_TextureTarget1.Dispose();
					if ( m_TextureTarget0 != null )
						m_TextureTarget0.Dispose();
					if ( m_TextureSourceThickness != null )
						m_TextureSourceThickness.Dispose();
					if ( m_TextureSourceNormal != null )
						m_TextureSourceNormal.Dispose();
					if ( m_TextureSourceTransmittance != null )
						m_TextureSourceTransmittance.Dispose();
					if ( m_TextureSourceAlbedo != null )
						m_TextureSourceAlbedo.Dispose();

					m_Device.Dispose();
				} catch ( Exception ) {
				}
			}
			base.Dispose( disposing );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			base.OnClosing( e );
		}

		private void	LoadThicknessMap( System.IO.FileInfo _FileName ) {
			try
			{
				groupBoxOptions.Enabled = false;

				// Dispose of existing resources
				if ( m_BitmapSourceThickness != null )
					m_BitmapSourceThickness.Dispose();
				m_BitmapSourceThickness = null;

				if ( m_TextureTarget_CPU != null )
					m_TextureTarget_CPU.Dispose();
				m_TextureTarget_CPU = null;
				if ( m_TextureTarget0 != null )
					m_TextureTarget0.Dispose();
				m_TextureTarget0 = null;
				if ( m_TextureTarget1 != null )
					m_TextureTarget1.Dispose();
				m_TextureTarget1 = null;
				if ( m_TextureSourceThickness != null )
					m_TextureSourceThickness.Dispose();
				m_TextureSourceThickness = null;

				// Load the source image assuming it's in linear space
				m_SourceFileName = _FileName;
				m_BitmapSourceThickness = new ImageUtility.Bitmap( _FileName, m_LinearProfile );
				imagePanelThicknessMap.Image = m_BitmapSourceThickness;

				W = m_BitmapSourceThickness.Width;
				H = m_BitmapSourceThickness.Height;

				// Build the source texture
				RendererManaged.PixelsBuffer	SourceHeightMap = new RendererManaged.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceHeightMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							Wr.Write( m_BitmapSourceThickness.ContentXYZ[X,Y].y );

				m_TextureSourceThickness = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceHeightMap } );

				// Build the target UAV & staging texture for readback
				m_TextureTarget0 = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_TextureTarget1 = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, true, null );
				m_TextureTarget_CPU = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, true, false, null );

				groupBoxOptions.Enabled = true;
				buttonGenerate.Focus();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the thickness map:\n\n", _e );
			}
		}

		private void	LoadNormalMap( System.IO.FileInfo _FileName ) {
			try
			{
				// Dispose of existing resources
				if ( m_BitmapSourceNormal != null )
					m_BitmapSourceNormal.Dispose();
				m_BitmapSourceNormal = null;

				// Load the source image assuming it's in linear space
				m_BitmapSourceNormal = new ImageUtility.Bitmap( _FileName, m_LinearProfile );
				imagePanelNormalMap.Image = m_BitmapSourceNormal;

				int	W = m_BitmapSourceNormal.Width;
				int	H = m_BitmapSourceNormal.Height;

				// Build the source texture
				ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
				m_LinearProfile.XYZ2RGB( m_BitmapSourceNormal.ContentXYZ, ContentRGB );

				RendererManaged.PixelsBuffer	SourceMap = new RendererManaged.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ ) {
							Wr.Write( ContentRGB[X,Y].x );
							Wr.Write( ContentRGB[X,Y].y );
							Wr.Write( ContentRGB[X,Y].z );
							Wr.Write( 1.0f );
						}

				m_TextureSourceNormal = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceMap } );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the normal map:\n\n", _e );
			}
		}

		private void	LoadTransmittanceMap( System.IO.FileInfo _FileName ) {
			try
			{
				// Dispose of existing resources
				if ( m_BitmapSourceTransmittance != null )
					m_BitmapSourceTransmittance.Dispose();
				m_BitmapSourceTransmittance = null;

				// Load the source image assuming it's in linear space
				m_BitmapSourceTransmittance = new ImageUtility.Bitmap( _FileName, m_sRGBProfile );
				imagePanelTransmittanceMap.Image = m_BitmapSourceTransmittance;

				int	W = m_BitmapSourceTransmittance.Width;
				int	H = m_BitmapSourceTransmittance.Height;

				// Build the source texture
				ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
				m_sRGBProfile.XYZ2RGB( m_BitmapSourceTransmittance.ContentXYZ, ContentRGB );

				RendererManaged.PixelsBuffer	SourceMap = new RendererManaged.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ ) {
							Wr.Write( ContentRGB[X,Y].x );
							Wr.Write( ContentRGB[X,Y].y );
							Wr.Write( ContentRGB[X,Y].z );
							Wr.Write( ContentRGB[X,Y].w );
						}

				m_TextureSourceTransmittance = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceMap } );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the transmittance map:\n\n", _e );
			}
		}

		private void	LoadAlbedoMap( System.IO.FileInfo _FileName ) {
			try
			{
				// Dispose of existing resources
				if ( m_BitmapSourceAlbedo != null )
					m_BitmapSourceAlbedo.Dispose();
				m_BitmapSourceAlbedo = null;

				// Load the source image assuming it's in linear space
				m_BitmapSourceAlbedo = new ImageUtility.Bitmap( _FileName, m_sRGBProfile );
				imagePanelAlbedoMap.Image = m_BitmapSourceAlbedo;

				int	W = m_BitmapSourceAlbedo.Width;
				int	H = m_BitmapSourceAlbedo.Height;

				// Build the source texture
				ImageUtility.float4[,]	ContentRGB = new ImageUtility.float4[W,H];
				m_sRGBProfile.XYZ2RGB( m_BitmapSourceAlbedo.ContentXYZ, ContentRGB );

				RendererManaged.PixelsBuffer	SourceMap = new RendererManaged.PixelsBuffer( W*H*4 );
				using ( System.IO.BinaryWriter Wr = SourceMap.OpenStreamWrite() )
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ ) {
							Wr.Write( ContentRGB[X,Y].x );
							Wr.Write( ContentRGB[X,Y].y );
							Wr.Write( ContentRGB[X,Y].z );
							Wr.Write( ContentRGB[X,Y].w );
						}

				m_TextureSourceAlbedo = new RendererManaged.Texture2D( m_Device, W, H, 1, 1, RendererManaged.PIXEL_FORMAT.RGBA32_FLOAT, false, false, new RendererManaged.PixelsBuffer[] { SourceMap } );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the albedo map:\n\n", _e );
			}
		}

		private unsafe void	Generate() {
			try {
				groupBoxOptions.Enabled = false;

				//////////////////////////////////////////////////////////////////////////
				// 1] Apply bilateral filtering to the input texture as a pre-process
				ApplyBilateralFiltering( m_TextureSourceThickness, m_TextureTarget0, floatTrackbarControlBilateralRadius.Value, floatTrackbarControlBilateralTolerance.Value, false );


				//////////////////////////////////////////////////////////////////////////
				// 2] Compute directional occlusion

				// Prepare computation parameters
				m_TextureTarget0.SetCS( 0 );
				m_TextureTarget1.SetCSUAV( 0 );
				m_SB_Rays.SetInput( 1 );

				m_CB_Input.m.RaysCount = (UInt32) Math.Min( MAX_THREADS, integerTrackbarControlRaysCount.Value );
				m_CB_Input.m.MaxStepsCount = (UInt32) integerTrackbarControlMaxStepsCount.Value;
				m_CB_Input.m.Tile = (uint) (checkBoxWrap.Checked ? 1 : 0);
				m_CB_Input.m.TexelSize_mm = 1000.0f / floatTrackbarControlPixelDensity.Value;
//				m_CB_Input.m.Displacement_mm = 10.0f * floatTrackbarControlHeight.Value;

				// Start
				if ( !m_CS_GenerateTranslucencyMap.Use() )
					throw new Exception( "Can't generate self-shadowed bump map as compute shader failed to compile!" );

				int	h = Math.Max( 1, MAX_LINES*1024 / W );
				int	CallsCount = (int) Math.Ceiling( (float) H / h );
				for ( int i=0; i < CallsCount; i++ )
				{
					m_CB_Input.m.Y0 = (UInt32) (i * h);
					m_CB_Input.UpdateData();

					m_CS_GenerateTranslucencyMap.Dispatch( W, h, 1 );

					m_Device.Present( true );

					progressBar.Value = (int) (0.01f * (BILATERAL_PROGRESS + (100-BILATERAL_PROGRESS) * (i+1) / (CallsCount)) * progressBar.Maximum);
//					for ( int a=0; a < 10; a++ )
						Application.DoEvents();
				}

				progressBar.Value = progressBar.Maximum;

				// Compute in a single shot (this is madness!)
// 				m_CB_Input.m.y = 0;
// 				m_CB_Input.UpdateData();
// 				m_CS_GenerateSSBumpMap.Dispatch( W, H, 1 );


				//////////////////////////////////////////////////////////////////////////
				// 3] Copy target to staging for CPU readback and update the resulting bitmap
				m_TextureTarget_CPU.CopyFrom( m_TextureTarget1 );

				if ( m_BitmapResult != null )
					m_BitmapResult.Dispose();
				m_BitmapResult = null;
				m_BitmapResult = new ImageUtility.Bitmap( W, H, m_LinearProfile );
				m_BitmapResult.HasAlpha = true;

				RendererManaged.PixelsBuffer	Pixels = m_TextureTarget_CPU.Map( 0, 0 );
				using ( System.IO.BinaryReader R = Pixels.OpenStreamRead() )
					for ( int Y=0; Y < H; Y++ )
					{
						R.BaseStream.Position = Y * Pixels.RowPitch;
						for ( int X=0; X < W; X++ )
						{
							ImageUtility.float4	Color = new ImageUtility.float4( R.ReadSingle(), R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );
							Color = m_LinearProfile.RGB2XYZ( Color );
							m_BitmapResult.ContentXYZ[X,Y] = Color;
						}
					}

				Pixels.Dispose();
				m_TextureTarget_CPU.UnMap( 0, 0 );

				// Assign result
				viewportPanelResult.Image = m_BitmapResult;

			} catch ( Exception _e ) {
				MessageBox( "An error occurred during generation!\r\n\r\nDetails: ", _e );
			} finally {
				groupBoxOptions.Enabled = true;
			}
		}

		private void	ApplyBilateralFiltering( RendererManaged.Texture2D _Source, RendererManaged.Texture2D _Target, float _BilateralRadius, float _BilateralTolerance, bool _Wrap ) {
			_Source.SetCS( 0 );
			_Target.SetCSUAV( 0 );

			m_CB_Filter.m.Radius = _BilateralRadius;
			m_CB_Filter.m.Tolerance = _BilateralTolerance;
			m_CB_Filter.m.Tile = (uint) (_Wrap ? 1 : 0);

			m_CS_BilateralFilter.Use();

			int	h = Math.Max( 1, MAX_LINES*1024 / W );
			int	CallsCount = (int) Math.Ceiling( (float) H / h );
			for ( int i=0; i < CallsCount; i++ )
			{
				m_CB_Filter.m.Y0 = (UInt32) (i * h);
				m_CB_Filter.UpdateData();

				m_CS_BilateralFilter.Dispatch( W, h, 1 );

				m_Device.Present( true );

				progressBar.Value = (int) (0.01f * (0 + BILATERAL_PROGRESS * (i+1) / CallsCount) * progressBar.Maximum);
//				for ( int a=0; a < 10; a++ )
					Application.DoEvents();
			}

			// Single gulp (crashes the driver on large images!)
//			m_CS_BilateralFilter.Dispatch( W, H, 1 );

			_Target.RemoveFromLastAssignedSlotUAV();	// So we can use it as input for next stage
		}

		private void	GenerateRays( int _RaysCount, RendererManaged.StructuredBuffer<RendererManaged.float3> _Target ) {
			_RaysCount = Math.Min( MAX_THREADS, _RaysCount );

			// Half-Life 2 basis
			RendererManaged.float3[]	HL2Basis = new RendererManaged.float3[] {
				new RendererManaged.float3( (float) Math.Sqrt( 2.0 / 3.0 ), 0.0f, (float) Math.Sqrt( 1.0 / 3.0 ) ),
				new RendererManaged.float3( -(float) Math.Sqrt( 1.0 / 6.0 ), (float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) ),
				new RendererManaged.float3( -(float) Math.Sqrt( 1.0 / 6.0 ), -(float) Math.Sqrt( 1.0 / 2.0 ), (float) Math.Sqrt( 1.0 / 3.0 ) )
			};

			float	CenterTheta = (float) Math.Acos( HL2Basis[0].z );
			float[]	CenterPhi = new float[] {
				(float) Math.Atan2( HL2Basis[0].y, HL2Basis[0].x ),
				(float) Math.Atan2( HL2Basis[1].y, HL2Basis[1].x ),
				(float) Math.Atan2( HL2Basis[2].y, HL2Basis[2].x ),
			};

			for ( int RayIndex=0; RayIndex < _RaysCount; RayIndex++ ) {
				double	Phi = (Math.PI / 3.0) * (2.0 * WMath.SimpleRNG.GetUniform() - 1.0);

				// Stratified version
				double	Theta = (Math.Acos( Math.Sqrt( (RayIndex + WMath.SimpleRNG.GetUniform()) / _RaysCount ) ));

// 				// Don't give a shit version (a.k.a. melonhead version)
// //				double	Theta = Math.Acos( Math.Sqrt(WMath.SimpleRNG.GetUniform() ) );
// 				double	Theta = 0.5 * Math.PI * WMath.SimpleRNG.GetUniform();

				Theta = Math.Min( 0.499f * Math.PI, Theta );


				double	CosTheta = Math.Cos( Theta );
				double	SinTheta = Math.Sin( Theta );

				double	LengthFactor = 1.0 / SinTheta;	// The ray is scaled so we ensure we always walk at least a texel in the texture
				CosTheta *= LengthFactor;
				SinTheta *= LengthFactor;	// Yeah, yields 1... :)

				_Target.m[0*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[0] + Phi ) * SinTheta),
														(float) (Math.Sin( CenterPhi[0] + Phi ) * SinTheta),
														(float) CosTheta );
				_Target.m[1*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[1] + Phi ) * SinTheta),
														(float) (Math.Sin( CenterPhi[1] + Phi ) * SinTheta),
														(float) CosTheta );
				_Target.m[2*MAX_THREADS+RayIndex].Set(	(float) (Math.Cos( CenterPhi[2] + Phi ) * SinTheta),
														(float) (Math.Sin( CenterPhi[2] + Phi ) * SinTheta),
														(float) CosTheta );
			}

			_Target.Write();
		}

		#region Helpers

		private string	GetRegKey( string _Key, string _Default )
		{
			string	Result = m_AppKey.GetValue( _Key ) as string;
			return Result != null ? Result : _Default;
		}
		private void	SetRegKey( string _Key, string _Value )
		{
			m_AppKey.SetValue( _Key, _Value );
		}

		private float	GetRegKeyFloat( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			float	Result;
			float.TryParse( Value, out Result );
			return Result;
		}

		private int		GetRegKeyInt( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			int		Result;
			int.TryParse( Value, out Result );
			return Result;
		}

		private DialogResult	MessageBox( string _Text )
		{
			return MessageBox( _Text, MessageBoxButtons.OK );
		}
		private DialogResult	MessageBox( string _Text, Exception _e )
		{
			return MessageBox( _Text + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			return MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			return MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			return System.Windows.Forms.MessageBox.Show( this, _Text, "SSBumpMap Generator", _Buttons, _Icon );
		}

		#endregion 

		#endregion

		#region EVENT HANDLERS

 		private unsafe void buttonGenerate_Click( object sender, EventArgs e )
 		{
 			Generate();
//			Generate_CPU( integerTrackbarControlRaysCount.Value );
		}

		private void integerTrackbarControlRaysCount_SliderDragStop( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _StartValue )
		{
			GenerateRays( _Sender.Value, m_SB_Rays );
		}

		private void radioButtonShowDirOccRGB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.RGB;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.RGB;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.RGB;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.RGB;
			}
		}

		private void radioButtonDirOccRGBtimeAO_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.RGB_AO;
			}
		}

		private void radioButtonDirOccR_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.R;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.R;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.R;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.R;
			}
		}

		private void radioButtonDirOccG_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.G;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.G;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.G;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.G;
			}
		}

		private void radioButtonDirOccB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.B;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.B;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.B;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.B;
			}
		}

		private void radioButton1_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.AO;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.AO;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.AO;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.AO;
			}
		}

		private void radioButtonAOfromRGB_CheckedChanged( object sender, EventArgs e )
		{
			if ( (sender as RadioButton).Checked ) {
				imagePanelResult0.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
				imagePanelResult1.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
				imagePanelResult2.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
				imagePanelResult3.ViewMode = ImagePanel.VIEW_MODE.AO_FROM_RGB;
			}
		}

		private void checkBoxShowsRGB_CheckedChanged( object sender, EventArgs e )
		{
			imagePanelThicknessMap.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult0.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult1.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult2.ViewLinear = !checkBoxShowsRGB.Checked;
			imagePanelResult3.ViewLinear = !checkBoxShowsRGB.Checked;
		}

		private void imagePanelResult0_Click(object sender, EventArgs e)
		{
			if ( m_BitmapResult == null )
			{
				MessageBox( "There is no result image to save!" );
				return;
			}

			string	SourceFileName = m_SourceFileName.FullName;
			string	TargetFileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( SourceFileName ), System.IO.Path.GetFileNameWithoutExtension( SourceFileName ) + "_ssbump.png" );

			saveFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( TargetFileName );
			saveFileDialogImage.FileName = System.IO.Path.GetFileName( TargetFileName );
			if ( saveFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			try
			{
				m_BitmapResult.Save( new System.IO.FileInfo( saveFileDialogImage.FileName ) );

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while saving the image:\n\n", _e );
			}
		}

		private void imagePanelResult1_Click(object sender, EventArgs e)
		{
		
		}

		private void imagePanelResult2_Click(object sender, EventArgs e)
		{
		
		}

		private void imagePanelResult3_Click(object sender, EventArgs e)
		{
		
		}

		#region Thickness Map (Input)

		private void imagePanelInputThicknessMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "ThicknessMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "ThicknessMapFileName", openFileDialogImage.FileName );

			LoadThicknessMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private string	m_DraggedFileName = null;
		private void imagePanelInputThicknessMap_DragEnter( object sender, DragEventArgs e )
		{
			m_DraggedFileName = null;
			if ( (e.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.Copy )
				return;

			Array	data = ((IDataObject) e.Data).GetData( "FileNameW" ) as Array;
			if ( data == null || data.Length != 1 )
				return;
			if ( !(data.GetValue(0) is String) )
				return;

			string	DraggedFileName = (data as string[])[0];

			string	Extension = System.IO.Path.GetExtension( DraggedFileName ).ToLower();
			if (	Extension == ".jpg"
				||	Extension == ".jpeg"
				||	Extension == ".png"
				||	Extension == ".tga"
				||	Extension == ".bmp"
				||	Extension == ".tif"
				||	Extension == ".tiff"
				||	Extension == ".hdr"
				||	Extension == ".crw"
				||	Extension == ".dng"
				)
			{
				m_DraggedFileName = DraggedFileName;	// Supported!
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void imagePanelInputThicknessMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadThicknessMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Normal Map (Input)

		private void imagePanelNormalMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "NormalMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "NormalMapFileName", openFileDialogImage.FileName );

			LoadNormalMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );
		}

		private void imagePanelNormalMap_DragEnter( object sender, DragEventArgs e )
		{
			imagePanelInputThicknessMap_DragEnter( sender, e );
		}

		private void imagePanelNormalMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadNormalMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Albedo Map (Input)

		private void imagePanelAlbedoMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "AlbedoMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "AlbedoMapFileName", openFileDialogImage.FileName );

			try
			{
				LoadAlbedoMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );

				groupBoxOptions.Enabled = true;
				buttonGenerate.Focus();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void imagePanelAlbedoMap_DragEnter( object sender, DragEventArgs e )
		{
			imagePanelInputThicknessMap_DragEnter( sender, e );
		}

		private void imagePanelAlbedoMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadAlbedoMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#region Transmittance Map (Input)

		private void imagePanelTransmittanceMap_Click( object sender, EventArgs e )
		{
			string	OldFileName = GetRegKey( "TransmittanceMapFileName", System.IO.Path.Combine( m_ApplicationPath, "Example.jpg" ) );
			openFileDialogImage.InitialDirectory = System.IO.Path.GetFullPath( OldFileName );
			openFileDialogImage.FileName = System.IO.Path.GetFileName( OldFileName );
			if ( openFileDialogImage.ShowDialog( this ) != DialogResult.OK )
				return;

			SetRegKey( "TransmittanceMapFileName", openFileDialogImage.FileName );

			try
			{
				LoadTransmittanceMap( new System.IO.FileInfo( openFileDialogImage.FileName ) );

				groupBoxOptions.Enabled = true;
				buttonGenerate.Focus();
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while opening the image:\n\n", _e );
			}
		}

		private void imagePanelTransmittanceMap_DragEnter( object sender, DragEventArgs e )
		{
			imagePanelInputThicknessMap_DragEnter( sender, e );
		}

		private void imagePanelTransmittanceMap_DragDrop( object sender, DragEventArgs e )
		{
			if ( m_DraggedFileName != null )
				LoadTransmittanceMap( new System.IO.FileInfo( m_DraggedFileName ) );
		}

		#endregion

		#endregion
	}
}