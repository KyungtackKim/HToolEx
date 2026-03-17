using HTool.Device.Transport;

namespace Tester.TestHelpers;

/// <summary>
///     테스트용 ITransport 구현. NSubstitute가 ReadOnlySpan 매개변수를 모킹할 수 없으므로 수동 구현.
///     Test ITransport implementation. Hand-written because NSubstitute cannot mock ReadOnlySpan parameters.
/// </summary>
internal sealed class FakeTransport : ITransport {
    /// <summary>
    ///     전송된 패킷을 기록하는 목록.
    ///     list recording written packets.
    /// </summary>
    public List<byte[]> WrittenPackets { get; } = [];

    /// <summary>
    ///     현재 연결 상태.
    ///     current connection state.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    ///     데이터 수신 이벤트. 동기적으로 소비해야 한다.
    ///     data received event. Must be consumed synchronously.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>>? DataReceived;

    /// <summary>
    ///     연결 상태 변경 이벤트.
    ///     connection state changed event.
    /// </summary>
    public event Action<bool>? ConnectionChanged;

    /// <summary>
    ///     비동기 연결을 시뮬레이션한다.
    ///     simulates an asynchronous connection.
    /// </summary>
    /// <param name="target">연결 대상 / connection target</param>
    /// <param name="option">연결 옵션 / connection option</param>
    /// <param name="ct">취소 토큰 / cancellation token</param>
    /// <returns>항상 true / always true</returns>
    public Task<bool> ConnectAsync(string target, int option, CancellationToken ct = default) {
        // 연결 상태를 true로 설정
        // set connected state to true
        IsConnected = true;
        // 연결 상태 변경 이벤트 발생
        // raise connection changed event
        ConnectionChanged?.Invoke(true);
        // 성공 결과 반환
        // return success result
        return Task.FromResult(true);
    }

    /// <summary>
    ///     연결을 닫는다.
    ///     closes the connection.
    /// </summary>
    public void Close() {
        // 연결 상태를 false로 설정
        // set connected state to false
        IsConnected = false;
        // 연결 상태 변경 이벤트 발생
        // raise connection changed event
        ConnectionChanged?.Invoke(false);
    }

    /// <summary>
    ///     데이터를 전송한다. 패킷을 기록 목록에 추가.
    ///     writes data. Adds packet to the recorded list.
    /// </summary>
    /// <param name="data">전송할 데이터 / data to write</param>
    /// <returns>항상 true / always true</returns>
    public bool Write(ReadOnlySpan<byte> data) {
        // 패킷을 배열로 복사하여 기록
        // copy packet to array and record
        WrittenPackets.Add(data.ToArray());
        // 성공 반환
        // return success
        return true;
    }

    /// <summary>
    ///     리소스를 해제한다.
    ///     disposes resources.
    /// </summary>
    public void Dispose() {
        // 연결 닫기
        // close connection
        Close();
    }

    /// <summary>
    ///     수신 데이터를 시뮬레이션한다.
    ///     simulates received data.
    /// </summary>
    /// <param name="data">수신할 데이터 / data to receive</param>
    public void SimulateReceive(byte[] data) {
        // 데이터 수신 이벤트 발생
        // raise data received event
        DataReceived?.Invoke(data);
    }
}