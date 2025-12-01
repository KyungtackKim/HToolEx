using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HTool.Util;

/// <summary>
///     <see cref="KeyedQueue{T, TKey}" />에서 항목 추가 시 중복 처리 방식 정의
///     Defines how <see cref="KeyedQueue{T, TKey}" /> handles duplicates when enqueuing.
/// </summary>
public enum EnqueueMode {
    /// <summary>
    ///     키 고유성 강제: 동일한 키를 가진 항목이 이미 존재하면
    ///     추가 시도를 건너뛰고 false 반환
    ///     Enforce uniqueness by key: if an item with the same key is already present,
    ///     the enqueue attempt is skipped and returns false.
    /// </summary>
    EnforceUnique = 0,

    /// <summary>
    ///     키 중복 허용: 항목을 항상 추가하고 키별 내부 카운터 증가
    ///     Allow duplicates by key: the item is always enqueued, and an internal
    ///     per-key counter is incremented.
    /// </summary>
    AllowDuplicate = 1
}

/// <summary>
///     큐 작업 중 키 선택자 함수 실패 시 발생하는 예외
///     Exception thrown when the key selector function fails during queue operations.
/// </summary>
public sealed class KeySelectorException(string message, Exception innerException) : InvalidOperationException(message, innerException);

/// <summary>
///     키 기반 중복 방지 기능이 있는 고성능 스레드 안전 FIFO 큐. HTool의 메시지 큐 관리에 사용됩니다.
///     High-performance thread-safe FIFO queue with key-based duplicate prevention. Used for HTool's message queue
///     management.
/// </summary>
/// <remarks>
///     <para>주요 기능 / Key Features:</para>
///     <list type="bullet">
///         <item>
///             <description>항목의 FIFO 순서 유지 / Maintains FIFO ordering for items.</description>
///         </item>
///         <item>
///             <description>
///                 각 항목의 키 추적 (키 선택자를 통해) 중복 방지 또는 허용 / Tracks keys for each item (via a key selector) to prevent
///                 or allow duplicates.
///             </description>
///         </item>
///         <item>
///             <description>
///                 타임아웃 및 취소와 함께 논블로킹/블로킹 Dequeue/Peek 지원 / Supports non-blocking and blocking Dequeue/Peek with
///                 timeout and cancellation.
///             </description>
///         </item>
///         <item>
///             <description>
///                 키 기반 쿼리 (포함 여부/대기 개수) 및 선택적 중간 큐 제거 제공 / Provides key-based queries (contains/pending counts)
///                 and optional mid-queue removal.
///             </description>
///         </item>
///     </list>
///     </summary>
///     <remarks>
///         <para>
///             <strong>성능 특성 / Performance Characteristics:</strong>
///         </para>
///         <list type="bullet">
///             <item>
///                 <description>Enqueue/Dequeue: 평균 O(1) / O(1) average.</description>
///             </item>
///             <item>
///                 <description>키 기반 쿼리: O(1) / Key-based queries: O(1).</description>
///             </item>
///             <item>
///                 <description>중간 큐 제거: O(n) — 드물게 사용 권장 / Mid-queue removal: O(n) — use sparingly.</description>
///             </item>
///         </list>
///         <para>
///             <strong>스레드 안전성 / Thread Safety:</strong> 단일 모니터 락. 모든 공개 작업은 스레드 안전 / Single monitor lock. All public
///             operations are thread-safe.
///         </para>
///         <para>
///             <strong>메모리 / Memory:</strong> 재계산 방지를 위해 키가 항목과 함께 저장됨. 크기가 많이 줄어들면 <see cref="TrimExcess" /> 호출 권장 / Keys
///             are stored with items to avoid recomputation. Call <see cref="TrimExcess" /> if
///             size shrinks a lot.
///         </para>
///     </remarks>
public class KeyedQueue<T, TKey> : IDisposable where TKey : notnull {
    /// <summary>
    ///     키별 카운터. 키가 값 N과 함께 존재하면 해당 키에 대해 N개의 대기 항목이 큐에 있음
    ///     Per-key counters. If a key is present with value N, there are N pending items for that key in the queue.
    /// </summary>
    private readonly Dictionary<TKey, int> _keyCounts;

    /// <summary>
    ///     항목 T를 키 TKey로 매핑하는 사용자 제공 함수
    ///     User-supplied function that maps an item T to its key TKey.
    /// </summary>
    private readonly Func<T, TKey> _keySelector;

