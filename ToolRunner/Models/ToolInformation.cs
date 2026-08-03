using HTool.Core.Util;

namespace ToolRunner.Models;

/// <summary>
///     the driver-side fields of the controller Information block, read with FC 0x04 (read input
///     registers) starting at relative address 1 — documented as 30002. only the fields shown in the
///     tool panel are parsed: driver identity, controller firmware version and MAC address; the
///     controller model / serial, production date, advance type, event revision and manufacturer
///     registers in between are skipped.
///     장치 정보 블록의 표시 대상 필드 (FC 0x04, 상대 주소 1부터 54 레지스터)
/// </summary>
public readonly struct ToolInformation {
    /// <summary>
    ///     first register of the block — relative address 1, documented as 30002 (Driver ID).
    ///     블록 시작 주소 (30002 = 상대 주소 1)
    /// </summary>
    public const ushort StartAddress = 1;

    /// <summary>
    ///     register count covering Driver ID through the MAC address (30002–30055).
    ///     읽을 레지스터 개수 (MAC 주소까지)
    /// </summary>
    public const ushort RegisterCount = 54;

    /// <summary>
    ///     register data size in bytes — two bytes per register, so 108 bytes.
    ///     레지스터 데이터 크기 (레지스터당 2바이트)
    /// </summary>
    public static int Size => RegisterCount * 2;

    /// <summary>
    ///     parses the Information block from an FC 0x04 response payload.
    ///     FC 0x04 응답 페이로드 파싱
    /// </summary>
    /// <param name="data">response payload, big-endian, register data starting at 30002</param>
    /// <exception cref="FormatException">when the payload is shorter than <see cref="Size" /></exception>
    public ToolInformation(ReadOnlySpan<byte> data) {
        // ensure the payload covers the whole block — a partial block cannot be parsed
        if (data.Length < Size)
            // reject the short payload
            throw new FormatException($"Data length {data.Length} is less than required {Size} bytes.");

        // HTool strips the MBAP header and function code but leaves the MODBUS byte-count byte in
        // front of the register data, so line the read position up with the last register rather
        // than assuming a data-only payload (verified on hardware: the leading byte-count made
        // every field read one byte early — Driver ID 0x7000, firmware 0.1024.0)
        // HTool 페이로드에는 byte-count 바이트가 남아 있어 레지스터 데이터 시작 위치를 보정한다
        var pos = data.Length - Size;

        // driver ID (30002, 1–15)
        DriverId = BinarySpanReader.ReadUInt16(data, ref pos);
        // driver model number (30003, 0–999)
        DriverModelNumber = BinarySpanReader.ReadUInt16(data, ref pos);
        // driver model name (30004–30019, 32 bytes ASCII)
        DriverModelName = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // driver serial number (30020–30024, 10 bytes ASCII)
        DriverSerialNumber = BinarySpanReader.ReadAsciiString(data, ref pos, 10);

        // skip the controller identity (30025–30046): model number, model name and serial number
        pos += 44;

        // controller firmware version major (30047)
        FirmwareMajor = BinarySpanReader.ReadUInt16(data, ref pos);
        // controller firmware version minor (30048)
        FirmwareMinor = BinarySpanReader.ReadUInt16(data, ref pos);
        // controller firmware version patch (30049)
        FirmwarePatch = BinarySpanReader.ReadUInt16(data, ref pos);

        // skip the production date (30050–30051) and advance type (30052)
        pos += 6;

        // MAC address (30053–30055, 6 raw bytes)
        MacAddress = data.Slice(pos, 6).ToArray();
    }

    /// <summary>
    ///     attempts to parse the Information block, returning false instead of throwing on a short
    ///     or malformed payload.
    ///     장치 정보 블록 파싱 시도
    /// </summary>
    /// <param name="data">response payload</param>
    /// <param name="result">parsed block, or default when parsing failed</param>
    /// <returns>true when the block was parsed</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, out ToolInformation result) {
        // reject a payload that cannot hold the block
        if (data.Length < Size) {
            // hand back the default value
            result = default;
            // report the failure
            return false;
        }

        // guard: a payload that passes the size check can still be malformed
        try {
            // parse the block
            result = new ToolInformation(data);
            // report success
            return true;
        } catch (Exception) {
            // malformed payload — hand back the default value
            result = default;
            // report the failure
            return false;
        }
    }

    /// <summary>
    ///     driver ID (30002, 1–15).
    ///     드라이버 ID
    /// </summary>
    public ushort DriverId { get; }

    /// <summary>
    ///     driver model number (30003).
    ///     드라이버 모델 번호
    /// </summary>
    public ushort DriverModelNumber { get; }

    /// <summary>
    ///     driver model name (30004–30019).
    ///     드라이버 모델명
    /// </summary>
    public string DriverModelName { get; }

    /// <summary>
    ///     driver serial number (30020–30024).
    ///     드라이버 시리얼 번호
    /// </summary>
    public string DriverSerialNumber { get; }

    /// <summary>
    ///     controller firmware version major (30047).
    ///     펌웨어 주 버전
    /// </summary>
    public ushort FirmwareMajor { get; }

    /// <summary>
    ///     controller firmware version minor (30048).
    ///     펌웨어 부 버전
    /// </summary>
    public ushort FirmwareMinor { get; }

    /// <summary>
    ///     controller firmware version patch (30049).
    ///     펌웨어 패치 버전
    /// </summary>
    public ushort FirmwarePatch { get; }

    /// <summary>
    ///     MAC address bytes (30053–30055). all zero means the controller reports no address.
    ///     MAC 주소 바이트 (전부 0이면 미보유)
    /// </summary>
    public byte[] MacAddress { get; }

    /// <summary>
    ///     driver identity for display — model name with the model number in parentheses, falling
    ///     back to the bare number when the controller reports no name.
    ///     드라이버 표시 문자열
    /// </summary>
    public string DriverText => string.IsNullOrEmpty(DriverModelName)
        ? DriverModelNumber.ToString()
        : $"{DriverModelName} ({DriverModelNumber})";

    /// <summary>
    ///     firmware version as Major.Minor.Patch.
    ///     펌웨어 버전 표시 문자열
    /// </summary>
    public string FirmwareText => $"{FirmwareMajor}.{FirmwareMinor}.{FirmwarePatch}";

    /// <summary>
    ///     MAC address as colon-separated hex, or a placeholder when the controller reports none.
    ///     MAC 주소 표시 문자열
    /// </summary>
    public string MacAddressText {
        get {
            // treat a missing or all-zero address as absent
            if (MacAddress is null || Array.TrueForAll(MacAddress, b => b is 0))
                // no address reported
                return "---";

            // join the bytes as upper-case hex pairs
            return string.Join(':', MacAddress.Select(b => b.ToString("X2")));
        }
    }
}
