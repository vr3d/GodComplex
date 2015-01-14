﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

using RendererManaged;

namespace AreaLightTest
{
	public partial class AreaLightForm : Form
	{
		private Device		m_Device = new Device();


		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Main {
			public float3		iResolution;
			public float		iGlobalTime;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Camera {
			public float4x4		_Camera2World;
			public float4x4		_World2Camera;
			public float4x4		_Proj2World;
			public float4x4		_World2Proj;
			public float4x4		_Camera2Proj;
			public float4x4		_Proj2Camera;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Light {
			public float3		_AreaLightX;
			public float		_AreaLightScaleX;
			public float3		_AreaLightY;
			public float		_AreaLightScaleY;
			public float3		_AreaLightZ;
			public float		_AreaLightDiffusion;
			public float3		_AreaLightT;
			public float		_AreaLightIntensity;
			public float3		_ProjectionDirectionDiff;	// Closer to portal when diffusion increases
			public float		__PAD00;
			public float3		_ProjectionDirectionSpec;	// Closer to portal when diffusion decreases
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct CB_Object {
			public float4x4		_Local2World;
			public float4x4		_World2Local;
			public float3		_DiffuseAlbedo;
			public float		_Gloss;
			public float3		_SpecularTint;
			public float		_Metal;
		}

		private ConstantBuffer<CB_Main>		m_CB_Main = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Light>	m_CB_Light = null;
		private ConstantBuffer<CB_Object>	m_CB_Object = null;

		private Shader		m_Shader_RenderShadowMap = null;
		private Shader		m_Shader_BuildSmoothie = null;
		private Shader		m_Shader_BuildSmoothieDistanceFieldH = null;
		private Shader		m_Shader_BuildSmoothieDistanceFieldV = null;
		private Shader		m_Shader_RenderAreaLight = null;
		private Shader		m_Shader_RenderScene = null;
		private Texture2D	m_Tex_AreaLight = null;
		private Texture2D	m_Tex_AreaLightSAT = null;
		private Texture2D	m_Tex_AreaLightSATFade = null;

		private Texture2D	m_Tex_ShadowMap = null;
		private Texture2D	m_Tex_ShadowSmoothie = null;
		private Texture2D[]	m_Tex_ShadowSmoothiePou = new Texture2D[2];

		private Primitive	m_Prim_Quad = null;
		private Primitive	m_Prim_Rectangle = null;
		private Primitive	m_Prim_Sphere = null;
		private Primitive	m_Prim_Cube = null;


		private Camera		m_Camera = new Camera();
		private CameraManipulator	m_Manipulator = new CameraManipulator();

		//////////////////////////////////////////////////////////////////////////
		// Timing
		public System.Diagnostics.Stopwatch	m_StopWatch = new System.Diagnostics.Stopwatch();
		private double						m_Ticks2Seconds;
		public float						m_StartTime = 0;
		public float						m_CurrentTime = 0;
		public float						m_DeltaTime = 0;		// Delta time used for the current frame

		public AreaLightForm()
		{
			InitializeComponent();

// Build SATs
//ComputeSAT( new System.IO.FileInfo( "Dummy.png" ), new System.IO.FileInfo( "DummySAT.dds" ) );
//ComputeSAT( new System.IO.FileInfo( "StainedGlass.png" ), new System.IO.FileInfo( "AreaLightSAT.dds" ) );
//ComputeSAT( new System.IO.FileInfo( "StainedGlass2.jpg" ), new System.IO.FileInfo( "AreaLightSAT2.dds" ) );
//ComputeSAT( new System.IO.FileInfo( "StainedGlass3.png" ), new System.IO.FileInfo( "AreaLightSAT3.dds" ) );
//ComputeSAT( new System.IO.FileInfo( "StainedGlass2Fade.png" ), new System.IO.FileInfo( "AreaLightSAT2Fade.dds" ) );

			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );

			Application.Idle += new EventHandler( Application_Idle );
		}

		#region Image Helpers

		public Texture2D	Image2Texture( System.IO.FileInfo _FileName )
		{
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				return Image2Texture( S );
		}
		public unsafe Texture2D	Image2Texture( System.IO.Stream _Stream )
		{
			int		W, H;
			byte[]	Content = null;
			using ( Bitmap B = Bitmap.FromStream( _Stream ) as Bitmap )
			{
				W = B.Width;
				H = B.Height;
				Content = new byte[W*H*4];

				BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
				for ( int Y=0; Y < H; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;
					int		Offset = 4*W*Y;
					for ( int X=0; X < W; X++, Offset+=4 )
					{
						Content[Offset+2] = *pScanline++;	// B
						Content[Offset+1] = *pScanline++;	// G
						Content[Offset+0] = *pScanline++;	// R
						Content[Offset+3] = *pScanline++;	// A
					}
				}
				B.UnlockBits( LockedBitmap );
			}
			return Image2Texture( W, H, Content );
		}
		public Texture2D	Image2Texture( int _Width, int _Height, byte[] _Content )
		{
			using ( PixelsBuffer Buff = new PixelsBuffer( _Content.Length ) )
			{
				using ( System.IO.BinaryWriter W = Buff.OpenStreamWrite() )
					W.Write( _Content );

				return Image2Texture( _Width, _Height, PIXEL_FORMAT.RGBA8_UNORM_sRGB, Buff );
			}
		}
		public Texture2D	PipoImage2Texture( System.IO.FileInfo _FileName ) {
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {

					int	W, H;
					W = R.ReadInt32();
					H = R.ReadInt32();

					PixelsBuffer	Buff = new PixelsBuffer( 4 * W * H * 4 );
					using ( System.IO.BinaryWriter Wr = Buff.OpenStreamWrite() )
					{
						WMath.Vector4D	C = new WMath.Vector4D();
						for ( int Y=0; Y < H; Y++ ) {
							for ( int X=0; X < W; X++ ) {
								C.x = R.ReadSingle();
								C.y = R.ReadSingle();
								C.z = R.ReadSingle();
								C.w = R.ReadSingle();

								Wr.Write( C.x );
								Wr.Write( C.y );
								Wr.Write( C.z );
								Wr.Write( C.w );
							}
						}
					}

					return Image2Texture( W, H, PIXEL_FORMAT.RGBA32_FLOAT, Buff );
				}
		}
		public Texture2D	Image2Texture( int _Width, int _Height, PIXEL_FORMAT _Format, PixelsBuffer _Content )
		{
			return new Texture2D( m_Device, _Width, _Height, 1, 1, _Format, false, false, new PixelsBuffer[] { _Content } );
		}

		/// <summary>
		/// Builds the SAT
		/// </summary>
		/// <param name="_FileName"></param>
		public unsafe void	ComputeSAT( System.IO.FileInfo _FileName, System.IO.FileInfo _TargetFileName )
		{
			int		W, H;
			byte[]	Content = null;
			using ( System.IO.FileStream S = _FileName.OpenRead() )
				using ( Bitmap B = Bitmap.FromStream( S ) as Bitmap )
				{
					W = B.Width;
					H = B.Height;
					Content = new byte[W*H*4];

					BitmapData	LockedBitmap = B.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb );
					for ( int Y=0; Y < H; Y++ )
					{
						byte*	pScanline = (byte*) LockedBitmap.Scan0 + Y * LockedBitmap.Stride;
						int		Offset = 4*W*Y;
						for ( int X=0; X < W; X++, Offset+=4 )
						{
							Content[Offset+2] = *pScanline++;	// B
							Content[Offset+1] = *pScanline++;	// G
							Content[Offset+0] = *pScanline++;	// R
							Content[Offset+3] = *pScanline++;	// A
						}
					}
					B.UnlockBits( LockedBitmap );
				}

			// Build the float4 image
			WMath.Vector4D[,]	Image = new WMath.Vector4D[W,H];
			for ( int Y=0; Y < H; Y++ ) {
				for ( int X=0; X < W; X++ ) {
					Image[X,Y] = new WMath.Vector4D( Content[4*(W*Y+X)+0] / 255.0f, Content[4*(W*Y+X)+1] / 255.0f, Content[4*(W*Y+X)+2] / 255.0f, 0.0f );

					// Linearize from gamma space
					Image[X,Y].x = (float) Math.Pow( Image[X,Y].x, 2.2 );
					Image[X,Y].y = (float) Math.Pow( Image[X,Y].y, 2.2 );
					Image[X,Y].z = (float) Math.Pow( Image[X,Y].z, 2.2 );
				}
			}

			// Perform the accumulation
			for ( int Y=0; Y < H; Y++ ) {
				for ( int X=1; X < W; X++ ) {
					Image[X,Y] += Image[X-1,Y];
				}
			}

			for ( int X=0; X < W; X++ ) {
				for ( int Y=1; Y < H; Y++ ) {
					Image[X,Y] += Image[X,Y-1];
				}
			}

			DirectXTexManaged.TextureCreator.CreateRGBA16FFile( _TargetFileName.FullName, Image );

			// Save as a simple format
			string	Pipo = _TargetFileName.FullName;
			Pipo = System.IO.Path.GetFileNameWithoutExtension( Pipo ) + ".pipo";
			System.IO.FileInfo	SimpleTargetFileName = new System.IO.FileInfo(  Pipo );
			using ( System.IO.FileStream S = SimpleTargetFileName.OpenWrite() )
				using ( System.IO.BinaryWriter Wr = new System.IO.BinaryWriter( S ) ) {

					Wr.Write( W );
					Wr.Write( H );
					for ( int Y=0; Y < H; Y++ ) {
						for ( int X=0; X < W; X++ ) {
							Wr.Write( Image[X,Y].x );
							Wr.Write( Image[X,Y].y );
							Wr.Write( Image[X,Y].z );
							Wr.Write( Image[X,Y].w );
						}
					}
				}
		} 

		#endregion

		private void	BuildPrimitives()
		{
			{
				VertexPt4[]	Vertices = new VertexPt4[4];
				Vertices[0] = new VertexPt4() { Pt = new float4( -1, +1, 0, 1 ) };	// Top-Left
				Vertices[1] = new VertexPt4() { Pt = new float4( -1, -1, 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexPt4() { Pt = new float4( +1, +1, 0, 1 ) };	// Top-Right
				Vertices[3] = new VertexPt4() { Pt = new float4( +1, -1, 0, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexPt4.FromArray( Vertices );

				m_Prim_Quad = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.Pt4 );
			}

			{
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[4];
				Vertices[0] = new VertexP3N3G3B3T2() { P = new float3( -1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 0 ) };	// Top-Left
				Vertices[1] = new VertexP3N3G3B3T2() { P = new float3( -1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 0, 1 ) };	// Bottom-Left
				Vertices[2] = new VertexP3N3G3B3T2() { P = new float3( +1, +1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 0 ) };	// Top-Right
				Vertices[3] = new VertexP3N3G3B3T2() { P = new float3( +1, -1, 0 ), N = new float3( 0, 0, 1 ), T = new float3( 1, 0, 0 ), B = new float3( 0, 1, 0 ), UV = new float2( 1, 1 ) };	// Bottom-Right

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Rectangle = new Primitive( m_Device, Vertices.Length, VerticesBuffer, null, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
			}

			{	// Build the sphere
				const int	W = 41;
				const int	H = 22;
				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[W*H];
				for ( int Y=0; Y < H; Y++ ) {
					double	Theta = Math.PI * Y / (H-1);
					float	CosTheta = (float) Math.Cos( Theta );
					float	SinTheta = (float) Math.Sin( Theta );
					for ( int X=0; X < W; X++ ) {
						double	Phi = 2.0 * Math.PI * X / (W-1);
						float	CosPhi = (float) Math.Cos( Phi );
						float	SinPhi = (float) Math.Sin( Phi );

						float3	N = new float3( SinTheta * SinPhi, CosTheta, SinTheta * CosPhi );
						float3	T = new float3( CosPhi, 0.0f, -SinPhi );
						float3	B = N.Cross( T );

						Vertices[W*Y+X] = new VertexP3N3G3B3T2() {
							P = N,
							N = N,
							T = T,
							B = B,
							UV = new float2( 2.0f * X / W, 1.0f * Y / H )
						};
					}
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				uint[]		Indices = new uint[(H-1) * (2*W+2)-2];
				int			IndexCount = 0;
				for ( int Y=0; Y < H-1; Y++ ) {
					for ( int X=0; X < W; X++ ) {
						Indices[IndexCount++] = (uint) ((Y+0) * W + X);
						Indices[IndexCount++] = (uint) ((Y+1) * W + X);
					}
					if ( Y < H-2 ) {
						Indices[IndexCount++] = (uint) ((Y+1) * W - 1);
						Indices[IndexCount++] = (uint) ((Y+1) * W + 0);
					}
				}

				m_Prim_Sphere = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_STRIP, VERTEX_FORMAT.P3N3G3B3T2 );
			}

			{	// Build the cube
				float3[]	Normals = new float3[6] {
					-float3.UnitX,
					float3.UnitX,
					-float3.UnitY,
					float3.UnitY,
					-float3.UnitZ,
					float3.UnitZ,
				};

				float3[]	Tangents = new float3[6] {
					float3.UnitZ,
					-float3.UnitZ,
					float3.UnitX,
					-float3.UnitX,
					-float3.UnitX,
					float3.UnitX,
				};

				VertexP3N3G3B3T2[]	Vertices = new VertexP3N3G3B3T2[6*4];
				uint[]		Indices = new uint[2*6*3];

				for ( int FaceIndex=0; FaceIndex < 6; FaceIndex++ ) {
					float3	N = Normals[FaceIndex];
					float3	T = Tangents[FaceIndex];
					float3	B = N.Cross( T );

					Vertices[4*FaceIndex+0] = new VertexP3N3G3B3T2() {
						P = N - T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 0 )
					};
					Vertices[4*FaceIndex+1] = new VertexP3N3G3B3T2() {
						P = N - T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 0, 1 )
					};
					Vertices[4*FaceIndex+2] = new VertexP3N3G3B3T2() {
						P = N + T - B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 1 )
					};
					Vertices[4*FaceIndex+3] = new VertexP3N3G3B3T2() {
						P = N + T + B,
						N = N,
						T = T,
						B = B,
						UV = new float2( 1, 0 )
					};

					Indices[2*3*FaceIndex+0] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+1] = (uint) (4*FaceIndex+1);
					Indices[2*3*FaceIndex+2] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+3] = (uint) (4*FaceIndex+0);
					Indices[2*3*FaceIndex+4] = (uint) (4*FaceIndex+2);
					Indices[2*3*FaceIndex+5] = (uint) (4*FaceIndex+3);
				}

				ByteBuffer	VerticesBuffer = VertexP3N3G3B3T2.FromArray( Vertices );

				m_Prim_Cube = new Primitive( m_Device, Vertices.Length, VerticesBuffer, Indices, Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
			}
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			try
			{
				m_Device.Init( panelOutput.Handle, false, false );
			}
			catch ( Exception _e )
			{
				m_Device = null;
				MessageBox.Show( "Failed to initialize DX device!\n\n" + _e.Message, "ShaderToy", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			BuildPrimitives();
//			m_Tex_AreaLight = Image2Texture( new System.IO.FileInfo( "StainedGlass.png" ) );
//			m_Tex_AreaLightSAT = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT.pipo" ) );

			m_Tex_AreaLight = Image2Texture( new System.IO.FileInfo( "StainedGlass2.jpg" ) );
			m_Tex_AreaLightSAT = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT2.pipo" ) );
			m_Tex_AreaLightSATFade = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT2Fade.pipo" ) );

//			m_Tex_AreaLightSAT = PipoImage2Texture( new System.IO.FileInfo( "AreaLightSAT3.pipo" ) );


			int	SHADOW_MAP_SIZE = 512;
			m_Tex_ShadowMap = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, DEPTH_STENCIL_FORMAT.D32 );
			m_Tex_ShadowSmoothie = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, PIXEL_FORMAT.RG16_FLOAT, false, false, null );
			m_Tex_ShadowSmoothiePou[0] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, PIXEL_FORMAT.RG16_FLOAT, false, false, null );
			m_Tex_ShadowSmoothiePou[1] = new Texture2D( m_Device, SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 1, 1, PIXEL_FORMAT.RG16_FLOAT, false, false, null );

			m_CB_Main = new ConstantBuffer<CB_Main>( m_Device, 0 );
			m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 1 );
			m_CB_Light = new ConstantBuffer<CB_Light>( m_Device, 2 );
			m_CB_Object = new ConstantBuffer<CB_Object>( m_Device, 3 );