    /// <summary>
    ///     큐와 키 카운터를 모두 보호하는 단일 락 객체
    ///     Single lock object guarding both the queue and the key counters.
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    ///     항목의 FIFO 저장소 (디큐 시 재계산 방지를 위해 계산된 키와 함께 저장)
    ///     FIFO store of items, alongside their computed keys (to avoid recomputing on dequeue).
    /// </summary>
    private readonly Queue<(T Item, TKey Key)> _queue;

    /// <summary>
    ///     Disposed 플래그 (빠른 실패를 위해 락 없이 확인; 상태 변경 시 락으로 보호)
    ///     Disposed flag (checked without lock for fast-fail; guarded by lock for state mutation).
    /// </summary>
    private volatile bool _disposed;

    /// <summary>
    ///     <see cref="KeyedQueue{T, TKey}" /> 클래스의 새 인스턴스 초기화
    ///     Initializes a new instance of the <see cref="KeyedQueue{T, TKey}" /> class.
    /// </summary>
    /// <param name="keySelector">
    ///     항목의 키를 반환하는 함수. null이 아니어야 하며 빠르고 부작용이 없어야 함.
    ///     이 함수가 예외를 발생시키면 오류가 <see cref="KeySelectorException" />으로 래핑됨.
    ///     A function that returns the key for an item. Must be non-null and should be fast and side-effect free.
    ///     If this function throws, the error is wrapped in <see cref="KeySelectorException" />.
    /// </param>
    /// <param name="keyComparer">
    ///     <typeparamref name="TKey" />에 대한 선택적 동등성 비교자. null이면 기본 비교자 사용.
    ///     Optional equality comparer for <typeparamref name="TKey" />. If null, the default comparer is used.
    /// </param>
    /// <param name="capacity">
    ///     기본 큐에 대한 선택적 초기 용량 힌트.
    ///     Optional initial capacity hint for the underlying queue.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="keySelector" />가 null일 때 발생 / Thrown when
    ///     <paramref name="keySelector" /> is null.
    /// </exception>
    protected KeyedQueue(
        Func<T, TKey>            keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        int                      capacity    = 0) {
        // 용량 확인
        // check capacity
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        // 키 선택자 설정
        // set key selector
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        // 큐 생성
        // create queue
        _queue = capacity > 0 ? new Queue<(T Item, TKey Key)>(capacity) : new Queue<(T Item, TKey Key)>();
        // 키 카운터 생성
        // create key counts
        _keyCounts = new Dictionary<TKey, int>(keyComparer);
    }

