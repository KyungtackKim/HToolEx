using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     공유 서비스 설정 (리비전별 280/283바이트).
///     shared service setting (280/283 bytes per revision).
/// </summary>
/// <remarks>
///     FTP, 백업, MODBUS 프록시, TCP, 원격 잡, Open Protocol 등 네트워크 서비스 설정을 포함합니다.
///     includes network service settings such as FTP, backup, MODBUS proxy, TCP, remote job, and Open Protocol.
/// </remarks>
public sealed class Share {
	/// <summary>
	///     기본 생성자
	///     default constructor
	/// </summary>
	public Share() { }

	/// <summary>
	///     원시 패킷 데이터에서 공유 설정을 파싱합니다.
	///     parses share setting from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Share(ReadOnlySpan<byte> data, int revision = 0) {
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

        // FTP 서버 활성화 여부 읽기
        // read FTP server enable flag
        FtpServer = BinarySpanReader.ReadByte(data, ref pos);
        // FTP 사용자명 읽기 (128바이트 ASCII)
        // read FTP user name (128 bytes ASCII)
        FtpUserName = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // FTP 비밀번호 읽기 (128바이트 ASCII)
        // read FTP password (128 bytes ASCII)
        FtpPassword = BinarySpanReader.ReadAsciiString(data, ref pos, 128);

        // 백업 데이터 전달 활성화 여부 읽기
        // read backup data forward enable flag
        BackupData = BinarySpanReader.ReadByte(data, ref pos);
        // 백업 데이터 IP 주소 읽기 (4바이트 → 점 표기법)
        // read backup data IP address (4 bytes -> dotted notation)
        BackupDataIp = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                       $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                       $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                       $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // 백업 데이터 포트 번호 읽기
        // read backup data port number
        BackupDataPort = BinarySpanReader.ReadUInt16(data, ref pos);

        // MODBUS 프록시 서버 활성화 여부 읽기
        // read MODBUS proxy server enable flag
        ModbusProxy = BinarySpanReader.ReadByte(data, ref pos);
        // MODBUS 프록시 서버 포트 번호 읽기
        // read MODBUS proxy server port number
        ModbusProxyPort = BinarySpanReader.ReadUInt16(data, ref pos);

        // TCP 클라이언트 활성화 여부 읽기
        // read TCP client enable flag
        TcpClient = BinarySpanReader.ReadByte(data, ref pos);
        // TCP 클라이언트 IP 주소 읽기 (4바이트 → 점 표기법)
        // read TCP client IP address (4 bytes -> dotted notation)
        TcpClientIp = $"{BinarySpanReader.ReadByte(data, ref pos)}"  +
                      $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                      $".{BinarySpanReader.ReadByte(data, ref pos)}" +
                      $".{BinarySpanReader.ReadByte(data, ref pos)}";
        // TCP 클라이언트 포트 번호 읽기
        // read TCP client port number
        TcpClientPort = BinarySpanReader.ReadUInt16(data, ref pos);

        // TCP 서버 활성화 여부 읽기
        // read TCP server enable flag
        TcpServer = BinarySpanReader.ReadByte(data, ref pos);
        // TCP 서버 포트 번호 읽기
        // read TCP server port number
        TcpServerPort = BinarySpanReader.ReadUInt16(data, ref pos);

        // 원격 잡 제어 활성화 여부 읽기
        // read remote job control enable flag
        RemoteJob = BinarySpanReader.ReadByte(data, ref pos);
        // 원격 잡 제어 포트 번호 읽기
        // read remote job control port number
        RemoteJobPort = BinarySpanReader.ReadUInt16(data, ref pos);

        // 리비전 1 이상 데이터 파싱
        // parse revision 1+ data
        if (revision >= 1) {
            // Open Protocol 활성화 여부 읽기
            // read Open Protocol enable flag
            OpenProtocol = BinarySpanReader.ReadByte(data, ref pos);
            // Open Protocol 포트 번호 읽기
            // read Open Protocol port number
            OpenProtocolPort = BinarySpanReader.ReadUInt16(data, ref pos);
        }

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [280, 283];

