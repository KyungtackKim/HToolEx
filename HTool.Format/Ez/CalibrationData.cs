using System.ComponentModel;
using HTool.Core.Type.Ez;
using HTool.Core.Type.Process;
using HTool.Core.Util;

namespace HTool.Format.Ez;

/// <summary>
///     EZTorQ-III 캘리브레이션 데이터 (41바이트: 일체형, 63바이트: 분리형).
///     EZTorQ-III calibration data (41 bytes: Integrated, 63 bytes: Separated).
/// </summary>
/// <remarks>
///     공통 정보(19바이트) + 캘리브레이션 값(오프셋 1개, 양방향 5개, 음방향 5개)으로 구성됩니다.
///     일체형은 ushort(2B), 분리형은 int32(4B) 단위를 사용합니다. 모든 다중 바이트 값은 리틀엔디안입니다.
///     composed of common info (19 bytes) + calibration values (1 offset, 5 positive, 5 negative).
///     Integrated uses ushort (2B), Separated uses int32 (4B). all multi-byte values are little-endian.
/// </remarks>
public readonly struct CalibrationData {
	/// <summary>
	///     3점 캘리브레이션 양방향 매핑 (CalPoint -> 배열 인덱스)
	///     3-point calibration positive mapping (CalPoint -> array index)
	/// </summary>
	private static readonly Dictionary<CalPoint, int> ThreePointPositiveMap = new() {
        [CalPoint.Point10P] = 0, [CalPoint.Point50P] = 1, [CalPoint.Point100P] = 2
    };

	/// <summary>
	///     3점 캘리브레이션 음방향 매핑 (CalPoint -> 배열 인덱스)
	///     3-point calibration negative mapping (CalPoint -> array index)
	/// </summary>
	private static readonly Dictionary<CalPoint, int> ThreePointNegativeMap = new() {
        [CalPoint.Point10M] = 0, [CalPoint.Point50M] = 1, [CalPoint.Point100M] = 2
    };

	/// <summary>
	///     5점 캘리브레이션 양방향 매핑 (CalPoint -> 배열 인덱스)
	///     5-point calibration positive mapping (CalPoint -> array index)
	/// </summary>
	private static readonly Dictionary<CalPoint, int> FivePointPositiveMap = new() {
        [CalPoint.Point20P]  = 0,
        [CalPoint.Point40P]  = 1,
        [CalPoint.Point60P]  = 2,
        [CalPoint.Point80P]  = 3,
        [CalPoint.Point100P] = 4
    };

	/// <summary>
	///     5점 캘리브레이션 음방향 매핑 (CalPoint -> 배열 인덱스)
	///     5-point calibration negative mapping (CalPoint -> array index)
	/// </summary>
	private static readonly Dictionary<CalPoint, int> FivePointNegativeMap = new() {
        [CalPoint.Point20M]  = 0,
        [CalPoint.Point40M]  = 1,
        [CalPoint.Point60M]  = 2,
        [CalPoint.Point80M]  = 3,
        [CalPoint.Point100M] = 4
    };

	/// <summary>
	///     허용 데이터 크기 (41: 일체형, 63: 분리형)
	///     allowed data sizes (41: Integrated, 63: Separated)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [41, 63];

	/// <summary>
	///     원시 패킷 데이터에서 캘리브레이션 데이터를 파싱합니다.
	///     parses calibration data from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 (41 또는 63바이트) / raw packet data (41 or 63 bytes)</param>
	/// <exception cref="FormatException">데이터 크기가 41 또는 63이 아닐 때 / when data size is not 41 or 63</exception>
	public CalibrationData(ReadOnlySpan<byte> data) {
        // 데이터 크기 캐싱
        // cache data length
        var size = data.Length;
        // 데이터 크기가 41 또는 63인지 확인
        // ensure data size is 41 or 63 bytes
        if (size is not 41 and not 63)
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException(
                $"Invalid calibration data size: {size} bytes. Expected 41 or 63 bytes.");

        /*      common information (0-18, little-endian) / 공통 정보 (0-18, 리틀엔디안)      */

        // 본체 타입 읽기
        // read body type
        Body = EnumUtil.IsDefined<Body>(data[0]) ? (Body)data[0] : Body.Integrated;
        // 모델 번호 읽기 (int32, LE)
        // read model number (int32, LE)
        Model = (uint)Utils.ReadInt32(data[1..5], isBigEndian: false);
        // 100.0f, LE) / 최대 토크 읽기 (int32 / 100.0f, LE)
        // read max torque (int32
        MaxTorque = Utils.ReadInt32(data[5..9], isBigEndian: false) / 100.0f;
        // 본체 시리얼 번호 읽기 (int32, LE)
        // read body serial number (int32, LE)
        BodySerial = (uint)Utils.ReadInt32(data[9..13], isBigEndian: false);
        // 센서 시리얼 번호 읽기 (int32, LE)
        // read sensor serial number (int32, LE)
        SensorSerial = (uint)Utils.ReadInt32(data[13..17], isBigEndian: false);
        // 단위 읽기
        // read unit
        Unit = EnumUtil.IsDefined<Unit>(data[17]) ? (Unit)data[17] : Unit.KgfCm;
        // 캘리브레이션 타입 읽기
        // read calibration type
        CalType = EnumUtil.IsDefined<CalPointMode>(data[18]) ? (CalPointMode)data[18] : CalPointMode.ThreePoint;

        /*      calibration information (19~, little-endian) / 캘리브레이션 정보 (19~, 리틀엔디안)      */

        // 본체 타입에 따라 분기
        // branch by body type
        switch (Body) {
            // 일체형 본체의 데이터 크기 검증
            // ensure integrated body data size is 41
            case Body.Integrated when size is not 41:
                throw new FormatException(
                    $"Integrated body type requires 41 bytes, but received {size} bytes.");
            // 분리형 본체의 데이터 크기 검증
            // ensure separated body data size is 63
            case Body.Separated when size is not 63:
                throw new FormatException(
                    $"Separated body type requires 63 bytes, but received {size} bytes.");
            case Body.Separated: {
                // 분리형: int32 (4바이트) 단위로 읽기
                // separated: read as int32 (4 bytes each)
                Offset = Utils.ReadInt32(data[19..23], isBigEndian: false);
                // 양방향 포인트 1 읽기
                // read positive point 1
                var p1 = Utils.ReadInt32(data[23..27], isBigEndian: false);
                // 양방향 포인트 2 읽기
                // read positive point 2
                var p2 = Utils.ReadInt32(data[27..31], isBigEndian: false);
                // 양방향 포인트 3 읽기
                // read positive point 3
                var p3 = Utils.ReadInt32(data[31..35], isBigEndian: false);
                // 양방향 포인트 4 읽기
                // read positive point 4
                var p4 = Utils.ReadInt32(data[35..39], isBigEndian: false);
                // 양방향 포인트 5 읽기
                // read positive point 5
                var p5 = Utils.ReadInt32(data[39..43], isBigEndian: false);
                // 양방향 배열 설정
                // set positive array
                Positives = [p1, p2, p3, p4, p5];
                // 음방향 포인트 1 읽기
                // read negative point 1
                var n1 = Utils.ReadInt32(data[43..47], isBigEndian: false);
                // 음방향 포인트 2 읽기
                // read negative point 2
                var n2 = Utils.ReadInt32(data[47..51], isBigEndian: false);
                // 음방향 포인트 3 읽기
                // read negative point 3
                var n3 = Utils.ReadInt32(data[51..55], isBigEndian: false);
                // 음방향 포인트 4 읽기
                // read negative point 4
                var n4 = Utils.ReadInt32(data[55..59], isBigEndian: false);
                // 음방향 포인트 5 읽기
                // read negative point 5
                var n5 = Utils.ReadInt32(data[59..63], isBigEndian: false);
                // 음방향 배열 설정
                // set negative array
                Negatives = [n1, n2, n3, n4, n5];
                // switch 종료
                // exit switch
                break;
            }
            case Body.Integrated:
            default: {
                // 일체형: ushort (2바이트) 단위로 읽기
                // integrated: read as ushort (2 bytes each)
                Offset = Utils.ReadUInt16(data[19..21], false);
                // 양방향 포인트 1 읽기
                // read positive point 1
                var p1 = Utils.ReadUInt16(data[21..23], false);
                // 양방향 포인트 2 읽기
                // read positive point 2
                var p2 = Utils.ReadUInt16(data[23..25], false);
                // 양방향 포인트 3 읽기
                // read positive point 3
                var p3 = Utils.ReadUInt16(data[25..27], false);
                // 양방향 포인트 4 읽기
                // read positive point 4
                var p4 = Utils.ReadUInt16(data[27..29], false);
                // 양방향 포인트 5 읽기
                // read positive point 5
                var p5 = Utils.ReadUInt16(data[29..31], false);
                // 양방향 배열 설정
                // set positive array
                Positives = [p1, p2, p3, p4, p5];
                // 음방향 포인트 1 읽기
                // read negative point 1
                var n1 = Utils.ReadUInt16(data[31..33], false);
                // 음방향 포인트 2 읽기
                // read negative point 2
                var n2 = Utils.ReadUInt16(data[33..35], false);
                // 음방향 포인트 3 읽기
                // read negative point 3
                var n3 = Utils.ReadUInt16(data[35..37], false);
                // 음방향 포인트 4 읽기
                // read negative point 4
                var n4 = Utils.ReadUInt16(data[37..39], false);
                // 음방향 포인트 5 읽기
                // read negative point 5
                var n5 = Utils.ReadUInt16(data[39..41], false);
                // 음방향 배열 설정
                // set negative array
                Negatives = [n1, n2, n3, n4, n5];
                // switch 종료
                // exit switch
                break;
            }
        }

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data);
    }

	/// <summary>
	///     기존 캘리브레이션 데이터를 기반으로 특정 포인트만 갱신한 복사본을 생성합니다.
	///     creates a copy based on source data with updated calibration points.
	/// </summary>
	/// <param name="src">원본 캘리브레이션 데이터 / source calibration data</param>
	/// <param name="calPoints">갱신할 캘리브레이션 포인트 / calibration points to update</param>
	public CalibrationData(CalibrationData src, Dictionary<CalPoint, int> calPoints) {
        // 본체 타입 복사
        // copy body type
        Body = src.Body;
        // 모델 번호 복사
        // copy model number
        Model = src.Model;
        // 최대 토크 복사
        // copy max torque
        MaxTorque = src.MaxTorque;
        // 본체 시리얼 복사
        // copy body serial
        BodySerial = src.BodySerial;
        // 센서 시리얼 복사
        // copy sensor serial
        SensorSerial = src.SensorSerial;
        // 단위 복사
        // copy unit
        Unit = src.Unit;
        // 캘리브레이션 타입 복사
        // copy calibration type
        CalType = src.CalType;

        // 오프셋 값 복사
        // copy offset value
        Offset = src.Offset;
        // 양방향 값 복사
        // copy positive values
        Positives = [.. src.Positives];
        // 음방향 값 복사
        // copy negative values
        Negatives = [.. src.Negatives];

        // 양방향 매핑 테이블 선택
        // select positive mapping table
        var positiveMap = CalType is CalPointMode.ThreePoint ? ThreePointPositiveMap : FivePointPositiveMap;
        // 음방향 매핑 테이블 선택
        // select negative mapping table
        var negativeMap = CalType is CalPointMode.ThreePoint ? ThreePointNegativeMap : FivePointNegativeMap;

        // 캘리브레이션 포인트 갱신 적용
        // apply calibration point updates
        foreach (var (point, value) in calPoints)
            // 포인트 타입 확인
            // check point type
            if (point is CalPoint.PointZero)
                // 오프셋 값 설정
                // set offset value
                Offset = value;
            else if (positiveMap.TryGetValue(point, out var posIdx))
                // 양방향 값 설정
                // set positive value
                Positives[posIdx] = value;
            else if (negativeMap.TryGetValue(point, out var negIdx))
                // 음방향 값 설정
                // set negative value
                Negatives[negIdx] = value;

        // 3점 캘리브레이션이면 미사용 포인트를 0으로 초기화
        // for 3-point calibration, zero out unused points
        if (CalType is CalPointMode.ThreePoint) {
            // 미사용 양방향 초기화
            // reset unused positive values
            Positives[3] = Positives[4] = 0;
            // 미사용 음방향 초기화
            // reset unused negative values
            Negatives[3] = Negatives[4] = 0;
        }

        // 직렬화 후 해시 계산
        // compute hash from serialized bytes
        Hash = DataHash.Compute(ToBytes());
    }

	/// <summary>
	///     본체 타입
	///     body type
	/// </summary>
	public Body Body { get; init; }

	/// <summary>
	///     모델 번호
	///     model number
	/// </summary>
	public uint Model { get; init; }

	/// <summary>
	///     최대 토크
	///     max torque
	/// </summary>
	public float MaxTorque { get; init; }

	/// <summary>
	///     본체 시리얼 번호
	///     body serial number
	/// </summary>
	public uint BodySerial { get; init; }

	/// <summary>
	///     센서 시리얼 번호
	///     sensor serial number
	/// </summary>
	public uint SensorSerial { get; init; }

	/// <summary>
	///     토크 단위
	///     torque unit
	/// </summary>
	public Unit Unit { get; init; }

	/// <summary>
	///     캘리브레이션 포인트 모드
	///     calibration point mode
	/// </summary>
	public CalPointMode CalType { get; init; }

	/// <summary>
	///     오프셋 값
	///     offset value
	/// </summary>
	public int Offset { get; init; }

	/// <summary>
	///     양방향 캘리브레이션 값 (5개)
	///     positive calibration values (5 elements)
	/// </summary>
	public int[] Positives { get; init; }

	/// <summary>
	///     음방향 캘리브레이션 값 (5개)
	///     negative calibration values (5 elements)
	/// </summary>
	public int[] Negatives { get; init; }

	/// <summary>
	///     데이터 해시 (변경 감지용)
	///     data hash (for change detection)
	/// </summary>
	[Browsable(false)]
    public ulong Hash { get; init; }

	/// <summary>
	///     캘리브레이션 데이터를 바이트 배열로 직렬화합니다.
	///     serializes calibration data to byte array.
	/// </summary>
	/// <returns>바이트 배열 (41: 일체형, 63: 분리형) / byte array (41: Integrated, 63: Separated)</returns>
	public byte[] ToBytes() {
        // 본체 타입에 따라 크기 결정
        // determine size based on body type
        var size = Body is Body.Separated ? 63 : 41;
        // 배열 생성
        // create byte array
        var bytes = new byte[size];
        // 제로카피 슬라이싱용 Span 뷰 생성
        // create span view for zero-copy slicing
        var s = bytes.AsSpan();

        // 공통 정보 직렬화 (0-18, 리틀엔디안)
        // serialize common information (0-18, little-endian)
        bytes[0] = (byte)Body;
        // 모델 번호 기록
        // write model number
        Utils.WriteInt32(s[1..], (int)Model, false);
        // 최대 토크 기록 (float -> int * 100)
        // write max torque (float -> int * 100)
        Utils.WriteInt32(s[5..], (int)(MaxTorque * 100.0f), false);
        // 본체 시리얼 기록
        // write body serial
        Utils.WriteInt32(s[9..], (int)BodySerial, false);
        // 센서 시리얼 기록
        // write sensor serial
        Utils.WriteInt32(s[13..], (int)SensorSerial, false);
        // 단위 기록
        // write unit
        bytes[17] = (byte)Unit;
        // 캘리브레이션 타입 기록
        // write calibration type
        bytes[18] = (byte)CalType;

        // 캘리브레이션 데이터 직렬화 (리틀엔디안)
        // serialize calibration data (little-endian)
        switch (Body) {
            case Body.Separated:
                // 분리형: int32 (4바이트) 단위로 기록
                // separated: write as int32 (4 bytes each)
                Utils.WriteInt32(s[19..], Offset, false);
                // 양방향 값 기록
                // write positive values
                for (var i = 0; i < 5; i++)
                    // 계산된 오프셋에 양방향 값 기록
                    // write positive value at calculated offset
                    Utils.WriteInt32(s[(23 + i * 4)..], Positives[i], false);
                // 음방향 값 기록
                // write negative values
                for (var i = 0; i < 5; i++)
                    // 계산된 오프셋에 음방향 값 기록
                    // write negative value at calculated offset
                    Utils.WriteInt32(s[(43 + i * 4)..], Negatives[i], false);
                // switch 종료
                // exit switch
                break;

            case Body.Integrated:
            default:
                // 일체형: ushort (2바이트) 단위로 기록
                // integrated: write as ushort (2 bytes each)
                Utils.WriteUInt16(s[19..], (ushort)Offset, false);
                // 양방향 값 기록
                // write positive values
                for (var i = 0; i < 5; i++)
                    // 계산된 오프셋에 양방향 값 기록
                    // write positive value at calculated offset
                    Utils.WriteUInt16(s[(21 + i * 2)..], (ushort)Positives[i], false);
                // 음방향 값 기록
                // write negative values
                for (var i = 0; i < 5; i++)
                    // 계산된 오프셋에 음방향 값 기록
                    // write negative value at calculated offset
                    Utils.WriteUInt16(s[(31 + i * 2)..], (ushort)Negatives[i], false);
                // switch 종료
                // exit switch
                break;
        }

        // 직렬화된 바이트 반환
        // return serialized bytes
        return bytes;
    }

	/// <summary>
	///     원시 데이터에서 캘리브레이션 데이터를 파싱합니다.
	///     attempts to parse calibration data from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, out CalibrationData result) {
        // 데이터 크기 확인
        // check data size
        if (data.Length is not 41 and not 63) {
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
            result = new CalibrationData(data);
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