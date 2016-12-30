////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 1D FFT
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#include "FFT.hlsl"

Texture2D<float2>	_texIn : register(t0);
RWTexture2D<float2>	_texOut : register(u0);

//#define	SYNC	GroupMemoryBarrier();	// We only need group barriers because groups don't interfere with each other
#define	SYNC	GroupMemoryBarrierWithGroupSync();

// Fetches and mixes inputs for a specific group shift
// We always have groups of size 1, 2, 4, 8, etc. that are classified into [Even] and (Odd) couples:
//
//	Stage 0 =>	[.](.) [.](.) [.](.) [.](.) [.](.) [.](.) [.](.) [.](.)		size 1
//	Stage 1 =>	 [..]   (..)   [..]   (..)   [..]   (..)   [..]   (..)		size 2
//	Stage 2 =>	    [....]       (....)         [....]        (....)		size 4
//	Stage 3 =>	        [........]   	             (........)				size 8
//
// Each thread fetches even and odd groups' values, mix them using the "twiddle factors" and stores the result:
//			
//				[E]		(O)
//				 |		 |
//				 |\     /|
//				 |  \ /  |
//				 |   X   |
//				 |  / \  |
//				 |/     \|
//				 |		 |
//			   (X_k)  (X_(k+N/2))
//
//	X_k = E_k + e^(-i * 2PI * k / N) * O_k
//	X_(k+N/2) = E_k - e^(-i * 2PI * k / N) * O_k
//
void	Twiddle( float2 _sinCos, inout float2 _V0, inout float2 _V1 ) {
	float2	E = _V0;
	float2	O = _V1;
	float	s = _sinCos.x;
	float	c = _sinCos.y;

	_V0 = float2(	E.x + c * O.x - s * O.y, 
					E.y + s * O.x + c * O.y );
	_V1 = float2(	E.x - c * O.x + s * O.y, 
					E.y - s * O.x - c * O.y );
}

void	FetchAndMix( uint _groupShift, uint _groupThreadIndex, float _frequency ) {
	uint	groupSize = 1U << _groupShift;
	uint	elementIndex = _groupThreadIndex & (groupSize-1U);
	uint	groupOffset = ((_groupThreadIndex - elementIndex) << 1U)
						+ elementIndex;

	float2	sc;
	sincos( elementIndex * _frequency, sc.x, sc.y );
	Twiddle( sc, gs_temp[groupOffset], gs_temp[groupOffset + groupSize] );
}

// Applies FFT from stage 0 (size 1) to stage 7 (size 128)
// NOTE: Each thread reads and writes 2 values so each thread at stage 0 reads 2 size-1 groups and writes an entire size-2 group by itself
//			while at stage 6, each thread group will read 2*64 values and write them as a single final size-128 group.
[numthreads( 64, 1, 1 )]
void	CS__1to128( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _groupThreadID.x;

	// Fetch level 0 - Group size = 2*1->1*2 - Frequency = 2PI/2
	gs_temp[2*index+0] = _texIn[uint2(2*_dispatchThreadID.x+0, 0)];
	gs_temp[2*index+1] = _texIn[uint2(2*_dispatchThreadID.x+1, 0)];
	Twiddle( float2( 0, 1 ), gs_temp[2*index+0], gs_temp[2*index+1] );
	SYNC

	// Fetch level 1 - Group size = 2*2->1*4 - Frequency = 2PI/4
	float	frequency = _sign * (0.5 * PI);
	FetchAndMix( 1, index, frequency );
	SYNC

	// Fetch level 2 - Group size = 2*4->1*8 - Frequency = 2PI/8
	frequency *= 0.5;
	FetchAndMix( 2, index, frequency );
	SYNC

	// Fetch level 3 - Group size = 2*8->1*16 - Frequency = 2PI/16
	frequency *= 0.5;
	FetchAndMix( 3, index, frequency );
	SYNC

	// Fetch level 4 - Group size = 2*16->1*32 - Frequency = 2PI/32
	frequency *= 0.5;
	FetchAndMix( 4, index, frequency );
	SYNC

	// Fetch level 5 - Group size = 2*32->1*64 - Frequency = 2PI/64
	frequency *= 0.5;
	FetchAndMix( 5, index, frequency );
	SYNC

	// Fetch level 6 - Group size = 2*64->1*128 - Frequency = 2PI/128
	frequency *= 0.5;
	FetchAndMix( 6, index, frequency );

	_texOut[uint2(2*_dispatchThreadID.x+0, 0)] = gs_temp[2*index+0];
	_texOut[uint2(2*_dispatchThreadID.x+1, 0)] = gs_temp[2*index+1];
}