	/// <summary>
	///     FTP 서버 활성화 여부
	///     FTP server enable flag
	/// </summary>
	public int FtpServer { get; set; }

	/// <summary>
	///     FTP 서버 사용자명
	///     FTP server user name
	/// </summary>
	public string FtpUserName { get; set; } = string.Empty;

	/// <summary>
	///     FTP 서버 비밀번호
	///     FTP server password
	/// </summary>
	public string FtpPassword { get; set; } = string.Empty;

	/// <summary>
	///     백업 데이터 전달 활성화 여부
	///     backup data forward enable flag
	/// </summary>
	public int BackupData { get; set; }

	/// <summary>
	///     백업 데이터 전달 IP 주소
	///     backup data forward IP address
	/// </summary>
	public string BackupDataIp { get; set; } = "0.0.0.0";

	/// <summary>
	///     백업 데이터 전달 포트 번호
	///     backup data forward port number
	/// </summary>
	public int BackupDataPort { get; set; }

	/// <summary>
	///     MODBUS 프록시 서버 활성화 여부
	///     MODBUS proxy server enable flag
	/// </summary>
	public int ModbusProxy { get; set; }

	/// <summary>
	///     MODBUS 프록시 서버 포트 번호
	///     MODBUS proxy server port number
	/// </summary>
	public int ModbusProxyPort { get; set; }

	/// <summary>
	///     Remote-Pro X TCP 클라이언트 활성화 여부
	///     Remote-Pro X TCP client enable flag
	/// </summary>
	public int TcpClient { get; set; }

	/// <summary>
	///     Remote-Pro X TCP 클라이언트 IP 주소
	///     Remote-Pro X TCP client IP address
	/// </summary>
	public string TcpClientIp { get; set; } = "0.0.0.0";

	/// <summary>
	///     Remote-Pro X TCP 클라이언트 포트 번호
	///     Remote-Pro X TCP client port number
	/// </summary>
	public int TcpClientPort { get; set; }

	/// <summary>
	///     Remote-Pro X TCP 서버 활성화 여부
	///     Remote-Pro X TCP server enable flag
	/// </summary>
	public int TcpServer { get; set; }

	/// <summary>
	///     Remote-Pro X TCP 서버 포트 번호
	///     Remote-Pro X TCP server port number
	/// </summary>
	public int TcpServerPort { get; set; }

	/// <summary>
	///     원격 잡 제어 활성화 여부
	///     remote job control enable flag
	/// </summary>
	public int RemoteJob { get; set; }

	/// <summary>
	///     원격 잡 제어 포트 번호
	///     remote job control port number
	/// </summary>
	public int RemoteJobPort { get; set; }

	/// <summary>
	///     Open Protocol 활성화 여부 (Rev.1+)
	///     Open Protocol enable flag (Rev.1+)
	/// </summary>
	public int OpenProtocol { get; set; }

