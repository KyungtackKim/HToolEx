using System.Runtime.CompilerServices;
using HTool.Type;

namespace HTool.Device;

/// <summary>
///     MODBUS 통신 도구 팩토리 클래스. 팩토리 패턴을 사용하여 프로토콜별 구현체를 생성합니다.
///     MODBUS communication tool factory class. Uses factory pattern to create protocol-specific implementations.
/// </summary>
/// <remarks>
///     이 클래스는 추상 클래스이며 정적 팩토리 메서드만 제공합니다. 인스턴스화할 수 없습니다.
///     This class is abstract and only provides static factory methods. Cannot be instantiated.
/// </remarks>
public abstract class Tool {
    /// <summary>
    ///     통신 타입에 따른 ITool 구현체를 생성합니다. RTU는 HcRtu, TCP는 HcTcp 인스턴스를 반환합니다.
    ///     Creates ITool implementation based on communication type. Returns HcRtu for RTU, HcTcp for TCP.
    /// </summary>
    /// <param name="type">통신 타입 (RTU 또는 TCP) / Communication type (RTU or TCP)</param>
    /// <returns>생성된 ITool 구현체 인스턴스 / Created ITool implementation instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">지원하지 않는 통신 타입인 경우 / Thrown when communication type is not supported</exception>
    public static ITool Create(ComTypes type) {
        return type switch {
            ComTypes.Rtu => new HcRtu(),
            ComTypes.Tcp => new HcTcp(),
            _            => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    ///     바이트 값이 라이브러리에서 지원하는 MODBUS 함수 코드인지 확인합니다. 성능 최적화를 위해 인라인됩니다.
    ///     Checks if byte value is a MODBUS function code supported by the library. Inlined for performance optimization.
    /// </summary>
    /// <param name="v">확인할 MODBUS 함수 코드 바이트 값 / MODBUS function code byte value to check</param>
    /// <returns>
    ///     지원하는 함수 코드이면 true (0x03, 0x04, 0x06, 0x10, 0x11, 0x64, 0x65)
    ///     True if supported function code (0x03, 0x04, 0x06, 0x10, 0x11, 0x64, 0x65)
    /// </returns>
    /// <remarks>
    ///     AggressiveInlining 속성으로 메서드 호출 오버헤드를 제거하여 프레임 분석 성능을 향상시킵니다.
    ///     AggressiveInlining attribute removes method call overhead to improve frame analysis performance.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKnownCode(byte v) {
        // 지원하는 함수 코드 확인
        // check supported function codes
        return v is (byte)CodeTypes.ReadHoldingReg or (byte)CodeTypes.ReadInputReg
            or (byte)CodeTypes.ReadInfoReg or (byte)CodeTypes.WriteSingleReg
            or (byte)CodeTypes.WriteMultiReg or (byte)CodeTypes.Graph
            or (byte)CodeTypes.GraphRes;
    }
}