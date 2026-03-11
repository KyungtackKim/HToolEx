using System.Text;
using HTool.Core.Util;

namespace HTool.Format.Pro.Setting;

/// <summary>
///     운영 설정 데이터 클래스 (리비전별 178/220/221/504/506바이트).
///     operation setting data class (178/220/221/504/506 bytes per revision).
/// </summary>
/// <remarks>
///     Job 모드, 바코드, I/O 툴 등 운영 관련 설정을 포함합니다. UI에서 수정 후 GetValues로 직렬화합니다.
///     contains operation-related settings such as job mode, barcode, I/O tools. modified in UI and serialized via
///     GetValues.
/// </remarks>
public sealed class Operation {
	/// <summary>
	///     기본 생성자. 문자열 속성을 빈 문자열로 초기화합니다.
	///     default constructor. initializes string properties to empty strings.
	/// </summary>
	public Operation() {
        // Job 이름 초기화
        // initialize job name
        JobNameOnBoot = string.Empty;
        // 비트 소켓 툴 이름 초기화
        // initialize bit socket tool name
        BitSocketToolName = string.Empty;
        // I/O 툴 이름 초기화
        // initialize I/O tool name
        InOutToolName = string.Empty;
    }

	/// <summary>
	///     원시 패킷 데이터에서 운영 설정을 파싱합니다.
	///     parses operation setting from raw packet data.
	/// </summary>
	/// <param name="data">원시 패킷 데이터 / raw packet data</param>
	/// <param name="revision">리비전 번호 / revision number</param>
	/// <exception cref="FormatException">데이터 길이가 부족할 때 / when data length is insufficient</exception>
	public Operation(ReadOnlySpan<byte> data, int revision = 0) : this() {
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

        // Job 없이 동작 모드 읽기
        // read without-job mode
        WithoutJobMode = BinarySpanReader.ReadByte(data, ref pos);
        // 나사 카운트 방향 읽기
        // read screw count direction
        DownCountForScrew = BinarySpanReader.ReadByte(data, ref pos);
        // 나사 카운트 단위 읽기
        // read screw count unit
        JobCountForScrew = BinarySpanReader.ReadByte(data, ref pos);
        // 입력을 통한 Job 선택 방식 읽기
        // read direct job selection mode via input
        DirectJobSelectionMode = BinarySpanReader.ReadByte(data, ref pos);
        // 원격 바코드 모드 읽기
        // read remote barcode mode
        RemoteBarcodeMode = BinarySpanReader.ReadByte(data, ref pos);
        // Job 실행 중 바코드 스캔 읽기
        // read scan barcode while job running
        ScanBarcodeWhileRun = BinarySpanReader.ReadByte(data, ref pos);
        // 재체결 실패 시 나사 건너뛰기 읽기
        // read skip screw on re-tight fail
        SkipScrewForReTightFail = BinarySpanReader.ReadByte(data, ref pos);
        // 운영 모드 읽기
        // read operation mode
        OperationMode = BinarySpanReader.ReadByte(data, ref pos);
        // 부팅 시 Job 이름 읽기 (128바이트 ASCII)
        // read job name on boot (128 bytes ASCII)
        JobNameOnBoot = BinarySpanReader.ReadAsciiString(data, ref pos, 128);
        // 비밀번호 없이 건너뛰기 읽기
        // read skip without password
        SkipWithoutPassword = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 뒤로가기 읽기
        // read back without password
        BackWithoutPassword = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 리셋 읽기
        // read reset without password
        ResetWithoutPassword = BinarySpanReader.ReadByte(data, ref pos);
        // Job 리셋 버튼 읽기
        // read job reset button
        JobResetButton = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 Job 선택 읽기
        // read job selection without password
        JobSelectionWithoutPassword = BinarySpanReader.ReadByte(data, ref pos);
        // 완료 후 자동 재시작 읽기
        // read auto restart when finished
        AutoRestartWhenFinished = BinarySpanReader.ReadByte(data, ref pos);
        // 자동 데이터 백업 읽기
        // read auto data backup
        AutoDataBackup = BinarySpanReader.ReadByte(data, ref pos);
        // 비트 소켓 트레이 읽기
        // read bit socket tray
        BitSocketTray = BinarySpanReader.ReadByte(data, ref pos);
        // 비트 소켓 툴 이름 읽기 (32바이트 ASCII)
        // read bit socket tool name (32 bytes ASCII)
        BitSocketToolName = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // 부팅 시 이력 로드 읽기
        // read load history on boot
        LoadHistoryOnBoot = BinarySpanReader.ReadByte(data, ref pos);
        // Job 복구 읽기
        // read job recovery
        JobRecovery = BinarySpanReader.ReadByte(data, ref pos);

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

        // I/O 툴 활성화 읽기
        // read enable I/O tool
        EnableInOutTool = BinarySpanReader.ReadByte(data, ref pos);
        // I/O 툴 이름 읽기 (32바이트 ASCII)
        // read I/O tool name (32 bytes ASCII)
        InOutToolName = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // 체결 OK 포트 읽기
        // read fasten OK port
        FastenOkPort = BinarySpanReader.ReadByte(data, ref pos);
        // 체결 NG 포트 읽기
        // read fasten NG port
        FastenNgPort = BinarySpanReader.ReadByte(data, ref pos);
        // 프리셋 1 포트 읽기
        // read preset 1 port
        Preset1 = BinarySpanReader.ReadByte(data, ref pos);
        // 프리셋 2 포트 읽기
        // read preset 2 port
        Preset2 = BinarySpanReader.ReadByte(data, ref pos);
        // 프리셋 3 포트 읽기
        // read preset 3 port
        Preset3 = BinarySpanReader.ReadByte(data, ref pos);
        // 잠금 포트 읽기
        // read lock port
        Lock = BinarySpanReader.ReadByte(data, ref pos);
        // 자동 스텝 전진 읽기
        // read auto step forward
        AutoStepForward = BinarySpanReader.ReadByte(data, ref pos);
        // 스텝별 건너뛰기 읽기
        // read skip by step
        SkipByStep = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 재체결 읽기
        // read re-tight without password
        ReTightWithoutPassword = BinarySpanReader.ReadByte(data, ref pos);

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

        // 프리셋 4 포트 읽기
        // read preset 4 port
        Preset4 = BinarySpanReader.ReadByte(data, ref pos);

        // 리비전 3 미만이면 종료
        // return if below revision 3
        if (revision < 3) {
            // 해시 계산
            // compute hash
            Hash = DataHash.Compute(data[..Sizes[revision]]);
            // 생성자 종료
            // exit constructor
            return;
        }

        // I/O 툴 2 읽기
        // read I/O tool 2
        ReadInOutTool(data, ref pos, InOutTool2);
        // I/O 툴 3 읽기
        // read I/O tool 3
        ReadInOutTool(data, ref pos, InOutTool3);
        // I/O 툴 4 읽기
        // read I/O tool 4
        ReadInOutTool(data, ref pos, InOutTool4);
        // I/O 툴 5 읽기
        // read I/O tool 5
        ReadInOutTool(data, ref pos, InOutTool5);
        // I/O 툴 6 읽기
        // read I/O tool 6
        ReadInOutTool(data, ref pos, InOutTool6);
        // I/O 툴 7 읽기
        // read I/O tool 7
        ReadInOutTool(data, ref pos, InOutTool7);
        // I/O 툴 8 읽기
        // read I/O tool 8
        ReadInOutTool(data, ref pos, InOutTool8);

        // Job 중단 비밀번호 없이 읽기
        // read abort job without password
        AbortJobWithoutPass = BinarySpanReader.ReadByte(data, ref pos);
        // 스텝 NG 사유 코멘트 읽기
        // read comment the cause of step NG
        CommentTheCauseOfStepNg = BinarySpanReader.ReadByte(data, ref pos);
        // Job NG 사유 코멘트 읽기
        // read comment the cause of job NG
        CommentTheCauseOfJobNg = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 ID 1 편집 읽기
        // read edit ID 1 without password
        EditId1WithoutPass = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 ID 2 편집 읽기
        // read edit ID 2 without password
        EditId2WithoutPass = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 ID 3 편집 읽기
        // read edit ID 3 without password
        EditId3WithoutPass = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 ID 4 편집 읽기
        // read edit ID 4 without password
        EditId4WithoutPass = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 ID 5 편집 읽기
        // read edit ID 5 without password
        EditId5WithoutPass = BinarySpanReader.ReadByte(data, ref pos);
        // 비밀번호 없이 ID 6 편집 읽기
        // read edit ID 6 without password
        EditId6WithoutPass = BinarySpanReader.ReadByte(data, ref pos);
        // 모든 나사 위치 동시 표시 읽기
        // read all screw positions appear at once
        AllScrewPosAppearAtOnce = BinarySpanReader.ReadByte(data, ref pos);

        // 리비전 4 미만이면 종료
        // return if below revision 4
        if (revision < 4) {
            // 해시 계산
            // compute hash
            Hash = DataHash.Compute(data[..Sizes[revision]]);
            // 생성자 종료
            // exit constructor
            return;
        }

        // 비밀번호 없이 툴 알람 리셋 읽기
        // read reset tool alarm without password
        ResetToolAlarmWithoutPassword = BinarySpanReader.ReadByte(data, ref pos);
        // 사이드 패널 활성화 읽기
        // read enable side panel
        EnableSidePanel = BinarySpanReader.ReadByte(data, ref pos);

        // 해시 계산
        // compute hash
        Hash = DataHash.Compute(data[..Sizes[revision]]);
    }

