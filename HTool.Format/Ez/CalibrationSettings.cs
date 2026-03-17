using System.ComponentModel;
using HTool.Core.Type.Ez;
using HTool.Core.Util;

namespace HTool.Format.Ez;

/// <summary>
///     EZTorQ-III 캘리브레이션 설정 데이터 (4바이트: 일체형, 6바이트: 분리형).
///     EZTorQ-III calibration settings data (4 bytes: Integrated, 6 bytes: Separated).
/// </summary>
/// <remarks>
///     포인트 모드(1B) + 인덱스(1B) + 값(2B 또는 4B)으로 구성됩니다.
///     읽기는 리틀엔디안, 쓰기는 빅엔디안입니다 (원본 프로토콜 동작 유지).
///     composed of point mode (1B) + index (1B) + value (2B or 4B).
///     reads are little-endian, writes are big-endian (preserving original protocol behavior).
/// </remarks>
public readonly struct CalibrationSettings {
	/// <summary>
	///     허용 데이터 크기 (4: 일체형, 6: 분리형)
	///     allowed data sizes (4: Integrated, 6: Separated)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [4, 6];

	/// <summary>
	///     원시 패킷 데이터에서 캘리브레이션 설정을 파싱합니다.
	///     parses calibration settings from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 (4 또는 6바이트) / raw packet data (4 or 6 bytes)</param>
	/// <param name="body">본체 타입 (기본값: Integrated) / body type (default: Integrated)</param>
	/// <exception cref="FormatException">데이터 크기가 4 또는 6이 아닐 때 / when data size is not 4 or 6</exception>
	public CalibrationSettings(ReadOnlySpan<byte> data, Body body = Body.Integrated) {
        // 데이터 크기 캐싱
        // cache data length
        var size = data.Length;
        // 데이터 크기가 4 또는 6인지 확인
        // ensure data size is 4 or 6 bytes
        if (size is not 4 and not 6)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException(
                $"Invalid calibration settings size: {size} bytes. Expected 4 or 6 bytes.");

        // 본체 타입 저장 (직렬화용)
        // store body type (for serialization)
        BodyType = body;

        /*      common information / 공통 정보      */

        // 캘리브레이션 포인트 모드 읽기
        // read calibration point mode
        Point = EnumUtil.IsDefined<CalPointMode>(data[0]) ? (CalPointMode)data[0] : CalPointMode.ThreePoint;
        // 캘리브레이션 포인트 인덱스 읽기
        // read calibration point index
        Index = data[1];

        /*      value information (little-endian) / 값 정보 (리틀엔디안)      */

        // 본체 타입에 따라 분기
        // branch by body type
        if (body is Body.Separated)
            // 분리형: int32 (4바이트, LE) 읽기
            // separated: read int32 (4 bytes, LE)
            Value = Utils.ReadInt32(data[2..], isBigEndian: false);
        else
            // 일체형: ushort (2바이트, LE) 읽기
            // integrated: read ushort (2 bytes, LE)
            Value = Utils.ReadUInt16(data[2..], false);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
    }

	/// <summary>
	///     캘리브레이션 포인트 모드
	///     calibration point mode
	/// </summary>
	public CalPointMode Point { get; init; }

	/// <summary>
	///     캘리브레이션 포인트 인덱스
	///     calibration point index
	/// </summary>
	public byte Index { get; init; }

	/// <summary>
	///     캘리브레이션 ADC 값
	///     calibration ADC value
	/// </summary>
	public int Value { get; init; }

	/// <summary>
	///     본체 타입 (직렬화용)
	///     body type (for serialization)
	/// </summary>
	[Browsable(false)]
    public Body BodyType { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }

	/// <summary>
	///     캘리브레이션 설정을 바이트 배열로 직렬화합니다.
	///     serializes calibration settings to byte array.
	/// </summary>
	/// <param name="withoutValue">true이면 값 없이 헤더만 직렬화 / if true, serialize header only without value</param>
	/// <returns>바이트 배열 (4: 일체형, 6: 분리형, withoutValue=true이면 2) / byte array (4: Integrated, 6: Separated, 2 if withoutValue)</returns>
	public byte[] ToBytes(bool withoutValue = false) {
        // 크기 계산 (값 제외 옵션 고려)
        // calculate size (considering withoutValue option)
        var valueSize = BodyType is Body.Separated ? 4 : 2;
        var size      = 2 + (withoutValue is false ? valueSize : 0);
        // 배열 생성
        // create byte array
        var bytes = new byte[size];

        // 포인트 모드 및 인덱스 기록
        // write point mode and index
        bytes[0] = (byte)Point;
        bytes[1] = Index;

        // 값 제외 옵션 확인
        // check withoutValue option
        if (withoutValue)
            // 값 없이 바이트 반환
            // return bytes without value
            return bytes;

        // 본체 타입에 따라 값 직렬화 (빅엔디안)
        // serialize value by body type (big-endian)
        switch (BodyType) {
            case Body.Separated:
                // 분리형: int32 (4바이트, BE) 기록
                // separated: write int32 (4 bytes, BE)
                Utils.WriteInt32(bytes.AsSpan(2), Value);
                // switch 종료
                // exit switch
                break;

            case Body.Integrated:
            default:
                // 일체형: ushort (2바이트, BE) 기록
                // integrated: write ushort (2 bytes, BE)
                Utils.WriteUInt16(bytes.AsSpan(2), (ushort)Value);
                // switch 종료
                // exit switch
                break;
        }

        // 직렬화된 바이트 반환
        // return serialized bytes
        return bytes;
    }

	/// <summary>
	///     원시 데이터에서 캘리브레이션 설정을 파싱합니다.
	///     attempts to parse calibration settings from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="body">본체 타입 / body type</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, Body body, out CalibrationSettings result) {
        // 데이터 크기 확인
        // check data size
        if (data.Length is not 4 and not 6) {
            // 기본값 설정
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new CalibrationSettings(data, body);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set default result
            result = default;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }
}