// Applies FFT from stage 7 (size 128) to stage 8 (size 256)
[numthreads( 128, 1, 1 )]
void	CS__128to256( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;	// in [0,128[ (2 groups of 64 threads)
	uint2	pos = uint2( index, 0 );

	// Read 8 source values with a stride of 128
	float2	V0 = _texIn[pos];	pos.x += 128U;
	float2	V1 = _texIn[pos];

	// Apply twiddling - Group size = 2*128->1*256 - Frequency = 2PI/256
	float	frequency = index * _sign * (PI / 128.0);
	float2	sinCos;
	sincos( frequency, sinCos.x, sinCos.y );

	Twiddle( sinCos, V0, V1 );

	// Store
	_texOut[pos] = V1;	pos.x -= 128U;
	_texOut[pos] = V0;
}

// Applies FFT from stage 7 (size 128) to stage 9 (size 512)
[numthreads( 128, 1, 1 )]
void	CS__128to512( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;	// in [0,128[ (2 groups of 64 threads)
	uint2	pos = uint2( index, 0 );

	// Read 8 source values with a stride of 128
	float2	V0 = _texIn[pos];	pos.x += 128U;
	float2	V1 = _texIn[pos];	pos.x += 128U;
	float2	V2 = _texIn[pos];	pos.x += 128U;
	float2	V3 = _texIn[pos];

	// Apply twiddling - Group size = 2*128->1*256 - Frequency = 2PI/256
	float	frequency = index * _sign * (PI / 128.0);
	float2	sinCos;
	sincos( frequency, sinCos.x, sinCos.y );

	Twiddle( sinCos, V0, V1 );
	Twiddle( sinCos, V2, V3 );

	// Apply twiddling - Group size = 2*256->1*512 - Frequency = 2PI/512
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V0, V2 );
	sinCos = float2( _sign * sinCos.y, -_sign * sinCos.x );			// frequency + PI/2
	Twiddle( sinCos, V1, V3 );

	// Store
	_texOut[pos] = V3;	pos.x -= 128U;
	_texOut[pos] = V2;	pos.x -= 128U;
	_texOut[pos] = V1;	pos.x -= 128U;
	_texOut[pos] = V0;
}

// Applies FFT from stage 7 (size 128) to stage 10 (size 1024)
[numthreads( 128, 1, 1 )]
void	CS__128to1024( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;	// in [0,128[ (2 groups of 64 threads)
	uint2	pos = uint2( index, 0 );

	// Read 8 source values with a stride of 128
	float2	V[8];
	[unroll]
	for ( uint i=0; i < 8; i++ ) {
		V[i] = _texIn[pos];
		pos.x += 128U;
	}

	// Apply twiddling - Group size = 2*128->1*256 - Frequency = 2PI/256
	float	frequency = index * _sign * (PI / 128.0);
	float2	sinCos;
	sincos( frequency, sinCos.x, sinCos.y );

	Twiddle( sinCos, V[0], V[1] );
	Twiddle( sinCos, V[2], V[3] );
	Twiddle( sinCos, V[4], V[5] );
	Twiddle( sinCos, V[6], V[7] );

	// Apply twiddling - Group size = 2*256->1*512 - Frequency = 2PI/512
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V[0], V[2] );
	Twiddle( sinCos, V[4], V[6] );

	sinCos = float2( _sign * sinCos.y, -_sign * sinCos.x );			// frequency + PI/2
	Twiddle( sinCos, V[1], V[3] );
	Twiddle( sinCos, V[5], V[7] );

	// Apply twiddling - Group size = 2*512->1*1024 - Frequency = 2PI/1024
	frequency *= 0.5;
	sincos( frequency, sinCos.x, sinCos.y );
	Twiddle( sinCos, V[0], V[4] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + PI/2
	Twiddle( sinCos, V[2], V[6] );

	sincos( frequency + _sign * (0.25 * PI), sinCos.x, sinCos.y );	// frequency + PI/4
	Twiddle( sinCos, V[1], V[5] );
	sinCos = _sign * float2( sinCos.y, -sinCos.x );					// frequency + 3PI/4
	Twiddle( sinCos, V[3], V[7] );

	// Store
	pos.x = _dispatchThreadID.x;
	[unroll]
	for ( uint j=0; j < 8; j++ ) {
		_texOut[pos] = V[j];
		pos.x += 128U;
	}
}


