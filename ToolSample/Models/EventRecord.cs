namespace ToolSample.Models;

/// <summary>
///     이벤트 레코드 모델. Event 클래스의 평탄화 표현. DataGrid 표시용.
///     Event record model. Flattened representation of Event class. For DataGrid display.
/// </summary>
public sealed class EventRecord {
	/// <summary>
	///     이벤트 ID.
	///     Event ID.
	/// </summary>
	public uint Id { get; init; }

	/// <summary>
	///     이벤트 일시.
	///     Event date/time.
	/// </summary>
	public string DateTime { get; init; } = "";

	/// <summary>
	///     프리셋 번호.
	///     Preset number.
	/// </summary>
	public int Preset { get; init; }

	/// <summary>
	///     이벤트 상태 (OK/NG/...).
	///     Event status (OK/NG/...).
	/// </summary>
	public string Status { get; init; } = "";

	/// <summary>
	///     회전 방향.
	///     Rotation direction.
	/// </summary>
	public string Direction { get; init; } = "";

	/// <summary>
	///     목표 토크.
	///     Target torque.
	/// </summary>
	public float TargetTorque { get; init; }

	/// <summary>
	///     실측 토크.
	///     Actual torque.
	/// </summary>
	public float Torque { get; init; }

	/// <summary>
	///     토크 단위.
	///     Torque unit.
	/// </summary>
	public string Unit { get; init; } = "";

	/// <summary>
	///     총 각도.
	///     Total angle.
	/// </summary>
	public int Angle { get; init; }

	/// <summary>
	///     모터 속도 (RPM).
	///     Motor speed (RPM).
	/// </summary>
	public int Speed { get; init; }

	/// <summary>
	///     바코드 데이터.
	///     Barcode data.
	/// </summary>
	public string Barcode { get; init; } = "";

	/// <summary>
	///     오류 코드.
	///     Error code.
	/// </summary>
	public int Error { get; init; }
}