	/// <summary>
	///     리비전별 데이터 크기 배열 (바이트)
	///     data size array per revision (bytes)
	/// </summary>
	public static ReadOnlySpan<int> Sizes => [178, 220, 221, 504, 506];

    #region Rev.2

    /// <summary>
    ///     출력: 프리셋 4 포트 번호 (I/O 툴)
    ///     output: preset 4 port number for I/O tool
    /// </summary>
    public int Preset4 { get; set; }

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
    ///     원시 데이터에서 운영 설정을 파싱합니다.
    ///     attempts to parse operation setting from raw data.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="revision">리비전 번호 / revision number</param>
    /// <param name="result">파싱 결과 / parsed result</param>
    /// <returns>파싱 성공 여부 / true if parsing succeeded</returns>
    public static bool TryParse(ReadOnlySpan<byte> data, int revision, out Operation? result) {
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
            result = new Operation(data, revision);
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

        // Job 이름 직렬화 (128바이트 고정)
        // serialize job name (128 bytes fixed)
        var jobName = Encoding.ASCII.GetBytes(JobNameOnBoot);
        // 비트 소켓 툴 이름 직렬화 (32바이트 고정)
        // serialize bit socket tool name (32 bytes fixed)
        var bstName = Encoding.ASCII.GetBytes(BitSocketToolName);

        // Job 없이 동작 모드 추가
        // add without-job mode
        values.Add(Convert.ToByte(WithoutJobMode));
        // 나사 카운트 방향 추가
        // add screw count direction
        values.Add(Convert.ToByte(DownCountForScrew));
        // 나사 카운트 단위 추가
        // add screw count unit
        values.Add(Convert.ToByte(JobCountForScrew));
        // 입력을 통한 Job 선택 방식 추가
        // add direct job selection mode
        values.Add(Convert.ToByte(DirectJobSelectionMode));
        // 원격 바코드 모드 추가
        // add remote barcode mode
        values.Add(Convert.ToByte(RemoteBarcodeMode));
        // Job 실행 중 바코드 스캔 추가
        // add scan barcode while run
        values.Add(Convert.ToByte(ScanBarcodeWhileRun));
        // 재체결 실패 시 나사 건너뛰기 추가
        // add skip screw on re-tight fail
        values.Add(Convert.ToByte(SkipScrewForReTightFail));
        // 운영 모드 추가
        // add operation mode
        values.Add(Convert.ToByte(OperationMode));
        // Job 이름 데이터 추가
        // add job name data
        values.AddRange(jobName);
        // Job 이름 패딩 추가
        // add job name padding
        values.AddRange(new byte[128 - jobName.Length]);
        // 비밀번호 없이 건너뛰기 추가
        // add skip without password
        values.Add(Convert.ToByte(SkipWithoutPassword));
        // 비밀번호 없이 뒤로가기 추가
        // add back without password
        values.Add(Convert.ToByte(BackWithoutPassword));
        // 비밀번호 없이 리셋 추가
        // add reset without password
        values.Add(Convert.ToByte(ResetWithoutPassword));
        // Job 리셋 버튼 추가
        // add job reset button
        values.Add(Convert.ToByte(JobResetButton));
        // 비밀번호 없이 Job 선택 추가
        // add job selection without password
        values.Add(Convert.ToByte(JobSelectionWithoutPassword));
        // 완료 시 자동 재시작 추가
        // add auto restart when finished
        values.Add(Convert.ToByte(AutoRestartWhenFinished));
        // 자동 데이터 백업 추가
        // add auto data backup
        values.Add(Convert.ToByte(AutoDataBackup));
        // 비트 소켓 트레이 추가
        // add bit socket tray
        values.Add(Convert.ToByte(BitSocketTray));
        // 비트 소켓 툴 이름 데이터 추가
        // add bit socket tool name data
        values.AddRange(bstName);
        // 비트 소켓 툴 이름 패딩 추가
        // add bit socket tool name padding
        values.AddRange(new byte[32 - bstName.Length]);
        // 부팅 시 이력 로드 추가
        // add load history on boot
        values.Add(Convert.ToByte(LoadHistoryOnBoot));
        // Job 복구 추가
        // add job recovery
        values.Add(Convert.ToByte(JobRecovery));

        // 리비전 1 미만이면 반환
        // return if below revision 1
        if (revision < 1)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // I/O 툴 이름 직렬화 (32바이트 고정)
        // serialize I/O tool name (32 bytes fixed)
        var ioName = Encoding.ASCII.GetBytes(InOutToolName);

        // Rev.1 값 추가
        // add Rev.1 values
        values.Add(Convert.ToByte(EnableInOutTool));
        // I/O 툴 이름 데이터 추가
        // add I/O tool name data
        values.AddRange(ioName);
        // I/O 툴 이름 패딩 추가
        // add I/O tool name padding
        values.AddRange(new byte[32 - ioName.Length]);
        // 체결 OK 포트 추가
        // add fasten OK port
        values.Add(Convert.ToByte(FastenOkPort));
        // 체결 NG 포트 추가
        // add fasten NG port
        values.Add(Convert.ToByte(FastenNgPort));
        // 프리셋 1 포트 추가
        // add preset 1 port
        values.Add(Convert.ToByte(Preset1));
        // 프리셋 2 포트 추가
        // add preset 2 port
        values.Add(Convert.ToByte(Preset2));
        // 프리셋 3 포트 추가
        // add preset 3 port
        values.Add(Convert.ToByte(Preset3));
        // 잠금 포트 추가
        // add lock port
        values.Add(Convert.ToByte(Lock));
        // 자동 스텝 전진 추가
        // add auto step forward
        values.Add(Convert.ToByte(AutoStepForward));
        // 스텝별 건너뛰기 추가
        // add skip by step
        values.Add(Convert.ToByte(SkipByStep));
        // 비밀번호 없이 재체결 추가
        // add re-tight without password
        values.Add(Convert.ToByte(ReTightWithoutPassword));

        // 리비전 2 미만이면 반환
        // return if below revision 2
        if (revision < 2)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // Rev.2 값 추가
        // add Rev.2 values
        values.Add(Convert.ToByte(Preset4));

        // 리비전 3 미만이면 반환
        // return if below revision 3
        if (revision < 3)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // I/O 툴 2 직렬화
        // serialize I/O tool 2
        WriteInOutTool(values, InOutTool2);
        // I/O 툴 3 직렬화
        // serialize I/O tool 3
        WriteInOutTool(values, InOutTool3);
        // I/O 툴 4 직렬화
        // serialize I/O tool 4
        WriteInOutTool(values, InOutTool4);
        // I/O 툴 5 직렬화
        // serialize I/O tool 5
        WriteInOutTool(values, InOutTool5);
        // I/O 툴 6 직렬화
        // serialize I/O tool 6
        WriteInOutTool(values, InOutTool6);
        // I/O 툴 7 직렬화
        // serialize I/O tool 7
        WriteInOutTool(values, InOutTool7);
        // I/O 툴 8 직렬화
        // serialize I/O tool 8
        WriteInOutTool(values, InOutTool8);

        // 비밀번호 없이 Job 중단 추가
        // add abort job without password
        values.Add(Convert.ToByte(AbortJobWithoutPass));
        // 스텝 NG 사유 코멘트 추가
        // add comment the cause of step NG
        values.Add(Convert.ToByte(CommentTheCauseOfStepNg));
        // Job NG 사유 코멘트 추가
        // add comment the cause of job NG
        values.Add(Convert.ToByte(CommentTheCauseOfJobNg));
        // 비밀번호 없이 ID 1 편집 추가
        // add edit ID 1 without password
        values.Add(Convert.ToByte(EditId1WithoutPass));
        // 비밀번호 없이 ID 2 편집 추가
        // add edit ID 2 without password
        values.Add(Convert.ToByte(EditId2WithoutPass));
        // 비밀번호 없이 ID 3 편집 추가
        // add edit ID 3 without password
        values.Add(Convert.ToByte(EditId3WithoutPass));
        // 비밀번호 없이 ID 4 편집 추가
        // add edit ID 4 without password
        values.Add(Convert.ToByte(EditId4WithoutPass));
        // 비밀번호 없이 ID 5 편집 추가
        // add edit ID 5 without password
        values.Add(Convert.ToByte(EditId5WithoutPass));
        // 비밀번호 없이 ID 6 편집 추가
        // add edit ID 6 without password
        values.Add(Convert.ToByte(EditId6WithoutPass));
        // 모든 나사 위치 동시 표시 추가
        // add all screw positions appear at once
        values.Add(Convert.ToByte(AllScrewPosAppearAtOnce));

        // 리비전 4 미만이면 반환
        // return if below revision 4
        if (revision < 4)
            // 직렬화된 배열 반환
            // return serialized array
            return values.ToArray();

        // 비밀번호 없이 툴 알람 리셋 추가
        // add reset tool alarm without password
        values.Add(Convert.ToByte(ResetToolAlarmWithoutPassword));
        // 사이드 패널 활성화 추가
        // add enable side panel
        values.Add(Convert.ToByte(EnableSidePanel));

        // 직렬화 결과 반환
        // return serialized result
        return values.ToArray();
    }

