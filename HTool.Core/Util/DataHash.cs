using System.IO.Hashing;
using System.Runtime.CompilerServices;

namespace HTool.Core.Util;

/// <summary>
///     XxHash3 기반 변경 감지 유틸리티. v1의 CheckSum(int, 단순 합계)을 대체합니다.
///     XxHash3-based change detection utility. Replaces v1 CheckSum (int, simple sum).
/// </summary>
/// <remarks>
///     사전 파싱 해시 비교: 데이터가 변경되지 않았을 때 파싱을 건너뛰는 폴링 최적화에 사용됩니다.
///     pre-parse hash comparison: skip parsing when data unchanged (polling optimization).
/// </remarks>
public static class DataHash {
	/// <summary>
	///     원시 바이트 데이터에서 해시를 계산합니다.
	///     computes hash from raw byte data.
	/// </summary>
	/// <param name="data">해시할 데이터 / data to hash</param>
	/// <returns>64비트 해시 값 / 64-bit hash value</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Compute(ReadOnlySpan<byte> data) {
        // compute and return XXH3 hash / XXH3 해시 계산 및 반환
        return XxHash3.HashToUInt64(data);
    }

	/// <summary>
	///     두 바이트 시퀀스가 해시 비교로 동일한지 확인합니다.
	///     checks if two byte sequences are identical by hash comparison.
	/// </summary>
	/// <param name="a">첫 번째 데이터 / first data</param>
	/// <param name="b">두 번째 데이터 / second data</param>
	/// <returns>해시가 동일하면 true / true if hashes are equal</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Equals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b) {
        // compare hashes and return result / 해시 비교 및 결과 반환
        return XxHash3.HashToUInt64(a) == XxHash3.HashToUInt64(b);
    }
}