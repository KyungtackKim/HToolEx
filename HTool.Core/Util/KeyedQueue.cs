using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HTool.Core.Util;

/// <summary>
///     <see cref="KeyedQueue{T, TKey}" />에서 항목 추가 시 중복 처리 방식 정의
///     defines how <see cref="KeyedQueue{T, TKey}" /> handles duplicates when enqueuing.
/// </summary>
public enum EnqueueMode {
    /// <summary>
    ///     키 고유성 강제: 동일한 키를 가진 항목이 이미 존재하면
    ///     추가 시도를 건너뛰고 false 반환
    ///     enforce uniqueness by key: if an item with the same key is already present,
    ///     the enqueue attempt is skipped and returns false.
    /// </summary>
    EnforceUnique = 0,

    /// <summary>
    ///     키 중복 허용: 항목을 항상 추가하고 키별 내부 카운터 증가
    ///     allow duplicates by key: the item is always enqueued, and an internal
    ///     per-key counter is incremented.
    /// </summary>
    AllowDuplicate = 1
}

/// <summary>
///     큐 작업 중 키 선택자 함수 실패 시 발생하는 예외
///     exception thrown when the key selector function fails during queue operations.
/// </summary>
public sealed class KeySelectorException(string message, Exception innerException) : InvalidOperationException(message, innerException);

/// <summary>
///     키 기반 중복 방지 기능이 있는 고성능 스레드 안전 FIFO 큐. HTool의 메시지 큐 관리에 사용됩니다.
///     high-performance thread-safe FIFO queue with key-based duplicate prevention. Used for HTool's message queue
///     management.
/// </summary>
/// <remarks>
///     <para>주요 기능 / Key Features:</para>
///     <list type="bullet">
///         <item>
///             <description>항목의 FIFO 순서 유지 / maintains FIFO ordering for items.</description>
///         </item>
///         <item>
///             <description>
///                 각 항목의 키 추적 (키 선택자를 통해) 중복 방지 또는 허용 / tracks keys for each item (via a key selector) to prevent
///                 or allow duplicates.
///             </description>
///         </item>
///         <item>
///             <description>
///                 타임아웃 및 취소와 함께 논블로킹/블로킹 Dequeue/Peek 지원 / supports non-blocking and blocking Dequeue/Peek with
///                 timeout and cancellation.
///             </description>
///         </item>
///         <item>
///             <description>
///                 키 기반 쿼리 (포함 여부/대기 개수) 및 선택적 중간 큐 제거 제공 / provides key-based queries (contains/pending counts)
///                 and optional mid-queue removal.
///             </description>
///         </item>
///     </list>
///     <para>
///         <strong>성능 특성 / Performance Characteristics:</strong>
///     </para>
///     <list type="bullet">
///         <item>
///             <description>Enqueue/Dequeue: 평균 O(1) / O(1) average.</description>
///         </item>
///         <item>
///             <description>키 기반 쿼리: O(1) / key-based queries: O(1).</description>
///         </item>
///         <item>
///             <description>중간 큐 제거: O(n) — 드물게 사용 권장 / mid-queue removal: O(n) — use sparingly.</description>
///         </item>
///     </list>
///     <para>
///         <strong>스레드 안전성 / Thread Safety:</strong> 단일 모니터 락. 모든 공개 작업은 스레드 안전 / single monitor lock. All public
///         operations are thread-safe.
///     </para>
///     <para>
///         <strong>메모리 / Memory:</strong> 재계산 방지를 위해 키가 항목과 함께 저장됨. 크기가 많이 줄어들면 <see cref="TrimExcess" /> 호출 권장 / keys
///         are stored with items to avoid recomputation. Call <see cref="TrimExcess" /> if
///         size shrinks a lot.
///     </para>
/// </remarks>
public class KeyedQueue<T, TKey> : IDisposable where TKey : notnull {
    /// <summary>
    ///     키별 카운터. 키가 값 N과 함께 존재하면 해당 키에 대해 N개의 대기 항목이 큐에 있음
    ///     per-key counters. If a key is present with value N, there are N pending items for that key in the queue.
    /// </summary>
    private readonly Dictionary<TKey, int> _keyCounts;

    /// <summary>
    ///     항목 T를 키 TKey로 매핑하는 사용자 제공 함수
    ///     user-supplied function that maps an item T to its key TKey.
    /// </summary>
    private readonly Func<T, TKey> _keySelector;