    /// <summary>
    ///     I/O 툴 데이터를 스팬에서 읽어 대상 객체에 설정합니다.
    ///     reads I/O tool data from span into the target object.
    /// </summary>
    /// <param name="data">원시 패킷 데이터 / raw packet data</param>
    /// <param name="pos">현재 위치 / current position</param>
    /// <param name="tool">대상 I/O 툴 객체 / target I/O tool object</param>
    private static void ReadInOutTool(ReadOnlySpan<byte> data, ref int pos, InOutTool tool) {
        // 툴 이름 읽기 (32바이트 ASCII)
        // read tool name (32 bytes ASCII)
        tool.Name = BinarySpanReader.ReadAsciiString(data, ref pos, 32);
        // 체결 OK 포트 읽기
        // read fasten OK port
        tool.FastenOk = BinarySpanReader.ReadByte(data, ref pos);
        // 체결 NG 포트 읽기
        // read fasten NG port
        tool.FastenNg = BinarySpanReader.ReadByte(data, ref pos);
        // 프리셋 1 포트 읽기
        // read preset 1 port
        tool.Preset1 = BinarySpanReader.ReadByte(data, ref pos);
        // 프리셋 2 포트 읽기
        // read preset 2 port
        tool.Preset2 = BinarySpanReader.ReadByte(data, ref pos);
        // 프리셋 3 포트 읽기
        // read preset 3 port
        tool.Preset3 = BinarySpanReader.ReadByte(data, ref pos);
        // 프리셋 4 포트 읽기
        // read preset 4 port
        tool.Preset4 = BinarySpanReader.ReadByte(data, ref pos);
        // 잠금 포트 읽기
        // read lock port
        tool.Lock = BinarySpanReader.ReadByte(data, ref pos);
    }

