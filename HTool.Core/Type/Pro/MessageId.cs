using System.ComponentModel;

namespace HTool.Core.Type.Pro;

/// <summary>
///     Remote-Pro X 메시지 ID
///     message ID for Remote-Pro X communication
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Device.Codec.ProCodec</c>, <c>HTool.Device.Protocol.ProRequest</c>,
///     <c>HTool.Device.Protocol.ProMessage</c>, <c>HTool.Device.Pro.ProService</c>.
///     used by: <c>HTool.Device.Codec.ProCodec</c>, <c>HTool.Device.Protocol.ProRequest</c>,
///     <c>HTool.Device.Protocol.ProMessage</c>, <c>HTool.Device.Pro.ProService</c>.
/// </remarks>
public enum MessageId {
    #region General

    /// <summary>
    ///     명령 수락 응답.
    ///     command accepted response.
    /// </summary>
    [Description("Command Accepted")]
    CommandAccepted = 0,

    /// <summary>
    ///     명령 오류 응답.
    ///     command error response.
    /// </summary>
    [Description("Command Error")]
    CommandError,

    /// <summary>
    ///     연결 유지 하트비트.
    ///     keep-alive heartbeat.
    /// </summary>
    [Description("Keep-Alive")]
    KeepAlive,

    /// <summary>
    ///     시스템 재부팅 요청.
    ///     system reboot request.
    /// </summary>
    [Description("System Reboot")]
    SystemReboot,

    /// <summary>
    ///     시스템 시간 동기화.
    ///     system time synchronization.
    /// </summary>
    [Description("System Time")]
    SystemTime,

    #endregion

    #region Member tools

    /// <summary>
    ///     멤버 툴 목록 요청.
    ///     request member tool list.
    /// </summary>
    [Description("Member Tool Request")]
    MemberToolRequest = 10,

    /// <summary>
    ///     멤버 툴 목록 응답.
    ///     member tool list reply.
    /// </summary>
    [Description("Member Tool Reply")]
    MemberToolReply,

    /// <summary>
    ///     스캔 툴 목록 요청.
    ///     request scan tool list.
    /// </summary>
    [Description("Scan Tool Request")]
    ScanToolRequest,

    /// <summary>
    ///     스캔 툴 목록 응답.
    ///     scan tool list reply.
    /// </summary>
    [Description("Scan Tool Reply")]
    ScanToolReply,

    /// <summary>
    ///     멤버 목록에 툴 추가.
    ///     add a tool to member list.
    /// </summary>
    [Description("Add Member Tool")]
    AddMemberTool,

    /// <summary>
    ///     멤버 목록에서 툴 해제.
    ///     release a tool from member list.
    /// </summary>
    [Description("Release Member Tool")]
    ReleaseMemberTool,

    /// <summary>
    ///     멤버 툴 이름 변경.
    ///     rename a member tool.
    /// </summary>
    [Description("Rename Member Tool")]
    RenameMemberTool,

    #endregion

    #region Job

    /// <summary>
    ///     잡 목록 요청.
    ///     request job list.
    /// </summary>
    [Description("Job List Request")]
    JobListRequest = 20,

    /// <summary>
    ///     잡 목록 응답.
    ///     job list reply.
    /// </summary>
    [Description("Job List Reply")]
    JobListReply,

    /// <summary>
    ///     잡 목록 갱신.
    ///     refresh job list.
    /// </summary>
    [Description("Job List Refresh")]
    JobListRefresh,

    /// <summary>
    ///     잡 코드 업데이트 및 갱신.
    ///     update job code and refresh.
    /// </summary>
    [Description("Job Code Update And Refresh")]
    JobCodeUpdateAndRefresh = 32,

    #endregion

    #region Setting

    /// <summary>
    ///     운용 설정 요청.
    ///     request operation settings.
    /// </summary>
    [Description("Operation Request")]
    OperationRequest = 40,

    /// <summary>
    ///     운용 설정 응답.
    ///     operation settings reply.
    /// </summary>
    [Description("Operation Reply")]
    OperationReply,

    /// <summary>
    ///     운용 설정 적용.
    ///     set operation settings.
    /// </summary>
    [Description("Operation Set")]
    OperationSet,

    /// <summary>
    ///     I/O 설정 요청.
    ///     request I/O settings.
    /// </summary>
    [Description("I/O Request")]
    InOutRequest,

    /// <summary>
    ///     I/O 설정 응답.
    ///     I/O settings reply.
    /// </summary>
    [Description("I/O Reply")]
    InOutReply,

    /// <summary>
    ///     I/O 설정 적용.
    ///     set I/O settings.
    /// </summary>
    [Description("I/O Set")]
    InOutSet,

    /// <summary>
    ///     로그 설정 요청.
    ///     request log settings.
    /// </summary>
    [Description("Log Request")]
    LogRequest,

