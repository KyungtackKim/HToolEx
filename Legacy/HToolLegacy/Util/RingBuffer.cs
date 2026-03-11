namespace HTool.Util;

/// <summary>
///     고정 크기 순환 버퍼 클래스. 용량이 초과되면 가장 오래된 데이터부터 자동으로 덮어씁니다.
///     Fixed-size circular buffer class. Automatically overwrites oldest data when capacity is exceeded.
/// </summary>
/// <remarks>
///     내부적으로 2의 거듭제곱 크기로 자동 조정되어 비트 마스크를 통한 고속 모듈러 연산을 수행합니다.
///     Internally adjusted to power-of-two size for fast modulo operation using bit mask.
/// </remarks>
public sealed class RingBuffer {
    /// <summary>
    ///     내부 버퍼 배열 (2의 거듭제곱 크기로 할당됨)
    ///     Internal buffer array (allocated as power-of-two size)
    /// </summary>
    private readonly byte[] _buffer;

    /// <summary>
    ///     빠른 모듈러 연산용 비트 마스크 (용량 - 1). 인덱스 래핑을 위해 & 연산 사용
    ///     Bit mask for fast modulo operation (capacity - 1). Used with & operator for index wrapping
    /// </summary>
    private readonly int _mask;

    /// <summary>
    ///     읽기 위치
    ///     Read position
    /// </summary>
    private int _readPos;

    /// <summary>
    ///     쓰기 위치
    ///     Write position
    /// </summary>
    private int _writePos;