    /// <summary>큐의 현재 항목 수 가져오기. O(1). 스레드 안전. / Gets the current number of items in the queue. O(1). Thread-safe.</summary>
    public int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ThrowIfDisposed();
            lock (_lock) {
                return _queue.Count;
            }
        }
    }

    /// <summary>큐가 현재 비어 있는지 여부 가져오기. O(1). 스레드 안전. / Gets whether the queue is currently empty. O(1). Thread-safe.</summary>
    public bool IsEmpty {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ThrowIfDisposed();
            lock (_lock) {
                return _queue.Count == 0;
            }
        }
    }

    /// <summary>현재 추적 중인 고유 키 수 가져오기. O(1). 스레드 안전. / Gets the number of unique keys currently tracked. O(1). Thread-safe.</summary>
    public int UniqueKeyCount {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            ThrowIfDisposed();
            lock (_lock) {
                return _keyCounts.Count;
            }
        }
    }

    /// <summary>
    ///     큐 해제: 상태를 지우고 대기 중인 스레드를 깨워 종료할 수 있게 함.
    ///     해제 후 모든 공개 멤버는 <see cref="ObjectDisposedException" /> 발생.
    ///     Disposes the queue: clears state and wakes any waiting threads so they can exit.
    ///     After disposal, all public members throw <see cref="ObjectDisposedException" />.
    /// </summary>
    public void Dispose() {
        // disposed 플래그 확인
        // check disposed flag
        if (_disposed)
            return;

        lock (_lock) {
            // disposed 플래그 이중 확인
            // double check disposed flag
            if (_disposed)
                return;
            // disposed 플래그 설정
            // set disposed flag
            _disposed = true;
            // 큐 지우기
            // clear queue
            _queue.Clear();
            // 키 카운터 지우기
            // clear key counts
            _keyCounts.Clear();
            // 대기 중인 스레드 깨우기
            // wake up waiting threads
            Monitor.PulseAll(_lock);
        }

        // 종료자 억제
        // suppress finalize
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     키 기반 큐 생성
    ///     Create the keyed-queue
    /// </summary>
    /// <param name="keySelector">키 선택자 / key selector</param>
    /// <param name="keyComparer">키 비교자 / key comparer</param>
    /// <param name="capacity">용량 / capacity</param>
    /// <returns>키 기반 큐 인스턴스 / keyed-queue instance</returns>
    public static KeyedQueue<T, TKey> Create(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null, int capacity = 0) {
        // 용량 확인
        // check capacity
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        // 인스턴스 생성
        // create instance
        return new KeyedQueue<T, TKey>(keySelector, keyComparer, capacity);
    }

    /// <summary>
    ///     지정된 중복 처리 모드로 항목 추가 시도.
    ///     평균 O(1). 스레드 안전. <see cref="Monitor.PulseAll(object)" />로 대기자 깨움.
    ///     Attempts to enqueue an item with the specified duplicate-handling mode.
    ///     O(1) average. Thread-safe. Wakes waiters via <see cref="Monitor.PulseAll(object)" />.
    /// </summary>
    /// <param name="item">추가할 항목 / Item to enqueue</param>
    /// <param name="mode">중복 처리 모드 / Duplicate handling mode</param>
    /// <returns>성공 시 true, 중복으로 추가 안 됨 시 false / True if added, false if duplicate rejected</returns>
    /// <exception cref="KeySelectorException">키 선택자가 예외 발생 시 / If key selector throws.</exception>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEnqueue(T item, EnqueueMode mode = EnqueueMode.EnforceUnique) {
        // disposed 플래그 확인
        // check disposed flag
        ThrowIfDisposed();

        TKey key;
        try {
            // 키 가져오기
            // get key
            key = _keySelector(item);
        } catch (Exception ex) {
            // 예외 발생
            // throw exception
            throw new KeySelectorException("Key selector function failed during enqueue.", ex);
        }

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 키가 이미 존재하는지 확인
            // check if key already exists
            var exists = _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
            // 모드 확인
            // check mode
            if (mode == EnqueueMode.EnforceUnique && exists)
                return false;
            // 항목 추가
            // enqueue item
            _queue.Enqueue((item, key));
            // ref 접근으로 두 번째 사전 조회 방지
            // avoid second dictionary lookup using ref access
            ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyCounts, key, out var added);
            // 슬롯 업데이트
            // update slot
            slot = added ? 1 : slot + 1;
            // 대기 중인 소비자 깨우기
            // wake waiting consumers
            Monitor.PulseAll(_lock);
            // true 반환
            // return true
            return true;
        }
    }

    /// <summary>
    ///     단일 락 획득으로 항목 배치 추가 시도. 승인/건너뛰기 및 실패(키 선택자 오류) 카운트 반환.
    ///     Attempts to enqueue a batch of items with a single lock acquisition.
    ///     Returns counts for accepted/skipped and failures (key selector errors).
    /// </summary>
    /// <param name="items">추가할 항목 컬렉션 / Collection of items to enqueue</param>
    /// <param name="mode">중복 처리 모드 / Duplicate handling mode</param>
    /// <returns>승인, 건너뛰기, 실패 항목 결과 / Result with accepted, skipped, and failed items</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" />가 null인 경우 / If <paramref name="items" /> is null.</exception>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    public BatchEnqueueResult<T> TryEnqueueRange(IEnumerable<T> items, EnqueueMode mode = EnqueueMode.EnforceUnique) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();
        // 항목 null 확인
        // check the items
        ArgumentNullException.ThrowIfNull(items);
        // 카운터 초기화
        // initialize counters
        var                                  accepted = 0;
        var                                  skipped  = 0;
        List<(T Item, Exception Exception)>? failures = null;

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 항목 반복
            // iterate over items
            foreach (var item in items) {
                TKey key;
                try {
                    // 키 가져오기
                    // get the key
                    key = _keySelector(item);
                } catch (Exception ex) {
                    // 실패 목록에 추가
                    // add to failures
                    failures ??= [];
                    failures.Add((item, new KeySelectorException("Key selector failed in batch enqueue.", ex)));
                    continue;
                }

                // 키가 이미 존재하는지 확인
                // check if the key already exists
                var exists = _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
                // 모드 확인
                // check the mode
                if (mode == EnqueueMode.EnforceUnique && exists) {
                    // 항목 건너뛰기
                    // skip the item
                    skipped++;
                    continue;
                }

                // 항목 추가
                // enqueue the item
                _queue.Enqueue((item, key));
                // 슬롯 업데이트
                // update the slot
                ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_keyCounts, key, out var added);
                slot = added ? 1 : slot + 1;
                // 승인 카운트 증가
                // increment accepted
                accepted++;
            }

            // 대기 중인 소비자 깨우기
            // wake any consumers waiting
            if (accepted > 0)
                Monitor.PulseAll(_lock);
        }

        // 실패 목록 가져오기 (또는 빈 목록)
        // get the failures or empty
        var failuresOrEmpty = (IReadOnlyList<(T, Exception)>?)failures ?? [];
        // 결과 반환
        // return the result
        return new BatchEnqueueResult<T>(accepted, skipped, failuresOrEmpty);
    }

    /// <summary>
    ///     블로킹 없이 FIFO 순서로 다음 항목 제거 및 반환 시도. O(1). 스레드 안전.
    ///     Attempts to remove and return the next item in FIFO order without blocking. O(1). Thread-safe.
    /// </summary>
    /// <param name="item">제거된 항목 (성공 시) / Dequeued item (if successful)</param>
    /// <returns>항목 제거 성공 시 true, 큐가 비어있으면 false / True if item removed, false if queue empty</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out T item) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 큐가 비어있는지 확인
            // check if the queue is empty
            if (_queue.Count == 0) {
                // 기본값 설정
                // set the default value
                item = default!;
                return false;
            }

            // 항목 디큐
            // dequeue the item
            var (val, key) = _queue.Dequeue();
            // 키 카운트 감소
            // decrement the key count
            DecrementKeyCount(key);
            // 항목 설정
            // set the item
            item = val;
            return true;
        }
    }

    /// <summary>
    ///     다음 항목 제거 및 반환 시도, 선택적으로 항목이 사용 가능할 때까지 블로킹.
    ///     <see cref="Monitor.Wait(object, int)" /> 사용하여 대기하고 <see cref="Monitor.PulseAll(object)" />로 깨움.
    ///     Attempts to remove and return the next item, optionally blocking until an item becomes available.
    ///     Uses <see cref="Monitor.Wait(object, int)" /> and wakes via <see cref="Monitor.PulseAll(object)" />.
    /// </summary>
    /// <param name="item">항목 / item</param>
    /// <param name="timeoutMs">0 = 논블로킹, -1 = 무한, &gt;0 = 밀리초 / 0 = non-blocking, -1 = infinite, &gt;0 = milliseconds.</param>
    /// <param name="cancellationToken">선택적 취소 / Optional cancellation.</param>
    /// <returns>
    ///     항목이 디큐되면 <c>true</c>, 타임아웃/취소시 <c>false</c> / <c>true</c> if an item was dequeued; <c>false</c> on
    ///     timeout/cancellation.
    /// </returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    public bool TryDequeue(out T item, int timeoutMs, CancellationToken cancellationToken = default) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();
        // 타임아웃이 양수이면 스톱워치 시작
        // start the stopwatch if timeout is positive
        var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;

        lock (_lock) {
            // 항목 대기
            // wait for the item
            while (_queue.Count == 0) {
                // 락 내에서 재확인
                // re-check under lock
                ThrowIfDisposed_NoInline();
                // 취소 요청 또는 타임아웃 0 확인
                // check if the cancellation is requested or timeout is zero
                if (cancellationToken.IsCancellationRequested || timeoutMs == 0) {
                    // 기본값 설정
                    // set the default value
                    item = default!;
                    return false;
                }

                // 남은 시간 계산
                // calculate the remaining time
                var remain = timeoutMs < 0 ? Timeout.Infinite : Math.Max(0, timeoutMs - (int)sw!.ElapsedMilliseconds);
                // 신호 대기
                // wait for the signal
                if (Monitor.Wait(_lock, remain))
                    continue;
                // 타임아웃
                // timeout
                item = default!;
                return false;
            }

            // 항목 디큐
            // dequeue the item
            var (val, key) = _queue.Dequeue();
            // 키 카운트 감소
            // decrement the key count
            DecrementKeyCount(key);
            // 항목 설정
            // set the item
            item = val;
            return true;
        }
    }

    /// <summary>
    ///     블로킹 없이 FIFO 순서로 다음 항목 읽기(제거하지 않음) 시도. O(1). 스레드 안전.
    ///     Attempts to read (without removing) the next item in FIFO order without blocking. O(1). Thread-safe.
    /// </summary>
    /// <param name="item">읽은 항목 (성공 시) / Peeked item (if successful)</param>
    /// <returns>항목 읽기 성공 시 true, 큐가 비어있으면 false / True if item read, false if queue empty</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out T item) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 큐가 비어있는지 확인
            // check if the queue is empty
            if (_queue.Count == 0) {
                // 기본값 설정
                // set the default value
                item = default!;
                return false;
            }

            // 항목 피크
            // peek the item
            item = _queue.Peek().Item;
            return true;
        }
    }

    /// <summary>
    ///     다음 항목 읽기(제거하지 않음) 시도, 선택적으로 항목이 사용 가능할 때까지 블로킹.
    ///     Attempts to read (without removing) the next item, optionally blocking until an item becomes available.
    /// </summary>
    /// <param name="item">항목 / item</param>
    /// <param name="timeoutMs">0 = 논블로킹, -1 = 무한, &gt;0 = 밀리초 / 0 = non-blocking, -1 = infinite, &gt;0 = milliseconds.</param>
    /// <param name="cancellationToken">선택적 취소 / Optional cancellation.</param>
    /// <returns>
    ///     항목이 있으면 <c>true</c>, 타임아웃/취소시 <c>false</c> / <c>true</c> if an item was available; <c>false</c> on
    ///     timeout/cancellation.
    /// </returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    public bool TryPeek(out T item, int timeoutMs, CancellationToken cancellationToken = default) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();
        // 타임아웃이 양수이면 스톱워치 시작
        // start the stopwatch if timeout is positive
        var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;

        lock (_lock) {
            // 항목 대기
            // wait for the item
            while (_queue.Count == 0) {
                // 락 내에서 재확인
                // re-check under lock
                ThrowIfDisposed_NoInline();
                // 취소 요청 또는 타임아웃 0 확인
                // check if the cancellation is requested or timeout is zero
                if (cancellationToken.IsCancellationRequested || timeoutMs == 0) {
                    // 기본값 설정
                    // set the default value
                    item = default!;
                    return false;
                }

                // 남은 시간 계산
                // calculate the remaining time
                var remain = timeoutMs < 0 ? Timeout.Infinite : Math.Max(0, timeoutMs - (int)sw!.ElapsedMilliseconds);
                // 신호 대기
                // wait for the signal
                if (Monitor.Wait(_lock, remain))
                    continue;
                // 타임아웃
                // timeout
                item = default!;
                return false;
            }

            // 항목 피크
            // peek the item
            item = _queue.Peek().Item;
            return true;
        }
    }

    /// <summary>
    ///     지정된 키를 가진 항목이 하나 이상 존재하는지 확인. O(1). 스레드 안전.
    ///     Checks whether at least one item with the specified key exists. O(1). Thread-safe.
    /// </summary>
    /// <param name="key">확인할 키 / Key to check</param>
    /// <returns>키가 존재하면 true, 없으면 false / True if key exists, false otherwise</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();
        lock (_lock) {
            // 키 존재 여부 확인
            // check if the key exists
            return _keyCounts.TryGetValue(key, out var cnt) && cnt > 0;
        }
    }

    /// <summary>
    ///     지정된 키를 가진 대기 중인 항목 수 가져오기. O(1). 스레드 안전.
    ///     Gets how many items with the specified key are pending. O(1). Thread-safe.
    /// </summary>
    /// <param name="key">확인할 키 / Key to check</param>
    /// <returns>해당 키를 가진 항목 수 (0 이상) / Number of items with the key (0 or more)</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PendingCountByKey(TKey key) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();
        lock (_lock) {
            // 카운트 가져오기
            // get the count
            return _keyCounts.GetValueOrDefault(key, 0);
        }
    }

    /// <summary>
    ///     지정된 키를 가진 항목의 첫 번째 항목 제거 (O(n)). 나머지 항목의 FIFO 순서 유지.
    ///     Removes the first occurrence of an item with the specified key (O(n)).
    ///     Preserves FIFO order of the remaining items.
    /// </summary>
    /// <param name="key">제거할 항목의 키 / Key of item to remove</param>
    /// <returns>항목 제거 성공 시 true, 키 없으면 false / True if removed, false if key not found</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    public bool TryRemoveByKey(TKey key) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 키 존재 여부 확인
            // check if the key exists
            if (!_keyCounts.TryGetValue(key, out var cnt) || cnt == 0)
                return false;
            // 비교자 가져오기
            // get the comparer
            var cmp     = _keyCounts.Comparer;
            var removed = false;
            var n       = _queue.Count;
            // 큐를 한 번 순회
            // rotate through the queue once
            for (var i = 0; i < n; i++) {
                // 엔트리 디큐
                // dequeue the entry
                var entry = _queue.Dequeue();
                // 키 일치 확인
                // check if the key matches
                if (!removed && cmp.Equals(entry.Key, key)) {
                    // 제거 플래그 설정
                    // set the removed flag
                    removed = true;
                    // 키 카운트 감소
                    // decrement the key count
                    DecrementKeyCount(key);
                    // 항목 드롭
                    // drop
                    continue;
                }

                // 엔트리 인큐
                // enqueue the entry
                _queue.Enqueue(entry);
            }

            return removed;
        }
    }

    /// <summary>
    ///     지정된 키를 가진 모든 항목 제거 (O(n)). 나머지 항목의 상대적 순서 유지.
    ///     Removes all items with the specified key (O(n)).
    ///     Preserves the relative order of the remaining items.
    /// </summary>
    /// <param name="key">제거할 항목들의 키 / Key of items to remove</param>
    /// <returns>제거된 항목 수 / The number of removed items</returns>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    public int RemoveAllByKey(TKey key) {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 키 존재 여부 확인
            // check if the key exists
            if (!_keyCounts.TryGetValue(key, out var cnt) || cnt == 0)
                return 0;
            // 비교자 가져오기
            // get the comparer
            var cmp     = _keyCounts.Comparer;
            var removed = 0;
            var n       = _queue.Count;
            // 큐 순회
            // iterate over the queue
            for (var i = 0; i < n; i++) {
                // 엔트리 디큐
                // dequeue the entry
                var entry = _queue.Dequeue();
                // 키 일치 확인
                // check if the key matches
                if (cmp.Equals(entry.Key, key)) {
                    // 제거 카운트 증가
                    // increment removed
                    removed++;
                    // 항목 드롭
                    // drop
                    continue;
                }

                // 엔트리 인큐
                // enqueue the entry
                _queue.Enqueue(entry);
            }

            // 키 제거
            // remove the key
            _keyCounts.Remove(key);
            return removed;
        }
    }

    /// <summary>
    ///     모든 항목 제거 및 키별 카운터 지우기. 대기 중인 스레드 깨우기.
    ///     Removes all items and clears all per-key counters. Wakes any waiters.
    /// </summary>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 큐 지우기
            // clear the queue
            _queue.Clear();
            // 키 카운터 지우기
            // clear the key counts
            _keyCounts.Clear();
            // 대기 중인 소비자 깨우기
            // wake any consumers waiting
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>
    ///     기본 저장소에 가능한 경우 사용하지 않는 메모리 해제 요청. 스레드 안전.
    ///     Requests underlying storage to release unused memory where possible. Thread-safe.
    /// </summary>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrimExcess() {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 큐 여유 공간 제거
            // trim the queue
            _queue.TrimExcess();
            // 키 카운터 여유 공간 제거
            // trim the key counts
            _keyCounts.TrimExcess();
        }
    }

    /// <summary>
    ///     FIFO 순서로 현재 항목의 스냅샷(얕은 복사) 반환.
    ///     Returns a snapshot (shallow copy) of the current items in FIFO order.
    /// </summary>
    /// <remarks>O(n). 스레드 안전. / O(n). Thread-safe.</remarks>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    public List<T> Snapshot() {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 리스트 생성
            // create the list
            var list = new List<T>(_queue.Count);
            // 큐 순회
            // iterate over the queue
            foreach (var (item, _) in _queue)
                // 항목 추가
                // add the item
                list.Add(item);
            // 리스트 반환
            // return the list
            return list;
        }
    }

    /// <summary>
    ///     현재 추적 중인 모든 키 및 카운트의 스냅샷 가져오기.
    ///     Gets a snapshot of all currently tracked keys and their counts.
    /// </summary>
    /// <remarks>O(k) (k = 고유 키 수). 스레드 안전. / O(k) with k = unique keys. Thread-safe.</remarks>
    /// <exception cref="ObjectDisposedException">큐가 해제된 경우 / If the queue has been disposed.</exception>
    public IReadOnlyDictionary<TKey, int> GetKeySnapshot() {
        // disposed 플래그 확인
        // check the disposed flag
        ThrowIfDisposed();

        lock (_lock) {
            // 락 내에서 재확인
            // re-check under lock
            ThrowIfDisposed_NoInline();
            // 스냅샷 반환
            // return the snapshot
            return new Dictionary<TKey, int>(_keyCounts);
        }
    }

    /// <summary>
    ///     키 카운트 감소 및 0에 도달하면 키 엔트리 제거.
    ///     Decrement key count and remove key entry if it reaches zero.
    /// </summary>
    /// <param name="key">키 / key</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DecrementKeyCount(TKey key) {
        // 키 존재 여부 확인
        // check the key exists
        if (!_keyCounts.TryGetValue(key, out var cnt))
            return;
        // 카운트 확인
        // check the count
        if (cnt <= 1)
            // 키 제거
            // remove the key
            _keyCounts.Remove(key);
        else
            // 카운트 감소
            // decrement the count
            _keyCounts[key] = cnt - 1;
    }

    /// <summary>
    ///     큐가 해제된 경우 예외 발생.
    ///     Throw if the queue has been disposed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed() {
        // disposed 플래그 확인
        // check the disposed flag
        if (!_disposed)
            return;
        // 예외 발생
        // throw exception
        throw new ObjectDisposedException(nameof(KeyedQueue<T, TKey>));
    }

    /// <summary>
    ///     큐가 해제된 경우 예외 발생 (인라인 안함).
    ///     Throw if the queue has been disposed (no inline).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed_NoInline() {
        // disposed 플래그 확인
        // check the disposed flag
        if (!_disposed)
            return;
        // 예외 발생
        // throw exception
        throw new ObjectDisposedException(nameof(KeyedQueue<T, TKey>));
    }
}