    /// <summary>
    ///     큐와 키 카운터를 모두 보호하는 단일 락 객체
    ///     single lock object guarding both the queue and the key counters.
    /// </summary>
    // TODO: .NET 10에서 System.Threading.Lock으로 전환 / migrate to System.Threading.Lock in .NET 10
    private readonly object _lock = new();

    /// <summary>
    ///     항목의 FIFO 저장소 (디큐 시 재계산 방지를 위해 계산된 키와 함께 저장)
    ///     FIFO store of items, alongside their computed keys (to avoid recomputing on dequeue).
    /// </summary>
    private readonly Queue<(T Item, TKey Key)> _queue;

    /// <summary>
    ///     Disposed 플래그 (빠른 실패를 위해 락 없이 확인; 상태 변경 시 락으로 보호)
    ///     disposed flag (checked without lock for fast-fail; guarded by lock for state mutation).
    /// </summary>
    private volatile bool _disposed;

    /// <summary>
    ///     <see cref="KeyedQueue{T, TKey}" /> 클래스의 새 인스턴스 초기화
    ///     initializes a new instance of the <see cref="KeyedQueue{T, TKey}" /> class.
    /// </summary>
    /// <param name="keySelector">
    ///     항목의 키를 반환하는 함수. null이 아니어야 하며 빠르고 부작용이 없어야 함.
    ///     이 함수가 예외를 발생시키면 오류가 <see cref="KeySelectorException" />으로 래핑됨.
    ///     a function that returns the key for an item. Must be non-null and should be fast and side-effect free.
    ///     If this function throws, the error is wrapped in <see cref="KeySelectorException" />.
    /// </param>
    /// <param name="keyComparer">
    ///     <typeparamref name="TKey" />에 대한 선택적 동등성 비교자. null이면 기본 비교자 사용.
    ///     optional equality comparer for <typeparamref name="TKey" />. If null, the default comparer is used.
    /// </param>
    /// <param name="capacity">
    ///     기본 큐에 대한 선택적 초기 용량 힌트.
    ///     optional initial capacity hint for the underlying queue.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="keySelector" />가 null일 때 발생 / thrown when
    ///     <paramref name="keySelector" /> is null.
    /// </exception>
    protected KeyedQueue(
        Func<T, TKey>            keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        int                      capacity    = 0) {
        // ensure capacity is non-negative / 용량 확인
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        // ensure key selector is provided / 키 선택자 설정
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        // create queue with optional initial capacity / 큐 생성
        _queue = capacity > 0 ? new Queue<(T Item, TKey Key)>(capacity) : new Queue<(T Item, TKey Key)>();
        // create key counter dictionary / 키 카운터 생성
        _keyCounts = new Dictionary<TKey, int>(keyComparer);
    }

