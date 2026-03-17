using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     로그 설정 데이터 클래스 (리비전별 27/56/63바이트).
///     log setting data class (27/56/63 bytes per revision).
/// </summary>
/// <remarks>
///     저장소, 데이터 필드, 그래프, 샘플 시간 등 로깅 설정을 포함합니다. UI에서 수정 후 GetValues로 직렬화합니다.
///     contains logging settings such as storage, data fields, graph, sample time. modified in UI and serialized via
///     GetValues.
/// </remarks>
public sealed class Log {
    /// <summary>
    ///     기본 생성자. 데이터 필드 배열과 문자열 속성을 초기화합니다.
    ///     default constructor. initializes data field array and string properties.
    /// </summary>
    public Log() {
        // 데이터 필드 배열 초기화
        // initialize data field array
        DataField = [];
        // USB 라벨 초기화
        // initialize USB label
        UsbLabel = string.Empty;
    }

    /// <summary>
    ///     원시 패킷 데이터에서 로그 설정을 파싱합니다.
    ///     parses log setting from raw packet data.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
    public Log(ReadOnlySpan<byte> data, int revision = 0) : this() {
        // 리비전 범위 보정
        // clamp revision to valid range
        if (revision < 0 || revision >= Sizes.Length)
            // 리비전 기본 값 설정
            // reset the revision
            revision = 0;

        // 데이터 크기 확인
        // ensure data length meets minimum requirement
        if (data.Length < Sizes[revision])
            // 데이터 부족 예외 발생
            // throw format exception for insufficient data
            throw new FormatException($"Data length {data.Length} is less than required {Sizes[revision]} bytes.");

        // 읽기 위치 초기화
        // initialize read position
        var pos = 0;

        // 저장소 타입 읽기
        // read storage type
        Storage = BinarySpanReader.ReadByte(data, ref pos);

        // 데이터 필드 배열 생성
        // create data field array
        DataField = new int[DataFieldCounts[revision]];

        // Rev.0 데이터 필드 읽기 (22개)
        // read Rev.0 data fields (22 entries)
        for (var i = 0; i < DataFieldCounts[0]; i++)
            // 데이터 필드 값 읽기
            // read data field value
            DataField[i] = BinarySpanReader.ReadByte(data, ref pos);

        // 그래프 설정 읽기
        // read graph setting
        Graph = BinarySpanReader.ReadByte(data, ref pos);
        // 채널 1 타입 읽기
        // read channel 1 type
        TypeOfChannel1 = BinarySpanReader.ReadByte(data, ref pos);
        // 채널 2 타입 읽기
        // read channel 2 type
        TypeOfChannel2 = BinarySpanReader.ReadByte(data, ref pos);
        // 샘플 시간 읽기
        // read sample time
        SampleTime = BinarySpanReader.ReadByte(data, ref pos);

        // 리비전 1 미만이면 종료
        // return if below revision 1
        if (revision < 1) {
            // 해시 계산
            // compute hash
            Hash = DataHash.Compute(data[..Sizes[revision]]);
            // 생성자 종료
            // exit constructor
            return;
        }

        // Rev.1 추가 데이터 필드 읽기 (22~35)
        // read Rev.1 additional data fields (22~35)
        for (var i = DataFieldCounts[0]; i < DataFieldCounts[1]; i++)
            // 데이터 필드 값 읽기
            // read data field value
            DataField[i] = BinarySpanReader.ReadByte(data, ref pos);

        // USB 라벨 읽기 (16바이트 ASCII)
        // read USB label (16 bytes ASCII)
        UsbLabel = BinarySpanReader.ReadAsciiString(data, ref pos, 16);

        // 리비전 2 미만이면 종료
        // return if below revision 2
        if (revision < 2) {
            // 해시 계산
            // compute hash
            Hash = DataHash.Compute(data[..Sizes[revision]]);
            // 생성자 종료
            // exit constructor
            return;
        }

        // Job 로깅 단위 읽기
        // read job logging unit
        JobLoggingUnit = BinarySpanReader.ReadByte(data, ref pos);
        // ID 1 포함 여부 읽기
        // read with ID 1 flag
        WithId1 = BinarySpanReader.ReadByte(data, ref pos);
        // ID 2 포함 여부 읽기
        // read with ID 2 flag
        WithId2 = BinarySpanReader.ReadByte(data, ref pos);
        // ID 3 포함 여부 읽기
        // read with ID 3 flag
        WithId3 = BinarySpanReader.ReadByte(data, ref pos);
        // ID 4 포함 여부 읽기
        // read with ID 4 flag
        WithId4 = BinarySpanReader.ReadByte(data, ref pos);
        // ID 5 포함 여부 읽기
        // read with ID 5 flag
        WithId5 = BinarySpanReader.ReadByte(data, ref pos);
        // ID 6 포함 여부 읽기
        // read with ID 6 flag
        WithId6 = BinarySpanReader.ReadByte(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

    /// <summary>
    ///     리비전별 데이터 크기 배열 (바이트)
    ///     data size array per revision (bytes)
    /// </summary>
    public static ReadOnlySpan<int> Sizes => [27, 56, 63];

    /// <summary>
    ///     리비전별 데이터 필드 수 배열
    ///     data field count array per revision
    /// </summary>
    public static ReadOnlySpan<int> DataFieldCounts => [22, 35, 35];

    #region Rev.1

    /// <summary>
    ///     USB 라벨
    ///     USB label
    /// </summary>
    public string UsbLabel { get; set; }

    #endregion

    /// <summary>
    ///     데이터 해시 (변경 감지용)
    ///     data hash (for change detection)
    /// </summary>
    public ulong Hash { get; set; }

    /// <summary>
    ///     지정한 리비전의 데이터 크기를 반환합니다.
    ///     returns data size for the specified revision.
    /// </summary>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>데이터 크기 (바이트) / data size (bytes)</returns>
    public static int SizeOf(int revision) {
        // 지정 리비전의 크기 반환
        // return size for given revision
        return Sizes[revision];
    }

    /// <summary>
    ///     원시 데이터에서 로그 설정을 파싱합니다.
    ///     attempts to parse log setting from raw data.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Log? result) {
        // 리비전 범위 보정
        // clamp revision to valid range
        if (revision < 0 || revision >= Sizes.Length)
            // 리비전 기본 값 설정
            // reset the revision
            revision = 0;

        // 데이터 크기 확인
        // check data size
        if (data.Length < Sizes[revision]) {
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }

        // guard: catch any parsing errors from malformed data
        try {
            // 데이터 파싱
            // parse data
            result = new Log(data, revision);
            // 파싱 성공 반환
            // indicate parsing success
            return true;
        } catch (Exception) {
            // malformed data that passed size check but failed parsing
            // 기본값 설정
            // set null result
            result = null;
            // 파싱 실패 반환
            // indicate parsing failure
            return false;
        }
    }

    /// <summary>
    ///     설정 값을 바이트 배열로 직렬화합니다.
    ///     serializes setting values to a byte array.
    /// </summary>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
    public byte[] GetValues(int revision = 0) {
        // 결과 목록 초기화
        // initialize result list
        var values = new List<byte>();

        // 저장소 타입 추가
        // add storage type
        values.Add(Convert.ToByte(Storage));

        // Rev.0 데이터 필드 추가 (22개)
        // add Rev.0 data fields (22 entries)
        for (var i = 0; i < DataFieldCounts[0]; i++)
            // 데이터 필드 값 추가
            // add data field value
            values.Add(Convert.ToByte(DataField[i]));

        // 그래프 설정 추가
        // add graph setting
        values.Add(Convert.ToByte(Graph));
        // 채널 1 타입 추가
        // add channel 1 type
        values.Add(Convert.ToByte(TypeOfChannel1));
        // 채널 2 타입 추가
        // add channel 2 type
        values.Add(Convert.ToByte(TypeOfChannel2));
        // 샘플 시간 추가
        // add sample time
        values.Add(Convert.ToByte(SampleTime));

        // 리비전 1 미만이면 반환
        // return if below revision 1
        if (revision < 1)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // Rev.1 추가 데이터 필드 추가 (22~35)
        // add Rev.1 additional data fields (22~35)
        for (var i = DataFieldCounts[0]; i < DataFieldCounts[1]; i++)
            // 데이터 필드 값 추가
            // add data field value
            values.Add(Convert.ToByte(DataField[i]));

        // USB 라벨 직렬화 (16바이트 고정)
        // serialize USB label (16 bytes fixed)
        var label = Encoding.ASCII.GetBytes(UsbLabel);
        // USB 라벨 데이터 추가
        // add USB label data
        values.AddRange(label);
        // USB 라벨 패딩 추가
        // add USB label padding
        values.AddRange(new byte[16 - label.Length]);

        // 리비전 2 미만이면 반환
        // return if below revision 2
        if (revision < 2)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // Job 로깅 단위 추가
        // add job logging unit
        values.Add(Convert.ToByte(JobLoggingUnit));
        // ID 1 포함 여부 추가
        // add with ID 1 flag
        values.Add(Convert.ToByte(WithId1));
        // ID 2 포함 여부 추가
        // add with ID 2 flag
        values.Add(Convert.ToByte(WithId2));
        // ID 3 포함 여부 추가
        // add with ID 3 flag
        values.Add(Convert.ToByte(WithId3));
        // ID 4 포함 여부 추가
        // add with ID 4 flag
        values.Add(Convert.ToByte(WithId4));
        // ID 5 포함 여부 추가
        // add with ID 5 flag
        values.Add(Convert.ToByte(WithId5));
        // ID 6 포함 여부 추가
        // add with ID 6 flag
        values.Add(Convert.ToByte(WithId6));

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }

    #region Rev.0

    /// <summary>
    ///     저장소 타입
    ///     storage type
    /// </summary>
    public int Storage { get; set; }

    /// <summary>
    ///     데이터 필드 배열
    ///     data field array
    /// </summary>
    public int[] DataField { get; set; }

    /// <summary>
    ///     그래프 설정
    ///     graph setting
    /// </summary>
    public int Graph { get; set; }

    /// <summary>
    ///     채널 1 타입
    ///     type of channel 1
    /// </summary>
    public int TypeOfChannel1 { get; set; }

    /// <summary>
    ///     채널 2 타입
    ///     type of channel 2
    /// </summary>
    public int TypeOfChannel2 { get; set; }

    /// <summary>
    ///     샘플링 시간
    ///     sampling time
    /// </summary>
    public int SampleTime { get; set; }

    #endregion

    #region Rev.2

    /// <summary>
    ///     Job 로깅 단위
    ///     job logging unit
    /// </summary>
    public int JobLoggingUnit { get; set; }

    /// <summary>
    ///     ID 1 포함 여부
    ///     with ID 1
    /// </summary>
    public int WithId1 { get; set; }

    /// <summary>
    ///     ID 2 포함 여부
    ///     with ID 2
    /// </summary>
    public int WithId2 { get; set; }

    /// <summary>
    ///     ID 3 포함 여부
    ///     with ID 3
    /// </summary>
    public int WithId3 { get; set; }

    /// <summary>
    ///     ID 4 포함 여부
    ///     with ID 4
    /// </summary>
    public int WithId4 { get; set; }

    /// <summary>
    ///     ID 5 포함 여부
    ///     with ID 5
    /// </summary>
    public int WithId5 { get; set; }

    /// <summary>
    ///     ID 6 포함 여부
    ///     with ID 6
    /// </summary>
    public int WithId6 { get; set; }

    #endregion
}