    /// <summary>
    ///     I/O 툴 데이터를 바이트 목록에 직렬화합니다.
    ///     serializes I/O tool data into a byte list.
    /// </summary>
    /// <param name="values">대상 바이트 목록 / target byte list</param>
    /// <param name="tool">I/O 툴 객체 / I/O tool object</param>
    private static void WriteInOutTool(List<byte> values, InOutTool tool) {
        // 툴 이름 직렬화 (32바이트 고정)
        // serialize tool name (32 bytes fixed)
        var name = Encoding.ASCII.GetBytes(tool.Name);
        // 이름 데이터 추가
        // add name data
        values.AddRange(name);
        // 이름 패딩 추가
        // add name padding
        values.AddRange(new byte[32 - name.Length]);
        // 체결 OK 포트 추가
        // add fasten OK port
        values.Add(Convert.ToByte(tool.FastenOk));
        // 체결 NG 포트 추가
        // add fasten NG port
        values.Add(Convert.ToByte(tool.FastenNg));
        // 프리셋 1 포트 추가
        // add preset 1 port
        values.Add(Convert.ToByte(tool.Preset1));
        // 프리셋 2 포트 추가
        // add preset 2 port
        values.Add(Convert.ToByte(tool.Preset2));
        // 프리셋 3 포트 추가
        // add preset 3 port
        values.Add(Convert.ToByte(tool.Preset3));
        // 프리셋 4 포트 추가
        // add preset 4 port
        values.Add(Convert.ToByte(tool.Preset4));
        // 잠금 포트 추가
        // add lock port
        values.Add(Convert.ToByte(tool.Lock));
    }