    /// <summary>
    ///     큐의 현재 항목 수 가져오기. O(1). 스레드 안전.
    ///     gets the current number of items in the queue. O(1). Thread-safe.
    /// </summary>
    public int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            // ensure queue is not disposed / 큐 해제 여부 확인
            ThrowIfDisposed();
            // lock: thread-safe read of queue count / lock: 큐 카운트 스레드 안전 읽기
            lock (_lock) {
                // return queue count / 큐 카운트 반환
                return _queue.Count;
            }
        }
    }

    /// <summary>
    ///     큐가 현재 비어 있는지 여부 가져오기. O(1). 스레드 안전.
    ///     gets whether the queue is currently empty. O(1). Thread-safe.
    /// </summary>
    public bool IsEmpty {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            // ensure queue is not disposed / 큐 해제 여부 확인
            ThrowIfDisposed();
            // lock: thread-safe read of queue count / lock: 큐 카운트 스레드 안전 읽기
            lock (_lock) {
                // return whether queue is empty / 큐 비어있는지 여부 반환
                return _queue.Count == 0;
            }
        }
    }

    /// <summary>
    ///     현재 추적 중인 고유 키 수 가져오기. O(1). 스레드 안전.
    ///     gets the number of unique keys currently tracked. O(1). Thread-safe.
    /// </summary>
    public int UniqueKeyCount {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            // ensure queue is not disposed / 큐 해제 여부 확인
            ThrowIfDisposed();
            // lock: thread-safe read of key counts / lock: 키 카운터 스레드 안전 읽기
            lock (_lock) {
                // return unique key count / 고유 키 수 반환
                return _keyCounts.Count;
            }
        }
    }

    /// <summary>
    ///     큐 해제: 상태를 지우고 대기 중인 스레드를 깨워 종료할 수 있게 함.
    ///     해제 후 모든 공개 멤버는 <see cref="ObjectDisposedException" /> 발생.
    ///     disposes the queue: clears state and wakes any waiting threads so they can exit.
    ///     After disposal, all public members throw <see cref="ObjectDisposedException" />.
    /// </summary>
    public void Dispose() {
        // early return if already disposed / 이미 해제된 경우 조기 반환
        if (_disposed)
            // exit method / 메서드 종료
            return;

        // lock: clear state and wake waiters atomically / lock: 상태 정리 및 대기 스레드 깨우기를 원자적으로 수행
        lock (_lock) {
            // double-check disposed flag under lock / 락 내에서 해제 플래그 이중 확인
            if (_disposed)
                // exit method / 메서드 종료
                return;
            // set disposed flag / disposed 플래그 설정
            _disposed = true;
            // clear all queued items / 큐 지우기
            _queue.Clear();
            // clear all key counters / 키 카운터 지우기
            _keyCounts.Clear();
            // wake up any waiting threads / 대기 중인 스레드 깨우기
            Monitor.PulseAll(_lock);
        }

        // suppress finalizer / 종료자 억제
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     키 기반 큐 생성
    ///     create the keyed-queue
    /// </summary>
    /// <param name="keySelector">키 선택자 / key selector</param>
    /// <param name="keyComparer">키 비교자 / key comparer</param>
    /// <param name="capacity">용량 / capacity</param>
    /// <returns>키 기반 큐 인스턴스 / keyed-queue instance</returns>
    public static KeyedQueue<T, TKey> Create(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null, int capacity = 0) {
        // ensure capacity is non-negative / 용량 확인
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        // create and return new instance / 인스턴스 생성
        return new KeyedQueue<T, TKey>(keySelector, keyComparer, capacity);
    }

    /// <summary>
    ///     지정된 중복 처리 모드로 항목 추가 시도.
    ///     평균 O(1). 스레드 안전. <see cref="Monitor.PulseAll(object)" />로 대기자 깨움.
    ///     attempts to enqueue an item with the specified duplicate-handling mode.
    ///     O(1) average. Thread-safe. Wakes waiters via <see cref="Monitor.PulseAll(object)" />.
    /// </summary>
    /// <param name="item">추가할 항목 / item to enqueue</param>
    /// <param name="mode">중복 처리 모드 / duplicate handling mode</param>
    /// <returns>성공 시 true, 중복으로 추가 안 됨 시 false / true if added, false if duplicate rejected</returns>
    /// <exception cref="KeySelectorException">키 선택자가 예외 발생 시 / if key selector throws.</exception>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEnqueue(T item, EnqueueMode mode = EnqueueMode.EnforceUnique) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // key extracted from the item / 항목에서 추출할 키
        TKey key;
        // guard: wrap key selector failures as KeySelectorException / guard: 키 선택자 실패 시 KeySelectorException으로 래핑
        try {
            // extract key from item / 항목에서 키 추출
            key = _keySelector(item);
        } catch (Exception ex) {
            // key selector threw during key extraction / 키 선택자가 예외를 발생시킨 경우
            throw new KeySelectorException("Key selector function failed during enqueue.", ex);
        }

        // lock: check duplicate and enqueue atomically / lock: 중복 확인 및 인큐를 원자적으로 수행
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // check if key already exists in queue / 키가 이미 존재하는지 확인
            var exists = _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
            // reject if enforce-unique and key exists / 모드 확인
            if (mode == EnqueueMode.EnforceUnique && exists)
                // duplicate key rejected / 중복 키 거부
                return false;
            // enqueue the item with its key / 항목 추가
            _queue.Enqueue((item, key));
            // avoid second dictionary lookup using ref access / ref 접근으로 두 번째 사전 조회 방지
            ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyCounts, key, out var added);
            // update key count slot / 슬롯 업데이트
            slot = added ? 1 : slot + 1;
            // wake waiting consumers / 대기 중인 소비자 깨우기
            Monitor.PulseAll(_lock);
            // return success / true 반환
            return true;
        }
    }

    /// <summary>
    ///     단일 락 획득으로 항목 배치 추가 시도. 승인/건너뛰기 및 실패(키 선택자 오류) 카운트 반환.
    ///     attempts to enqueue a batch of items with a single lock acquisition.
    ///     returns counts for accepted/skipped and failures (key selector errors).
    /// </summary>
    /// <param name="items">추가할 항목 컬렉션 / collection of items to enqueue</param>
    /// <param name="mode">중복 처리 모드 / duplicate handling mode</param>
    /// <returns>승인, 건너뛰기, 실패 항목 결과 / result with accepted, skipped, and failed items</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" />가 null인 경우 / if <paramref name="items" /> is null.</exception>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    public BatchEnqueueResult<T> TryEnqueueRange(IEnumerable<T> items, EnqueueMode mode = EnqueueMode.EnforceUnique) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();
        // ensure items collection is not null / 항목 null 확인
        ArgumentNullException.ThrowIfNull(items);
        // initialize accepted counter / 승인 카운터 초기화
        var accepted = 0;
        // initialize skipped counter / 건너뛰기 카운터 초기화
        var skipped = 0;
        // lazy-init failure list / 실패 목록 지연 초기화
        List<(T Item, Exception Exception)>? failures = null;

        // lock: batch enqueue with single lock acquisition / lock: 단일 락 획득으로 배치 인큐 수행
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // iterate over each item in collection / 항목 반복
            foreach (var item in items) {
                // key extracted from the current item / 현재 항목에서 추출할 키
                TKey key;
                // guard: record key selector failures per item and continue / guard: 개별 항목의 키 선택자 실패를 기록하고 계속 진행
                try {
                    // extract key from item / 항목에서 키 추출
                    key = _keySelector(item);
                } catch (Exception ex) {
                    // key selector threw — record failure and continue with next item / 키 선택자가 예외를 발생시킨 경우 — 실패 목록에 기록
                    failures ??= [];
                    // append failed item and wrapped exception / 실패 항목과 래핑된 예외 추가
                    failures.Add((item, new KeySelectorException("Key selector failed in batch enqueue.", ex)));
                    // skip to next item in batch / 배치의 다음 항목으로 건너뛰기
                    continue;
                }

                // check if the key already exists in queue / 키가 이미 존재하는지 확인
                var exists = _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
                // check if item should be skipped due to uniqueness / 모드 확인
                if (mode == EnqueueMode.EnforceUnique && exists) {
                    // skip the duplicate item / 항목 건너뛰기
                    skipped++;
                    // move to next item in batch / 배치의 다음 항목으로 이동
                    continue;
                }

                // enqueue the item with its key / 항목 추가
                _queue.Enqueue((item, key));
                // update key count using ref access / 슬롯 업데이트
                ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyCounts, key, out var added);
                // set count to 1 if new key, otherwise increment / 새 키이면 1, 아니면 증가
                slot = added ? 1 : slot + 1;
                // increment accepted counter / 승인 카운트 증가
                accepted++;
            }

            // wake any consumers waiting for items / 대기 중인 소비자 깨우기
            if (accepted > 0)
                Monitor.PulseAll(_lock);
        }

        // get failures list or empty / 실패 목록 가져오기 (또는 빈 목록)
        var failuresOrEmpty = (IReadOnlyList<(T, Exception)>?)failures ?? [];
        // return batch result / 결과 반환
        return new BatchEnqueueResult<T>(accepted, skipped, failuresOrEmpty);
    }

    /// <summary>
    ///     블로킹 없이 FIFO 순서로 다음 항목 제거 및 반환 시도. O(1). 스레드 안전.
    ///     attempts to remove and return the next item in FIFO order without blocking. O(1). Thread-safe.
    /// </summary>
    /// <param name="item">제거된 항목 (성공 시) / dequeued item (if successful)</param>
    /// <returns>항목 제거 성공 시 true, 큐가 비어있으면 false / true if item removed, false if queue empty</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out T item) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: dequeue and update key count atomically / lock: 디큐 및 키 카운트 갱신을 원자적으로 수행
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // check if the queue is empty / 큐가 비어있는지 확인
            if (_queue.Count == 0) {
                // set default output value / 기본값 설정
                item = default!;
                // queue is empty, nothing to dequeue / 큐가 비어 있어 디큐 불가
                return false;
            }

            // dequeue the front item / 항목 디큐
            var (val, key) = _queue.Dequeue();
            // decrement the key count / 키 카운트 감소
            DecrementKeyCount(key);
            // set the output item / 항목 설정
            item = val;
            // item successfully dequeued / 항목 디큐 성공
            return true;
        }
    }

    /// <summary>
    ///     다음 항목 제거 및 반환 시도, 선택적으로 항목이 사용 가능할 때까지 블로킹.
    ///     <see cref="Monitor.Wait(object, int)" /> 사용하여 대기하고 <see cref="Monitor.PulseAll(object)" />로 깨움.
    ///     attempts to remove and return the next item, optionally blocking until an item becomes available.
    ///     uses <see cref="Monitor.Wait(object, int)" /> and wakes via <see cref="Monitor.PulseAll(object)" />.
    /// </summary>
    /// <param name="item">항목 / item</param>
    /// <param name="timeoutMs">0 = 논블로킹, -1 = 무한, &gt;0 = 밀리초 / 0 = non-blocking, -1 = infinite, &gt;0 = milliseconds.</param>
    /// <param name="cancellationToken">선택적 취소 / optional cancellation.</param>
    /// <returns>
    ///     항목이 디큐되면 <c>true</c>, 타임아웃/취소시 <c>false</c> / <c>true</c> if an item was dequeued; <c>false</c> on
    ///     timeout/cancellation.
    /// </returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    public bool TryDequeue(out T item, int timeoutMs, CancellationToken cancellationToken = default) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();
        // start stopwatch if timeout is positive / 타임아웃이 양수이면 스톱워치 시작
        var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;

        // lock: block until item available then dequeue / lock: 항목이 사용 가능할 때까지 대기 후 디큐
        lock (_lock) {
            // wait until queue has items / 항목 대기
            while (_queue.Count == 0) {
                // re-check disposed under lock / 락 내에서 재확인
                ThrowIfDisposed_NoInline();
                // check if cancellation requested or non-blocking / 취소 요청 또는 타임아웃 0 확인
                if (cancellationToken.IsCancellationRequested || timeoutMs == 0) {
                    // set default output value / 기본값 설정
                    item = default!;
                    // cancelled or non-blocking mode with empty queue / 취소되었거나 논블로킹 모드에서 큐가 비어 있음
                    return false;
                }

                // calculate remaining wait time / 남은 시간 계산
                var remain = timeoutMs >= 0 ? Math.Max(0, timeoutMs - (int)sw!.ElapsedMilliseconds) : Timeout.Infinite;
                // wait for signal from producer / 신호 대기
                if (Monitor.Wait(_lock, remain))
                    // signaled, re-check queue / 신호 수신, 큐 재확인
                    continue;
                // timeout expired / 타임아웃
                item = default!;
                // wait timed out without item available / 대기 시간 초과, 항목 없음
                return false;
            }

            // dequeue the front item / 항목 디큐
            var (val, key) = _queue.Dequeue();
            // decrement the key count / 키 카운트 감소
            DecrementKeyCount(key);
            // set the output item / 항목 설정
            item = val;
            // item successfully dequeued after wait / 대기 후 항목 디큐 성공
            return true;
        }
    }

    /// <summary>
    ///     블로킹 없이 FIFO 순서로 다음 항목 읽기(제거하지 않음) 시도. O(1). 스레드 안전.
    ///     attempts to read (without removing) the next item in FIFO order without blocking. O(1). Thread-safe.
    /// </summary>
    /// <param name="item">읽은 항목 (성공 시) / peeked item (if successful)</param>
    /// <returns>항목 읽기 성공 시 true, 큐가 비어있으면 false / true if item read, false if queue empty</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out T item) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: thread-safe read of front item / lock: 선두 항목 스레드 안전 읽기
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // check if the queue is empty / 큐가 비어있는지 확인
            if (_queue.Count == 0) {
                // set default output value / 기본값 설정
                item = default!;
                // queue is empty, nothing to peek / 큐가 비어 있어 피크 불가
                return false;
            }

            // peek the front item / 항목 피크
            item = _queue.Peek().Item;
            // item available at front of queue / 큐 선두에 항목 존재
            return true;
        }
    }

    /// <summary>
    ///     다음 항목 읽기(제거하지 않음) 시도, 선택적으로 항목이 사용 가능할 때까지 블로킹.
    ///     attempts to read (without removing) the next item, optionally blocking until an item becomes available.
    /// </summary>
    /// <param name="item">항목 / item</param>
    /// <param name="timeoutMs">0 = 논블로킹, -1 = 무한, &gt;0 = 밀리초 / 0 = non-blocking, -1 = infinite, &gt;0 = milliseconds.</param>
    /// <param name="cancellationToken">선택적 취소 / optional cancellation.</param>
    /// <returns>
    ///     항목이 있으면 <c>true</c>, 타임아웃/취소시 <c>false</c> / <c>true</c> if an item was available; <c>false</c> on
    ///     timeout/cancellation.
    /// </returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    public bool TryPeek(out T item, int timeoutMs, CancellationToken cancellationToken = default) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();
        // start stopwatch if timeout is positive / 타임아웃이 양수이면 스톱워치 시작
        var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;

        // lock: block until item available then peek / lock: 항목이 사용 가능할 때까지 대기 후 피크
        lock (_lock) {
            // wait until queue has items / 항목 대기
            while (_queue.Count == 0) {
                // re-check disposed under lock / 락 내에서 재확인
                ThrowIfDisposed_NoInline();
                // check if cancellation requested or non-blocking / 취소 요청 또는 타임아웃 0 확인
                if (cancellationToken.IsCancellationRequested || timeoutMs == 0) {
                    // set default output value / 기본값 설정
                    item = default!;
                    // cancelled or non-blocking mode with empty queue / 취소되었거나 논블로킹 모드에서 큐가 비어 있음
                    return false;
                }

                // calculate remaining wait time / 남은 시간 계산
                var remain = timeoutMs >= 0 ? Math.Max(0, timeoutMs - (int)sw!.ElapsedMilliseconds) : Timeout.Infinite;
                // wait for signal from producer / 신호 대기
                if (Monitor.Wait(_lock, remain))
                    // signaled, re-check queue / 신호 수신, 큐 재확인
                    continue;
                // timeout expired / 타임아웃
                item = default!;
                // wait timed out without item available / 대기 시간 초과, 항목 없음
                return false;
            }

            // peek the front item / 항목 피크
            item = _queue.Peek().Item;
            // item available at front of queue / 큐 선두에 항목 존재
            return true;
        }
    }

    /// <summary>
    ///     지정된 키를 가진 항목이 하나 이상 존재하는지 확인. O(1). 스레드 안전.
    ///     checks whether at least one item with the specified key exists. O(1). Thread-safe.
    /// </summary>
    /// <param name="key">확인할 키 / key to check</param>
    /// <returns>키가 존재하면 true, 없으면 false / true if key exists, false otherwise</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();
        // lock: thread-safe key existence check / lock: 키 존재 여부 스레드 안전 확인
        lock (_lock) {
            // check if the key exists with count > 0 / 키 존재 여부 확인
            return _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
        }
    }

    /// <summary>
    ///     지정된 키를 가진 대기 중인 항목 수 가져오기. O(1). 스레드 안전.
    ///     gets how many items with the specified key are pending. O(1). Thread-safe.
    /// </summary>
    /// <param name="key">확인할 키 / key to check</param>
    /// <returns>해당 키를 가진 항목 수 (0 이상) / number of items with the key (0 or more)</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PendingCountByKey(TKey key) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();
        // lock: thread-safe read of per-key count / lock: 키별 카운트 스레드 안전 읽기
        lock (_lock) {
            // get the count for this key / 카운트 가져오기
            return _keyCounts.GetValueOrDefault(key, 0);
        }
    }

    /// <summary>
    ///     지정된 키를 가진 항목의 첫 번째 항목 제거 (O(n)). 나머지 항목의 FIFO 순서 유지.
    ///     removes the first occurrence of an item with the specified key (O(n)).
    ///     preserves FIFO order of the remaining items.
    /// </summary>
    /// <param name="key">제거할 항목의 키 / key of item to remove</param>
    /// <returns>항목 제거 성공 시 true, 키 없으면 false / true if removed, false if key not found</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    public bool TryRemoveByKey(TKey key) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: scan queue and remove first matching item / lock: 큐 순회 및 첫 번째 일치 항목 제거
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // check if the key exists in counts / 키 존재 여부 확인
            if (!_keyCounts.TryGetValue(key, out var cnt) || cnt == 0)
                // key not found, nothing to remove / 키 없음, 제거할 항목 없음
                return false;
            // get the key comparer / 비교자 가져오기
            var cmp = _keyCounts.Comparer;
            // track whether a matching item was removed / 일치 항목 제거 여부 추적
            var removed = false;
            // snapshot current queue length for rotation / 순회를 위한 현재 큐 길이 스냅샷
            var n = _queue.Count;
            // rotate through the queue once / 큐를 한 번 순회
            for (var i = 0; i < n; i++) {
                // dequeue front entry / 엔트리 디큐
                var entry = _queue.Dequeue();
                // check if this entry matches the target key / 키 일치 확인
                if (!removed && cmp.Equals(entry.Key, key)) {
                    // mark as removed / 제거 플래그 설정
                    removed = true;
                    // decrement the key count / 키 카운트 감소
                    DecrementKeyCount(key);
                    // drop this entry / 항목 드롭
                    continue;
                }

                // re-enqueue non-matching entry / 엔트리 인큐
                _queue.Enqueue(entry);
            }

            // return whether an item was removed / 항목 제거 여부 반환
            return removed;
        }
    }

    /// <summary>
    ///     지정된 키를 가진 모든 항목 제거 (O(n)). 나머지 항목의 상대적 순서 유지.
    ///     removes all items with the specified key (O(n)).
    ///     preserves the relative order of the remaining items.
    /// </summary>
    /// <param name="key">제거할 항목들의 키 / key of items to remove</param>
    /// <returns>제거된 항목 수 / the number of removed items</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    public int RemoveAllByKey(TKey key) {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: scan queue and remove all matching items / lock: 큐 순회 및 모든 일치 항목 제거
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // check if the key exists in counts / 키 존재 여부 확인
            if (!_keyCounts.TryGetValue(key, out var cnt) || cnt == 0)
                // key not found, nothing to remove / 키 없음, 제거할 항목 없음
                return 0;
            // get the key comparer / 비교자 가져오기
            var cmp = _keyCounts.Comparer;
            // count of removed items / 제거된 항목 수
            var removed = 0;
            // snapshot current queue length for rotation / 순회를 위한 현재 큐 길이 스냅샷
            var n = _queue.Count;
            // iterate through all queue entries / 큐 순회
            for (var i = 0; i < n; i++) {
                // dequeue front entry / 엔트리 디큐
                var entry = _queue.Dequeue();
                // check if this entry matches the target key / 키 일치 확인
                if (cmp.Equals(entry.Key, key)) {
                    // increment removed counter / 제거 카운트 증가
                    removed++;
                    // drop this entry / 항목 드롭
                    continue;
                }

                // re-enqueue non-matching entry / 엔트리 인큐
                _queue.Enqueue(entry);
            }

            // remove the key entirely from counts / 키 제거
            _keyCounts.Remove(key);
            // return total number of removed items / 총 제거된 항목 수 반환
            return removed;
        }
    }

    /// <summary>
    ///     모든 항목 제거 및 키별 카운터 지우기. 대기 중인 스레드 깨우기.
    ///     removes all items and clears all per-key counters. Wakes any waiters.
    /// </summary>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: clear all items and wake waiters / lock: 전체 항목 제거 및 대기자 깨우기
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // clear all queued items / 큐 지우기
            _queue.Clear();
            // clear all key counters / 키 카운터 지우기
            _keyCounts.Clear();
            // wake any consumers waiting / 대기 중인 소비자 깨우기
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>
    ///     기본 저장소에 가능한 경우 사용하지 않는 메모리 해제 요청. 스레드 안전.
    ///     requests underlying storage to release unused memory where possible. Thread-safe.
    /// </summary>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess() {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: trim underlying storage / lock: 기본 저장소 여유 공간 제거
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // trim queue internal array / 큐 여유 공간 제거
            _queue.TrimExcess();
            // trim key counts dictionary / 키 카운터 여유 공간 제거
            _keyCounts.TrimExcess();
        }
    }

    /// <summary>
    ///     FIFO 순서로 현재 항목의 스냅샷(얕은 복사) 반환.
    ///     returns a snapshot (shallow copy) of the current items in FIFO order.
    /// </summary>
    /// <remarks>O(n). 스레드 안전. / O(n). Thread-safe.</remarks>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    public List<T> Snapshot() {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: copy all items for consistent snapshot / lock: 일관된 스냅샷을 위해 전체 항목 복사
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // create list with pre-allocated capacity / 리스트 생성
            var list = new List<T>(_queue.Count);
            // iterate over all queued items / 큐 순회
            foreach (var (item, _) in _queue)
                // add item to snapshot list / 항목 추가
                list.Add(item);
            // return the snapshot / 리스트 반환
            return list;
        }
    }

    /// <summary>
    ///     현재 추적 중인 모든 키 및 카운트의 스냅샷 가져오기.
    ///     gets a snapshot of all currently tracked keys and their counts.
    /// </summary>
    /// <remarks>O(k) (k = 고유 키 수). 스레드 안전. / O(k) with k = unique keys. Thread-safe.</remarks>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / if the queue has been disposed.</exception>
    public IReadOnlyDictionary<TKey, int> GetKeySnapshot() {
        // ensure queue is not disposed / 큐 해제 여부 확인
        ThrowIfDisposed();

        // lock: copy key counts for consistent snapshot / lock: 일관된 스냅샷을 위해 키 카운트 복사
        lock (_lock) {
            // re-check disposed under lock / 락 내에서 재확인
            ThrowIfDisposed_NoInline();
            // return copy of key counts / 스냅샷 반환
            return new Dictionary<TKey, int>(_keyCounts);
        }
    }

    /// <summary>
    ///     키 카운트 감소 및 0에 도달하면 키 엔트리 제거.
    ///     decrement key count and remove key entry if it reaches zero.
    /// </summary>
    /// <param name="key">키 / key</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecrementKeyCount(TKey key) {
        // check if the key exists in counts / 키 존재 여부 확인
        if (!_keyCounts.TryGetValue(key, out var cnt))
            // key not tracked, nothing to decrement / 추적되지 않는 키, 감소 불필요
            return;
        // check if count is at or below 1 / 카운트 확인
        if (cnt <= 1)
            // remove key entry entirely / 키 제거
            _keyCounts.Remove(key);
        else
            // decrement the count / 카운트 감소
            _keyCounts[key] = cnt - 1;
    }

    /// <summary>
    ///     큐가 해제된 경우 예외 발생.
    ///     throw if the queue has been disposed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed() {
        // pass if not disposed / 해제되지 않았으면 통과
        if (!_disposed)
            // not disposed, safe to proceed / 해제되지 않음, 계속 진행
            return;
        // ensure queue is not disposed before access / 해제된 큐에 접근 시도 — ObjectDisposedException 발생
        throw new ObjectDisposedException(nameof(KeyedQueue<T, TKey>));
    }

    /// <summary>
    ///     큐가 해제된 경우 예외 발생 (인라인 안함).
    ///     throw if the queue has been disposed (no inline).
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ThrowIfDisposed_NoInline() {
        // pass if not disposed / 해제되지 않았으면 통과
        if (!_disposed)
            // not disposed, safe to proceed / 해제되지 않음, 계속 진행
            return;
        // ensure queue is not disposed before access / 해제된 큐에 접근 시도 — ObjectDisposedException 발생
        throw new ObjectDisposedException(nameof(KeyedQueue<T, TKey>));
    }
}