			try
			{
				m_Shader_RenderAreaLight = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderAreaLight.hlsl" ) ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader \"RenderAreaLight\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderAreaLight = null;
			}

			try
			{
				m_Shader_RenderShadowMap = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderShadowMap.hlsl" ) ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader \"RenderShadow\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderShadowMap = null;
			}

			try
			{
				m_Shader_BuildSmoothie = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/BuildSmoothie.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS_Edge", null );;
				m_Shader_BuildSmoothieDistanceFieldH = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/BuildSmoothie.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS_DistanceFieldH", null );;
				m_Shader_BuildSmoothieDistanceFieldV = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/BuildSmoothie.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", null, "PS_DistanceFieldV", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader \"BuildSmoothie\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_BuildSmoothie = null;
				m_Shader_BuildSmoothieDistanceFieldH = null;
				m_Shader_BuildSmoothieDistanceFieldV = null;
			}

			try
			{
				m_Shader_RenderScene = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( "Shaders/RenderScene.hlsl" ) ), VERTEX_FORMAT.P3N3G3B3T2, "VS", null, "PS", null );;
			}
			catch ( Exception _e )
			{
				MessageBox.Show( "Shader \"RenderScene\" failed to compile!\n\n" + _e.Message, "Area Light Test", MessageBoxButtons.OK, MessageBoxIcon.Error );
				m_Shader_RenderScene = null;
			}

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) panelOutput.Width / panelOutput.Height, 0.01f, 100.0f );
			m_Manipulator.Attach( panelOutput, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, 1, 4 ), new float3( 0, 1, 0 ), float3.UnitY );

			// Start game time
			m_Ticks2Seconds = 1.0 / System.Diagnostics.Stopwatch.Frequency;
			m_StopWatch.Start();
			m_StartTime = GetGameTime();
		}

		protected override void OnFormClosed( FormClosedEventArgs e )
		{
			if ( m_Device == null )
				return;

			if ( m_Shader_RenderShadowMap != null ) {
				m_Shader_RenderShadowMap.Dispose();
			}
			if ( m_Shader_BuildSmoothie != null ) {
				m_Shader_BuildSmoothie.Dispose();
				m_Shader_BuildSmoothieDistanceFieldH.Dispose();
				m_Shader_BuildSmoothieDistanceFieldV.Dispose();
			}
			if ( m_Shader_RenderAreaLight != null ) {
				m_Shader_RenderAreaLight.Dispose();
			}
			if ( m_Shader_RenderScene != null ) {
				m_Shader_RenderScene.Dispose();
			}

			m_CB_Main.Dispose();
			m_CB_Camera.Dispose();
			m_CB_Light.Dispose();
			m_CB_Object.Dispose();

			m_Prim_Quad.Dispose();
			m_Prim_Rectangle.Dispose();
			m_Prim_Sphere.Dispose();
			m_Prim_Cube.Dispose();

			m_Tex_ShadowMap.Dispose();
			m_Tex_ShadowSmoothie.Dispose();
			m_Tex_AreaLight.Dispose();
			m_Tex_AreaLightSAT.Dispose();
			m_Tex_AreaLightSATFade.Dispose();

			m_Device.Exit();

			base.OnFormClosed( e );
		}

		/// <summary>
		/// Gets the current game time in seconds
		/// </summary>
		/// <returns></returns>
		public float	GetGameTime()
		{
			long	Ticks = m_StopWatch.ElapsedTicks;
			float	Time = (float) (Ticks * m_Ticks2Seconds);
			return Time;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e )
		{
			m_CB_Camera.m._Camera2World = m_Camera.Camera2World;
			m_CB_Camera.m._World2Camera = m_Camera.World2Camera;

			m_CB_Camera.m._Camera2Proj = m_Camera.Camera2Proj;
			m_CB_Camera.m._Proj2Camera = m_CB_Camera.m._Camera2Proj.Inverse;

			m_CB_Camera.m._World2Proj = m_CB_Camera.m._World2Camera * m_CB_Camera.m._Camera2Proj;
			m_CB_Camera.m._Proj2World = m_CB_Camera.m._Camera2World * m_CB_Camera.m._Proj2Camera;

			m_CB_Camera.UpdateData();
		}

		void RenderScene( Shader _Shader ) {

			// Render a floor plane
			if ( _Shader == m_Shader_RenderScene )
			{
				m_CB_Object.m._Local2World.MakeLookAt( float3.Zero, float3.UnitY, float3.UnitX );
				m_CB_Object.m._Local2World.Scale( new float3( 2.0f, 2.0f, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 1, 1 );
				m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.93f );
				m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
				m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
				m_CB_Object.UpdateData();

				m_Prim_Rectangle.Render( _Shader );
			}

			// Render the sphere
			m_CB_Object.m._Local2World.MakeLookAt( new float3( 0, 0.5f, 1.0f ), new float3( 0, 0.5f, 2 ), float3.UnitY );
//			m_CB_Object.m._Local2World.MakeLookAt( new float3( 0, 0.3f, 1.0f ), new float3( 0, 0.3f, 2 ), float3.UnitY );
			m_CB_Object.m._Local2World.Scale( new float3( 0.5f, 0.5f, 0.5f ) );
			m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
			m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 0.8f, 0.5f );
			m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.4f, 0.03f );
 			m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
 			m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
			m_CB_Object.UpdateData();

			m_Prim_Sphere.Render( _Shader );

			// Render the tiny cube
			m_CB_Object.m._Local2World.MakeLookAt( new float3( 1.0f, 0.1f, 0.0f ), new float3( 1.0f, 0.1f, 1 ), float3.UnitY );
			m_CB_Object.m._Local2World.Scale( new float3( 0.1f, 0.1f, 0.1f ) );
			m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
			m_CB_Object.m._DiffuseAlbedo = 0.5f * new float3( 1, 1, 1 );
			m_CB_Object.m._SpecularTint = new float3( 0.95f, 0.94f, 0.92f );
 			m_CB_Object.m._Gloss = floatTrackbarControlGloss.Value;
 			m_CB_Object.m._Metal = floatTrackbarControlMetal.Value;
			m_CB_Object.UpdateData();

			m_Prim_Cube.Render( _Shader );
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( m_Device == null )
				return;

			// Setup global data
			m_CB_Main.m.iResolution = new float3( panelOutput.Width, panelOutput.Height, 0 );
			m_CB_Main.m.iGlobalTime = GetGameTime() - m_StartTime;
			m_CB_Main.UpdateData();

			// Setup area light buffer
			float		SizeX = 1;//0.5f;
			float		SizeY = 1.0f;
			float		RollAngle = (float) (Math.PI * floatTrackbarControlLightRoll.Value / 180.0);
			float3		LightPosition = new float3( 1.2f + floatTrackbarControlLightPosX.Value, 1.0f + floatTrackbarControlLightPosY.Value, -1.0f + floatTrackbarControlLightPosZ.Value );
			float3		LightTarget = new float3( LightPosition.x + floatTrackbarControlLightTargetX.Value, LightPosition.y + floatTrackbarControlLightTargetY.Value, LightPosition.z + 2.0f + floatTrackbarControlLightTargetZ.Value );
			float3		LightUp = new float3( (float) Math.Sin( -RollAngle ), (float) Math.Cos( RollAngle ), 0.0f );
			float4x4	AreaLight2World = new float4x4(); 
						AreaLight2World.MakeLookAt( LightPosition, LightTarget, LightUp );

			float4x4	World2AreaLight = AreaLight2World.Inverse;

			double		Phi = Math.PI * floatTrackbarControlProjectionPhi.Value / 180.0;
			double		Theta = Math.PI * floatTrackbarControlProjectionTheta.Value / 180.0;
			float3		Direction = new float3( (float) (Math.Sin(Theta) * Math.Sin(Phi)), (float) (Math.Sin(Theta) * Math.Cos(Phi)), (float) Math.Cos( Theta ) );

			const float	DiffusionMin = 1e-2f;
			const float	DiffusionMax = 1000.0f;
