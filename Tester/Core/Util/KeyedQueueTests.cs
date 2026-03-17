using System.Collections.Concurrent;
using HTool.Core.Util;

namespace Tester.Core.Util;

/// <summary>
///     KeyedQueue의 기능 및 스레드 안전성을 검증하는 테스트 클래스.
///     test class verifying functionality and thread safety of KeyedQueue.
/// </summary>
public sealed class KeyedQueueTests {
    // ──────────────────────────────────────────────
    // Create / 팩토리 메서드
    // ──────────────────────────────────────────────

    /// <summary>
    ///     null 키 선택자로 생성 시 예외가 발생하는지 검증한다.
    ///     verifies that null key selector throws on creation.
    /// </summary>
    [Fact]
    public void Create_NullKeySelector_ThrowsArgumentNullException() {
        // null 키 선택자로 생성 시도
        // attempt to create with null key selector
        var ex = Assert.Throws<ArgumentNullException>(
            // 키 선택자가 null인 큐 생성
            // create queue with null key selector
            () => KeyedQueue<TestItem, int>.Create(null!)
        );

        // 파라미터 이름이 keySelector인지 확인
        // verify parameter name is keySelector
        Assert.Equal("keySelector", ex.ParamName);
    }

    /// <summary>
    ///     음수 용량으로 생성 시 예외가 발생하는지 검증한다.
    ///     verifies that negative capacity throws on creation.
    /// </summary>
    [Fact]
    public void Create_NegativeCapacity_ThrowsArgumentOutOfRangeException() {
        // 음수 용량으로 생성 시도
        // attempt to create with negative capacity
        Assert.Throws<ArgumentOutOfRangeException>(
            // 음수 용량 큐 생성
            // create queue with negative capacity
            () => KeyedQueue<TestItem, int>.Create(x => x.Id, capacity: -1)
        );
    }

    /// <summary>
    ///     유효한 인자로 큐가 정상 생성되는지 검증한다.
    ///     verifies that queue is created successfully with valid arguments.
    /// </summary>
    [Fact]
    public void Create_ValidArgs_ReturnsEmptyQueue() {
        // 유효한 인자로 큐 생성
        // create queue with valid args
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id, capacity: 8);