    /// <summary>
    ///     로그 설정 응답.
    ///     log settings reply.
    /// </summary>
    [Description("Log Reply")]
    LogReply,

    /// <summary>
    ///     로그 설정 적용.
    ///     set log settings.
    /// </summary>
    [Description("Log Set")]
    LogSet,

    /// <summary>
    ///     바코드 설정 요청.
    ///     request barcode settings.
    /// </summary>
    [Description("Barcode Request")]
    BarcodeRequest,

    /// <summary>
    ///     바코드 설정 응답.
    ///     barcode settings reply.
    /// </summary>
    [Description("Barcode Reply")]
    BarcodeReply,

    /// <summary>
    ///     바코드 설정 갱신.
    ///     refresh barcode settings.
    /// </summary>
    [Description("Barcode Refresh")]
    BarcodeRefresh,

    /// <summary>
    ///     네트워크 설정 요청.
    ///     request network settings.
    /// </summary>
    [Description("Network Request")]
    NetworkRequest,

    /// <summary>
    ///     네트워크 설정 응답.
    ///     network settings reply.
    /// </summary>
    [Description("Network Reply")]
    NetworkReply,

    /// <summary>
    ///     네트워크 설정 적용.
    ///     set network settings.
    /// </summary>
    [Description("Network Set")]
    NetworkSet,

    /// <summary>
    ///     공유 설정 요청.
    ///     request share settings.
    /// </summary>
    [Description("Share Request")]
    ShareRequest,

    /// <summary>
    ///     공유 설정 응답.
    ///     share settings reply.
    /// </summary>
    [Description("Share Reply")]
    ShareReply,

    /// <summary>
    ///     공유 설정 적용.
    ///     set share settings.
    /// </summary>
    [Description("Share Set")]
    ShareSet,

    /// <summary>
    ///     사운드 설정 요청.
    ///     request sound settings.
    /// </summary>
    [Description("Sound Request")]
    SoundRequest,

    /// <summary>
    ///     사운드 설정 응답.
    ///     sound settings reply.
    /// </summary>
    [Description("Sound Reply")]
    SoundReply,

    /// <summary>
    ///     사운드 설정 적용.
    ///     set sound settings.
    /// </summary>
    [Description("Sound Set")]
    SoundSet,

    /// <summary>
    ///     스텝 NG 원인 설정 요청.
    ///     request step NG cause settings.
    /// </summary>
    [Description("Step NG Cause Request")]
    StepNgCauseRequest,

    /// <summary>
    ///     스텝 NG 원인 설정 응답.
    ///     step NG cause settings reply.
    /// </summary>
    [Description("Step NG Cause Reply")]
    StepNgCauseReply,

    /// <summary>
    ///     스텝 NG 원인 설정 적용.
    ///     set step NG cause settings.
    /// </summary>
    [Description("Step NG Cause Set")]
    StepNgCauseSet,

    /// <summary>
    ///     잡 NG 원인 설정 요청.
    ///     request job NG cause settings.
    /// </summary>
    [Description("Job NG Cause Request")]
    JobNgCauseRequest,

    /// <summary>
    ///     잡 NG 원인 설정 응답.
    ///     job NG cause settings reply.
    /// </summary>
    [Description("Job NG Cause Reply")]
    JobNgCauseReply,

    /// <summary>
    ///     잡 NG 원인 설정 적용.
    ///     set job NG cause settings.
    /// </summary>
    [Description("Job NG Cause Set")]
    JobNgCauseSet,

    /// <summary>
    ///     I/O 툴 이름 요청.
    ///     request I/O tool name.
    /// </summary>
    [Description("I/O Tool Name Request")]
    IoToolNameRequest,

    /// <summary>
    ///     I/O 툴 이름 응답.
    ///     I/O tool name reply.
    /// </summary>
    [Description("I/O Tool Name Reply")]
    IoToolNameReply,

    #endregion

    #region System

    /// <summary>
    ///     시스템 정보 요청.
    ///     request system information.
    /// </summary>
    [Description("Information Request")]
    InformationRequest = 70,

    /// <summary>
    ///     시스템 정보 응답.
    ///     system information reply.
    /// </summary>
    [Description("Information Reply")]
    InformationReply,

    /// <summary>
    ///     XML 구성 요청.
    ///     request XML configuration.
    /// </summary>
    [Description("XML Request")]
    XmlRequest,

    /// <summary>
    ///     XML 구성 응답.
    ///     XML configuration reply.
    /// </summary>
    [Description("XML Reply")]
    XmlReply,

    /// <summary>
    ///     XML 업데이트 및 갱신.
    ///     update XML and refresh.
    /// </summary>
    [Description("XML Update And Refresh")]
    XmlUpdateAndRefresh,

    #endregion

    #region Operation

    /// <summary>
    ///     잡 선택.
    ///     select a job.
    /// </summary>
    [Description("Select Job")]
    SelectJob = 80,

