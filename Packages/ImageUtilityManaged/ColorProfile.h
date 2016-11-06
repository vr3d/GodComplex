//////////////////////////////////////////////////////////////////////////
//
//////////////////////////////////////////////////////////////////////////
//
#pragma once

using namespace System;
using namespace SharpMath;

namespace ImageUtility {

	public ref class ColorProfile {
	public:
		#pragma region CONSTANTS

		static const float2		ILLUMINANT_A	= float2( ImageUtilityLib::ColorProfile::ILLUMINANT_A.x,	ImageUtilityLib::ColorProfile::ILLUMINANT_A.y );	// Incandescent, tungsten
		static const float2		ILLUMINANT_D50	= float2( ImageUtilityLib::ColorProfile::ILLUMINANT_D50.x,	ImageUtilityLib::ColorProfile::ILLUMINANT_D50.y );	// Daylight, Horizon
		static const float2		ILLUMINANT_D55	= float2( ImageUtilityLib::ColorProfile::ILLUMINANT_D55.x,	ImageUtilityLib::ColorProfile::ILLUMINANT_D55.y );	// Mid-Morning, Mid-Afternoon
		static const float2		ILLUMINANT_D65	= float2( ImageUtilityLib::ColorProfile::ILLUMINANT_D65.x,	ImageUtilityLib::ColorProfile::ILLUMINANT_D65.y );	// Daylight, Noon, Overcast (sRGB reference illuminant)
		static const float2		ILLUMINANT_E	= float2( ImageUtilityLib::ColorProfile::ILLUMINANT_E.x,	 ImageUtilityLib::ColorProfile::ILLUMINANT_E.y );	// Reference

		static const float		GAMMA_EXPONENT_sRGB = ImageUtilityLib::ColorProfile::GAMMA_EXPONENT_sRGB;
		static const float		GAMMA_EXPONENT_ADOBE = ImageUtilityLib::ColorProfile::GAMMA_EXPONENT_ADOBE;
		static const float		GAMMA_EXPONENT_PRO_PHOTO = ImageUtilityLib::ColorProfile::GAMMA_EXPONENT_PRO_PHOTO;

		#pragma endregion


	public:
		#pragma region NESTED TYPES

		enum class STANDARD_PROFILE {
			INVALID,		// The profile is invalid (meaning one of the chromaticities was not initialized!)
			CUSTOM,			// No recognizable standard profile (custom)
			LINEAR,			// sRGB with linear gamma
			sRGB,			// sRGB with D65 illuminant
			ADOBE_RGB_D50,	// Adobe RGB with D50 illuminant
			ADOBE_RGB_D65,	// Adobe RGB with D65 illuminant
			PRO_PHOTO,		// ProPhoto with D50 illuminant
			RADIANCE,		// Radiance HDR format with E illuminant
		};

		// Enumerates the various supported gamma curves
		enum class GAMMA_CURVE {
			STANDARD,		// Standard gamma curve using a single exponent and no linear slope
			sRGB,			// sRGB gamma with linear slope
			PRO_PHOTO,		// ProPhoto gamma with linear slope
		};

		// Describes the Red, Green, Blue and White Point chromaticities of a simple/standard color profile
		[System::Diagnostics::DebuggerDisplay( "R=({R.x},{R.y}) G=({G.x},{G.y}) B=({B.x},{B.y}) W=({W.x},{W.y}) Prof={RecognizedChromaticity}" )]
		ref struct	Chromaticities {
		internal:
		bool												m_ownedObject;	// True if we created the object and should also delete it, false if it's owned by someone else
			ImageUtilityLib::ColorProfile::Chromaticities*	m_nativeObject;	// Can NEVER be NULL! (we always wrap a valid native profile!)

			// Special wrapper constructor
			Chromaticities( ImageUtilityLib::ColorProfile::Chromaticities& _nativeObject ) {
				m_nativeObject = &_nativeObject;
				m_ownedObject = false;	// This object is owned by someone else! It's not ours to delete!
			}

		public:	// PROPERTIES
			static property Chromaticities^	Empty			{ Chromaticities^ get() { return gcnew Chromaticities( float2(), float2(), float2(), float2() ); } }
			static property Chromaticities^	sRGB			{ Chromaticities^ get() { return gcnew Chromaticities( float2( 0.6400f, 0.3300f ), float2( 0.3000f, 0.6000f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_D65 ); } }
			static property Chromaticities^	AdobeRGB_D50	{ Chromaticities^ get() { return gcnew Chromaticities( float2( 0.6400f, 0.3300f ), float2( 0.2100f, 0.7100f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_D50 ); } }
			static property Chromaticities^	AdobeRGB_D65	{ Chromaticities^ get() { return gcnew Chromaticities( float2( 0.6400f, 0.3300f ), float2( 0.2100f, 0.7100f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_D65 ); } }
			static property Chromaticities^	ProPhoto		{ Chromaticities^ get() { return gcnew Chromaticities( float2( 0.7347f, 0.2653f ), float2( 0.1596f, 0.8404f ), float2( 0.0366f, 0.0001f ), ILLUMINANT_D50 ); } }
			static property Chromaticities^	Radiance		{ Chromaticities^ get() { return gcnew Chromaticities( float2( 0.6400f, 0.3300f ), float2( 0.2900f, 0.6000f ), float2( 0.1500f, 0.0600f ), ILLUMINANT_E ); } }

			/// <summary>
			/// Attempts to recognize the current chromaticities as a standard profile
			/// </summary>
			/// <returns></returns>
			property STANDARD_PROFILE	RecognizedChromaticity {
				STANDARD_PROFILE	get() {
					ImageUtilityLib::ColorProfile::STANDARD_PROFILE	nativeStandardProfile = m_nativeObject->FindRecognizedChromaticity();
					return STANDARD_PROFILE( nativeStandardProfile );
				}
			}

		public:

			Chromaticities( Chromaticities^ _other ) {
				m_nativeObject = new ImageUtilityLib::ColorProfile::Chromaticities( *_other->m_nativeObject );
				m_ownedObject = true;
			}
			Chromaticities( float xr, float yr, float xg, float yg, float xb, float yb, float xw, float yw ) {
				m_nativeObject = new ImageUtilityLib::ColorProfile::Chromaticities( xr, yr, xg, yg, xb, yb, xw, yw );
				m_ownedObject = true;
			}
			Chromaticities( float2 r, float2 g, float2 b, float2 w ) {
				m_nativeObject = new ImageUtilityLib::ColorProfile::Chromaticities( bfloat2( r.x, r.y ), bfloat2( g.x, g.y ), bfloat2( b.x, b.y ), bfloat2( w.x, w.y ) );
				m_ownedObject = true;
			}
			~Chromaticities() {
				SAFE_DELETE( m_nativeObject );
			}

			static bool		operator==( Chromaticities^ a, Chromaticities^ b ) {
				if ( (System::Object^) a == (System::Object^) b ) {
					return true;
				}
				if ( (System::Object^) a == nullptr || (System::Object^) b == nullptr ) {
					return false;
				}
				return a->m_nativeObject->Equals( *b->m_nativeObject );
			}
			static bool		operator!=( Chromaticities^ a, Chromaticities^ b ) {
				if ( (System::Object^) a == (System::Object^) b ) {
					return false;
				}
				if ( (System::Object^) a == nullptr || (System::Object^) b == nullptr ) {
					return true;
				}
				return !a->m_nativeObject->Equals( *b->m_nativeObject );
			}
		};

		#pragma endregion

	internal:
		bool							m_ownedObject;	// True if we created the object and should also delete it, false if it's owned by someone else
		ImageUtilityLib::ColorProfile*	m_nativeObject;	// Can NEVER be NULL! (we always wrap a valid native profile!)

		// Special wrapper constructor
		ColorProfile( ImageUtilityLib::ColorProfile& _nativeObject ) {
			m_nativeObject = &_nativeObject;
			m_ownedObject = false;	// This object is owned by someone else! It's not ours to delete!
		}

	public:	// PROPERTIES

		/// <summary>
		/// Gets the chromaticities attached to the profile
		/// </summary>
		property Chromaticities^	Chromas {
			Chromaticities^	get() { return gcnew Chromaticities( m_nativeObject->GetChromas() ); }
			void			set( Chromaticities^ value ) {
				m_nativeObject->SetChromas( *value->m_nativeObject );
			}
		}

		/// <summary>
		/// Gets the transform to convert RGB to CIEXYZ
		/// </summary>
		property SharpMath::float4x4^	MatrixRGB2XYZ {
			SharpMath::float4x4^	get();
		}

		/// <summary>
		/// Gets the transform to convert CIEXYZ to RGB
		/// </summary>
		property SharpMath::float4x4^	MatrixXYZ2RGB {
			SharpMath::float4x4^	get();
		}

		/// <summary>
		/// Gets or sets the image gamma curve
		/// </summary>
		property GAMMA_CURVE	GammaCurve {
			GAMMA_CURVE	get()					{ return GAMMA_CURVE( m_nativeObject->GetGammaCurve() ); }
			void		set( GAMMA_CURVE value ){ m_nativeObject->SetGammaCurve( ImageUtilityLib::ColorProfile::GAMMA_CURVE( value ) ); }
		}

		/// <summary>
		/// Gets or sets the image gamma
		/// </summary>
		property float			GammaExponent {
			float	get()				{ return m_nativeObject->GetGammaExponent(); }
			void	set( float value )	{ m_nativeObject->SetGammaExponent( value ); }
		}

		/// <summary>
		/// True if the profile was found in the file's metadata and can be considered accurate.
		/// False if it's the default assumed profile and may NOT be the actual image's profile.
		/// </summary>
		property bool			ProfileFoundInFile {
			bool	get()				{ return m_nativeObject->GetProfileFoundInFile(); }
			void	set( bool value )	{ m_nativeObject->SetProfileFoundInFile( value ); }
		}

	public:	// METHODS
		ColorProfile() {
			m_nativeObject = new ImageUtilityLib::ColorProfile();
			m_ownedObject = true;
		}
		ColorProfile( STANDARD_PROFILE _profile ) {
			m_nativeObject = new ImageUtilityLib::ColorProfile( ImageUtilityLib::ColorProfile::STANDARD_PROFILE( _profile ) );
			m_ownedObject = true;
		}
		ColorProfile( const Chromaticities^ _chromaticities, GAMMA_CURVE _gammaCurve, float _gammaExponent ) {
			m_nativeObject = new ImageUtilityLib::ColorProfile( *_chromaticities->m_nativeObject, ImageUtilityLib::ColorProfile::GAMMA_CURVE( _gammaCurve ), _gammaExponent );
			m_ownedObject = true;
		}
		~ColorProfile() {
			if ( m_ownedObject ) {
				SAFE_DELETE( m_nativeObject );
			}
		}

	public:

		#pragma region IColorConverter Members

		/// <summary>
		/// Converts a CIEXYZ color to a RGB color
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		void	XYZ2RGB( float4^ _XYZ, float4^ _RGB ) {
			bfloat4	XYZ( _XYZ->x, _XYZ->y, _XYZ->z, _XYZ->w );
			bfloat4	RGB;
			m_nativeObject->XYZ2RGB( XYZ, RGB );
			_RGB->Set( RGB.x, RGB.y, RGB.z, RGB.w );
		}

		/// <summary>
		/// Converts a RGB color to a CIEXYZ color
		/// </summary>
		/// <param name="_RGB"></param>
		/// <returns></returns>
		void	RGB2XYZ( float4^ _RGB, float4^ _XYZ ) {
			bfloat4	RGB( _RGB->x, _RGB->y, _RGB->z, _RGB->w );
			bfloat4	XYZ;
			m_nativeObject->RGB2XYZ( RGB, XYZ );
			_XYZ->Set( XYZ.x, XYZ.y, XYZ.z, XYZ.w );
		}

		/// <summary>
		/// Converts a CIEXYZ color to a RGB color
		/// </summary>
		/// <param name="_XYZ"></param>
		cli::array<float4>^	XYZ2RGB( cli::array<float4>^ _XYZ );

		/// <summary>
		/// Converts a RGB color to a CIEXYZ color
		/// </summary>
		/// <param name="_RGB"></param>
		cli::array<float4>^	RGB2XYZ( cli::array<float4>^ _RGB );

		#pragma endregion

	public:

		#pragma region Helpers

		/// <summary>
		/// Converts from XYZ to xyY
		/// </summary>
		/// <param name="_XYZ"></param>
		/// <returns></returns>
		static void	XYZ2xyY( float3^ _XYZ, float3^ _xyY ) {
			bfloat3	result;
			ImageUtilityLib::ColorProfile::XYZ2xyY( bfloat3( _XYZ->x, _XYZ->y, _XYZ->z ), result );
			_xyY->Set( result.x, result.y, result.z );
		}

		/// <summary>
		/// Converts from xyY to XYZ
		/// </summary>
		/// <param name="_xyY"></param>
		/// <returns></returns>
		static void	xyY2XYZ( float3^ _xyY, float3^ _XYZ ) {
			bfloat3	result;
			ImageUtilityLib::ColorProfile::xyY2XYZ( bfloat3( _xyY->x, _xyY->y, _xyY->z ), result );
			_XYZ->Set( result.x, result.y, result.z );
		}

		/// <summary>
		/// Applies gamma correction to the provided color
		/// </summary>
		/// <param name="c">The color to gamma-correct</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		inline static float	GammaCorrect( float c, float _imageGamma ) {
			return ImageUtilityLib::ColorProfile::GammaCorrect( c, _imageGamma );
		}

		/// <summary>
		/// Un-aplies gamma correction to the provided color
		/// </summary>
		/// <param name="c">The color to gamma-uncorrect</param>
		/// <param name="_ImageGamma">The gamma correction the image was encoded with (JPEG is 2.2 for example, if not sure use 1.0)</param>
		/// <returns></returns>
		inline static float	GammaUnCorrect( float c, float _imageGamma ) {
			return ImageUtilityLib::ColorProfile::GammaUnCorrect( c, _imageGamma );
		}

		/// <summary>
		/// Converts from linear space to sRGB
		/// Code borrowed from D3DX_DXGIFormatConvert.inl from the DX10 SDK
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		inline static float		Linear2sRGB( float c ) {
			return ImageUtilityLib::ColorProfile::Linear2sRGB( c );
		}

		/// <summary>
		/// Converts from sRGB to linear space
		/// Code borrowed from D3DX_DXGIFormatConvert.inl from the DX10 SDK
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		inline static float		sRGB2Linear( float c ) {
			return ImageUtilityLib::ColorProfile::sRGB2Linear( c );
		}

		#pragma endregion
	};
}
