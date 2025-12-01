using System.ComponentModel;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     HANTAS 장치의 상세 정보를 담는 클래스 (200바이트). Gen.2 장치에서 함수 코드 0x04로 읽어옵니다.
///     Class containing detailed HANTAS device information (200 bytes). Read from Gen.2 devices using function code 0x04.
/// </summary>
/// <remarks>
///     드라이버 모델, 컨트롤러 모델, 시리얼 번호, 펌웨어 버전 등의 상세 정보를 포함합니다. Gen.1 장치는 FormatSimpleInfo를 사용합니다.
///     Includes detailed info like driver model, controller model, serial number, firmware version. Gen.1 devices use
///     FormatSimpleInfo.
/// </remarks>
public sealed class FormatInfo {
    /// <summary>
    ///     기본 생성자
    ///     Default constructor
    /// </summary>
    public FormatInfo() {
        // 값 초기화
        // reset values
        SystemInfo             = 0;
        DriverId               = 0;
        DriverModelNumber      = 0;
        DriverModelName        = string.Empty;
        DriverSerialNumber     = string.Empty;
        ControllerModelNumber  = 0;
        ControllerModelName    = string.Empty;
        ControllerSerialNumber = string.Empty;
        FirmwareVersionMajor   = 0;
        FirmwareVersionMinor   = 0;
        FirmwareVersionPatch   = 0;
        ProductionDate         = 0;
        AdvanceType            = 0;
        MacAddress             = new byte[6];
        EventDataRevision      = 0;
        Manufacturer           = 0;
    }

    /// <summary>
    ///     생성자
    ///     Constructor
    /// </summary>
    /// <param name="values">원시 패킷 데이터 / raw packet data</param>
    public FormatInfo(byte[] values) {
        // Span 및 위치 초기화
        // initialize span and position
        var span = values.AsSpan();
        var pos  = 0;

        // 시스템 정보 읽기 (2바이트)
        // read system info (2 bytes)
        SystemInfo = BinarySpanReader.ReadUInt16(span, ref pos);

        // 드라이버 정보 읽기
        // read driver info
        DriverId           = BinarySpanReader.ReadUInt16(span, ref pos);
        DriverModelNumber  = BinarySpanReader.ReadUInt16(span, ref pos);
        DriverModelName    = BinarySpanReader.ReadAsciiString(span, ref pos, 32);
        DriverSerialNumber = BinarySpanReader.ReadAsciiString(span, ref pos, 10);

        // 컨트롤러 정보 읽기
        // read controller info
        ControllerModelNumber  = BinarySpanReader.ReadUInt16(span, ref pos);
        ControllerModelName    = BinarySpanReader.ReadAsciiString(span, ref pos, 32);
        ControllerSerialNumber = BinarySpanReader.ReadAsciiString(span, ref pos, 10);

        // 펌웨어 버전 읽기
        // read firmware version
        FirmwareVersionMajor = BinarySpanReader.ReadUInt16(span, ref pos);
        FirmwareVersionMinor = BinarySpanReader.ReadUInt16(span, ref pos);
        FirmwareVersionPatch = BinarySpanReader.ReadUInt16(span, ref pos);

        // 생산 정보 읽기
        // read production info
        ProductionDate = BinarySpanReader.ReadUInt32(span, ref pos);
        AdvanceType    = BinarySpanReader.ReadUInt16(span, ref pos);

        // MAC 주소 읽기
        // read MAC address
        MacAddress =  span.Slice(pos, 6).ToArray();
        pos        += 6;

        // 메타데이터 읽기
        // read metadata
        EventDataRevision = BinarySpanReader.ReadUInt16(span, ref pos);
        Manufacturer      = BinarySpanReader.ReadUInt16(span, ref pos);

        // 예약 영역 건너뛰기 (86바이트)
        // skip reserved area (86 bytes)
        pos += 86;

        // 체크섬 계산
        // calculate checksum
        CheckSum = Utils.CalculateCheckSumFast(span);
        // 모든 데이터를 읽었는지 확인
        // verify all data has been read
        if (pos != span.Length)
            // 예외 발생
            // throw exception
            throw new InvalidDataException($"Not all bytes have been consumed. " +
                                           $"{span.Length - pos} byte(s) remain");
    }

    /// <summary>
    ///     정보 데이터 크기 (바이트)
    ///     Information data size (bytes)
    /// </summary>
    public static int Size => 200;

    /// <summary>
    ///     시스템 정보 (예약됨)
    ///     System info (reserved)
    /// </summary>
    [Browsable(false)]
    public int SystemInfo { get; }

    /// <summary>
    ///     드라이버 ID
    ///     Driver ID
    /// </summary>
    public int DriverId { get; }

    /// <summary>
    ///     드라이버 모델 번호
    ///     Driver model number
    /// </summary>
    public int DriverModelNumber { get; }

    /// <summary>
    ///     드라이버 모델명
    ///     Driver model name
    /// </summary>
    public string DriverModelName { get; } = string.Empty;

    /// <summary>
    ///     드라이버 시리얼 번호
    ///     Driver serial number
    /// </summary>
    public string DriverSerialNumber { get; } = string.Empty;

    /// <summary>
    ///     컨트롤러 모델 번호
    ///     Controller model number
    /// </summary>
    public int ControllerModelNumber { get; }

    /// <summary>
    ///     컨트롤러 모델명
    ///     Controller model name
    /// </summary>
    public string ControllerModelName { get; } = string.Empty;

    /// <summary>
    ///     컨트롤러 시리얼 번호
    ///     Controller serial number
    /// </summary>
    public string ControllerSerialNumber { get; } = string.Empty;

    /// <summary>
    ///     펌웨어 버전 주 번호
    ///     Firmware version major
    /// </summary>
    public int FirmwareVersionMajor { get; }

    /// <summary>
    ///     펌웨어 버전 부 번호
    ///     Firmware version minor
    /// </summary>
    public int FirmwareVersionMinor { get; }

    /// <summary>
    ///     펌웨어 버전 패치 번호
    ///     Firmware version patch
    /// </summary>
    public int FirmwareVersionPatch { get; }

    /// <summary>
    ///     펌웨어 버전 문자열
    ///     Firmware version string
    /// </summary>
    public string FirmwareVersion => $"{FirmwareVersionMajor}.{FirmwareVersionMinor}.{FirmwareVersionPatch}";

    /// <summary>
    ///     생산 일자 (YYYYMMDD)
    ///     Production date (YYYYMMDD)
    /// </summary>
    public uint ProductionDate { get; }

    /// <summary>
    ///     고급 타입 (0=일반, 1=플러스)
    ///     Advance type (0=Normal, 1=Plus)
    /// </summary>
    public int AdvanceType { get; }

    /// <summary>
    ///     MAC 주소 바이트 배열
    ///     MAC address bytes
    /// </summary>
    public byte[] MacAddress { get; } = new byte[6];

    /// <summary>
    ///     MAC 주소 문자열 (xx:xx:xx:xx:xx:xx)
    ///     MAC address string (xx:xx:xx:xx:xx:xx)
    /// </summary>
    public string MacAddressString => string.Join(":", MacAddress.Select(b => b.ToString("X2")));

    /// <summary>
    ///     이벤트 데이터 리비전
    ///     Event data revision
    /// </summary>
    public int EventDataRevision { get; }

    /// <summary>
    ///     제조사 (1=Hantas, 2=Mountz)
    ///     Manufacturer (1=Hantas, 2=Mountz)
    /// </summary>
    public int Manufacturer { get; }

    /// <summary>
    ///     체크섬 값
    ///     Checksum value
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; }
}