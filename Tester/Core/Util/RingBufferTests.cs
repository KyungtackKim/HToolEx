using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     RingBuffer의 순환 버퍼 기능을 검증하는 테스트 클래스.
///     test class verifying circular buffer functionality of RingBuffer.
/// </summary>
public sealed class RingBufferTests {
    // ──────────────────────────────────────────────
    // 생성자 / Constructor
    // ──────────────────────────────────────────────

    /// <summary>
    ///     2의 거듭제곱 용량으로 올림되는지 검증한다.
    ///     verifies that capacity is rounded up to power of two.
    /// </summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 4)]
    [InlineData(5, 8)]
    [InlineData(7, 8)]
    [InlineData(9, 16)]
    [InlineData(100, 128)]
    public void Constructor_Capacity_RoundsUpToPowerOfTwo(int requested, int expected) {
        // 요청된 용량으로 링 버퍼 생성
        // create ring buffer with requested capacity
        var rb = new RingBuffer(requested);

        // 실제 용량이 2의 거듭제곱으로 올림되었는지 확인
        // verify actual capacity rounded up to power of two
        Assert.Equal(expected, rb.Capacity);
        // 초기 사용 가능 크기가 0인지 확인
        // verify initial available is 0
        Assert.Equal(0, rb.Available);
    }

