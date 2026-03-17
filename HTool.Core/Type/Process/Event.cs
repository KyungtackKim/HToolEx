using System.ComponentModel;

namespace HTool.Core.Type.Process;

/// <summary>
///     체결 작업 이벤트 종류 열거형. FormatEvent에서 발생한 이벤트 유형(OK, NG, 오류 등)을 나타냅니다.
///     fastening operation event type enumeration. Represents event type (OK, NG, error, etc.) from FormatEvent.
/// </summary>
public enum Event {
    /// <summary>
    ///     기타
    ///     miscellaneous
    /// </summary>
    [Description("Etc.")]
    Etc,

    /// <summary>
    ///     체결 OK
    ///     fastening OK
    /// </summary>
    [Description("Fastening OK")]
    FastenOk,

    /// <summary>
    ///     체결 NG
    ///     fastening NG
    /// </summary>
    [Description("Fastening NG")]
    FastenNg,

    /// <summary>
    ///     체결 / 풀림 전환
    ///     fasten-loosen toggle
    /// </summary>
    [Description("F / L")]
    Fl,

    /// <summary>
    ///     프리셋 변경
    ///     preset change
    /// </summary>
    [Description("Preset change")]
    PresetChange,

    /// <summary>
    ///     알람 리셋
    ///     alarm reset
    /// </summary>
    [Description("Alarm reset")]
    AlarmReset,

    /// <summary>
    ///     에러
    ///     error
    /// </summary>
    [Description("Error")]
    Error,

    /// <summary>
    ///     바코드
    ///     barcode
    /// </summary>
    [Description("Barcode")]
    Barcode,

    /// <summary>
    ///     나사 취소
    ///     screw cancel
    /// </summary>
    [Description("Screw cancel")]
    ScrewCancel,

    /// <summary>
    ///     나사 카운트 리셋
    ///     screw count reset
    /// </summary>
    [Description("Screw count reset")]
    ScrewCountReset,

    /// <summary>
    ///     스텝 OK (PRO X)
    ///     step OK (PRO X)
    /// </summary>
    [Description("Step OK")]
    StepOk = 100,

    /// <summary>
    ///     스텝 NG (PRO X)
    ///     step NG (PRO X)
    /// </summary>
    [Description("Step NG")]
    StepNg,

    /// <summary>
    ///     작업 OK (PRO X)
    ///     job OK (PRO X)
    /// </summary>
    [Description("Job OK")]
    JobOk,

    /// <summary>
    ///     작업 NG (PRO X)
    ///     job NG (PRO X)
    /// </summary>
    [Description("Job NG")]
    JobNg,

    /// <summary>
    ///     작업 중단 (PRO X)
    ///     job abort (PRO X)
    /// </summary>
    [Description("Job Abort")]
    JobAbort,

    /// <summary>
    ///     건너뛰기 (PRO X)
    ///     skip (PRO X)
    /// </summary>
    [Description("Skip")]
    Skip,

    /// <summary>
    ///     되돌리기 (PRO X)
    ///     back (PRO X)
    /// </summary>
    [Description("Back")]
    Back,

    /// <summary>
    ///     스텝 리셋 (PRO X)
    ///     reset step (PRO X)
    /// </summary>
    [Description("Reset Step")]
    ResetStep,

    /// <summary>
    ///     작업 리셋 (PRO X)
    ///     reset job (PRO X)
    /// </summary>
    [Description("Reset Job")]
    ResetJob,

    /// <summary>
    ///     작업 시작 (PRO X)
    ///     start job (PRO X)
    /// </summary>
    [Description("Start Job")]
    StartJob
}