//			float		Diffusion_Diffuse = DiffusionMin / (DiffusionMin / DiffusionMax + floatTrackbarControlProjectionDiffusion.Value);
			float		Diffusion_Diffuse = DiffusionMax + (DiffusionMin - DiffusionMax) * (float) Math.Pow( floatTrackbarControlProjectionDiffusion.Value, 0.01f );
			float		Diffusion_Specular = DiffusionMax + (DiffusionMin - DiffusionMax) * (float) Math.Pow( 1.0f - floatTrackbarControlProjectionDiffusion.Value, 0.05f );

//			float3		LocalDirection_Diffuse = (float3) (new float4( Diffusion_Diffuse * Direction, 0 ) * World2AreaLight);
			float3		LocalDirection_Diffuse = Diffusion_Diffuse * Direction;
			float3		LocalDirection_Specular = (float3) (new float4( Diffusion_Specular * Direction, 0 ) * World2AreaLight);

			m_CB_Light.m._AreaLightX = (float3) AreaLight2World.GetRow( 0 );
			m_CB_Light.m._AreaLightY = (float3) AreaLight2World.GetRow( 1 );
			m_CB_Light.m._AreaLightZ = (float3) AreaLight2World.GetRow( 2 );
			m_CB_Light.m._AreaLightT = (float3) AreaLight2World.GetRow( 3 );
			m_CB_Light.m._AreaLightScaleX = SizeX;
			m_CB_Light.m._AreaLightScaleY = SizeY;
			m_CB_Light.m._AreaLightDiffusion = floatTrackbarControlProjectionDiffusion.Value;
			m_CB_Light.m._AreaLightIntensity = floatTrackbarControlLightIntensity.Value;
			m_CB_Light.m._ProjectionDirectionDiff = LocalDirection_Diffuse;
			m_CB_Light.m._ProjectionDirectionSpec = LocalDirection_Specular;
			m_CB_Light.UpdateData();


			// =========== Render shadow map ===========
			if ( m_Shader_RenderShadowMap != null && m_Shader_RenderShadowMap.Use() ) {
				m_Tex_ShadowMap.RemoveFromLastAssignedSlots();

				m_Device.SetRenderTargets( m_Tex_ShadowMap.Width, m_Tex_ShadowMap.Height, new View2D[0], m_Tex_ShadowMap );
				m_Device.ClearDepthStencil( m_Tex_ShadowMap, 1.0f, 0, true, false );

				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS_EQUAL, BLEND_STATE.DISABLED );

				RenderScene( m_Shader_RenderShadowMap );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowMap.SetPS( 2 );
			}

			if ( m_Shader_BuildSmoothie != null && m_Shader_BuildSmoothie.Use() ) {
				m_Tex_ShadowSmoothie.RemoveFromLastAssignedSlots();

				m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

				// Render the (silhouette + Z) RG16 buffer
				m_Device.SetRenderTarget( m_Tex_ShadowSmoothie, null );

				m_Prim_Quad.Render( m_Shader_BuildSmoothie );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowSmoothie.SetPS( 3 );

				// Build distance field
				m_Device.SetRenderTarget( m_Tex_ShadowSmoothiePou[0], null );
				m_Shader_BuildSmoothieDistanceFieldH.Use();
				m_Prim_Quad.Render( m_Shader_BuildSmoothieDistanceFieldH );

// m_Device.RemoveRenderTargets();
// m_Tex_ShadowSmoothiePou[0].SetPS( 3 );

				m_Device.SetRenderTarget( m_Tex_ShadowSmoothiePou[1], null );
				m_Tex_ShadowSmoothiePou[0].SetPS( 0 );
				m_Shader_BuildSmoothieDistanceFieldV.Use();
				m_Prim_Quad.Render( m_Shader_BuildSmoothieDistanceFieldV );

				m_Device.RemoveRenderTargets();
				m_Tex_ShadowSmoothiePou[1].SetPS( 3 );
			}


			// =========== Render scene ===========
			m_Device.SetRenderTarget( m_Device.DefaultTarget, m_Device.DefaultDepthStencil );

			m_Device.Clear( m_Device.DefaultTarget, float4.Zero );
			m_Device.ClearDepthStencil( m_Device.DefaultDepthStencil, 1.0f, 0, true, false );

			m_Tex_AreaLightSAT.SetPS( 0 );
			m_Tex_AreaLightSATFade.SetPS( 1 );

			// Render the area light itself
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.READ_WRITE_DEPTH_LESS_EQUAL, BLEND_STATE.DISABLED );
			if ( m_Shader_RenderAreaLight != null && m_Shader_RenderAreaLight.Use() ) {

				m_CB_Object.m._Local2World = AreaLight2World;
				m_CB_Object.m._Local2World.Scale( new float3( SizeX, SizeY, 1.0f ) );
				m_CB_Object.m._World2Local = m_CB_Object.m._Local2World.Inverse;
				m_CB_Object.UpdateData();

				m_Tex_AreaLight.SetPS( 4 );

				m_Prim_Rectangle.Render( m_Shader_RenderAreaLight );
			} else {
				m_Device.Clear( new float4( 1, 0, 0, 0 ) );
			}


			// Render the scene
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_BACK, DEPTHSTENCIL_STATE.NOCHANGE, BLEND_STATE.NOCHANGE );
			if ( m_Shader_RenderScene != null && m_Shader_RenderScene.Use() ) {

				RenderScene( m_Shader_RenderScene );

			} else {
				m_Device.Clear( new float4( 1, 1, 0, 0 ) );
			}


			// Show!
			m_Device.Present( false );


			// Update window text
//			Text = "Zombizous Prototype - " + m_Game.m_CurrentGameTime.ToString( "G5" ) + "s";
		}

		private void buttonReload_Click( object sender, EventArgs e )
		{
			if ( m_Device != null )
				m_Device.ReloadModifiedShaders();
		}
	}
}