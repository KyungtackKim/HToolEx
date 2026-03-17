using System.ComponentModel;
using HTool.Type;
using HTool.Util;

namespace HTool.Format;

/// <summary>
///     HANTAS 장치의 기본 정보를 담는 클래스 (13바이트). 모든 세대 장치에서 함수 코드 0x11로 읽어옵니다.
///     Class containing basic HANTAS device information (13 bytes). Read from all generation devices using function code
///     0x11.
/// </summary>
/// <remarks>
///     ID, 컨트롤러, 드라이버, 펌웨어 버전, 시리얼 번호, 모델 등의 기본 정보를 포함합니다. 연결 시 자동으로 읽어와 장치 세대를 판별합니다.
///     Includes basic info like ID, controller, driver, firmware version, serial number, model. Auto-read on connection to
///     determine device generation.
/// </remarks>
public sealed class FormatSimpleInfo {
    /// <summary>
    ///     기본 생성자
    ///     Default constructor
    /// </summary>
    public FormatSimpleInfo() {
        // 값 초기화
        // reset values
        Id         = 0;
        Controller = 0;
        Driver     = 0;
        Firmware   = 0;
        Serial     = "0000000000";
        Model      = 0;
    }

    /// <summary>
    ///     생성자
    ///     Constructor
    /// </summary>
    /// <param name="values">원시 패킷 데이터 / raw packet data</param>
    public FormatSimpleInfo(byte[] values) {
        // 데이터 크기 확인
        // check data size
        if (values.Length < Size)
            return;

        // Span 및 위치 초기화
        // initialize span and position
        var span = values.AsSpan();
        var pos  = 0;

        // 기본 값 읽기
        // read basic values
        Id         = BinarySpanReader.ReadUInt16(span, ref pos);
        Controller = BinarySpanReader.ReadUInt16(span, ref pos);
        Driver     = BinarySpanReader.ReadUInt16(span, ref pos);
        Firmware   = BinarySpanReader.ReadUInt16(span, ref pos);
        // 시리얼 원시 데이터 읽기
        // read serial raw data
        var s = span.Slice(pos, 5);
        pos += 5;
        // 시리얼 번호 조합
        // compose serial number
        Serial = $"{s[^1]:D2}{s[^2]:D2}{s[^3]:D2}{s[^4]:D2}{s[^5]:D2}";
        // 길이 확인 (255255xx255255 패턴)
        // check length (255255xx255255 pattern)
        if (Serial.Length == 14)
            // 기본 모델 코드 설정
            // set default model code
            Serial = $"0000{s[^3]:D2}0000";
        // 위치 확인 후 사용 횟수 읽기
        // check position and read used count
        if (pos <= span.Length - sizeof(uint))
            // 사용 횟수 설정
            // set used count
            Used = BinarySpanReader.ReadUInt32(span, ref pos);
        // 정의된 모델 타입 확인
        // check if model type is defined
        if (Enum.IsDefined(typeof(ModelTypes), Convert.ToInt32(Serial[4..6])))
            // 모델 타입 설정
            // set model type
            Model = (ModelTypes)Convert.ToInt32(Serial[4..6]);
        else
            // 기본 모델 타입 설정
            // set default model type
            Model = ModelTypes.Ad;
        // AD 모델 타입 확인
        // check AD model type
        if (Model == ModelTypes.Ad)
            // 더미 데이터 건너뛰기
            // skip dummy data
            pos += 1;

        // 체크섬 계산
        // calculate checksum
        CheckSum = values.Sum(v => v);
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
    public static int Size => 13;

    /// <summary>
    ///     장치 ID
    ///     Device ID
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     컨트롤러 모델 번호
    ///     Controller model number
    /// </summary>
    public int Controller { get; }

    /// <summary>
    ///     드라이버 모델 번호
    ///     Driver model number
    /// </summary>
    public int Driver { get; }

    /// <summary>
    ///     펌웨어 버전
    ///     Firmware version
    /// </summary>
    public int Firmware { get; }

    /// <summary>
    ///     시리얼 번호
    ///     Serial number
    /// </summary>
    public string Serial { get; } = string.Empty;

    /// <summary>
    ///     사용 횟수
    ///     Used count
    /// </summary>
    public uint Used { get; }

    /// <summary>
    ///     모델 타입
    ///     Model type
    /// </summary>
    public ModelTypes Model { get; }

    /// <summary>
    ///     체크섬 값
    ///     Checksum value
    /// </summary>
    [Browsable(false)]
    public int CheckSum { get; }
}