    /// <summary>
    ///     I/O 툴 설정 데이터 클래스.
    ///     I/O tool setting data class.
    /// </summary>
    /// <remarks>
    ///     이름, 체결 OK/NG 포트, 프리셋 1~4 포트, 잠금 포트 설정을 담습니다.
    ///     contains name, fasten OK/NG ports, preset 1~4 ports, and lock port settings.
    /// </remarks>
    public sealed class InOutTool {
	    /// <summary>
	    ///     툴 이름
	    ///     tool name
	    /// </summary>
	    public string Name { get; set; } = string.Empty;

	    /// <summary>
	    ///     입력: 체결 OK
	    ///     input: fasten OK
	    /// </summary>
	    public int FastenOk { get; set; }

	    /// <summary>
	    ///     입력: 체결 NG
	    ///     input: fasten NG
	    /// </summary>
	    public int FastenNg { get; set; }

	    /// <summary>
	    ///     출력: 프리셋 1
	    ///     output: preset 1
	    /// </summary>
	    public int Preset1 { get; set; }

	    /// <summary>
	    ///     출력: 프리셋 2
	    ///     output: preset 2
	    /// </summary>
	    public int Preset2 { get; set; }

	    /// <summary>
	    ///     출력: 프리셋 3
	    ///     output: preset 3
	    /// </summary>
	    public int Preset3 { get; set; }