    /// <summary>
    ///     용량이 1보다 작은 경우 예외가 발생하는지 검증한다.
    ///     verifies that capacity less than 1 throws exception.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidCapacity_ThrowsArgumentException(int capacity) {
        // 유효하지 않은 용량으로 생성 시 예외 확인
        // verify exception on invalid capacity
        Assert.Throws<ArgumentException>(
            // 유효하지 않은 용량으로 링 버퍼 생성
            // create ring buffer with invalid capacity
            () => new RingBuffer(capacity)
        );
    }

    // ──────────────────────────────────────────────
    // Write (single byte) / 단일 바이트 쓰기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     단일 바이트 쓰기 후 Available이 증가하는지 검증한다.
    ///     verifies that Available increases after single byte write.
    /// </summary>
    [Fact]
    public void Write_SingleByte_IncreasesAvailable() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);

        // 1바이트 쓰기
        // write 1 byte
        rb.Write(0xAA);

        // 사용 가능 크기가 1인지 확인
        // verify available is 1
        Assert.Equal(1, rb.Available);
    }

    /// <summary>
    ///     용량 초과 시 가장 오래된 데이터가 덮어쓰여지는지 검증한다.
    ///     verifies that oldest data is overwritten when capacity is exceeded.
    /// </summary>
    [Fact]
    public void Write_ExceedsCapacity_OverwritesOldestData() {
        // 용량 2의 링 버퍼 생성 (실제 용량 2)
        // create ring buffer with capacity 2 (actual capacity 2)
        var rb = new RingBuffer(2);
        // 3바이트 쓰기 (용량 초과)
        // write 3 bytes (exceeds capacity)
        rb.Write(0x01);
        rb.Write(0x02);
        rb.Write(0x03);

        // 사용 가능 크기가 용량으로 고정되는지 확인
        // verify available capped at capacity
        Assert.Equal(2, rb.Available);
        // 읽기 결과가 마지막 2바이트인지 확인
        // verify read result is last 2 bytes
        var data = rb.ReadBytes(2);
        // 첫 번째 바이트 확인 (0x01은 덮어쓰여짐)
        // verify first byte (0x01 overwritten)
        Assert.Equal(0x02, data[0]);
        // 두 번째 바이트 확인
        // verify second byte
        Assert.Equal(0x03, data[1]);
    }

    // ──────────────────────────────────────────────
    // WriteBytes (byte array) / 바이트 배열 쓰기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     바이트 배열 쓰기 후 Available이 올바르게 설정되는지 검증한다.
    ///     verifies that Available is correctly set after byte array write.
    /// </summary>
    [Fact]
    public void WriteBytes_Array_SetsAvailableCorrectly() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 데이터 배열
        // data array
        byte[] data = [0x01, 0x02, 0x03, 0x04, 0x05];

        // 배열 쓰기
        // write array
        rb.WriteBytes(data);

        // 사용 가능 크기가 5인지 확인
        // verify available is 5
        Assert.Equal(5, rb.Available);
    }

    /// <summary>
    ///     빈 배열 쓰기 시 아무 변화가 없는지 검증한다.
    ///     verifies that writing empty array has no effect.
    /// </summary>
    [Fact]
    public void WriteBytes_EmptyArray_NoChange() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);

        // 빈 배열 쓰기
        // write empty array
        rb.WriteBytes([]);

        // 사용 가능 크기가 0인지 확인
        // verify available is 0
        Assert.Equal(0, rb.Available);
    }

    /// <summary>
    ///     용량 초과 배열 쓰기 시 무시되는지 검증한다.
    ///     verifies that writing array exceeding capacity is ignored.
    /// </summary>
    [Fact]
    public void WriteBytes_ExceedsCapacity_Ignored() {
        // 용량 4의 링 버퍼 생성 (실제 용량 4)
        // create ring buffer with capacity 4 (actual capacity 4)
        var rb = new RingBuffer(4);
        // 5바이트 배열 (용량 초과)
        // 5-byte array (exceeds capacity)
        byte[] data = [0x01, 0x02, 0x03, 0x04, 0x05];

        // 용량 초과 배열 쓰기
        // write array exceeding capacity
        rb.WriteBytes(data);

        // 쓰기가 무시되었는지 확인 (Available = 0)
        // verify write was ignored (Available = 0)
        Assert.Equal(0, rb.Available);
    }

    /// <summary>
    ///     null 배열 쓰기 시 예외가 발생하는지 검증한다.
    ///     verifies that writing null array throws exception.
    /// </summary>
    [Fact]
    public void WriteBytes_NullArray_ThrowsArgumentNullException() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);

        // null 배열 쓰기 시 예외 확인
        // verify exception on null array
        Assert.Throws<ArgumentNullException>(
            // null 데이터 쓰기
            // write null data
            () => rb.WriteBytes(null!)
        );
    }

    // ──────────────────────────────────────────────
    // WriteBytes (ReadOnlySpan) / 스팬 쓰기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadOnlySpan 쓰기 후 데이터가 올바르게 저장되는지 검증한다.
    ///     verifies that data is correctly stored after ReadOnlySpan write.
    /// </summary>
    [Fact]
    public void WriteBytes_Span_StoresDataCorrectly() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 스팬 데이터
        // span data
        ReadOnlySpan<byte> span = [0xAA, 0xBB, 0xCC];

        // 스팬 쓰기
        // write span
        rb.WriteBytes(span);

        // 사용 가능 크기 확인
        // verify available
        Assert.Equal(3, rb.Available);
        // 읽기 결과 확인
        // verify read result
        var data = rb.ReadBytes(3);
        // 값 확인
        // verify values
        Assert.Equal([0xAA, 0xBB, 0xCC], data);
    }

    // ──────────────────────────────────────────────
    // 순환 쓰기 (Wrap-around) / 래핑
    // ──────────────────────────────────────────────

    /// <summary>
    ///     순환 쓰기 후 데이터가 올바르게 읽히는지 검증한다.
    ///     verifies that data is correctly read after wrap-around write.
    /// </summary>
    [Fact]
    public void WriteAndRead_WrapAround_DataPreserved() {
        // 용량 4의 링 버퍼 생성 (실제 용량 4)
        // create ring buffer with capacity 4 (actual capacity 4)
        var rb = new RingBuffer(4);
        // 3바이트 쓰기
        // write 3 bytes
        rb.WriteBytes([0x01, 0x02, 0x03]);
        // 2바이트 읽기 (읽기 위치 이동)
        // read 2 bytes (advance read position)
        rb.ReadBytes(2);
        // 3바이트 추가 쓰기 (래핑 발생)
        // write 3 more bytes (causes wrap-around)
        rb.WriteBytes([0x04, 0x05, 0x06]);

        // 사용 가능 크기 확인
        // verify available
        Assert.Equal(4, rb.Available);
        // 래핑 후 데이터 읽기
        // read data after wrap-around
        var data = rb.ReadBytes(4);
        // 올바른 데이터 순서 확인
        // verify correct data order
        Assert.Equal([0x03, 0x04, 0x05, 0x06], data);
    }

    // ──────────────────────────────────────────────
    // Peek / 피크
    // ──────────────────────────────────────────────

    /// <summary>
    ///     Peek가 올바른 오프셋의 바이트를 반환하는지 검증한다.
    ///     verifies that Peek returns correct byte at offset.
    /// </summary>
    [Fact]
    public void Peek_ValidOffset_ReturnsCorrectByte() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 데이터 쓰기
        // write data
        rb.WriteBytes([0x10, 0x20, 0x30]);

        // 오프셋 0 피크
        // peek at offset 0
        var first = rb.Peek(0);
        // 오프셋 2 피크
        // peek at offset 2
        var third = rb.Peek(2);

        // 첫 번째 값 확인
        // verify first value
        Assert.Equal(0x10, first);
        // 세 번째 값 확인
        // verify third value
        Assert.Equal(0x30, third);
        // Available 변하지 않았는지 확인
        // verify Available unchanged
        Assert.Equal(3, rb.Available);
    }

    /// <summary>
    ///     유효 범위 벗어난 오프셋으로 Peek 시 예외가 발생하는지 검증한다.
    ///     verifies that Peek with out-of-range offset throws exception.
    /// </summary>
    [Fact]
    public void Peek_InvalidOffset_ThrowsArgumentOutOfRangeException() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);
        // 2바이트 쓰기
        // write 2 bytes
        rb.WriteBytes([0x01, 0x02]);

        // 범위 초과 오프셋으로 피크 시 예외 확인
        // verify exception on out-of-range offset
        Assert.Throws<ArgumentOutOfRangeException>(
            // 오프셋 2로 피크 (Available = 2이므로 최대 인덱스 1)
            // peek at offset 2 (Available = 2, max index is 1)
            () => rb.Peek(2)
        );
    }

    // ──────────────────────────────────────────────
    // PeekBytes / 전체 피크
    // ──────────────────────────────────────────────

    /// <summary>
    ///     PeekBytes가 모든 사용 가능한 데이터를 반환하는지 검증한다.
    ///     verifies that PeekBytes returns all available data.
    /// </summary>
    [Fact]
    public void PeekBytes_WithData_ReturnsAllAvailable() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 데이터 쓰기
        // write data
        rb.WriteBytes([0xAA, 0xBB, 0xCC]);

        // 전체 데이터 피크
        // peek all data
        var peeked = rb.PeekBytes();

        // 데이터 길이 확인
        // verify data length
        Assert.Equal(3, peeked.Length);
        // 값 확인
        // verify values
        Assert.Equal(0xAA, peeked[0]);
        Assert.Equal(0xBB, peeked[1]);
        Assert.Equal(0xCC, peeked[2]);
        // Available 변하지 않았는지 확인
        // verify Available unchanged
        Assert.Equal(3, rb.Available);
    }

    /// <summary>
    ///     빈 버퍼에서 PeekBytes가 빈 스팬을 반환하는지 검증한다.
    ///     verifies that PeekBytes returns empty span on empty buffer.
    /// </summary>
    [Fact]
    public void PeekBytes_EmptyBuffer_ReturnsEmptySpan() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);

        // 빈 버퍼에서 피크
        // peek on empty buffer
        var peeked = rb.PeekBytes();

        // 빈 스팬인지 확인
        // verify empty span
        Assert.Equal(0, peeked.Length);
    }

    // ──────────────────────────────────────────────
    // ReadBytes / 바이트 읽기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     ReadBytes가 올바른 데이터를 읽고 Available을 감소시키는지 검증한다.
    ///     verifies that ReadBytes reads correct data and decreases Available.
    /// </summary>
    [Fact]
    public void ReadBytes_ValidLength_ReadsAndDecreasesAvailable() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 5바이트 쓰기
        // write 5 bytes
        rb.WriteBytes([0x01, 0x02, 0x03, 0x04, 0x05]);

        // 3바이트 읽기
        // read 3 bytes
        var data = rb.ReadBytes(3);

        // 읽은 데이터 확인
        // verify read data
        Assert.Equal([0x01, 0x02, 0x03], data);
        // 남은 사용 가능 크기 확인
        // verify remaining available
        Assert.Equal(2, rb.Available);
    }

    /// <summary>
    ///     Available보다 큰 길이로 ReadBytes 호출 시 사용 가능한 만큼만 반환하는지 검증한다.
    ///     verifies that ReadBytes returns only available data when length exceeds available.
    /// </summary>
    [Fact]
    public void ReadBytes_ExceedsAvailable_ReturnsOnlyAvailable() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 2바이트 쓰기
        // write 2 bytes
        rb.WriteBytes([0xAA, 0xBB]);

        // 10바이트 요청 (Available은 2)
        // request 10 bytes (Available is 2)
        var data = rb.ReadBytes(10);

        // 실제 사용 가능한 2바이트만 반환되는지 확인
        // verify only 2 available bytes returned
        Assert.Equal(2, data.Length);
        // Available이 0인지 확인
        // verify Available is 0
        Assert.Equal(0, rb.Available);
    }

    /// <summary>
    ///     빈 버퍼에서 ReadBytes가 빈 배열을 반환하는지 검증한다.
    ///     verifies that ReadBytes returns empty array on empty buffer.
    /// </summary>
    [Fact]
    public void ReadBytes_EmptyBuffer_ReturnsEmptyArray() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);

        // 빈 버퍼에서 읽기 시도
        // attempt read from empty buffer
        var data = rb.ReadBytes(5);

        // 빈 배열 반환 확인
        // verify empty array returned
        Assert.Empty(data);
    }

    /// <summary>
    ///     음수 길이로 ReadBytes 호출 시 예외가 발생하는지 검증한다.
    ///     verifies that ReadBytes with negative length throws exception.
    /// </summary>
    [Fact]
    public void ReadBytes_NegativeLength_ThrowsArgumentOutOfRangeException() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);

        // 음수 길이로 읽기 시 예외 확인
        // verify exception on negative length
        Assert.Throws<ArgumentOutOfRangeException>(
            // 음수 길이로 읽기
            // read with negative length
            () => rb.ReadBytes(-1)
        );
    }

    // ──────────────────────────────────────────────
    // RemoveBytes / 바이트 제거
    // ──────────────────────────────────────────────

    /// <summary>
    ///     RemoveBytes가 데이터 복사 없이 위치를 전진하는지 검증한다.
    ///     verifies that RemoveBytes advances position without data copy.
    /// </summary>
    [Fact]
    public void RemoveBytes_ValidLength_DecreasesAvailable() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 4바이트 쓰기
        // write 4 bytes
        rb.WriteBytes([0x01, 0x02, 0x03, 0x04]);

        // 2바이트 제거
        // remove 2 bytes
        rb.RemoveBytes(2);

        // 남은 사용 가능 크기 확인
        // verify remaining available
        Assert.Equal(2, rb.Available);
        // 남은 데이터가 올바른지 확인
        // verify remaining data is correct
        var remaining = rb.ReadBytes(2);
        // 남은 데이터 값 확인
        // verify remaining data values
        Assert.Equal([0x03, 0x04], remaining);
    }

    /// <summary>
    ///     Available보다 큰 길이로 RemoveBytes 호출 시 사용 가능한 만큼만 제거하는지 검증한다.
    ///     verifies that RemoveBytes removes only available when length exceeds available.
    /// </summary>
    [Fact]
    public void RemoveBytes_ExceedsAvailable_RemovesOnlyAvailable() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);
        // 2바이트 쓰기
        // write 2 bytes
        rb.WriteBytes([0x01, 0x02]);

        // 100바이트 제거 시도
        // attempt to remove 100 bytes
        rb.RemoveBytes(100);

        // Available이 0인지 확인
        // verify Available is 0
        Assert.Equal(0, rb.Available);
    }

    /// <summary>
    ///     음수 길이로 RemoveBytes 호출 시 예외가 발생하는지 검증한다.
    ///     verifies that RemoveBytes with negative length throws exception.
    /// </summary>
    [Fact]
    public void RemoveBytes_NegativeLength_ThrowsArgumentOutOfRangeException() {
        // 용량 4의 링 버퍼 생성
        // create ring buffer with capacity 4
        var rb = new RingBuffer(4);

        // 음수 길이로 제거 시 예외 확인
        // verify exception on negative length
        Assert.Throws<ArgumentOutOfRangeException>(
            // 음수 길이로 제거
            // remove with negative length
            () => rb.RemoveBytes(-1)
        );
    }

    // ──────────────────────────────────────────────
    // Clear / 초기화
    // ──────────────────────────────────────────────

    /// <summary>
    ///     Clear가 모든 상태를 초기화하는지 검증한다.
    ///     verifies that Clear resets all state.
    /// </summary>
    [Fact]
    public void Clear_WithData_ResetsToEmpty() {
        // 용량 8의 링 버퍼 생성
        // create ring buffer with capacity 8
        var rb = new RingBuffer(8);
        // 데이터 쓰기
        // write data
        rb.WriteBytes([0x01, 0x02, 0x03]);

        // 버퍼 초기화
        // clear buffer
        rb.Clear();

        // Available이 0인지 확인
        // verify Available is 0
        Assert.Equal(0, rb.Available);
        // 용량은 변하지 않았는지 확인
        // verify capacity unchanged
        Assert.Equal(8, rb.Capacity);
    }

    // ──────────────────────────────────────────────
    // PeekBytes after wrap-around / 래핑 후 전체 피크
    // ──────────────────────────────────────────────

    /// <summary>
    ///     래핑 후 PeekBytes가 올바른 데이터를 반환하는지 검증한다.
    ///     verifies that PeekBytes returns correct data after wrap-around.
    /// </summary>
    [Fact]
    public void PeekBytes_AfterWrapAround_ReturnsCorrectData() {
        // 용량 4의 링 버퍼 생성 (실제 용량 4)
        // create ring buffer with capacity 4 (actual capacity 4)
        var rb = new RingBuffer(4);
        // 3바이트 쓰기
        // write 3 bytes
        rb.WriteBytes([0x01, 0x02, 0x03]);
        // 2바이트 읽기 (읽기 위치 전진)
        // read 2 bytes (advance read position)
        rb.ReadBytes(2);
        // 3바이트 추가 (래핑 발생: 버퍼 끝 넘어감)
        // write 3 more (wrap-around: crosses buffer end)
        rb.WriteBytes([0x04, 0x05, 0x06]);

        // 전체 피크
        // peek all
        var peeked = rb.PeekBytes();

        // 데이터 길이 확인
        // verify data length
        Assert.Equal(4, peeked.Length);
        // 올바른 데이터 순서 확인
        // verify correct data order
        Assert.Equal(0x03, peeked[0]);
        Assert.Equal(0x04, peeked[1]);
        Assert.Equal(0x05, peeked[2]);
        Assert.Equal(0x06, peeked[3]);
    }
}