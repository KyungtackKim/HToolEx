using System.Buffers.Binary;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HTool.Device.Protocol;
using HTool.Type;
using ToolSample.Models;
using ToolSample.Services;

namespace ToolSample.ViewModels;

/// <summary>
///     레지스터 R/W 페이지 ViewModel. MODBUS 레지스터 읽기/쓰기.
///     Register R/W page ViewModel. MODBUS register read/write operations.
/// </summary>
public sealed partial class RegisterViewModel(
    HToolService       htool,
    IDispatcherService dispatcher) : ObservableObject, IPageViewModel {
    // 시작 주소 (문자열 — hex/dec 입력 지원)
    // start address (string — supports hex/dec input)
    [ObservableProperty]
    private string _address = "0";

    // 읽기 레지스터 개수
    // read register count
    [ObservableProperty]
    private ushort _count = 10;

    // 마지막 요청의 시작 주소 (응답 파싱 시 사용)
    // last request start address (used for response parsing)
    private ushort _lastRequestAddress;

    // 선택된 함수 코드
    // selected function code
    [ObservableProperty]
    private FunctionCode _selectedFunctionCode = FunctionCode.ReadHoldingReg;

    // 문자열 고정 바이트 길이 (0 = 자동)
    // string fixed byte length (0 = auto)
    [ObservableProperty]
    private int _stringLength;

    // 문자열 쓰기 값 (FC 0x10, WriteStrReg)
    // string write value (FC 0x10, WriteStrReg)
    [ObservableProperty]
    private string _writeString = "";

    // 단일 쓰기 값 (FC 0x06)
    // single write value (FC 0x06)
    [ObservableProperty]
    private string _writeValue = "0";

    // 다중 쓰기 값 (FC 0x10, 쉼표 구분)
    // multi write values (FC 0x10, comma separated)
    [ObservableProperty]
    private string _writeValues = "";

    /// <summary>
    ///     지원하는 함수 코드 목록.
    ///     Supported function code list.
    /// </summary>
    public FunctionCode[] FunctionCodes => [
        FunctionCode.ReadHoldingReg, FunctionCode.ReadInputReg, FunctionCode.WriteSingleReg, FunctionCode.WriteMultiReg
    ];

    /// <summary>
    ///     레지스터 읽기 결과 목록.
    ///     Register read result list.
    /// </summary>
    public ObservableCollection<RegisterResult> Results { get; } = [];

    /// <inheritdoc />
    public void Activate() {
        // 데이터 수신 이벤트 구독
        // subscribe to data received event
        htool.DataReceived += OnDataReceived;
    }

    /// <inheritdoc />
    public void Deactivate() {
        // 데이터 수신 이벤트 구독 해제
        // unsubscribe from data received event
        htool.DataReceived -= OnDataReceived;
    }

    /// <summary>
    ///     레지스터 명령을 실행한다.
    ///     Executes register command.
    /// </summary>
    [RelayCommand]
    private void Execute() {
        // 주소 파싱 (hex 접두사 지원)
        // parse address (supports hex prefix)
        var addr = ParseAddress(Address);
        // 마지막 요청 주소 저장
        // store last request address
        _lastRequestAddress = addr;

        // 함수 코드에 따라 명령 분기
        // branch command based on function code
        switch (SelectedFunctionCode) {
            // 보유 레지스터 읽기
            // read holding registers
            case FunctionCode.ReadHoldingReg:
                // 읽기 요청 전송
                // send read request
                htool.Tool.ReadHoldingReg(addr, Count);
                // 읽기 분기 종료
                // end read branch
                break;
            // 입력 레지스터 읽기
            // read input registers
            case FunctionCode.ReadInputReg:
                // 읽기 요청 전송
                // send read request
                htool.Tool.ReadInputReg(addr, Count);
                // 읽기 분기 종료
                // end read branch
                break;
            // 단일 레지스터 쓰기
            // write single register
            case FunctionCode.WriteSingleReg:
                // 쓰기 값 파싱
                // parse write value
                var val = ParseUshort(WriteValue);
                // 쓰기 요청 전송
                // send write request
                htool.Tool.WriteSingleReg(addr, val);
                // 쓰기 분기 종료
                // end write branch
                break;
            // 다중 레지스터 쓰기
            // write multiple registers
            case FunctionCode.WriteMultiReg:
                // 문자열 쓰기 여부 확인
                // check if string write
                if (!string.IsNullOrWhiteSpace(WriteString)) {
                    // 문자열 쓰기 요청 전송
                    // send string write request
                    htool.Tool.WriteStrReg(addr, WriteString, StringLength);
                } else {
                    // 쉼표 구분 값 파싱
                    // parse comma separated values
                    var values = ParseMultiValues(WriteValues);
                    // 다중 쓰기 요청 전송
                    // send multi write request
                    htool.Tool.WriteMultiReg(addr, values);
                }

                // 다중 쓰기 분기 종료
                // end multi write branch
                break;
        }
    }

    /// <summary>
    ///     결과 목록을 초기화한다.
    ///     Clears result list.
    /// </summary>
    [RelayCommand]
    private void Clear() {
        // 결과 목록 초기화
        // clear result list
        Results.Clear();
    }

    /// <summary>
    ///     MODBUS 응답 수신 핸들러.
    ///     MODBUS response received handler.
    /// </summary>
    private void OnDataReceived(ModbusResponse response) {
        // 읽기 응답만 처리 (FC 0x03 / 0x04)
        // handle read responses only (FC 0x03 / 0x04)
        if (response.Code is not (FunctionCode.ReadHoldingReg or FunctionCode.ReadInputReg))
            // 읽기 응답이 아님 — 무시
            // not a read response — ignore
            return;

        // 페이로드를 배열로 복사 (Span은 람다에서 사용 불가)
        // copy payload to array (Span cannot be used in lambdas)
        var payload = response.Payload.ToArray();
        // 레지스터 개수 계산 (2바이트 단위)
        // calculate register count (2 bytes per register)
        var regCount = payload.Length / 2;
        // 시작 주소 결정
        // determine start address
        var baseAddr = (ushort)response.Address;

        // UI 스레드에서 결과 갱신
        // update results on UI thread
        dispatcher.Invoke(() => {
            // 결과 목록 초기화
            // clear result list
            Results.Clear();

            // 각 레지스터 파싱
            // parse each register
            for (var i = 0; i < regCount; i++) {
                // Big-Endian 16비트 값 읽기
                // read Big-Endian 16-bit value
                var value = BinaryPrimitives.ReadUInt16BigEndian(payload[(i * 2)..]);
                // ASCII 문자 변환 (상위/하위 바이트)
                // convert to ASCII characters (high/low bytes)
                var hi = (char)(value >> 8);
                // 하위 바이트 추출
                // extract low byte
                var lo = (char)(value & 0xFF);
                // 출력 가능 문자 필터링
                // filter printable characters
                var ascii = $"{(char.IsControl(hi) ? '.' : hi)}{(char.IsControl(lo) ? '.' : lo)}";

                // 결과 항목 추가
                // add result item
                Results.Add(new RegisterResult {
                    // 레지스터 주소 설정
                    // set register address
                    Address = (ushort)(baseAddr + i),
                    // 16진수 문자열 설정
                    // set hex string
                    HexValue = $"0x{value:X4}",
                    // 10진수 값 설정
                    // set decimal value
                    DecValue = value,
                    // ASCII 문자열 설정
                    // set ASCII string
                    AsciiValue = ascii
                });
            }
        });
    }

    /// <summary>
    ///     주소 문자열을 ushort로 파싱한다. 0x 접두사를 지원한다.
    ///     Parses address string to ushort. Supports 0x prefix.
    /// </summary>
    private static ushort ParseAddress(string text) {
        // 공백 제거
        // trim whitespace
        text = text.Trim();
        // hex 접두사 확인
        // check hex prefix
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            // hex 값 파싱 후 반환
            // parse hex value and return
            return Convert.ToUInt16(text[2..], 16);
        // 10진수 파싱 후 반환
        // parse decimal value and return
        return ushort.TryParse(text, out var val) ? val : (ushort)0;
    }

    /// <summary>
    ///     값 문자열을 ushort로 파싱한다.
    ///     Parses value string to ushort.
    /// </summary>
    private static ushort ParseUshort(string text) {
        // 공백 제거
        // trim whitespace
        text = text.Trim();
        // hex 접두사 확인
        // check hex prefix
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            // hex 값 파싱 후 반환
            // parse hex value and return
            return Convert.ToUInt16(text[2..], 16);
        // 10진수 파싱 후 반환
        // parse decimal value and return
        return ushort.TryParse(text, out var val) ? val : (ushort)0;
    }

    /// <summary>
    ///     쉼표 구분 문자열을 ushort 배열로 파싱한다.
    ///     Parses comma-separated string to ushort array.
    /// </summary>
    private static ushort[] ParseMultiValues(string text) {
        // 비어있으면 빈 배열 반환
        // return empty array if empty
        if (string.IsNullOrWhiteSpace(text))
            // 빈 배열 반환
            // return empty array
            return [];

        // 쉼표로 분할 후 파싱
        // split by comma and parse
        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // 파싱 결과 배열 생성
        // create parsed result array
        var result = new ushort[parts.Length];
        // 각 항목 파싱
        // parse each item
        for (var i = 0; i < parts.Length; i++)
            // 개별 값 파싱
            // parse individual value
            result[i] = ParseUshort(parts[i]);
        // 결과 배열 반환
        // return result array
        return result;
    }
}