    /// <summary>
    ///     이전 잡으로 이동.
    ///     move to previous job.
    /// </summary>
    [Description("Previous Job")]
    PreviousJob,

    /// <summary>
    ///     다음 잡으로 이동.
    ///     move to next job.
    /// </summary>
    [Description("Next Job")]
    NextJob,

    /// <summary>
    ///     현재 잡 초기화.
    ///     reset current job.
    /// </summary>
    [Description("Reset Job")]
    ResetJob,

    /// <summary>
    ///     현재 스텝 초기화.
    ///     reset current step.
    /// </summary>
    [Description("Reset Step")]
    ResetStep,

    /// <summary>
    ///     이전 스텝으로 복귀.
    ///     go back one step.
    /// </summary>
    [Description("Back")]
    Back,

    /// <summary>
    ///     현재 스텝 건너뜀.
    ///     skip current step.
    /// </summary>
    [Description("Skip")]
    Skip,

    /// <summary>
    ///     잡 이벤트 구독.
    ///     subscribe to job events.
    /// </summary>
    [Description("Job Event Subscribe")]
    JobEventSubscribe,

    /// <summary>
    ///     잡 이벤트 알림.
    ///     job event notification.
    /// </summary>
    [Description("Job Event")]
    JobEvent,

    /// <summary>
    ///     잡 이벤트 확인 응답.
    ///     acknowledge a job event.
    /// </summary>
    [Description("Job Event Acknowledge")]
    JobEventAcknowledge,

    /// <summary>
    ///     잡 이벤트 구독 해제.
    ///     unsubscribe from job events.
    /// </summary>
    [Description("Job Event Unsubscribe")]
    JobEventUnsubscribe,

    #endregion

    #region Event

    /// <summary>
    ///     마지막 이벤트 구독.
    ///     subscribe to last event.
    /// </summary>
    [Description("Last Event Subscribe")]
    LastEventSubscribe = 100,

    /// <summary>
    ///     마지막 이벤트 미사용.
    ///     last event not in use.
    /// </summary>
    [Description("Last Event Not Use")]
    LastEventNotUse,

    /// <summary>
    ///     마지막 이벤트 알림.
    ///     last event notification.
    /// </summary>
    [Description("Last Event")]
    LastEvent,

    /// <summary>
    ///     마지막 이벤트 확인 응답.
    ///     acknowledge last event.
    /// </summary>
    [Description("Last Event Acknowledge")]
    LastEventAcknowledge,

    /// <summary>
    ///     마지막 이벤트 구독 해제.
    ///     unsubscribe from last event.
    /// </summary>
    [Description("Last Event Unsubscribe")]
    LastEventUnsubscribe,

    /// <summary>
    ///     과거 이벤트 이력 요청.
    ///     request old event history.
    /// </summary>
    [Description("Old Event Request")]
    OldEventRequest,

    /// <summary>
    ///     과거 이벤트 이력 응답.
    ///     old event history reply.
    /// </summary>
    [Description("Old Event Reply")]
    OldEventReply,

    /// <summary>
    ///     마지막 이벤트 ID 요청.
    ///     request last event ID.
    /// </summary>
    [Description("Last Event ID Request")]
    LastEventIdRequest,

    /// <summary>
    ///     마지막 이벤트 ID 응답.
    ///     last event ID reply.
    /// </summary>
    [Description("Last Event ID Reply")]
    LastEventIdReply,

    #endregion

    #region MODBUS

    /// <summary>
    ///     MODBUS 패스스루 요청.
    ///     MODBUS pass-through request.
    /// </summary>
    [Description("MODBUS Request")]
    ModbusRequest = 110,

    /// <summary>
    ///     MODBUS 패스스루 응답.
    ///     MODBUS pass-through reply.
    /// </summary>
    [Description("MODBUS Reply")]
    ModbusReply,

    #endregion

    #region Encoder

    /// <summary>
    ///     엔코더 설정 요청.
    ///     request encoder settings.
    /// </summary>
    [Description("Encoder Request")]
    EncoderRequest = 120,

    /// <summary>
    ///     엔코더 설정 응답.
    ///     encoder settings reply.
    /// </summary>
    [Description("Encoder Reply")]
    EncoderReply,

    /// <summary>
    ///     엔코더 설정 적용.
    ///     set encoder settings.
    /// </summary>
    [Description("Encoder Set")]
    EncoderSet,

    /// <summary>
    ///     엔코더 값 요청.
    ///     request encoder value.
    /// </summary>
    [Description("Encoder Value Request")]
    EncoderValueRequest,

    /// <summary>
    ///     엔코더 값 응답.
    ///     encoder value reply.
    /// </summary>
    [Description("Encoder Value Reply")]
    EncoderValueReply,

    #endregion

    /// <summary>
    ///     메시지 없음 / 자리표시자.
    ///     no message / placeholder.
    /// </summary>
    [Description("None")]
    None = 999
}