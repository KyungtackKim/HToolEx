using HTool.Core.Util;
using HTool.Type;

namespace HTool.Device.Codec;

/// <summary>
///     MODBUS 프레임 인코딩/디코딩 인터페이스.
///     MODBUS frame encoding/decoding interface.
/// </summary>
/// <remarks>
///     <para>
///         RTU와 TCP 프레임 형식의 차이를 추상화한다.
///         RTU: [ID(1)][FC(1)][Data][CRC(2)], TCP: [MBAP(7)][FC(1)][Data].
///     </para>
///     <para>
///         Abstracts the differences between RTU and TCP frame formats.
///         RTU: [ID(1)][FC(1)][Data][CRC(2)], TCP: [MBAP(7)][FC(1)][Data].
///     </para>
/// </remarks>
public interface IModbusCodec {
	/// <summary>
	///     프로토콜 헤더 크기 (RTU=2, TCP=8).
	///     Protocol header size (RTU=2, TCP=8).
	/// </summary>
	int HeaderSize { get; }

	/// <summary>
	///     프레임 내 함수 코드 위치 (RTU=1, TCP=7).
	///     Function code position within the frame (RTU=1, TCP=7).
	/// </summary>
	int FunctionPos { get; }

	/// <summary>
	///     보유 레지스터 읽기 패킷을 생성한다 (FC 0x03).
	///     Builds a Read Holding Registers packet (FC 0x03).
	/// </summary>
	/// <param name="addr">시작 주소 / start address</param>
	/// <param name="count">레지스터 개수 / register count</param>
	/// <param name="dId">슬레이브 장치 ID / slave device ID</param>
	/// <param name="tId">트랜잭션 ID (TCP 전용, RTU는 무시) / transaction ID (TCP only, ignored by RTU)</param>
	/// <returns>완성된 프레임 바이트 배열 / complete frame byte array</returns>
	byte[] BuildReadHoldingReg(ushort addr, ushort count, byte dId, ushort tId = 0);

	/// <summary>
	///     입력 레지스터 읽기 패킷을 생성한다 (FC 0x04).
	///     Builds a Read Input Registers packet (FC 0x04).
	/// </summary>
	/// <param name="addr">시작 주소 / start address</param>
	/// <param name="count">레지스터 개수 / register count</param>
	/// <param name="dId">슬레이브 장치 ID / slave device ID</param>
	/// <param name="tId">트랜잭션 ID (TCP 전용, RTU는 무시) / transaction ID (TCP only, ignored by RTU)</param>
	/// <returns>완성된 프레임 바이트 배열 / complete frame byte array</returns>
	byte[] BuildReadInputReg(ushort addr, ushort count, byte dId, ushort tId = 0);

	/// <summary>
	///     단일 레지스터 쓰기 패킷을 생성한다 (FC 0x06).
	///     Builds a Write Single Register packet (FC 0x06).
	/// </summary>
	/// <param name="addr">레지스터 주소 / register address</param>
	/// <param name="value">쓸 값 / value to write</param>
	/// <param name="dId">슬레이브 장치 ID / slave device ID</param>
	/// <param name="tId">트랜잭션 ID (TCP 전용, RTU는 무시) / transaction ID (TCP only, ignored by RTU)</param>
	/// <returns>완성된 프레임 바이트 배열 / complete frame byte array</returns>
	byte[] BuildWriteSingleReg(ushort addr, ushort value, byte dId, ushort tId = 0);

	/// <summary>
	///     다중 레지스터 쓰기 패킷을 생성한다 (FC 0x10).
	///     Builds a Write Multiple Registers packet (FC 0x10).
	/// </summary>
	/// <param name="addr">시작 주소 / start address</param>
	/// <param name="values">쓸 값 배열 / values to write</param>
	/// <param name="dId">슬레이브 장치 ID / slave device ID</param>
	/// <param name="tId">트랜잭션 ID (TCP 전용, RTU는 무시) / transaction ID (TCP only, ignored by RTU)</param>
	/// <returns>완성된 프레임 바이트 배열 / complete frame byte array</returns>
	byte[] BuildWriteMultiReg(ushort addr, ReadOnlySpan<ushort> values, byte dId, ushort tId = 0);

	/// <summary>
	///     문자열 레지스터 쓰기 패킷을 생성한다 (FC 0x10, 문자열 인코딩).
	///     Builds a Write String Register packet (FC 0x10, string encoding).
	/// </summary>
	/// <param name="addr">시작 주소 / start address</param>
	/// <param name="str">쓸 문자열 / string to write</param>
	/// <param name="length">
	///     고정 바이트 길이 (0이면 문자열 길이 사용).
	///     Fixed byte length (0 uses string length).
	/// </param>
	/// <param name="dId">슬레이브 장치 ID / slave device ID</param>
	/// <param name="tId">트랜잭션 ID (TCP 전용, RTU는 무시) / transaction ID (TCP only, ignored by RTU)</param>
	/// <returns>완성된 프레임 바이트 배열 / complete frame byte array</returns>
	byte[] BuildWriteStrReg(ushort addr, string str, int length, byte dId, ushort tId = 0);

	/// <summary>
	///     장치 정보 읽기 패킷을 생성한다 (FC 0x11, HANTAS 전용).
	///     Builds a Read Info Register packet (FC 0x11, HANTAS custom).
	/// </summary>
	/// <param name="dId">슬레이브 장치 ID / slave device ID</param>
	/// <param name="tId">트랜잭션 ID (TCP 전용, RTU는 무시) / transaction ID (TCP only, ignored by RTU)</param>
	/// <returns>완성된 프레임 바이트 배열 / complete frame byte array</returns>
	byte[] BuildReadInfoReg(byte dId, ushort tId = 0);

	/// <summary>
	///     수신 버퍼에서 완전한 프레임의 길이를 계산한다.
	///     Calculates the expected frame length from the receive buffer.
	/// </summary>
	/// <param name="buffer">
	///     수신 데이터가 담긴 링 버퍼.
	///     Ring buffer containing received data.
	/// </param>
	/// <returns>
	///     프레임 전체 길이. 데이터 부족 시 -1.
	///     Total frame length, or -1 if insufficient data.
	/// </returns>
	int CalcFrameLength(RingBuffer buffer);

	/// <summary>
	///     프레임에서 함수 코드를 추출한다.
	///     Extracts the function code from a frame.
	/// </summary>
	/// <param name="frame">완전한 프레임 데이터 / complete frame data</param>
	/// <returns>추출된 함수 코드 / extracted function code</returns>
	FunctionCode ExtractCode(ReadOnlySpan<byte> frame);

	/// <summary>
	///     프레임에서 페이로드를 추출한다 (헤더/CRC 제외).
	///     Extracts the payload from a frame (excluding header/CRC).
	/// </summary>
	/// <param name="frame">완전한 프레임 데이터 / complete frame data</param>
	/// <returns>페이로드 바이트 배열 / payload byte array</returns>
	byte[] ExtractPayload(ReadOnlySpan<byte> frame);

	/// <summary>
	///     수신 프레임의 무결성을 검증한다 (RTU: CRC-16, TCP: 항상 true).
	///     Validates the integrity of a received frame (RTU: CRC-16, TCP: always true).
	/// </summary>
	/// <param name="frame">완전한 프레임 데이터 / complete frame data</param>
	/// <returns>검증 통과 여부 / true if frame is valid</returns>
	bool ValidateFrame(ReadOnlySpan<byte> frame);
}