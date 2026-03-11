using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     네트워크 설정 데이터 클래스 (리비전별 69/70바이트).
///     network setting data class (69/70 bytes per revision).
/// </summary>
/// <remarks>
///     Wi-Fi SSID, 비밀번호, 대역, 국가, 채널 등 네트워크 설정을 포함합니다. UI에서 수정 후 GetValues로 직렬화합니다.
///     contains network settings such as Wi-Fi SSID, password, band, country, channel. modified in UI and serialized via
///     GetValues.
/// </remarks>
public sealed class Network {
    /// <summary>
    ///     기본 생성자. 문자열 속성을 빈 문자열로 초기화합니다.
    ///     default constructor. initializes string properties to empty strings.
    /// </summary>
    public Network() {
        // SSID 초기화
        // initialize SSID
        Ssid = string.Empty;
        // 비밀번호 초기화
        // initialize password
        Password = string.Empty;
    }

    /// <summary>
    ///     원시 패킷 데이터에서 네트워크 설정을 파싱합니다.
    ///     parses network setting from raw packet data.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
    public Network(ReadOnlySpan<byte> data, int revision = 0) : this() {
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

        // SSID 읽기 (32바이트 ASCII)
        // read SSID (32 bytes ASCII)
        Ssid = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // 비밀번호 읽기 (32바이트 ASCII)
        // read password (32 bytes ASCII)
        Password = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // 5GHz 대역 사용 여부 읽기
        // read 5GHz band flag
        BandBy5G = BinarySpanReader.ReadByte(data, ref pos);
        // 국가 타입 읽기
        // read country type
        CountryType = BinarySpanReader.ReadByte(data, ref pos);
        // 수동 채널 선택 읽기
        // read manual channel selection
        ManualChannel = BinarySpanReader.ReadByte(data, ref pos);
        // 채널 번호 읽기 (빅엔디안 2바이트)
        // read channel number (big-endian 2 bytes)
        Channel = BinarySpanReader.ReadUInt16(data, ref pos);

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

        // DHCP 사용 여부 읽기
        // read DHCP usage flag
        UsedDhcp = BinarySpanReader.ReadByte(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

    /// <summary>
    ///     리비전별 데이터 크기 배열 (바이트)
    ///     data size array per revision (bytes)
    /// </summary>
    public static ReadOnlySpan<int> Sizes => [69, 70];

    #region Rev.1

    /// <summary>
    ///     DHCP 사용 여부
    ///     DHCP usage flag
    /// </summary>
    public int UsedDhcp { get; set; }

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
    ///     원시 데이터에서 네트워크 설정을 파싱합니다.
    ///     attempts to parse network setting from raw data.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Network? result) {
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
            result = new Network(data, revision);
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

        // SSID 직렬화 (32바이트 고정)
        // serialize SSID (32 bytes fixed)
        var ssid = Encoding.ASCII.GetBytes(Ssid);
        // 비밀번호 직렬화 (32바이트 고정)
        // serialize password (32 bytes fixed)
        var password = Encoding.ASCII.GetBytes(Password);

        // SSID 데이터 추가
        // add SSID data
        values.AddRange(ssid);
        // SSID 패딩 추가
        // add SSID padding
        values.AddRange(new byte[32 - ssid.Length]);
        // 비밀번호 데이터 추가
        // add password data
        values.AddRange(password);
        // 비밀번호 패딩 추가
        // add password padding
        values.AddRange(new byte[32 - password.Length]);
        // 5GHz 대역 추가
        // add 5GHz band flag
        values.Add(Convert.ToByte(BandBy5G));
        // 국가 타입 추가
        // add country type
        values.Add(Convert.ToByte(CountryType));
        // 수동 채널 선택 추가
        // add manual channel selection
        values.Add(Convert.ToByte(ManualChannel));
        // 채널 번호 추가 (빅엔디안 2바이트)
        // add channel number (big-endian 2 bytes)
        values.Add(Convert.ToByte((Channel >> 8) & 0xFF));
        values.Add(Convert.ToByte(Channel        & 0xFF));

        // 리비전 1 미만이면 반환
        // return if below revision 1
        if (revision < 1)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // DHCP 사용 여부 추가
        // add DHCP usage flag
        values.Add(Convert.ToByte(UsedDhcp));

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }

    #region Rev.0

    /// <summary>
    ///     AP SSID
    ///     AP SSID
    /// </summary>
    public string Ssid { get; set; }

    /// <summary>
    ///     AP 비밀번호
    ///     AP password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     5GHz 대역 사용 여부
    ///     5GHz band usage flag
    /// </summary>
    public int BandBy5G { get; set; }

    /// <summary>
    ///     국가 타입
    ///     country type
    /// </summary>
    public int CountryType { get; set; }

    /// <summary>
    ///     수동 채널 선택
    ///     manual channel selection
    /// </summary>
    public int ManualChannel { get; set; }

    /// <summary>
    ///     수동 채널 번호
    ///     manual channel number
    /// </summary>
    public int Channel { get; set; }

    #endregion
}