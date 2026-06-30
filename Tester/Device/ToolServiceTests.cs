using HTool.Device;
using HTool.Device.Pro;
using HTool.Format.Pro;
using HTool.Type;
using Tester.TestHelpers;

namespace Tester.Device;

/// <summary>
///     ToolService 및 ProService 스모크 테스트.
///     ToolService and ProService smoke tests.
/// </summary>
public sealed class ToolServiceTests {
    /// <summary>
    ///     물리적 툴(ToolType=1) 80바이트 데이터를 생성한다.
    ///     creates 80-byte physical tool (ToolType=1) data.
    /// </summary>
    /// <param name="serial">시리얼 번호 / serial number</param>
    /// <param name="model">모델명 / model name</param>
    /// <returns>80바이트 배열 / 80-byte array</returns>
    private static byte[] MakeMemberTool(string serial, string model = "MDT-100") {
        // 물리적 툴 데이터 생성
        // build physical tool data
        return new ByteBuilder()
            .Byte(1)                                 // ToolType (1 = 물리적 / physical)
            .Ascii(model, 16)                        // Model (16바이트 / bytes)
            .Ascii(serial, 16)                       // Serial (16바이트 / bytes)
            .UInt16BigEndian(100)                    // Version
            .Raw(192, 168, 1, 10)                    // IP 주소 / address
            .UInt16BigEndian(502)                    // Port
            .Raw(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0x01) // MAC 주소 / address
            .Ascii("Tool-" + serial, 32)             // Name (32바이트 / bytes)
            .Byte(1)                                 // Status (활성 / active)
            .Build();
    }

    /// <summary>
    ///     I/O 가상 툴(ToolType=0) 80바이트 데이터를 생성한다.
    ///     creates 80-byte I/O virtual tool (ToolType=0) data.
    /// </summary>
    /// <param name="serial">시리얼 번호 / serial number</param>
    /// <returns>80바이트 배열 / 80-byte array</returns>
    private static byte[] MakeIoTool(string serial) {
        // I/O 가상 툴 데이터 생성
        // build I/O virtual tool data
        return new ByteBuilder()
            .Byte(0)                                 // ToolType (0 = I/O 가상 / virtual)
            .Ascii("IO-MODULE", 16)                  // Model
            .Ascii(serial, 16)                       // Serial
            .UInt16BigEndian(50)                     // Version
            .Raw(192, 168, 1, 20)                    // IP 주소 / address
            .UInt16BigEndian(502)                    // Port
            .Raw(0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0x02) // MAC 주소 / address
            .Ascii("IO-" + serial, 32)               // Name
            .Byte(1)                                 // Status
            .Build();
    }

    /// <summary>
    ///     ProToolInfo 목록을 바이트 데이터에서 생성한다.
    ///     creates a ProToolInfo list from byte data arrays.
    /// </summary>
    /// <param name="toolDataList">바이트 데이터 배열 목록 / list of byte data arrays</param>
    /// <returns>ProToolInfo 읽기 전용 목록 / ProToolInfo readonly list</returns>
    private static IReadOnlyList<ProToolInfo> MakeToolList(params byte[][] toolDataList) {
        // 각 바이트 데이터를 ProToolInfo로 파싱하여 목록 반환
        // parse each byte data into ProToolInfo and return as list
        return toolDataList.Select(data => new ProToolInfo(data)).ToList();
    }

    [Fact]
    public void UpdateMemberTools_ListUpdated() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 물리적 툴 2개 + I/O 툴 1개 데이터 생성
        // create 2 physical tools + 1 I/O tool data
        var tools = MakeToolList(
            MakeMemberTool("SN001"),
            MakeMemberTool("SN002"),
            MakeIoTool("IO001")
        );

        // 멤버 목록 갱신
        // update member tools
        var changed = svc.UpdateMemberTools(tools);

