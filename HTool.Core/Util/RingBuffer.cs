namespace HTool.Core.Util;

/// <summary>
///     고정 크기 순환 버퍼 클래스. 용량이 초과되면 가장 오래된 데이터부터 자동으로 덮어씁니다.
///     fixed-size circular buffer class. Automatically overwrites oldest data when capacity is exceeded.
/// </summary>
/// <remarks>
///     내부적으로 2의 거듭제곱 크기로 자동 조정되어 비트 마스크를 통한 고속 모듈러 연산을 수행합니다.
///     internally adjusted to power-of-two size for fast modulo operation using bit mask.
/// </remarks>
public sealed class RingBuffer {
	/// <summary>
	///     내부 버퍼 배열 (2의 거듭제곱 크기로 할당됨)
	///     internal buffer array (allocated as power-of-two size)
	/// </summary>
	private readonly byte[] _buffer;

	/// <summary>
	///     빠른 모듈러 연산용 비트 마스크 (용량 - 1). 인덱스 래핑을 위해 &amp; 연산 사용
	///     bit mask for fast modulo operation (capacity - 1). Used with &amp; operator for index wrapping
	/// </summary>
	private readonly int _mask;

	/// <summary>
	///     읽기 위치
	///     read position
	/// </summary>
	private int _readPos;

	/// <summary>
	///     쓰기 위치
	///     write position
	/// </summary>
	private int _writePos;