	    /// <summary>
	    ///     출력: 프리셋 4
	    ///     output: preset 4
	    /// </summary>
	    public int Preset4 { get; set; }

	    /// <summary>
	    ///     출력: 잠금
	    ///     output: lock
	    /// </summary>
	    public int Lock { get; set; }
    }

    #region Rev.0

    /// <summary>
    ///     Job 없이 동작 모드
    ///     without-job operation mode
    /// </summary>
    public int WithoutJobMode { get; set; }

    /// <summary>
    ///     나사 카운트 방향
    ///     screw count direction
    /// </summary>
    public int DownCountForScrew { get; set; }

    /// <summary>
    ///     나사 카운트 단위
    ///     screw count unit
    /// </summary>
    public int JobCountForScrew { get; set; }

    /// <summary>
    ///     입력을 통한 Job 직접 선택 모드
    ///     job selection type via input
    /// </summary>
    public int DirectJobSelectionMode { get; set; }

    /// <summary>
    ///     원격 바코드 인터페이스 (Job 없이)
    ///     barcode interface (only without job)
    /// </summary>
    public int RemoteBarcodeMode { get; set; }

    /// <summary>
    ///     Job 실행 중 바코드 스캔
    ///     barcode scan while job running
    /// </summary>
    [Obsolete("Supported by Rev.0 only.")]
    public int ScanBarcodeWhileRun { get; set; }

