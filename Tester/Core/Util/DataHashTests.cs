using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     DataHash의 XxHash3 기반 해시 계산 및 비교 기능을 검증하는 테스트 클래스.
///     test class verifying XxHash3-based hash computation and comparison of DataHash.
/// </summary>
public sealed class DataHashTests {
    /// <summary>
    ///     동일 입력에 대해 동일 해시가 반환되는지 검증한다.
    ///     verifies that same input produces same hash.
    /// </summary>
    [Fact]
    public void Compute_SameInput_ReturnsSameHash() {
        // 테스트 데이터 생성
        // create test data
        ReadOnlySpan<byte> data = [0x01, 0x02, 0x03, 0x04, 0x05];

        // 첫 번째 해시 계산
        // compute first hash
        var hash1 = DataHash.Compute(data);
        // 두 번째 해시 계산
        // compute second hash
        var hash2 = DataHash.Compute(data);

        // 두 해시가 동일한지 확인
        // verify both hashes are equal
        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    ///     다른 입력에 대해 다른 해시가 반환되는지 검증한다.
    ///     verifies that different input produces different hash.
    /// </summary>
    [Fact]
    public void Compute_DifferentInput_ReturnsDifferentHash() {
        // 첫 번째 데이터
        // first data
        ReadOnlySpan<byte> dataA = [0x01, 0x02, 0x03];
        // 두 번째 데이터 (다른 내용)
        // second data (different content)
        ReadOnlySpan<byte> dataB = [0x04, 0x05, 0x06];

        // 첫 번째 해시 계산
        // compute first hash
        var hashA = DataHash.Compute(dataA);
        // 두 번째 해시 계산
        // compute second hash
        var hashB = DataHash.Compute(dataB);

        // 두 해시가 다른지 확인
        // verify both hashes differ
        Assert.NotEqual(hashA, hashB);
    }

    /// <summary>
    ///     동일 데이터에 대해 Equals가 true를 반환하는지 검증한다.
    ///     verifies that Equals returns true for identical data.
    /// </summary>
    [Fact]
    public void Equals_IdenticalData_ReturnsTrue() {
        // 동일한 내용의 데이터 A
        // data A with same content
        ReadOnlySpan<byte> a = [0xAA, 0xBB, 0xCC];
        // 동일한 내용의 데이터 B
        // data B with same content
        ReadOnlySpan<byte> b = [0xAA, 0xBB, 0xCC];

        // Equals 결과 확인
        // verify Equals result
        var result = DataHash.Equals(a, b);

        // true 반환 확인
        // verify true returned
        Assert.True(result);
    }

    /// <summary>
    ///     다른 데이터에 대해 Equals가 false를 반환하는지 검증한다.
    ///     verifies that Equals returns false for different data.
    /// </summary>
    [Fact]
    public void Equals_DifferentData_ReturnsFalse() {
        // 데이터 A
        // data A
        ReadOnlySpan<byte> a = [0x01, 0x02];
        // 데이터 B (다른 내용)
        // data B (different content)
        ReadOnlySpan<byte> b = [0x03, 0x04];

        // Equals 결과 확인
        // verify Equals result
        var result = DataHash.Equals(a, b);

        // false 반환 확인
        // verify false returned
        Assert.False(result);
    }

    /// <summary>
    ///     빈 스팬에 대해 Compute가 예외 없이 해시를 반환하는지 검증한다.
    ///     verifies that Compute on empty span returns hash without throwing.
    /// </summary>
    [Fact]
    public void Compute_EmptySpan_DoesNotThrow() {
        // 빈 스팬
        // empty span
        ReadOnlySpan<byte> empty = [];

        // 빈 데이터 해시 계산 (예외 없이 실행)
        // compute hash of empty data (no exception)
        var hash = DataHash.Compute(empty);

        // 해시가 반환되었는지 확인 (특정 값 보다는 예외 없음을 검증)
        // verify hash is returned (verify no exception rather than specific value)
        Assert.IsType<ulong>(hash);
    }
}