    /// <summary>
    ///     링 버퍼 생성자. 요청된 용량 이상의 2의 거듭제곱 크기로 내부 버퍼를 할당합니다.
    ///     Ring buffer constructor. Allocates internal buffer with power-of-two size equal to or greater than requested
    ///     capacity.
    /// </summary>
    /// <param name="capacity">요청 용량 (최소 1 이상) / Requested capacity (minimum 1)</param>
    /// <exception cref="ArgumentException">용량이 1보다 작을 경우 / Thrown when capacity is less than 1</exception>
    /// <example>
    ///     예: capacity=1000 요청 시 실제 버퍼 크기는 1024 (2^10)
    ///     Example: capacity=1000 results in actual buffer size of 1024 (2^10)
    /// </example>
    public RingBuffer(int capacity) {
        // 용량 확인
        // check the capacity
        if (capacity < 1)
            // 예외 발생
            // throw the exception
            throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));
        // 실제 크기 계산
        // get the actual size
        var size = GetNextPowerOfTwo(capacity);
        // 버퍼 생성
        // create buffer
        _buffer = new byte[size];
        // 마스크 설정
        // set the mask
        _mask = size - 1;
    }

    /// <summary>
    ///     버퍼의 총 용량 (2의 거듭제곱 크기)
    ///     Total capacity of buffer (power-of-two size)
    /// </summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    ///     현재 버퍼에 저장된 읽기 가능한 데이터 바이트 수
    ///     Number of readable data bytes currently stored in buffer
    /// </summary>
    public int Available { get; private set; }

    /// <summary>
    ///     입력값 이상의 가장 작은 2의 거듭제곱 값을 계산합니다. 비트 마스크를 이용한 고속 모듈러 연산을 위해 필요합니다.
    ///     Calculates the smallest power-of-two value greater than or equal to input. Required for fast modulo operation using
    ///     bit mask.
    /// </summary>
    /// <param name="value">입력 값 (양수) / Input value (positive)</param>
    /// <returns>
    ///     입력값 이상의 2의 거듭제곱 값. 입력값이 이미 2의 거듭제곱이면 그대로 반환
    ///     Power-of-two value equal to or greater than input. Returns input as-is if already power-of-two
    /// </returns>
    /// <remarks>
    ///     예: 10 → 16, 1000 → 1024, 2048 → 2048
    ///     Example: 10 → 16, 1000 → 1024, 2048 → 2048
    /// </remarks>
    private static int GetNextPowerOfTwo(int value) {
        // 값이 2^n인지 확인
        // check the value is 2 ^ n
        if ((value & (value - 1)) == 0)
            return value;
        var result = 1;
        // 다음 2의 거듭제곱 찾기
        // find the next higher power of 2
        while (result < value)
            // 시프트
            // shift
            result <<= 1;
        // 결과 반환
        // return the power value
        return result;
    }

    /// <summary>
    ///     단일 바이트를 버퍼에 씁니다. 버퍼가 가득 찬 경우 가장 오래된 데이터를 덮어씁니다.
    ///     Writes a single byte to buffer. Overwrites oldest data if buffer is full.
    /// </summary>
    /// <param name="data">쓸 바이트 데이터 / Byte data to write</param>
    public void Write(byte data) {
        // 데이터 설정
        // set data
        _buffer[_writePos] = data;
        // 위치 업데이트
        // update position
        _writePos = (_writePos + 1) & _mask;
        // 용량 확인
        // check capacity
        if (Available < Capacity)
            // 사용 가능 크기 업데이트
            // update available
            Available++;
        else
            // 데이터 덮어쓰기
            // overwritten data
            _readPos = (_readPos + 1) & _mask;
    }

    /// <summary>
    ///     바이트 배열을 버퍼에 씁니다. 버퍼 끝에 도달하면 처음으로 돌아가 순환 쓰기합니다.
    ///     Writes byte array to buffer. Wraps around to beginning when reaching end of buffer.
    /// </summary>
    /// <param name="data">쓸 바이트 배열. null이거나 용량 초과 시 무시됨 / Byte array to write. Ignored if null or exceeds capacity</param>
    /// <exception cref="ArgumentNullException">data가 null인 경우 / Thrown when data is null</exception>
    /// <remarks>
    ///     용량 초과 시 가장 오래된 데이터부터 덮어씁니다.
    ///     Overwrites oldest data first when capacity is exceeded.
    /// </remarks>
    public void WriteBytes(byte[] data) {
        // 데이터 확인
        // check the data
        ArgumentNullException.ThrowIfNull(data);
        // 길이 확인
        // check the length
        if (data.Length == 0 || data.Length > Capacity)
            return;
        // 데이터 길이 가져오기
        // get the data length
        var length = data.Length;
        var remain = Capacity - _writePos;
        // 길이 확인
        // check the length
        if (length <= remain) {
            // 모든 데이터 복사
            // copy all data
            Array.Copy(data, 0, _buffer, _writePos, length);
            // 위치 업데이트
            // update the position
            _writePos = (_writePos + length) & _mask;
        } else {
            // 남은 데이터 길이 계산
            // get the remain data length
            var offset = length - remain;
            // 첫 번째 블록 데이터 복사
            // copy the first block data
            Buffer.BlockCopy(data, 0, _buffer, _writePos, remain);
            Buffer.BlockCopy(data, remain, _buffer, 0, offset);
            // 위치 업데이트
            // update the position
            _writePos = offset & _mask;
        }

        // 새 사용 가능 크기 계산
        // get the new available size
        var newAvailable = Available + length;
        // 사용 가능 크기 확인
        // check the available
        if (newAvailable > Capacity) {
            // 덮어쓰기 크기 계산
            // get the overwritten size
            var size = newAvailable - Capacity;
            // 데이터 덮어쓰기
            // overwritten the data
            _readPos = (_readPos + size) & _mask;
            // 사용 가능 크기 설정
            // set the available
            Available = Capacity;
        } else {
            // 사용 가능 크기 설정
            // set the available
            Available = newAvailable;
        }
    }

    /// <summary>
    ///     ReadOnlySpan을 버퍼에 씁니다. Zero-copy 방식으로 고성능 쓰기를 수행합니다.
    ///     Writes ReadOnlySpan to buffer. Performs high-performance zero-copy write.
    /// </summary>
    /// <param name="data">쓸 데이터 스팬. 빈 스팬이거나 용량 초과 시 무시됨 / Data span to write. Ignored if empty or exceeds capacity</param>
    /// <remarks>
    ///     버퍼 배열 방식보다 메모리 할당이 적고 성능이 우수합니다.
    ///     More efficient than byte array method with less memory allocation.
    /// </remarks>
    public void WriteBytes(ReadOnlySpan<byte> data) {
        // 길이 확인
        // check the length
        if (data.Length == 0 || data.Length > Capacity)
            return;
        // 데이터 길이 가져오기
        // get the data length
        var length = data.Length;
        var remain = Capacity - _writePos;

        // 길이 확인
        // check the length
        if (length <= remain) {
            // 모든 데이터 직접 복사
            // copy all data directly
            data.CopyTo(_buffer.AsSpan(_writePos, length));
            // 위치 업데이트
            // update the position
            _writePos = (_writePos + length) & _mask;
        } else {
            // 남은 데이터 길이 계산
            // get the remain data length
            var offset = length - remain;
            // 첫 번째 블록 데이터 복사
            // copy the first block data
            data[..remain].CopyTo(_buffer.AsSpan(_writePos, remain));
            data[remain..].CopyTo(_buffer.AsSpan(0, offset));
            // 위치 업데이트
            // update the position
            _writePos = offset & _mask;
        }

        // 새 사용 가능 크기 계산
        // get the new available size
        var newAvailable = Available + length;
        // 사용 가능 크기 확인
        // check the available
        if (newAvailable > Capacity) {
            // 덮어쓰기 크기 계산
            // get the overwritten size
            var size = newAvailable - Capacity;
            // 데이터 덮어쓰기
            // overwritten the data
            _readPos = (_readPos + size) & _mask;
            // 사용 가능 크기 설정
            // set the available
            Available = Capacity;
        } else {
            // 사용 가능 크기 설정
            // set the available
            Available = newAvailable;
        }
    }

    /// <summary>
    ///     읽기 위치를 변경하지 않고 지정된 오프셋의 바이트를 조회합니다.
    ///     Retrieves byte at specified offset without changing read position.
    /// </summary>
    /// <param name="offset">읽기 위치로부터의 오프셋 (0 ~ Available-1) / Offset from read position (0 ~ Available-1)</param>
    /// <returns>오프셋 위치의 바이트 / Byte at offset position</returns>
    /// <exception cref="ArgumentOutOfRangeException">오프셋이 유효 범위를 벗어난 경우 / Thrown when offset is out of valid range</exception>
    public byte Peek(int offset) {
        // 값 가져오기
        // get the values
        var available = Available;
        var pos       = _readPos;
        // 오프셋 확인
        // check offset
        return (uint)offset < (uint)available ? _buffer[(pos + offset) & _mask] :
            throw new ArgumentOutOfRangeException(nameof(offset), "Invalid offset");
    }

    /// <summary>
    ///     읽기 위치를 변경하지 않고 버퍼의 모든 사용 가능한 데이터를 조회합니다.
    ///     Retrieves all available data in buffer without changing read position.
    /// </summary>
    /// <returns>
    ///     사용 가능한 데이터의 ReadOnlySpan. 데이터가 없으면 빈 스팬 반환
    ///     ReadOnlySpan of available data. Returns empty span if no data available
    /// </returns>
    /// <remarks>
    ///     데이터가 버퍼 끝을 넘어가면 임시 배열을 생성하여 연속된 데이터로 반환합니다.
    ///     Creates temporary array to return contiguous data when data wraps around buffer end.
    /// </remarks>
    public ReadOnlySpan<byte> PeekBytes() {
        // 값 가져오기
        // get the values
        var available = Available;
        var pos       = _readPos;
        // 데이터 확인
        // check data
        if (available == 0)
            return ReadOnlySpan<byte>.Empty;
        // 길이 가져오기
        // get the length
        var len = _buffer.Length;
        // 길이 확인
        // check the length
        if (pos + available <= len)
            return new ReadOnlySpan<byte>(_buffer, pos, available);

        // 배열 생성
        // create the array
        var result = new byte[available];
        // 첫 번째 부분 길이 계산
        // get the first part length
        var first = len - pos;
        // 데이터 복사
        // copy the data
        Buffer.BlockCopy(_buffer, pos, result, 0, first);
        Buffer.BlockCopy(_buffer, 0, result, first, available - first);
        // 모든 데이터 반환
        // return all data
        return result;
    }

    /// <summary>
    ///     지정된 길이만큼 데이터를 읽고 읽기 위치를 이동합니다. 사용 가능한 데이터가 부족하면 가능한 만큼만 읽습니다.
    ///     Reads specified length of data and advances read position. Reads only available data if insufficient.
    /// </summary>
    /// <param name="length">읽을 바이트 수 (0 이상) / Number of bytes to read (0 or more)</param>
    /// <returns>
    ///     읽은 데이터 배열. 사용 가능한 데이터가 없으면 빈 배열 반환
    ///     Array of read data. Returns empty array if no data available
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">length가 음수인 경우 / Thrown when length is negative</exception>
    public byte[] ReadBytes(int length) {
        // 길이 확인
        // check length
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");
        // 값 가져오기
        // get the values
        var available = Available;
        var pos       = _readPos;
        // 실제 길이 계산
        // get the actual length
        var actual = Math.Min(length, available);
        // 실제 길이 확인
        // check the actual length
        if (actual == 0)
            return [];
        // 버퍼 길이 가져오기
        // get the buffer length
        var len = _buffer.Length;
        // 결과 버퍼 생성
        // create the result buffer
        var result = new byte[actual];
        // 길이 확인
        // check the length
        if (pos + actual <= len) {
            // 단일 블록 복사
            // copy the single block
            Buffer.BlockCopy(_buffer, pos, result, 0, actual);
        } else {
            // 첫 번째 블록 인덱스 계산
            // get the first block index
            var first = len - pos;
            // 두 블록 복사
            // copy the two blocks
            Buffer.BlockCopy(_buffer, pos, result, 0, first);
            Buffer.BlockCopy(_buffer, 0, result, first, actual - first);
        }

        // 위치 업데이트
        // update the position
        _readPos = (pos + actual) & _mask;
        // 사용 가능 크기 재설정
        // reset available
        Available = available - actual;
        // 결과 반환
        // return result
        return result;
    }

    /// <summary>
    ///     데이터를 읽지 않고 읽기 위치만 이동하여 데이터를 제거합니다. ReadBytes와 달리 배열 할당이 없어 더 빠릅니다.
    ///     Removes data by advancing read position without reading. Faster than ReadBytes as it doesn't allocate array.
    /// </summary>
    /// <param name="length">제거할 바이트 수 (0 이상) / Number of bytes to remove (0 or more)</param>
    /// <exception cref="ArgumentOutOfRangeException">length가 음수인 경우 / Thrown when length is negative</exception>
    /// <remarks>
    ///     사용 가능한 데이터보다 많은 길이를 요청하면 가능한 만큼만 제거합니다.
    ///     Removes only available data if requested length exceeds available data.
    /// </remarks>
    public void RemoveBytes(int length) {
        // 길이 확인
        // check length
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");

        // 실제 길이 계산
        // get the actual length
        var actual = Math.Min(length, Available);
        // 실제 길이 확인
        // check the actual length
        if (actual == 0)
            return;
        // 읽기 위치 이동
        // move read position
        _readPos = (_readPos + actual) & _mask;
        // 사용 가능 크기 재설정
        // reset available
        Available -= actual;
    }

    /// <summary>
    ///     버퍼의 모든 데이터를 제거하고 읽기/쓰기 위치를 초기화합니다. 버퍼 배열 자체는 해제하지 않습니다.
    ///     Removes all data from buffer and resets read/write positions. Does not deallocate buffer array itself.
    /// </summary>
    /// <remarks>
    ///     이 메서드는 O(1) 시간 복잡도로 즉시 실행됩니다.
    ///     This method executes in O(1) time complexity.
    /// </remarks>
    public void Clear() {
        // 위치 초기화
        // reset positions
        _readPos  = 0;
        _writePos = 0;
        Available = 0;
    }
}