	/// <summary>
	///     링 버퍼 생성자. 요청된 용량 이상의 2의 거듭제곱 크기로 내부 버퍼를 할당합니다.
	///     ring buffer constructor. Allocates internal buffer with power-of-two size equal to or greater than requested
	///     capacity.
	/// </summary>
	/// <param name="capacity">요청 용량 (최소 1 이상) / requested capacity (minimum 1)</param>
	/// <exception cref="ArgumentException">용량이 1보다 작을 경우 / thrown when capacity is less than 1</exception>
	public RingBuffer(int capacity) {
        // ensure capacity is at least 1 / 용량 확인
        if (capacity < 1)
            // throw argument exception for invalid capacity / 유효하지 않은 용량 인자 예외 발생
            throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));
        // get the actual power-of-two size / 실제 크기 계산
        var size = GetNextPowerOfTwo(capacity);
        // create buffer with computed size / 버퍼 생성
        _buffer = new byte[size];
        // set the bit mask for fast modulo / 마스크 설정
        _mask = size - 1;
    }

	/// <summary>
	///     버퍼의 총 용량 (2의 거듭제곱 크기)
	///     total capacity of buffer (power-of-two size)
	/// </summary>
	public int Capacity => _buffer.Length;

	/// <summary>
	///     현재 버퍼에 저장된 읽기 가능한 데이터 바이트 수
	///     number of readable data bytes currently stored in buffer
	/// </summary>
	public int Available { get; private set; }

	/// <summary>
	///     입력값 이상의 가장 작은 2의 거듭제곱 값을 계산합니다.
	///     calculates the smallest power-of-two value greater than or equal to input.
	/// </summary>
	/// <param name="value">입력 값 (양수) / input value (positive)</param>
	/// <returns>2의 거듭제곱 값 / power-of-two value</returns>
	private static int GetNextPowerOfTwo(int value) {
        // check if value is already a power of 2 / 값이 2^n인지 확인
        if ((value & (value - 1)) == 0)
            // return as-is if already power of 2 / 이미 2의 거듭제곱이면 그대로 반환
            return value;
        // start from smallest power of 2 / 최소 2의 거듭제곱부터 시작
        var result = 1;
        // find the next higher power of 2 / 다음 2의 거듭제곱 찾기
        while (result < value)
            // shift left to double / 시프트
            result <<= 1;
        // return the computed power value / 결과 반환
        return result;
    }

	/// <summary>
	///     단일 바이트를 버퍼에 씁니다. 버퍼가 가득 찬 경우 가장 오래된 데이터를 덮어씁니다.
	///     writes a single byte to buffer. Overwrites oldest data if buffer is full.
	/// </summary>
	/// <param name="data">쓸 바이트 데이터 / byte data to write</param>
	public void Write(byte data) {
        // set data at current write position / 데이터 설정
        _buffer[_writePos] = data;
        // advance write position with wrap-around / 위치 업데이트
        _writePos = (_writePos + 1) & _mask;
        // check if buffer has remaining capacity / 용량 확인
        if (Available < Capacity)
            // increment available count / 사용 가능 크기 업데이트
            Available++;
        else
            // advance read position to discard oldest / 데이터 덮어쓰기
            _readPos = (_readPos + 1) & _mask;
    }

	/// <summary>
	///     바이트 배열을 버퍼에 씁니다. 버퍼 끝에 도달하면 처음으로 돌아가 순환 쓰기합니다.
	///     writes byte array to buffer. Wraps around to beginning when reaching end of buffer.
	/// </summary>
	/// <param name="data">쓸 바이트 배열 / byte array to write</param>
	/// <exception cref="ArgumentNullException">data가 null인 경우 / thrown when data is null</exception>
	public void WriteBytes(byte[] data) {
        // ensure data is not null / 데이터 확인
        ArgumentNullException.ThrowIfNull(data);
        // check if length is valid / 길이 확인
        if (data.Length == 0 || data.Length > Capacity)
            // skip empty or oversized input / 빈 데이터 또는 초과 데이터 무시
            return;
        // get the data length / 데이터 길이 가져오기
        var length = data.Length;
        // calculate remaining space before wrap / 쓰기 위치부터 버퍼 끝까지 남은 공간 계산
        var remain = Capacity - _writePos;
        // check if data fits without wrap-around / 길이 확인
        if (length <= remain) {
            // copy all data in one block / 모든 데이터 복사
            Array.Copy(data, 0, _buffer, _writePos, length);
            // advance write position / 위치 업데이트
            _writePos = (_writePos + length) & _mask;
        } else {
            // calculate overflow offset / 남은 데이터 길이 계산
            var offset = length - remain;
            // copy first block to end of buffer / 첫 번째 블록 데이터 복사
            Buffer.BlockCopy(data, 0, _buffer, _writePos, remain);
            // copy second block from start of buffer / 두 번째 블록 데이터 복사
            Buffer.BlockCopy(data, remain, _buffer, 0, offset);
            // update write position after wrap / 위치 업데이트
            _writePos = offset & _mask;
        }

        // compute new available byte count / 새 사용 가능 크기 계산
        var newAvailable = Available + length;
        // check if capacity was exceeded / 사용 가능 크기 확인
        if (newAvailable > Capacity) {
            // calculate how many bytes were overwritten / 덮어쓰기 크기 계산
            var size = newAvailable - Capacity;
            // advance read position past overwritten data / 데이터 덮어쓰기
            _readPos = (_readPos + size) & _mask;
            // cap available at capacity / 사용 가능 크기 설정
            Available = Capacity;
        } else {
            // update available count / 사용 가능 크기 설정
            Available = newAvailable;
        }
    }

	/// <summary>
	///     ReadOnlySpan을 버퍼에 씁니다. Zero-copy 방식으로 고성능 쓰기를 수행합니다.
	///     writes ReadOnlySpan to buffer. Performs high-performance zero-copy write.
	/// </summary>
	/// <param name="data">쓸 데이터 스팬 / data span to write</param>
	public void WriteBytes(ReadOnlySpan<byte> data) {
        // check if length is valid / 길이 확인
        if (data.Length == 0 || data.Length > Capacity)
            // skip empty or oversized input / 빈 데이터 또는 초과 데이터 무시
            return;
        // get the data length / 데이터 길이 가져오기
        var length = data.Length;
        // calculate remaining space before wrap / 쓰기 위치부터 버퍼 끝까지 남은 공간 계산
        var remain = Capacity - _writePos;

        // check if data fits without wrap-around / 길이 확인
        if (length <= remain) {
            // copy all data directly via span / 모든 데이터 직접 복사
            data.CopyTo(_buffer.AsSpan(_writePos, length));
            // advance write position / 위치 업데이트
            _writePos = (_writePos + length) & _mask;
        } else {
            // calculate overflow offset / 남은 데이터 길이 계산
            var offset = length - remain;
            // copy first block to end of buffer / 첫 번째 블록 데이터 복사
            data[..remain].CopyTo(_buffer.AsSpan(_writePos, remain));
            // copy second block from start of buffer / 두 번째 블록 데이터 복사
            data[remain..].CopyTo(_buffer.AsSpan(0, offset));
            // update write position after wrap / 위치 업데이트
            _writePos = offset & _mask;
        }

        // compute new available byte count / 새 사용 가능 크기 계산
        var newAvailable = Available + length;
        // check if capacity was exceeded / 사용 가능 크기 확인
        if (newAvailable > Capacity) {
            // calculate how many bytes were overwritten / 덮어쓰기 크기 계산
            var size = newAvailable - Capacity;
            // advance read position past overwritten data / 데이터 덮어쓰기
            _readPos = (_readPos + size) & _mask;
            // cap available at capacity / 사용 가능 크기 설정
            Available = Capacity;
        } else {
            // update available count / 사용 가능 크기 설정
            Available = newAvailable;
        }
    }

	/// <summary>
	///     읽기 위치를 변경하지 않고 지정된 오프셋의 바이트를 조회합니다.
	///     retrieves byte at specified offset without changing read position.
	/// </summary>
	/// <param name="offset">읽기 위치로부터의 오프셋 / offset from read position</param>
	/// <returns>오프셋 위치의 바이트 / byte at offset position</returns>
	/// <exception cref="ArgumentOutOfRangeException">오프셋이 유효 범위를 벗어난 경우 / thrown when offset is out of valid range</exception>
	public byte Peek(int offset) {
        // get current available count / 사용 가능 크기 가져오기
        var available = Available;
        // get current read position / 읽기 위치 가져오기
        var pos = _readPos;
        // ensure offset is within available range / 오프셋 범위 확인
        return (uint)offset < (uint)available ? _buffer[(pos + offset) & _mask] :
            throw new ArgumentOutOfRangeException(nameof(offset), "Invalid offset");
    }

	/// <summary>
	///     읽기 위치를 변경하지 않고 버퍼의 모든 사용 가능한 데이터를 조회합니다.
	///     retrieves all available data in buffer without changing read position.
	/// </summary>
	/// <returns>사용 가능한 데이터의 ReadOnlySpan / ReadOnlySpan of available data</returns>
	public ReadOnlySpan<byte> PeekBytes() {
        // get current available count / 사용 가능 크기 가져오기
        var available = Available;
        // get current read position / 읽기 위치 가져오기
        var pos = _readPos;
        // check if data is available / 데이터 확인
        if (available == 0)
            // return empty span when no data exists / 데이터 없으면 빈 스팬 반환
            return ReadOnlySpan<byte>.Empty;
        // get internal buffer length / 길이 가져오기
        var len = _buffer.Length;
        // check if data is contiguous / 데이터 연속 여부 확인
        if (pos + available <= len)
            // return contiguous span directly / 연속 데이터 직접 반환
            return new ReadOnlySpan<byte>(_buffer, pos, available);

        // create temporary array for non-contiguous data / 배열 생성
        var result = new byte[available];
        // calculate first segment length / 첫 번째 부분 길이 계산
        var first = len - pos;
        // copy first segment / 데이터 복사
        Buffer.BlockCopy(_buffer, pos, result, 0, first);
        // copy second segment / 두 번째 부분 복사
        Buffer.BlockCopy(_buffer, 0, result, first, available - first);
        // return combined data / 모든 데이터 반환
        return result;
    }

	/// <summary>
	///     지정된 길이만큼 데이터를 읽고 읽기 위치를 이동합니다.
	///     reads specified length of data and advances read position.
	/// </summary>
	/// <param name="length">읽을 바이트 수 / number of bytes to read</param>
	/// <returns>읽은 데이터 배열 / array of read data</returns>
	/// <exception cref="ArgumentOutOfRangeException">length가 음수인 경우 / thrown when length is negative</exception>
	public byte[] ReadBytes(int length) {
        // ensure length is non-negative / 길이 확인
        if (length < 0)
            // throw out-of-range exception for negative length / 음수 길이 범위 초과 예외 발생
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");
        // get current available count / 사용 가능 크기 가져오기
        var available = Available;
        // get current read position / 읽기 위치 가져오기
        var pos = _readPos;
        // calculate actual read length / 실제 길이 계산
        var actual = Math.Min(length, available);
        // check if there is data to read / 실제 길이 확인
        if (actual == 0)
            // return empty array when no data available / 데이터 없으면 빈 배열 반환
            return [];
        // get internal buffer length / 버퍼 길이 가져오기
        var len = _buffer.Length;
        // create result buffer / 결과 버퍼 생성
        var result = new byte[actual];
        // check if data is contiguous / 길이 확인
        if (pos + actual <= len) {
            // copy single contiguous block / 단일 블록 복사
            Buffer.BlockCopy(_buffer, pos, result, 0, actual);
        } else {
            // calculate first segment length / 첫 번째 블록 인덱스 계산
            var first = len - pos;
            // copy first segment / 첫 번째 블록 복사
            Buffer.BlockCopy(_buffer, pos, result, 0, first);
            // copy second segment / 두 번째 블록 복사
            Buffer.BlockCopy(_buffer, 0, result, first, actual - first);
        }

        // advance read position / 위치 업데이트
        _readPos = (pos + actual) & _mask;
        // update available count / 사용 가능 크기 재설정
        Available = available - actual;
        // return read data / 결과 반환
        return result;
    }

	/// <summary>
	///     데이터를 읽지 않고 읽기 위치만 이동하여 데이터를 제거합니다.
	///     removes data by advancing read position without reading.
	/// </summary>
	/// <param name="length">제거할 바이트 수 / number of bytes to remove</param>
	/// <exception cref="ArgumentOutOfRangeException">length가 음수인 경우 / thrown when length is negative</exception>
	public void RemoveBytes(int length) {
        // ensure length is non-negative / 길이 확인
        if (length < 0)
            // throw out-of-range exception for negative length / 음수 길이 범위 초과 예외 발생
            throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");

        // calculate actual removal length / 실제 길이 계산
        var actual = Math.Min(length, Available);
        // check if there is data to remove / 실제 길이 확인
        if (actual == 0)
            // skip when nothing to remove / 제거할 데이터 없으면 무시
            return;
        // advance read position / 읽기 위치 이동
        _readPos = (_readPos + actual) & _mask;
        // update available count / 사용 가능 크기 재설정
        Available -= actual;
    }

	/// <summary>
	///     버퍼의 모든 데이터를 제거하고 읽기/쓰기 위치를 초기화합니다.
	///     removes all data from buffer and resets read/write positions.
	/// </summary>
	public void Clear() {
        // reset read position / 읽기 위치 초기화
        _readPos = 0;
        // reset write position / 쓰기 위치 초기화
        _writePos = 0;
        // reset available count / 사용 가능 크기 초기화
        Available = 0;
    }
}