        // 변경 감지 확인
        // verify change detected
        Assert.True(changed);
        // 물리적 툴만 MemberTools에 포함 확인
        // verify only physical tools in MemberTools
        Assert.Equal(2, svc.MemberTools.Count);
        // I/O 툴이 IoTools에 포함 확인
        // verify I/O tools in IoTools
        Assert.Single(svc.IoTools);
    }

    [Fact]
    public void UpdateMemberTools_SameData_ReturnsFalse() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 동일한 툴 데이터 생성
        // create identical tool data
        var toolData = MakeMemberTool("SN001");
        // 첫 번째 갱신
        // first update
        svc.UpdateMemberTools(MakeToolList(toolData));

        // 동일 데이터로 재갱신
        // re-update with same data
        var changed = svc.UpdateMemberTools(MakeToolList(toolData));

        // 변경 없음 확인
        // verify no change detected
        Assert.False(changed);
    }

    [Fact]
    public void TrySelectTool_ValidIndex_ReturnsTrue() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 목록 설정
        // set member tools
        svc.UpdateMemberTools(MakeToolList(
            MakeMemberTool("SN001"),
            MakeMemberTool("SN002")
        ));

        // 유효한 인덱스로 선택
        // select with valid index
        var result = svc.TrySelectTool(0);

        // 선택 성공 확인
        // verify selection succeeded
        Assert.True(result);
        // 선택된 인덱스 확인
        // verify selected index
        Assert.Equal(0, svc.SelectedToolId);
        // 유효한 선택 상태 확인
        // verify valid selection state
        Assert.True(svc.IsSelectedToolValid);
    }

    [Fact]
    public void TrySelectTool_OutOfRange_ReturnsFalse() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 1개만 등록
        // register only 1 member
        svc.UpdateMemberTools(MakeToolList(MakeMemberTool("SN001")));

        // 범위 초과 인덱스로 선택 시도
        // attempt selection with out-of-range index
        var result = svc.TrySelectTool(5);

        // 선택 실패 확인
        // verify selection failed
        Assert.False(result);
    }

    [Fact]
    public void SelectedToolId_Default_IsNegativeOne() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();

        // 기본 선택 인덱스는 -1
        // default selected index is -1
        Assert.Equal(-1, svc.SelectedToolId);
        // 유효하지 않은 상태 확인
        // verify invalid state
        Assert.False(svc.IsSelectedToolValid);
    }

    [Fact]
    public void Clear_ResetsAllState() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 목록 설정
        // set member tools
        svc.UpdateMemberTools(MakeToolList(MakeMemberTool("SN001")));
        // 툴 선택
        // select tool
        svc.TrySelectTool(0);

        // 모든 상태 초기화
        // clear all state
        svc.Clear();

        // 멤버 목록 비어 있음 확인
        // verify member list is empty
        Assert.Empty(svc.MemberTools);
        // I/O 목록 비어 있음 확인
        // verify I/O list is empty
        Assert.Empty(svc.IoTools);
        // 스캔 목록 비어 있음 확인
        // verify scan list is empty
        Assert.Empty(svc.ScanTools);
        // 선택 해제 확인
        // verify selection cleared
        Assert.Equal(-1, svc.SelectedToolId);
    }

    [Fact]
    public void FindBySerial_Existing_ReturnsTool() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 목록 설정
        // set member tools
        svc.UpdateMemberTools(MakeToolList(
            MakeMemberTool("SN001"),
            MakeMemberTool("SN002")
        ));

        // 시리얼 번호로 검색
        // find by serial number
        var found = svc.FindBySerial("SN002");

        // 검색 결과 존재 확인
        // verify result found
        Assert.NotNull(found);
        // 시리얼 번호 일치 확인
        // verify serial number matches
        Assert.Equal("SN002", found.Value.Serial);
    }

    [Fact]
    public void FindBySerial_NotExisting_ReturnsNull() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 목록 설정
        // set member tools
        svc.UpdateMemberTools(MakeToolList(MakeMemberTool("SN001")));

        // 존재하지 않는 시리얼 번호로 검색
        // find by non-existing serial number
        var found = svc.FindBySerial("UNKNOWN");

        // null 반환 확인
        // verify null returned
        Assert.Null(found);
    }

    [Fact]
    public void TryBeginOperation_ValidTool_ReturnsIndex() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 목록 설정
        // set member tools
        svc.UpdateMemberTools(MakeToolList(MakeMemberTool("SN001")));
        // 툴 선택
        // select tool
        svc.TrySelectTool(0);

        // 작업 시작 시도
        // attempt to begin operation
        var index = svc.TryBeginOperation();

        // 선택된 인덱스 반환 확인
        // verify selected index returned
        Assert.Equal(0, index);

        // 정리: 작업 종료
        // cleanup: end operation
        svc.EndOperation();
    }

    [Fact]
    public void TryBeginOperation_NoToolSelected_ReturnsNegativeOne() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 목록 설정 (선택 안 함)
        // set member tools (no selection)
        svc.UpdateMemberTools(MakeToolList(MakeMemberTool("SN001")));

        // 선택 없이 작업 시작 시도
        // attempt operation without selection
        var index = svc.TryBeginOperation();

        // 실패 (-1) 반환 확인
        // verify failure (-1) returned
        Assert.Equal(-1, index);
    }

    [Fact]
    public void TryBeginOperation_AlreadyActive_ReturnsNegativeOne() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();
        // 멤버 목록 설정 및 선택
        // set member tools and select
        svc.UpdateMemberTools(MakeToolList(MakeMemberTool("SN001")));
        // 툴 선택
        // select tool
        svc.TrySelectTool(0);
        // 첫 번째 작업 시작
        // begin first operation
        svc.TryBeginOperation();

        // 두 번째 작업 시작 시도 (이미 활성화 상태)
        // attempt second operation (already active)
        var index = svc.TryBeginOperation();

        // 실패 (-1) 반환 확인
        // verify failure (-1) returned
        Assert.Equal(-1, index);

        // 정리: 작업 종료
        // cleanup: end operation
        svc.EndOperation();
    }

    [Fact]
    public void UpdateScanTools_NewList_ReturnsTrue() {
        // ToolService 생성
        // create ToolService
        var svc = new ToolService();

        // 스캔 목록 갱신
        // update scan tools
        var changed = svc.UpdateScanTools(MakeToolList(MakeMemberTool("SCAN001")));

        // 변경 감지 확인
        // verify change detected
        Assert.True(changed);
        // 스캔 목록 항목 수 확인
        // verify scan list count
        Assert.Single(svc.ScanTools);
    }

    [Fact]
    public void ProService_Construction_WithFakeTransport() {
        // 가짜 전송 계층 생성
        // create fake transport
        using var transport = new FakeTransport();
        // 로거 생성
        // create logger
        using var logger = new HToolLogger();
        // 설정 생성
        // create settings
        var settings = new HToolSettings();

        // ProService 인스턴스 생성
        // create ProService instance
        using var pro = new ProService(transport, logger, settings);

        // Tools 속성이 초기화되었는지 확인
        // verify Tools property is initialized
        Assert.NotNull(pro.Tools);
        // FTP 서비스는 연결 전 null 확인
        // verify FTP is null before connection
        Assert.Null(pro.Ftp);
        // 이벤트 구독 초기 상태 확인
        // verify initial subscription state
        Assert.False(pro.IsToolEventSubscribed);
        // 작업 이벤트 구독 초기 상태 확인
        // verify initial job event subscription state
        Assert.False(pro.IsJobEventSubscribed);
    }

    [Fact]
    public void ProService_StartStop_Lifecycle() {
        // 가짜 전송 계층 생성
        // create fake transport
        using var transport = new FakeTransport();
        // 로거 생성
        // create logger
        using var logger = new HToolLogger();
        // 설정 생성
        // create settings
        var settings = new HToolSettings();
        // ProService 생성
        // create ProService
        using var pro = new ProService(transport, logger, settings);

        // 서비스 시작
        // start service
        pro.Start("192.168.1.1");

        // FTP 서비스가 생성되었는지 확인
        // verify FTP service was created
        Assert.NotNull(pro.Ftp);

        // 서비스 정지
        // stop service
        pro.Stop();

        // FTP 서비스가 해제되었는지 확인
        // verify FTP service was disposed
        Assert.Null(pro.Ftp);
        // 이벤트 구독 해제 확인
        // verify event subscriptions cleared
        Assert.False(pro.IsToolEventSubscribed);
    }
}