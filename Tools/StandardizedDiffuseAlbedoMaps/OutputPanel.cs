﻿#define ABS_NORMAL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class OutputPanel : Panel
	{
		private Bitmap		m_Bitmap = null;

		private float[,]	m_Luminance = null;
		public float[,]		Luminance
		{
			get { return m_Luminance; }
			set {
				m_Luminance = value;
				UpdateBitmap();
			}
		}

		public OutputPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
		}

		public unsafe void		UpdateBitmap()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			// Fill pixel per pixel
			if ( m_Luminance != null )
			{
				int		SizeX = m_Luminance.GetLength( 0 );
				int		SizeY = m_Luminance.GetLength( 1 );

				BitmapData	LockedBitmap = m_Bitmap.LockBits( new Rectangle( 0, 0, W, H ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
				byte		R, G, B, A = 0xFF;
				for ( int Y=0; Y < H; Y++ )
				{
					byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + LockedBitmap.Stride * Y;
					for ( int X=0; X < W; X++ )
					{
						float	Lum = m_Luminance[SizeX*X/W, SizeY*Y/H];
						R = G = B = (byte) Math.Min( 255, 255.0f * Lum );
						*pScanline++ = B;
						*pScanline++ = G;
						*pScanline++ = R;
						*pScanline++ = A;
					}
				}
				m_Bitmap.UnlockBits( LockedBitmap );
			}

			Invalidate();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();

			m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			UpdateBitmap();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );
//				e.Graphics.DrawImage( m_Bitmap, new Rectangle( 0, 0, Width, Height ), 0, 0, m_Bitmap.Width, m_Bitmap.Height, GraphicsUnit.Pixel );
		}
	}
}