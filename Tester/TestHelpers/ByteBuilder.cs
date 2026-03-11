using System.Buffers.Binary;
using System.Text;

namespace Tester.TestHelpers;

/// <summary>
///     Big-Endian 바이트 배열을 플루언트하게 구성하는 빌더.
///     Fluent builder for constructing Big-Endian byte arrays.
/// </summary>
internal sealed class ByteBuilder {
    // 내부 바이트 목록
    // internal byte list
    private readonly List<byte> _bytes = new();

    /// <summary>
    ///     단일 바이트를 추가한다.
    ///     appends a single byte.
    /// </summary>
    /// <param name="v">바이트 값 / byte value</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder Byte(byte v) {
        // 바이트 추가
        // add byte
        _bytes.Add(v);
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     16비트 부호 없는 정수를 Big-Endian으로 추가한다.
    ///     appends a 16-bit unsigned integer in Big-Endian order.
    /// </summary>
    /// <param name="v">16비트 값 / 16-bit value</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder UInt16BigEndian(ushort v) {
        // 2바이트 임시 버퍼 할당
        // allocate 2-byte temp buffer
        Span<byte> buf = stackalloc byte[2];
        // Big-Endian 쓰기
        // write Big-Endian
        BinaryPrimitives.WriteUInt16BigEndian(buf, v);
        // 버퍼 내용을 바이트 목록에 추가
        // append buffer contents to byte list
        _bytes.Add(buf[0]);
        _bytes.Add(buf[1]);
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     16비트 부호 있는 정수를 Big-Endian으로 추가한다.
    ///     appends a 16-bit signed integer in Big-Endian order.
    /// </summary>
    /// <param name="v">16비트 값 / 16-bit value</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder Int16BigEndian(short v) {
        // 2바이트 임시 버퍼 할당
        // allocate 2-byte temp buffer
        Span<byte> buf = stackalloc byte[2];
        // Big-Endian 쓰기
        // write Big-Endian
        BinaryPrimitives.WriteInt16BigEndian(buf, v);
        // 버퍼 내용을 바이트 목록에 추가
        // append buffer contents to byte list
        _bytes.Add(buf[0]);
        _bytes.Add(buf[1]);
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     32비트 부호 있는 정수를 Big-Endian으로 추가한다.
    ///     appends a 32-bit signed integer in Big-Endian order.
    /// </summary>
    /// <param name="v">32비트 값 / 32-bit value</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder Int32BigEndian(int v) {
        // 4바이트 임시 버퍼 할당
        // allocate 4-byte temp buffer
        Span<byte> buf = stackalloc byte[4];
        // Big-Endian 쓰기
        // write Big-Endian
        BinaryPrimitives.WriteInt32BigEndian(buf, v);
        // 버퍼 내용을 바이트 목록에 추가
        // append buffer contents to byte list
        _bytes.AddRange(buf.ToArray());
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     32비트 부호 없는 정수를 Big-Endian으로 추가한다.
    ///     appends a 32-bit unsigned integer in Big-Endian order.
    /// </summary>
    /// <param name="v">32비트 값 / 32-bit value</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder UInt32BigEndian(uint v) {
        // 4바이트 임시 버퍼 할당
        // allocate 4-byte temp buffer
        Span<byte> buf = stackalloc byte[4];
        // Big-Endian 쓰기
        // write Big-Endian
        BinaryPrimitives.WriteUInt32BigEndian(buf, v);
        // 버퍼 내용을 바이트 목록에 추가
        // append buffer contents to byte list
        _bytes.AddRange(buf.ToArray());
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     단정밀도 부동소수점을 Big-Endian으로 추가한다.
    ///     appends a single-precision float in Big-Endian order.
    /// </summary>
    /// <param name="v">부동소수점 값 / float value</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder SingleBigEndian(float v) {
        // 4바이트 임시 버퍼 할당
        // allocate 4-byte temp buffer
        Span<byte> buf = stackalloc byte[4];
        // Big-Endian 쓰기
        // write Big-Endian
        BinaryPrimitives.WriteSingleBigEndian(buf, v);
        // 버퍼 내용을 바이트 목록에 추가
        // append buffer contents to byte list
        _bytes.AddRange(buf.ToArray());
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     배정밀도 부동소수점을 Big-Endian으로 추가한다.
    ///     appends a double-precision float in Big-Endian order.
    /// </summary>
    /// <param name="v">배정밀도 값 / double value</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder DoubleBigEndian(double v) {
        // 8바이트 임시 버퍼 할당
        // allocate 8-byte temp buffer
        Span<byte> buf = stackalloc byte[8];
        // Big-Endian 쓰기
        // write Big-Endian
        BinaryPrimitives.WriteDoubleBigEndian(buf, v);
        // 버퍼 내용을 바이트 목록에 추가
        // append buffer contents to byte list
        _bytes.AddRange(buf.ToArray());
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     ASCII 문자열을 지정 길이만큼 제로 패딩하여 추가한다.
    ///     appends an ASCII string zero-padded to the specified length.
    /// </summary>
    /// <param name="s">ASCII 문자열 / ASCII string</param>
    /// <param name="padTo">총 바이트 수 / total byte count</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder Ascii(string s, int padTo) {
        // ASCII 바이트로 변환
        // convert to ASCII bytes
        var encoded = Encoding.ASCII.GetBytes(s);
        // 지정 길이만큼 배열 생성 (자동 제로 패딩)
        // create array with specified length (auto zero-padded)
        var padded = new byte[padTo];
        // 원본 데이터 복사 (길이 초과 시 잘라냄)
        // copy source data (truncate if exceeds length)
        Array.Copy(encoded, padded, Math.Min(encoded.Length, padTo));
        // 패딩된 바이트 추가
        // append padded bytes
        _bytes.AddRange(padded);
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     지정 길이만큼 0으로 채운다.
    ///     appends the specified count of zero bytes.
    /// </summary>
    /// <param name="count">0 바이트 수 / zero byte count</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder Zeros(int count) {
        // 지정 수만큼 제로 바이트 추가
        // add specified number of zero bytes
        _bytes.AddRange(new byte[count]);
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     원시 바이트 배열을 그대로 추가한다.
    ///     appends raw byte data as-is.
    /// </summary>
    /// <param name="data">원시 바이트 / raw bytes</param>
    /// <returns>현재 빌더 인스턴스 / current builder instance</returns>
    internal ByteBuilder Raw(params byte[] data) {
        // 원시 바이트 추가
        // append raw bytes
        _bytes.AddRange(data);
        // 체이닝을 위해 자기 자신 반환
        // return self for chaining
        return this;
    }

    /// <summary>
    ///     바이트 배열을 빌드하여 반환한다.
    ///     builds and returns the byte array.
    /// </summary>
    /// <returns>구성된 바이트 배열 / constructed byte array</returns>
    internal byte[] Build() {
        // 목록을 배열로 변환하여 반환
        // convert list to array and return
        return _bytes.ToArray();
    }
}