    /// <summary>
    ///     재체결 실패 시 나사 건너뛰기
    ///     skip screw on re-tightening failure
    /// </summary>
    public int SkipScrewForReTightFail { get; set; }

    /// <summary>
    ///     부팅 시 운영 모드
    ///     operation mode on boot
    /// </summary>
    public int OperationMode { get; set; }

    /// <summary>
    ///     부팅 시 Job 이름 (Job 모드 전용)
    ///     job name on boot (with-job mode only)
    /// </summary>
    public string JobNameOnBoot { get; set; }

    /// <summary>
    ///     비밀번호 없이 건너뛰기 접근
    ///     skip button access without password
    /// </summary>
    public int SkipWithoutPassword { get; set; }

    /// <summary>
    ///     비밀번호 없이 뒤로가기 접근
    ///     back button access without password
    /// </summary>
    public int BackWithoutPassword { get; set; }

    /// <summary>
    ///     비밀번호 없이 Job/Step 리셋
    ///     job/step reset button without password
    /// </summary>
    public int ResetWithoutPassword { get; set; }

    /// <summary>
    ///     Job 리셋 버튼 표시
    ///     display job reset button
    /// </summary>
    public int JobResetButton { get; set; }

    /// <summary>
    ///     비밀번호 없이 Job 선택 접근
    ///     job selection access without password
    /// </summary>
    public int JobSelectionWithoutPassword { get; set; }

    /// <summary>
    ///     완료 시 자동 재시작
    ///     automatically restart job when finished
    /// </summary>
    public int AutoRestartWhenFinished { get; set; }

    /// <summary>
    ///     자동 데이터 백업
    ///     automatic data backup
    /// </summary>
    public int AutoDataBackup { get; set; }

    /// <summary>
    ///     비트 소켓 트레이 활성화
    ///     enable bit socket tray
    /// </summary>
    public int BitSocketTray { get; set; }

    /// <summary>
    ///     비트 소켓 트레이 툴 이름
    ///     tool name for bit socket tray
    /// </summary>
    public string BitSocketToolName { get; set; }

    /// <summary>
    ///     부팅 시 당일 운영 이력 로드
    ///     load the day's operation history on boot
    /// </summary>
    public int LoadHistoryOnBoot { get; set; }

    /// <summary>
    ///     Job 상태 백업/복구 활성화
    ///     enable job status backup/recovery
    /// </summary>
    public int JobRecovery { get; set; }

    #endregion

    #region Rev.1

    /// <summary>
    ///     I/O 툴 활성화
    ///     enable I/O tool
    /// </summary>
    public int EnableInOutTool { get; set; }

    /// <summary>
    ///     I/O 툴 이름
    ///     I/O tool name
    /// </summary>
    public string InOutToolName { get; set; }

    /// <summary>
    ///     입력: 체결 OK 포트 번호 (I/O 툴)
    ///     input: fastening OK port number for I/O tool
    /// </summary>
    public int FastenOkPort { get; set; }

    /// <summary>
    ///     입력: 체결 NG 포트 번호 (I/O 툴)
    ///     input: fastening NG port number for I/O tool
    /// </summary>
    public int FastenNgPort { get; set; }

    /// <summary>
    ///     출력: 프리셋 1 포트 번호 (I/O 툴)
    ///     output: preset 1 port number for I/O tool
    /// </summary>
    public int Preset1 { get; set; }

    /// <summary>
    ///     출력: 프리셋 2 포트 번호 (I/O 툴)
    ///     output: preset 2 port number for I/O tool
    /// </summary>
    public int Preset2 { get; set; }

