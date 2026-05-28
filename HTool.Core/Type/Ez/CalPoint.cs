using System.ComponentModel;

namespace HTool.Core.Type.Ez;

/// <summary>
///     캘리브레이션 포인트 열거형. 정격 용량 대비 백분율을 나타냅니다.
///     calibration point enumeration. Represents percentage of rated capacity.
/// </summary>
/// <remarks>
///     사용처: <c>HTool.Format.Ez.CalibrationData</c>.
///     used by: <c>HTool.Format.Ez.CalibrationData</c>.
/// </remarks>
public enum CalPoint {
	/// <summary>
	///     영점 (0%)
	///     zero point (0%)
	/// </summary>
	[Description("0%")]
    PointZero = 0,

	/// <summary>
	///     음방향 10%
	///     minus 10%
	/// </summary>
	[Description("-10%")]
    Point10M = -10,

	/// <summary>
	///     음방향 20%
	///     minus 20%
	/// </summary>
	[Description("-20%")]
    Point20M = -20,

	/// <summary>
	///     음방향 40%
	///     minus 40%
	/// </summary>
	[Description("-40%")]
    Point40M = -40,

	/// <summary>
	///     음방향 50%
	///     minus 50%
	/// </summary>
	[Description("-50%")]
    Point50M = -50,

	/// <summary>
	///     음방향 60%
	///     minus 60%
	/// </summary>
	[Description("-60%")]
    Point60M = -60,

	/// <summary>
	///     음방향 80%
	///     minus 80%
	/// </summary>
	[Description("-80%")]
    Point80M = -80,

	/// <summary>
	///     음방향 100%
	///     minus 100%
	/// </summary>
	[Description("-100%")]
    Point100M = -100,

	/// <summary>
	///     양방향 10%
	///     plus 10%
	/// </summary>
	[Description("+10%")]
    Point10P = 10,

	/// <summary>
	///     양방향 20%
	///     plus 20%
	/// </summary>
	[Description("+20%")]
    Point20P = 20,

	/// <summary>
	///     양방향 40%
	///     plus 40%
	/// </summary>
	[Description("+40%")]
    Point40P = 40,

	/// <summary>
	///     양방향 50%
	///     plus 50%
	/// </summary>
	[Description("+50%")]
    Point50P = 50,

	/// <summary>
	///     양방향 60%
	///     plus 60%
	/// </summary>
	[Description("+60%")]
    Point60P = 60,

	/// <summary>
	///     양방향 80%
	///     plus 80%
	/// </summary>
	[Description("+80%")]
    Point80P = 80,

	/// <summary>
	///     양방향 100%
	///     plus 100%
	/// </summary>
	[Description("+100%")]
    Point100P = 100
}