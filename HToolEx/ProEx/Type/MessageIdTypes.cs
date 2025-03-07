namespace HToolEx.ProEx.Type;

/// <summary>
///     Message ID types for Remote-Pro X
/// </summary>
public enum MessageIdTypes {
    // General
    CommandAccepted = 0,
    CommandError,
    KeepAlive,
    SystemReboot,
    SystemTime,

    // Member tools
    MemberToolRequest = 10,
    MemberToolReply,
    ScanToolRequest,
    ScanToolReply,
    AddMemberTool,
    ReleaseMemberTool,
    RenameMemberTool,

    // Job
    JobListRequest = 20,
    JobListReply,
    JobListRefresh,
    JobCodeUpdateAndRefresh = 32,

    // Setting
    OperationRequest = 40,
    OperationReply,
    OperationSet,
    InOutRequest,
    InOutReply,
    InOutSet,
    LogRequest,
    LogReply,
    LogSet,
    BarcodeRequest,
    BarcodeReply,
    BarcodeRefresh,
    NetworkRequest,
    NetworkReply,
    NetworkSet,
    ShareRequest,
    ShareReply,
    ShareSet,
    SoundRequest,
    SoundReply,
    SoundSet,
    StepNgCauseRequest,
    StepNgCauseReply,
    StepNgCauseSet,
    JobNgCauseRequest,
    JobNgCauseReply,
    JobNgCauseSet,
    IoToolNameRequest,
    IoToolNameReply,

    // System
    InformationRequest = 70,
    InformationReply,
    XmlRequest,
    XmlReply,
    XmlUpdateAndRefresh,

    // Operation
    SelectJob = 80,
    PreviousJob,
    NextJob,
    ResetJob,
    ResetStep,
    Back,
    Skip,
    JobEventSubscribe,
    JobEvent,
    JobEventAcknowledge,
    JobEventUnsubscribe,

    // Event
    LastEventSubscribe = 100,
    LastEventNotUse,
    LastEvent,
    LastEventAcknowledge,
    LastEventUnsubscribe,
    OldEventRequest,
    OldEventReply,
    LastEventIdRequest,
    LastEventIdReply,

    // MODBUS
    ModbusRequest = 110,
    ModbusReply,

    // Dummy
    None = 999
}