/// <summary>
///     배치 인큐 작업 결과.
///     Result of a batch enqueue operation.
/// </summary>
/// <typeparam name="T">인큐된 요소의 항목 타입 / Item type of the enqueued elements.</typeparam>
/// <param name="Accepted">성공적으로 인큐된 항목 수 / Number of items successfully enqueued.</param>
/// <param name="Skipped">고유성 강제로 인해 건너뛴 항목 수 / Number of items skipped due to uniqueness enforcement.</param>
/// <param name="Failures">키 선택자 예외로 인해 실패한 항목 (래핑됨) / Items that failed due to key selector exceptions (wrapped).</param>
public readonly record struct BatchEnqueueResult<T>(
    int                           Accepted,
    int                           Skipped,
    IReadOnlyList<(T, Exception)> Failures) {
    /// <summary>하나 이상의 실패가 발생한 경우 true / True if at least one failure occurred.</summary>
    public bool HasFailures => Failures.Count > 0;

    /// <summary>전체 = Accepted + Skipped + Failures.Count / Total = Accepted + Skipped + Failures.Count.</summary>
    public int TotalProcessed => Accepted + Skipped + Failures.Count;
}

/// <summary>
///     항목 자체를 키로 사용하는 <see cref="KeyedQueue{T, TKey}" />의 편의 래퍼.
///     동등성 의미론(Equals/GetHashCode 또는 제공된 비교자) 기반.
///     Convenience wrapper for <see cref="KeyedQueue{T, TKey}" /> that uses the item itself as the key,
///     based on its equality semantics (Equals/GetHashCode or a supplied comparer).
/// </summary>
public sealed class KeyedQueue<T> : KeyedQueue<T, T> where T : notnull {
    /// <summary>
    ///     <typeparamref name="T" /> 자체를 키로 사용하여 새 인스턴스 초기화. / Initializes a new instance using <typeparamref name="T" />
    ///     itself as the key.
    /// </summary>
    public KeyedQueue(IEqualityComparer<T>? comparer = null, int capacity = 0)
        : base(static x => x, comparer, capacity) { }
}