	/// <summary>
	///     Open Protocol 포트 번호 (Rev.1+)
	///     Open Protocol port number (Rev.1+)
	/// </summary>
	public int OpenProtocolPort { get; set; }

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
	///     원시 데이터에서 공유 설정을 파싱합니다.
	///     attempts to parse share setting from raw data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <param name="result">파싱 결과 / parsed result</param>
	/// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
	public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Share? result) {
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
            result = new Share(data, revision);
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
	///     serializes setting values to byte array.
	/// </summary>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <returns>직렬화된 바이트 배열 / serialized byte array</returns>
	public byte[] GetValues(int revision = 0) {
        // 결과 바이트 목록 생성
        // create result byte list
        var values = new List<byte>();

        // FTP 사용자명을 128바이트 고정 길이로 변환
        // convert FTP user name to 128-byte fixed length
        var user = Encoding.ASCII.GetBytes(FtpUserName);
        // FTP 비밀번호를 128바이트 고정 길이로 변환
        // convert FTP password to 128-byte fixed length
        var password = Encoding.ASCII.GetBytes(FtpPassword);

        // 백업 데이터 IP 주소를 바이트 배열로 변환
        // convert backup data IP address to byte array
        var backupIp = BackupDataIp.Split('.').Select(byte.Parse);
        // TCP 클라이언트 IP 주소를 바이트 배열로 변환
        // convert TCP client IP address to byte array
        var clientIp = TcpClientIp.Split('.').Select(byte.Parse);

        // FTP 서버 활성화 플래그 추가
        // add FTP server enable flag
        values.Add((byte)FtpServer);
        // FTP 사용자명 추가 (128바이트 패딩)
        // add FTP user name (128-byte padded)
        values.AddRange(user);
        // 사용자명 나머지 영역 제로 패딩
        // zero-pad remaining user name area
        values.AddRange(new byte[128 - user.Length]);
        // FTP 비밀번호 추가 (128바이트 패딩)
        // add FTP password (128-byte padded)
        values.AddRange(password);
        // 비밀번호 나머지 영역 제로 패딩
        // zero-pad remaining password area
        values.AddRange(new byte[128 - password.Length]);

        // 백업 데이터 활성화 플래그 추가
        // add backup data enable flag
        values.Add((byte)BackupData);
        // 백업 데이터 IP 주소 추가
        // add backup data IP address
        values.AddRange(backupIp);
        // 백업 데이터 포트 상위 바이트 추가 (빅엔디안)
        // add backup data port high byte (big-endian)
        values.Add((byte)((BackupDataPort >> 8) & 0xFF));
        // 백업 데이터 포트 하위 바이트 추가
        // add backup data port low byte
        values.Add((byte)(BackupDataPort & 0xFF));

        // MODBUS 프록시 활성화 플래그 추가
        // add MODBUS proxy enable flag
        values.Add((byte)ModbusProxy);
        // MODBUS 프록시 포트 상위 바이트 추가 (빅엔디안)
        // add MODBUS proxy port high byte (big-endian)
        values.Add((byte)((ModbusProxyPort >> 8) & 0xFF));
        // MODBUS 프록시 포트 하위 바이트 추가
        // add MODBUS proxy port low byte
        values.Add((byte)(ModbusProxyPort & 0xFF));

        // TCP 클라이언트 활성화 플래그 추가
        // add TCP client enable flag
        values.Add((byte)TcpClient);
        // TCP 클라이언트 IP 주소 추가
        // add TCP client IP address
        values.AddRange(clientIp);
        // TCP 클라이언트 포트 상위 바이트 추가 (빅엔디안)
        // add TCP client port high byte (big-endian)
        values.Add((byte)((TcpClientPort >> 8) & 0xFF));
        // TCP 클라이언트 포트 하위 바이트 추가
        // add TCP client port low byte
        values.Add((byte)(TcpClientPort & 0xFF));

        // TCP 서버 활성화 플래그 추가
        // add TCP server enable flag
        values.Add((byte)TcpServer);
        // TCP 서버 포트 상위 바이트 추가 (빅엔디안)
        // add TCP server port high byte (big-endian)
        values.Add((byte)((TcpServerPort >> 8) & 0xFF));
        // TCP 서버 포트 하위 바이트 추가
        // add TCP server port low byte
        values.Add((byte)(TcpServerPort & 0xFF));

        // 원격 잡 활성화 플래그 추가
        // add remote job enable flag
        values.Add((byte)RemoteJob);
        // 원격 잡 포트 상위 바이트 추가 (빅엔디안)
        // add remote job port high byte (big-endian)
        values.Add((byte)((RemoteJobPort >> 8) & 0xFF));
        // 원격 잡 포트 하위 바이트 추가
        // add remote job port low byte
        values.Add((byte)(RemoteJobPort & 0xFF));

        // 리비전 0이면 여기서 반환
        // return here if revision 0
        if (revision < 1)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // Open Protocol 활성화 플래그 추가
        // add Open Protocol enable flag
        values.Add((byte)OpenProtocol);
        // Open Protocol 포트 상위 바이트 추가 (빅엔디안)
        // add Open Protocol port high byte (big-endian)
        values.Add((byte)((OpenProtocolPort >> 8) & 0xFF));
        // Open Protocol 포트 하위 바이트 추가
        // add Open Protocol port low byte
        values.Add((byte)(OpenProtocolPort & 0xFF));

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }
}