    /// <summary>
    ///     출력: 프리셋 3 포트 번호 (I/O 툴)
    ///     output: preset 3 port number for I/O tool
    /// </summary>
    public int Preset3 { get; set; }

    /// <summary>
    ///     출력: 잠금 포트 번호 (I/O 툴)
    ///     output: lock port number for I/O tool
    /// </summary>
    public int Lock { get; set; }

    /// <summary>
    ///     자동 스텝 전진 활성화
    ///     enable auto step forward
    /// </summary>
    public int AutoStepForward { get; set; }

    /// <summary>
    ///     스텝별 건너뛰기
    ///     skip by step
    /// </summary>
    public int SkipByStep { get; set; }

    /// <summary>
    ///     비밀번호 없이 재체결 허용
    ///     allow re-tight without password
    /// </summary>
    public int ReTightWithoutPassword { get; set; }

    #endregion

    #region Rev.3

    /// <summary>
    ///     I/O 툴 2
    ///     I/O tool 2
    /// </summary>
    public InOutTool InOutTool2 { get; } = new();

    /// <summary>
    ///     I/O 툴 3
    ///     I/O tool 3
    /// </summary>
    public InOutTool InOutTool3 { get; } = new();

    /// <summary>
    ///     I/O 툴 4
    ///     I/O tool 4
    /// </summary>
    public InOutTool InOutTool4 { get; } = new();

    /// <summary>
    ///     I/O 툴 5
    ///     I/O tool 5
    /// </summary>
    public InOutTool InOutTool5 { get; } = new();

    /// <summary>
    ///     I/O 툴 6
    ///     I/O tool 6
    /// </summary>
    public InOutTool InOutTool6 { get; } = new();

    /// <summary>
    ///     I/O 툴 7
    ///     I/O tool 7
    /// </summary>
    public InOutTool InOutTool7 { get; } = new();

    /// <summary>
    ///     I/O 툴 8
    ///     I/O tool 8
    /// </summary>
    public InOutTool InOutTool8 { get; } = new();

    /// <summary>
    ///     비밀번호 없이 Job 중단
    ///     abort job without password
    /// </summary>
    public int AbortJobWithoutPass { get; set; }

    /// <summary>
    ///     스텝 NG 사유 코멘트
    ///     comment the cause of step NG
    /// </summary>
    public int CommentTheCauseOfStepNg { get; set; }

    /// <summary>
    ///     Job NG 사유 코멘트
    ///     comment the cause of job NG
    /// </summary>
    public int CommentTheCauseOfJobNg { get; set; }

    /// <summary>
    ///     비밀번호 없이 ID 1 편집
    ///     edit ID 1 without password
    /// </summary>
    public int EditId1WithoutPass { get; set; }

    /// <summary>
    ///     비밀번호 없이 ID 2 편집
    ///     edit ID 2 without password
    /// </summary>
    public int EditId2WithoutPass { get; set; }

    /// <summary>
    ///     비밀번호 없이 ID 3 편집
    ///     edit ID 3 without password
    /// </summary>
    public int EditId3WithoutPass { get; set; }

    /// <summary>
    ///     비밀번호 없이 ID 4 편집
    ///     edit ID 4 without password
    /// </summary>
    public int EditId4WithoutPass { get; set; }

    /// <summary>
    ///     비밀번호 없이 ID 5 편집
    ///     edit ID 5 without password
    /// </summary>
    public int EditId5WithoutPass { get; set; }

    /// <summary>
    ///     비밀번호 없이 ID 6 편집
    ///     edit ID 6 without password
    /// </summary>
    public int EditId6WithoutPass { get; set; }

    /// <summary>
    ///     모든 나사 위치 동시 표시
    ///     all screw positions appear at once
    /// </summary>
    public int AllScrewPosAppearAtOnce { get; set; }

    #endregion

    #region Rev.4

    /// <summary>
    ///     비밀번호 없이 툴 알람 리셋
    ///     reset tool alarm without password
    /// </summary>
    public int ResetToolAlarmWithoutPassword { get; set; }

    /// <summary>
    ///     사이드 패널 활성화
    ///     enable side panel
    /// </summary>
    public int EnableSidePanel { get; set; }

    #endregion
}