/*
// Applies FFT from stage 7 (size 128) to stage 8 (size 256)
[numthreads( 64, 1, 1 )]
void	CS__128to256( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;

	// Fetch level 7 - Group size = 2*128->1*256 - Frequency = PI/64
	float	frequency = _sign * PI / 64.0;
	FetchAndMix_Large( 7, index, frequency, _texIn, _texOut );
}

// Applies FFT from stage 7 (size 128) to stage 9 (size 512)
[numthreads( 64, 1, 1 )]
void	CS__128to512( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;

	// Fetch level 7 - Group size = 2*128->1*256 - Frequency = PI/64
	float	frequency = _sign * PI / 64.0;
	FetchAndMix_Large( 7, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 8 - Group size = 2*256->1*512 - Frequency = PI/128
	frequency *= 0.5;
	FetchAndMix_Large( 8, index, frequency, _texIn, _texOut );
}

// Applies FFT from stage 7 (size 128) to stage 11 (size 2048)
[numthreads( 64, 1, 1 )]
void	CS__128to2048( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;

	// Fetch level 7 - Group size = 2*128->1*256 - Frequency = PI/64
	float	frequency = _sign * PI / 64.0;
	FetchAndMix_Large( 7, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 8 - Group size = 2*256->1*512 - Frequency = PI/128
	frequency *= 0.5;
	FetchAndMix_Large( 8, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 9 - Group size = 2*512->1*1024 - Frequency = PI/256
	frequency *= 0.5;
	FetchAndMix_Large( 9, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 10 - Group size = 2*1024->1*2048 - Frequency = PI/512
	frequency *= 0.5;
	FetchAndMix_Large( 10, index, frequency, _texIn, _texOut );
}

// Applies FFT from stage 7 (size 128) to stage 12 (size 4096)
[numthreads( 64, 1, 1 )]
void	CS__128to4096( uint3 _groupID : SV_GROUPID, uint3 _groupThreadID : SV_GROUPTHREADID, uint3 _dispatchThreadID : SV_DISPATCHTHREADID ) {
	uint	index = _dispatchThreadID.x;

	// Fetch level 7 - Group size = 2*128->1*256 - Frequency = PI/64
	float	frequency = _sign * PI / 64.0;
	FetchAndMix_Large( 7, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 8 - Group size = 2*256->1*512 - Frequency = PI/128
	frequency *= 0.5;
	FetchAndMix_Large( 8, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 9 - Group size = 2*512->1*1024 - Frequency = PI/256
	frequency *= 0.5;
	FetchAndMix_Large( 9, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 10 - Group size = 2*1024->1*2048 - Frequency = PI/512
	frequency *= 0.5;
	FetchAndMix_Large( 10, index, frequency, _texIn, _texOut );
	GroupMemoryBarrierWithGroupSync();

	// Fetch level 11 - Group size = 2*2048->1*4096 - Frequency = PI/1024
	frequency *= 0.5;
	FetchAndMix_Large( 11, index, frequency, _texIn, _texOut );
}

/*
			Complex[]	temp = new Complex[_size];

			Complex[]	bufferIn = (_POT & 1) != 0 ? temp : _output;
			Complex[]	bufferOut = (_POT & 1) != 0 ? _output : temp;

			// Generate most-displacement indices then copy and displace source
 			int[]	indices = PermutationTables.ms_tables[_POT];
			for ( int i=0; i < _size; i++ )
				bufferIn[i] = _input[indices[i]];

			// Apply grouping and twiddling
			int		groupsCount = _size >> 1;
			int		groupSize = 1;
			double	frequency = 0.5 * _baseFrequency;
			for ( int stageIndex=0; stageIndex < _POT; stageIndex++ ) {

				int	k_even = 0;
				int	k_odd = groupSize;
				for ( int groupIndex=0; groupIndex < groupsCount; groupIndex++ ) {
					for ( int i=0; i < groupSize; i++ ) {
						Complex	E = bufferIn[k_even];
						Complex	O = bufferIn[k_odd];

						double	omega = frequency * i;
						double	c = Math.Cos( omega );
						double	s = Math.Sin( omega );

						bufferOut[k_even].Set(	E.x + c * O.x - s * O.y, 
												E.y + s * O.x + c * O.y );

						bufferOut[k_odd].Set(	E.x - c * O.x + s * O.y, 
												E.y - s * O.x - c * O.y );

						k_even++;
						k_odd++;
					}

					k_even += groupSize;
					k_odd += groupSize;
				}

				// Double group size and halve frequency resolution
				groupsCount >>= 1;
				groupSize <<= 1;
				frequency *= 0.5;

				// Swap buffers
				Complex[]	t = bufferIn;
				bufferIn = bufferOut;
				bufferOut = t;
			}

			if ( bufferIn != _output )
				throw new Exception( "Unexpected buffer as output!" );
*/