/// <summary>
///     배치 인큐 작업 결과.
///     result of a batch enqueue operation.
/// </summary>
/// <typeparam name="T">인큐된 요소의 항목 타입 / item type of the enqueued elements.</typeparam>
/// <param name="Accepted">성공적으로 인큐된 항목 수 / number of items successfully enqueued.</param>
/// <param name="Skipped">고유성 강제로 인해 건너뛴 항목 수 / number of items skipped due to uniqueness enforcement.</param>
/// <param name="Failures">키 선택자 예외로 인해 실패한 항목 (래핑됨) / items that failed due to key selector exceptions (wrapped).</param>
public readonly record struct BatchEnqueueResult<T>(
    int                           Accepted,
    int                           Skipped,
    IReadOnlyList<(T, Exception)> Failures) {
    /// <summary>
    ///     하나 이상의 실패가 발생한 경우 true.
    ///     true if at least one failure occurred.
    /// </summary>
    public bool HasFailures => Failures.Count > 0;

    /// <summary>
    ///     전체 = Accepted + Skipped + Failures.Count.
    ///     total = Accepted + Skipped + Failures.Count.
    /// </summary>
    public int TotalProcessed => Accepted + Skipped + Failures.Count;
}

/// <summary>
///     항목 자체를 키로 사용하는 <see cref="KeyedQueue{T, TKey}" />의 편의 래퍼.
///     동등성 의미론(Equals/GetHashCode 또는 제공된 비교자) 기반.
///     convenience wrapper for <see cref="KeyedQueue{T, TKey}" /> that uses the item itself as the key,
///     based on its equality semantics (Equals/GetHashCode or a supplied comparer).
/// </summary>
public sealed class KeyedQueue<T> : KeyedQueue<T, T> where T : notnull {
    /// <summary>
    ///     <typeparamref name="T" /> 자체를 키로 사용하여 새 인스턴스 초기화. / initializes a new instance using <typeparamref name="T" />
    ///     itself as the key.
    /// </summary>
    public KeyedQueue(IEqualityComparer<T>? comparer = null, int capacity = 0)
        : base(static x => x, comparer, capacity) { }
}