        // 초기 카운트가 0인지 확인
        // verify initial count is zero
        Assert.Equal(0, q.Count);
        // 초기 상태가 비어있는지 확인
        // verify initial state is empty
        Assert.True(q.IsEmpty);
        // 초기 고유 키 수가 0인지 확인
        // verify initial unique key count is zero
        Assert.Equal(0, q.UniqueKeyCount);
    }

    // ──────────────────────────────────────────────
    // TryEnqueue — EnforceUnique / 고유 모드
    // ──────────────────────────────────────────────

    /// <summary>
    ///     EnforceUnique 모드에서 첫 항목이 성공적으로 추가되는지 검증한다.
    ///     verifies that first item is enqueued successfully in EnforceUnique mode.
    /// </summary>
    [Fact]
    public void TryEnqueue_EnforceUnique_FirstItem_ReturnsTrue() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 첫 항목 추가
        // enqueue first item
        var result = q.TryEnqueue(new TestItem(1, "A"));

        // 추가 성공 확인
        // verify enqueue succeeded
        Assert.True(result);
        // 카운트가 1인지 확인
        // verify count is 1
        Assert.Equal(1, q.Count);
    }

    /// <summary>
    ///     EnforceUnique 모드에서 중복 키 항목이 거부되는지 검증한다.
    ///     verifies that duplicate key item is rejected in EnforceUnique mode.
    /// </summary>
    [Fact]
    public void TryEnqueue_EnforceUnique_DuplicateKey_ReturnsFalse() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 첫 항목 추가
        // enqueue first item
        q.TryEnqueue(new TestItem(1, "A"));

        // 동일 키로 두 번째 항목 추가 시도
        // attempt to enqueue second item with same key
        var result = q.TryEnqueue(new TestItem(1, "B"));

        // 추가 거부 확인
        // verify enqueue rejected
        Assert.False(result);
        // 카운트가 1인지 확인
        // verify count remains 1
        Assert.Equal(1, q.Count);
    }

    // ──────────────────────────────────────────────
    // TryEnqueue — AllowDuplicate / 중복 허용 모드
    // ──────────────────────────────────────────────

    /// <summary>
    ///     AllowDuplicate 모드에서 중복 키 항목이 허용되는지 검증한다.
    ///     verifies that duplicate key items are allowed in AllowDuplicate mode.
    /// </summary>
    [Fact]
    public void TryEnqueue_AllowDuplicate_DuplicateKey_ReturnsTrue() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 첫 항목 추가
        // enqueue first item
        q.TryEnqueue(new TestItem(1, "A"), EnqueueMode.AllowDuplicate);

        // 동일 키로 두 번째 항목 추가
        // enqueue second item with same key
        var result = q.TryEnqueue(new TestItem(1, "B"), EnqueueMode.AllowDuplicate);

        // 추가 성공 확인
        // verify enqueue succeeded
        Assert.True(result);
        // 카운트가 2인지 확인
        // verify count is 2
        Assert.Equal(2, q.Count);
        // 고유 키 수가 1인지 확인
        // verify unique key count is 1
        Assert.Equal(1, q.UniqueKeyCount);
    }

    /// <summary>
    ///     키 선택자가 예외 발생 시 KeySelectorException으로 래핑되는지 검증한다.
    ///     verifies that key selector exception is wrapped in KeySelectorException.
    /// </summary>
    [Fact]
    public void TryEnqueue_KeySelectorThrows_ThrowsKeySelectorException() {
        // 예외를 발생시키는 키 선택자로 큐 생성
        // create queue with throwing key selector
        using var q = KeyedQueue<TestItem, int>.Create(
            // 키 선택자가 항상 예외를 발생시킴
            // key selector always throws
            _ => throw new InvalidOperationException("test")
        );

        // KeySelectorException 발생 확인
        // verify KeySelectorException is thrown
        var ex = Assert.Throws<KeySelectorException>(
            // 항목 추가 시도
            // attempt to enqueue
            () => q.TryEnqueue(new TestItem(1, "A"))
        );

        // 내부 예외가 원래 예외인지 확인
        // verify inner exception is original
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    // ──────────────────────────────────────────────
    // TryEnqueueRange / 배치 인큐
    // ──────────────────────────────────────────────

    /// <summary>
    ///     배치 인큐에서 고유 항목이 모두 승인되는지 검증한다.
    ///     verifies that all unique items are accepted in batch enqueue.
    /// </summary>
    [Fact]
    public void TryEnqueueRange_AllUnique_AllAccepted() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 테스트 항목 목록 생성
        // create test item list
        var items = new[] { new TestItem(1, "A"), new TestItem(2, "B"), new TestItem(3, "C") };

        // 배치 인큐 실행
        // execute batch enqueue
        var result = q.TryEnqueueRange(items);

        // 승인 수 확인
        // verify accepted count
        Assert.Equal(3, result.Accepted);
        // 건너뛰기 수 확인
        // verify skipped count
        Assert.Equal(0, result.Skipped);
        // 실패 없음 확인
        // verify no failures
        Assert.False(result.HasFailures);
        // 총 처리 수 확인
        // verify total processed
        Assert.Equal(3, result.TotalProcessed);
    }

    /// <summary>
    ///     배치 인큐에서 중복 키가 건너뛰어지는지 검증한다.
    ///     verifies that duplicate keys are skipped in batch enqueue.
    /// </summary>
    [Fact]
    public void TryEnqueueRange_WithDuplicates_SkipsDuplicates() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 중복 포함 항목 목록
        // items list with duplicates
        var items = new[] { new TestItem(1, "A"), new TestItem(1, "B"), new TestItem(2, "C") };

        // 배치 인큐 실행
        // execute batch enqueue
        var result = q.TryEnqueueRange(items);

        // 승인 수 확인 (1과 2)
        // verify accepted count (1 and 2)
        Assert.Equal(2, result.Accepted);
        // 건너뛰기 수 확인 (중복 1)
        // verify skipped count (duplicate 1)
        Assert.Equal(1, result.Skipped);
    }

    /// <summary>
    ///     키 선택자 실패가 Failures에 기록되는지 검증한다.
    ///     verifies that key selector failures are recorded in Failures.
    /// </summary>
    [Fact]
    public void TryEnqueueRange_KeySelectorFails_RecordsFailures() {
        // 항목 카운터
        // item counter
        var callCount = 0;
        // 두 번째 호출에서 예외를 발생시키는 키 선택자로 큐 생성
        // create queue with key selector that throws on second call
        using var q = KeyedQueue<TestItem, int>.Create(x => {
            // 호출 카운터 증가
            // increment call counter
            callCount++;
            // 두 번째 호출에서 예외 발생
            // throw on second call
            if (callCount == 2)
                // 강제 예외 발생
                // force throw
                throw new InvalidOperationException("fail");
            // 정상적으로 키 반환
            // return key normally
            return x.Id;
        });
        // 3개 항목 목록
        // list of 3 items
        var items = new[] { new TestItem(1, "A"), new TestItem(2, "B"), new TestItem(3, "C") };

        // 배치 인큐 실행
        // execute batch enqueue
        var result = q.TryEnqueueRange(items);

        // 실패 존재 확인
        // verify failures exist
        Assert.True(result.HasFailures);
        // 실패 항목 수 확인
        // verify failure count
        Assert.Single(result.Failures);
        // 승인 수 확인
        // verify accepted count
        Assert.Equal(2, result.Accepted);
    }

    /// <summary>
    ///     null 항목 컬렉션 전달 시 예외가 발생하는지 검증한다.
    ///     verifies that null items collection throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void TryEnqueueRange_NullItems_ThrowsArgumentNullException() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // null 전달 시 예외 확인
        // verify exception on null input
        Assert.Throws<ArgumentNullException>(
            // null 항목으로 배치 인큐
            // batch enqueue with null items
            () => q.TryEnqueueRange(null!)
        );
    }

    // ──────────────────────────────────────────────
    // TryDequeue / 디큐
    // ──────────────────────────────────────────────

    /// <summary>
    ///     빈 큐에서 디큐 시 false를 반환하는지 검증한다.
    ///     verifies that dequeue from empty queue returns false.
    /// </summary>
    [Fact]
    public void TryDequeue_EmptyQueue_ReturnsFalse() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 빈 큐에서 디큐 시도
        // attempt dequeue from empty queue
        var result = q.TryDequeue(out _);

        // 디큐 실패 확인
        // verify dequeue failed
        Assert.False(result);
    }

    /// <summary>
    ///     단일 항목 디큐가 올바른 항목을 반환하는지 검증한다.
    ///     verifies that single item dequeue returns correct item.
    /// </summary>
    [Fact]
    public void TryDequeue_SingleItem_ReturnsCorrectItem() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 테스트 항목 생성
        // create test item
        var expected = new TestItem(42, "Test");
        // 항목 추가
        // enqueue item
        q.TryEnqueue(expected);

        // 항목 디큐
        // dequeue item
        var result = q.TryDequeue(out var item);

        // 디큐 성공 확인
        // verify dequeue succeeded
        Assert.True(result);
        // 올바른 항목인지 확인
        // verify correct item
        Assert.Equal(expected, item);
        // 큐가 비어있는지 확인
        // verify queue is empty
        Assert.True(q.IsEmpty);
    }

    /// <summary>
    ///     FIFO 순서로 디큐되는지 검증한다.
    ///     verifies that items are dequeued in FIFO order.
    /// </summary>
    [Fact]
    public void TryDequeue_MultipleItems_FifoOrder() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 3개 항목 순서대로 추가
        // enqueue 3 items in order
        q.TryEnqueue(new TestItem(1, "First"));
        q.TryEnqueue(new TestItem(2, "Second"));
        q.TryEnqueue(new TestItem(3, "Third"));

        // 첫 번째 항목 디큐
        // dequeue first item
        q.TryDequeue(out var first);
        // 두 번째 항목 디큐
        // dequeue second item
        q.TryDequeue(out var second);
        // 세 번째 항목 디큐
        // dequeue third item
        q.TryDequeue(out var third);

        // 첫 번째 항목 확인
        // verify first item
        Assert.Equal(1, first.Id);
        // 두 번째 항목 확인
        // verify second item
        Assert.Equal(2, second.Id);
        // 세 번째 항목 확인
        // verify third item
        Assert.Equal(3, third.Id);
    }

    // ──────────────────────────────────────────────
    // TryDequeue with timeout / 타임아웃 디큐
    // ──────────────────────────────────────────────

    /// <summary>
    ///     타임아웃 0으로 빈 큐에서 디큐 시 즉시 false 반환하는지 검증한다.
    ///     verifies that dequeue with timeout 0 returns false immediately on empty queue.
    /// </summary>
    [Fact]
    public void TryDequeue_Timeout0_EmptyQueue_ReturnsFalseImmediately() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 타임아웃 0으로 디큐 시도
        // attempt dequeue with timeout 0
        var result = q.TryDequeue(out _, 0);

        // 즉시 false 반환 확인
        // verify immediate false return
        Assert.False(result);
    }

    /// <summary>
    ///     양수 타임아웃으로 빈 큐에서 디큐 시 타임아웃 후 false 반환하는지 검증한다.
    ///     verifies that dequeue with positive timeout returns false after timeout on empty queue.
    /// </summary>
    [Fact]
    public void TryDequeue_PositiveTimeout_EmptyQueue_ReturnsFalseAfterTimeout() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 50ms 타임아웃으로 디큐 시도
        // attempt dequeue with 50ms timeout
        var result = q.TryDequeue(out _, 50);

        // 타임아웃 후 false 반환 확인
        // verify false returned after timeout
        Assert.False(result);
    }

    /// <summary>
    ///     취소 토큰으로 블로킹 디큐가 취소되는지 검증한다.
    ///     verifies that blocking dequeue is cancelled by cancellation token.
    /// </summary>
    [Fact]
    public void TryDequeue_CancellationRequested_ReturnsFalse() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 이미 취소된 토큰 생성
        // create already cancelled token
        using var cts = new CancellationTokenSource();
        // 즉시 취소
        // cancel immediately
        cts.Cancel();

        // 취소된 토큰으로 디큐 시도
        // attempt dequeue with cancelled token
        var result = q.TryDequeue(out _, -1, cts.Token);

        // false 반환 확인
        // verify false returned
        Assert.False(result);
    }

    /// <summary>
    ///     블로킹 디큐에서 다른 스레드가 항목을 추가하면 성공하는지 검증한다.
    ///     verifies that blocking dequeue succeeds when another thread enqueues.
    /// </summary>
    [Fact]
    public async Task TryDequeue_BlockingWait_SucceedsWhenItemEnqueued() {
        // 큐 생성 (람다 캡처 후 명시적 해제)
        // create queue (explicit dispose after lambda capture)
        var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 예상 항목
        // expected item
        var expected = new TestItem(99, "Async");

        // 100ms 후 항목을 추가하는 백그라운드 작업 시작
        // start background task to enqueue after 100ms
        var enqueueTask = Task.Run(async () => {
            // 100ms 대기
            // wait 100ms
            await Task.Delay(100);
            // 항목 추가
            // enqueue item
            q.TryEnqueue(expected);
        });

        // 2초 타임아웃으로 블로킹 디큐
        // blocking dequeue with 2s timeout
        var result = q.TryDequeue(out var item, 2000);

        // 백그라운드 작업 완료 대기
        // await background task completion
        await enqueueTask;

        // 디큐 성공 확인
        // verify dequeue succeeded
        Assert.True(result);
        // 올바른 항목인지 확인
        // verify correct item
        Assert.Equal(expected, item);
    }

    // ──────────────────────────────────────────────
    // TryPeek / 피크
    // ──────────────────────────────────────────────

    /// <summary>
    ///     빈 큐에서 피크 시 false를 반환하는지 검증한다.
    ///     verifies that peek on empty queue returns false.
    /// </summary>
    [Fact]
    public void TryPeek_EmptyQueue_ReturnsFalse() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 빈 큐에서 피크 시도
        // attempt peek on empty queue
        var result = q.TryPeek(out _);

        // false 반환 확인
        // verify false returned
        Assert.False(result);
    }

    /// <summary>
    ///     피크가 항목을 제거하지 않고 반환하는지 검증한다.
    ///     verifies that peek returns item without removing it.
    /// </summary>
    [Fact]
    public void TryPeek_WithItem_ReturnsItemWithoutRemoving() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 항목 추가
        // enqueue item
        q.TryEnqueue(new TestItem(1, "Peek"));

        // 항목 피크
        // peek item
        var result = q.TryPeek(out var item);

        // 피크 성공 확인
        // verify peek succeeded
        Assert.True(result);
        // 올바른 항목인지 확인
        // verify correct item
        Assert.Equal("Peek", item.Name);
        // 카운트가 변하지 않았는지 확인
        // verify count unchanged
        Assert.Equal(1, q.Count);
    }

    /// <summary>
    ///     타임아웃 0으로 빈 큐에서 피크 시 false 반환하는지 검증한다.
    ///     verifies that peek with timeout 0 on empty queue returns false.
    /// </summary>
    [Fact]
    public void TryPeek_Timeout0_EmptyQueue_ReturnsFalse() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 타임아웃 0으로 피크 시도
        // attempt peek with timeout 0
        var result = q.TryPeek(out _, 0);

        // false 반환 확인
        // verify false returned
        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // ContainsKey / 키 포함 여부
    // ──────────────────────────────────────────────

    /// <summary>
    ///     존재하는 키에 대해 true를 반환하는지 검증한다.
    ///     verifies that existing key returns true.
    /// </summary>
    [Fact]
    public void ContainsKey_ExistingKey_ReturnsTrue() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 항목 추가
        // enqueue item
        q.TryEnqueue(new TestItem(5, "Five"));

        // 키 포함 여부 확인
        // check key containment
        var result = q.ContainsKey(5);

        // true 반환 확인
        // verify true returned
        Assert.True(result);
    }

    /// <summary>
    ///     존재하지 않는 키에 대해 false를 반환하는지 검증한다.
    ///     verifies that non-existing key returns false.
    /// </summary>
    [Fact]
    public void ContainsKey_NonExistingKey_ReturnsFalse() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 존재하지 않는 키 확인
        // check non-existing key
        var result = q.ContainsKey(999);

        // false 반환 확인
        // verify false returned
        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // PendingCountByKey / 키별 대기 수
    // ──────────────────────────────────────────────

    /// <summary>
    ///     중복 허용 모드에서 키별 대기 수를 정확히 반환하는지 검증한다.
    ///     verifies that pending count by key is correct in AllowDuplicate mode.
    /// </summary>
    [Fact]
    public void PendingCountByKey_DuplicateKeys_ReturnsCorrectCount() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 동일 키 항목 3개 추가
        // enqueue 3 items with same key
        q.TryEnqueue(new TestItem(1, "A"), EnqueueMode.AllowDuplicate);
        q.TryEnqueue(new TestItem(1, "B"), EnqueueMode.AllowDuplicate);
        q.TryEnqueue(new TestItem(1, "C"), EnqueueMode.AllowDuplicate);

        // 키별 대기 수 확인
        // check pending count by key
        var count = q.PendingCountByKey(1);

        // 큐에 항목 3개가 있는지 확인
        // verify 3 items in queue
        Assert.Equal(3, q.Count);
        // 키별 카운트가 1 이상인지 확인
        // verify key count is at least 1
        Assert.True(count >= 1);
    }

    /// <summary>
    ///     존재하지 않는 키의 대기 수가 0인지 검증한다.
    ///     verifies that pending count for non-existing key is 0.
    /// </summary>
    [Fact]
    public void PendingCountByKey_NonExistingKey_ReturnsZero() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 존재하지 않는 키 대기 수 확인
        // check pending count for non-existing key
        var count = q.PendingCountByKey(999);

        // 0 반환 확인
        // verify zero returned
        Assert.Equal(0, count);
    }

    // ──────────────────────────────────────────────
    // TryRemoveByKey / 키로 제거
    // ──────────────────────────────────────────────

    /// <summary>
    ///     존재하는 키의 첫 번째 항목이 제거되는지 검증한다.
    ///     verifies that first item with existing key is removed.
    /// </summary>
    [Fact]
    public void TryRemoveByKey_ExistingKey_RemovesFirstOccurrence() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 3개 항목 추가
        // enqueue 3 items
        q.TryEnqueue(new TestItem(1, "A"));
        q.TryEnqueue(new TestItem(2, "B"));
        q.TryEnqueue(new TestItem(3, "C"));

        // 키 2 항목 제거
        // remove item with key 2
        var result = q.TryRemoveByKey(2);

        // 제거 성공 확인
        // verify removal succeeded
        Assert.True(result);
        // 카운트가 2인지 확인
        // verify count is 2
        Assert.Equal(2, q.Count);
        // 키 2가 더 이상 존재하지 않는지 확인
        // verify key 2 no longer exists
        Assert.False(q.ContainsKey(2));
    }

    /// <summary>
    ///     존재하지 않는 키로 제거 시 false를 반환하는지 검증한다.
    ///     verifies that removing non-existing key returns false.
    /// </summary>
    [Fact]
    public void TryRemoveByKey_NonExistingKey_ReturnsFalse() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 항목 추가
        // enqueue item
        q.TryEnqueue(new TestItem(1, "A"));

        // 존재하지 않는 키로 제거 시도
        // attempt to remove non-existing key
        var result = q.TryRemoveByKey(999);

        // false 반환 확인
        // verify false returned
        Assert.False(result);
    }

    // ──────────────────────────────────────────────
    // RemoveAllByKey / 키로 전체 제거
    // ──────────────────────────────────────────────

    /// <summary>
    ///     동일 키의 모든 항목이 제거되는지 검증한다.
    ///     verifies that all items with same key are removed.
    /// </summary>
    [Fact]
    public void RemoveAllByKey_ExistingKey_RemovesAllOccurrences() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 동일 키 3개 + 다른 키 1개 추가
        // enqueue 3 same-key + 1 different-key
        q.TryEnqueue(new TestItem(1, "A"), EnqueueMode.AllowDuplicate);
        q.TryEnqueue(new TestItem(1, "B"), EnqueueMode.AllowDuplicate);
        q.TryEnqueue(new TestItem(2, "C"));
        q.TryEnqueue(new TestItem(1, "D"), EnqueueMode.AllowDuplicate);

        // 키 1의 모든 항목 제거
        // remove all items with key 1
        var removed = q.RemoveAllByKey(1);

        // 제거 수 확인
        // verify removed count
        Assert.Equal(3, removed);
        // 남은 항목 수 확인
        // verify remaining count
        Assert.Equal(1, q.Count);
        // 키 1이 제거되었는지 확인
        // verify key 1 is gone
        Assert.False(q.ContainsKey(1));
    }

    /// <summary>
    ///     존재하지 않는 키로 전체 제거 시 0을 반환하는지 검증한다.
    ///     verifies that removing all by non-existing key returns 0.
    /// </summary>
    [Fact]
    public void RemoveAllByKey_NonExistingKey_ReturnsZero() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 존재하지 않는 키로 전체 제거
        // remove all by non-existing key
        var removed = q.RemoveAllByKey(999);

        // 0 반환 확인
        // verify zero returned
        Assert.Equal(0, removed);
    }

    // ──────────────────────────────────────────────
    // Clear / 지우기
    // ──────────────────────────────────────────────

    /// <summary>
    ///     Clear가 모든 항목과 키를 제거하는지 검증한다.
    ///     verifies that Clear removes all items and keys.
    /// </summary>
    [Fact]
    public void Clear_WithItems_RemovesAllItemsAndKeys() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 항목 3개 추가
        // enqueue 3 items
        q.TryEnqueue(new TestItem(1, "A"));
        q.TryEnqueue(new TestItem(2, "B"));
        q.TryEnqueue(new TestItem(3, "C"));

        // 큐 비우기
        // clear queue
        q.Clear();

        // 카운트가 0인지 확인
        // verify count is 0
        Assert.Equal(0, q.Count);
        // 비어있는지 확인
        // verify empty
        Assert.True(q.IsEmpty);
        // 고유 키 수가 0인지 확인
        // verify unique key count is 0
        Assert.Equal(0, q.UniqueKeyCount);
    }

    // ──────────────────────────────────────────────
    // Snapshot / 스냅샷
    // ──────────────────────────────────────────────

    /// <summary>
    ///     Snapshot이 FIFO 순서의 얕은 복사를 반환하는지 검증한다.
    ///     verifies that Snapshot returns shallow copy in FIFO order.
    /// </summary>
    [Fact]
    public void Snapshot_WithItems_ReturnsFifoOrderedCopy() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 항목 추가
        // enqueue items
        q.TryEnqueue(new TestItem(1, "A"));
        q.TryEnqueue(new TestItem(2, "B"));

        // 스냅샷 가져오기
        // get snapshot
        var snapshot = q.Snapshot();

        // 스냅샷 크기 확인
        // verify snapshot size
        Assert.Equal(2, snapshot.Count);
        // 첫 번째 항목 확인
        // verify first item
        Assert.Equal(1, snapshot[0].Id);
        // 두 번째 항목 확인
        // verify second item
        Assert.Equal(2, snapshot[1].Id);
        // 원본 큐는 변경되지 않았는지 확인
        // verify original queue unchanged
        Assert.Equal(2, q.Count);
    }

    // ──────────────────────────────────────────────
    // GetKeySnapshot / 키 스냅샷
    // ──────────────────────────────────────────────

    /// <summary>
    ///     GetKeySnapshot이 키별 카운트를 정확히 반환하는지 검증한다.
    ///     verifies that GetKeySnapshot returns accurate per-key counts.
    /// </summary>
    [Fact]
    public void GetKeySnapshot_WithDuplicates_ReturnsAccurateCounts() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 중복 포함 항목 추가
        // enqueue items with duplicates
        q.TryEnqueue(new TestItem(1, "A"), EnqueueMode.AllowDuplicate);
        q.TryEnqueue(new TestItem(1, "B"), EnqueueMode.AllowDuplicate);
        q.TryEnqueue(new TestItem(2, "C"));

        // 키 스냅샷 가져오기
        // get key snapshot
        var snapshot = q.GetKeySnapshot();

        // 큐에 항목 3개가 있는지 확인
        // verify 3 items in queue
        Assert.Equal(3, q.Count);
        // 키 수 확인
        // verify key count
        Assert.Equal(2, snapshot.Count);
        // 키 1이 존재하는지 확인
        // verify key 1 exists
        Assert.True(snapshot.ContainsKey(1));
        // 키 2 카운트 확인
        // verify key 2 count
        Assert.Equal(1, snapshot[2]);
    }

    // ──────────────────────────────────────────────
    // Dispose / 해제
    // ──────────────────────────────────────────────

    /// <summary>
    ///     Dispose 후 모든 작업에서 ObjectDisposedException이 발생하는지 검증한다.
    ///     verifies that all operations throw ObjectDisposedException after Dispose.
    /// </summary>
    [Fact]
    public void Dispose_SubsequentCalls_ThrowObjectDisposedException() {
        // 큐 생성
        // create queue
        var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 큐 해제
        // dispose queue
        q.Dispose();

        // TryEnqueue 시 예외 확인
        // verify TryEnqueue throws
        Assert.Throws<ObjectDisposedException>(() => q.TryEnqueue(new TestItem(1, "A")));
        // TryDequeue 시 예외 확인
        // verify TryDequeue throws
        Assert.Throws<ObjectDisposedException>(() => q.TryDequeue(out _));
        // TryPeek 시 예외 확인
        // verify TryPeek throws
        Assert.Throws<ObjectDisposedException>(() => q.TryPeek(out _));
        // Count 시 예외 확인
        // verify Count throws
        Assert.Throws<ObjectDisposedException>(() => _ = q.Count);
        // ContainsKey 시 예외 확인
        // verify ContainsKey throws
        Assert.Throws<ObjectDisposedException>(() => q.ContainsKey(1));
        // Clear 시 예외 확인
        // verify Clear throws
        Assert.Throws<ObjectDisposedException>(() => q.Clear());
        // Snapshot 시 예외 확인
        // verify Snapshot throws
        Assert.Throws<ObjectDisposedException>(() => q.Snapshot());
    }

    /// <summary>
    ///     이중 Dispose가 예외를 발생시키지 않는지 검증한다.
    ///     verifies that double Dispose does not throw.
    /// </summary>
    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow() {
        // 큐 생성
        // create queue
        var q = KeyedQueue<TestItem, int>.Create(x => x.Id);

        // 두 번 연속 해제 시 예외 없음 확인
        // verify no exception on double dispose
        q.Dispose();
        // 두 번째 Dispose 호출
        // call Dispose second time
        q.Dispose();
    }

    // ──────────────────────────────────────────────
    // 동시성 / Concurrency
    // ──────────────────────────────────────────────

    /// <summary>
    ///     다중 스레드에서 동시 인큐/디큐가 안전한지 검증한다.
    ///     verifies that concurrent enqueue/dequeue is thread-safe.
    /// </summary>
    [Fact]
    public async Task Concurrency_MultipleProducersConsumers_NoDataLoss() {
        // 큐 생성 (람다 캡처 후 명시적 해제)
        // create queue (explicit dispose after lambda capture)
        var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 항목 수
        // item count
        const int itemCount = 1000;
        // 디큐된 항목 목록
        // dequeued items list
        var dequeued = new ConcurrentBag<TestItem>();

        // 생산자: 1000개 항목을 중복 허용 모드로 추가
        // producer: enqueue 1000 items in AllowDuplicate mode
        var producerTask = Task.Run(() => {
            // 각 항목을 순서대로 추가
            // enqueue each item in order
            for (var i = 0; i < itemCount; i++)
                // 항목 추가
                // enqueue item
                q.TryEnqueue(new TestItem(i, $"Item{i}"), EnqueueMode.AllowDuplicate);
        });

        // 소비자: 항목이 모두 소비될 때까지 디큐
        // consumer: dequeue until all items consumed
        var consumerTask = Task.Run(() => {
            // 소비된 수가 총 수에 도달할 때까지 반복
            // loop until consumed count reaches total
            var consumed = 0;
            // 소비 루프
            // consumption loop
            while (consumed < itemCount)
                // 항목 디큐 시도
                // attempt dequeue
                if (q.TryDequeue(out var item, 100)) {
                    // 디큐된 항목 저장
                    // store dequeued item
                    dequeued.Add(item);
                    // 소비 카운트 증가
                    // increment consumed count
                    consumed++;
                }
        });

        // 두 작업 모두 완료 대기 (5초 타임아웃)
        // await both tasks with 5s timeout
        await Task.WhenAll(producerTask, consumerTask).WaitAsync(TimeSpan.FromSeconds(5));

        // 모든 항목이 소비되었는지 확인
        // verify all items consumed
        Assert.Equal(itemCount, dequeued.Count);
    }

    // ──────────────────────────────────────────────
    // KeyedQueue<T> convenience class / 편의 래퍼
    // ──────────────────────────────────────────────

    /// <summary>
    ///     KeyedQueue&lt;T&gt;가 항목 자체를 키로 사용하는지 검증한다.
    ///     verifies that KeyedQueue&lt;T&gt; uses item itself as key.
    /// </summary>
    [Fact]
    public void KeyedQueueT_UsesItemAsKey_EnforceUnique() {
        // 항목 자체를 키로 사용하는 큐 생성
        // create queue using item itself as key
        using var q = new KeyedQueue<string>();
        // 첫 번째 문자열 추가
        // enqueue first string
        q.TryEnqueue("hello");

        // 동일 문자열 추가 시도
        // attempt to enqueue same string
        var result = q.TryEnqueue("hello");

        // 중복 거부 확인
        // verify duplicate rejected
        Assert.False(result);
        // 카운트가 1인지 확인
        // verify count is 1
        Assert.Equal(1, q.Count);
    }

    /// <summary>
    ///     KeyedQueue&lt;T&gt;에서 다른 항목은 추가되는지 검증한다.
    ///     verifies that different items are enqueued in KeyedQueue&lt;T&gt;.
    /// </summary>
    [Fact]
    public void KeyedQueueT_DifferentItems_AllAccepted() {
        // 정수 큐 생성
        // create integer queue
        using var q = new KeyedQueue<int>();
        // 3개 항목 추가
        // enqueue 3 items
        q.TryEnqueue(1);
        q.TryEnqueue(2);
        q.TryEnqueue(3);

        // 카운트가 3인지 확인
        // verify count is 3
        Assert.Equal(3, q.Count);
    }

    /// <summary>
    ///     디큐 후 동일 키를 다시 추가할 수 있는지 검증한다.
    ///     verifies that same key can be re-enqueued after dequeue.
    /// </summary>
    [Fact]
    public void TryEnqueue_AfterDequeue_SameKeyAccepted() {
        // 큐 생성
        // create queue
        using var q = KeyedQueue<TestItem, int>.Create(x => x.Id);
        // 항목 추가
        // enqueue item
        q.TryEnqueue(new TestItem(1, "First"));
        // 항목 디큐
        // dequeue item
        q.TryDequeue(out _);

        // 동일 키로 재추가
        // re-enqueue with same key
        var result = q.TryEnqueue(new TestItem(1, "Second"));

        // 재추가 성공 확인
        // verify re-enqueue succeeded
        Assert.True(result);
        // 카운트가 1인지 확인
        // verify count is 1
        Assert.Equal(1, q.Count);
    }

    // 테스트용 항목 레코드
    // test item record
    private record TestItem(int Id, string Name);
}