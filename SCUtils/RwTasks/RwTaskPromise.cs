using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    public interface IRwTaskSource
    {
        RwTaskStatus GetStatus(short token);

        void OnCompleted(Action continuation, short token);

        bool Execute();

        ref IRwTaskSource NextNode {  get; }
    }
    public interface IRwTaskSourceVoid : IRwTaskSource
    {
        void GetResult(short token);
    }

    public interface IRwTaskSource<T> : IRwTaskSource
    {
        T GetResult(short token);
    }



    public class RwTaskPromise : IRwTaskSourceVoid
    {
        private static readonly ConcurrentStack<RwTaskPromise> _pool = new ConcurrentStack<RwTaskPromise>();
   

        public static RwTaskPromise Create(CancellationToken cancellationToken = default, RwLoopRunner runner = null, bool forceNextFrame = false)
        {
            RwTaskPromise promise;
            if (_pool.TryPop(out promise))   { }
            else promise = new RwTaskPromise();   
            promise._cancellationToken = cancellationToken;
            promise._runner = runner ?? RwTaskContext.Current;
            promise._forceNextFrame = forceNextFrame;
            promise.Setup();
            return promise;
        }


        public static RwTaskPromise CreateCompleted()
        {
            RwTaskPromise promise;
            if (_pool.TryPop(out promise)) { }
            else promise = new RwTaskPromise();
            promise._status = RwTaskStatus.Succeeded;
            promise._isFinished = true;
            return promise;
        }

        public static RwTaskPromise CreateCanceled(CancellationToken token)
        {
            RwTaskPromise promise;
            if (_pool.TryPop(out promise)) { }
            else promise = new RwTaskPromise();
            promise._cancellationToken = token;
            promise._status = RwTaskStatus.Canceled;
            promise._isFinished = true;
            promise._exception = new OperationCanceledException(token);
            return promise;
        }

        protected virtual void Return(RwTaskPromise promise)
        {
            promise.Reset();
            _pool.Push(promise);
        }


        protected Action _continuation;
        protected RwLoopRunner _runner;
        protected Exception _exception;
        protected ManualResetEventSlim _waitEv;
        protected RwTaskStatus _status;
        protected CancellationToken _cancellationToken;
        protected short _token;
        protected IRwTaskSource _nextNode;
        protected bool _isFinished;
        protected bool _forceNextFrame;
        protected CancellationTokenRegistration _ctr;

        protected RwTaskPromise()
        {
            _token = 0;
            _status = RwTaskStatus.Pending;
        }

        protected void Setup()
        {
            _ctr = _cancellationToken.Register(() => SetCancel());
        }
 
        public virtual bool Execute()
        {
            if (_isFinished) return true;

            if(_status != RwTaskStatus.Pending)
            {
                return Finish();
            }
            return false;
      
        }


        public RwTaskStatus GetStatus(short token)
        {
            if (token != _token) return RwTaskStatus.Succeeded;
            return _status;
        }

        public void OnCompleted(Action continuation, short token)
        {
            if (token != _token) return;
            if (continuation == null)
            {
                Interlocked.Exchange(ref _continuation, null);
                return;
            }
            Action wrappedAction = continuation;
            var capturedContext = ExecutionContext.Capture();
            if (capturedContext is null)
                wrappedAction = continuation;
            else
                wrappedAction = () => ExecutionContext.Run(capturedContext, _ => continuation(), null);

            var oldContinuation = Interlocked.CompareExchange(ref _continuation, wrappedAction, null);

            if (oldContinuation != null)
            {
                throw new InvalidOperationException("Task already has a continuation.");
            }


            if (_isFinished) //如果_isFinished为true，说明任务已经完成，
                             //需要立刻执行continuation（如果continuation被执行的情况下，_continuation应该为null）
            {
                var span = Interlocked.Exchange(ref _continuation, null);
                span?.Invoke();
            }
        }

        public void GetResult(short token)
        {
            if (token != _token) throw new InvalidOperationException();

            try
            {
                if (_status is RwTaskStatus.Canceled or RwTaskStatus.Faulted)
                    ExceptionDispatchInfo.Capture(_exception).Throw();

                if (_status is not RwTaskStatus.Succeeded)
                {
                    if (SCHelperUtils.IsMainThread)
                        throw new InvalidOperationException("Call GetResult for an uncompleted rwTask in mainThread will cause deadlocked");

                    var ev = Volatile.Read(ref _waitEv);
                    if (ev is null)
                    {
                        var newEv = new ManualResetEventSlim(false);
                        var current = Interlocked.CompareExchange(ref _waitEv, newEv, null);

                        if (current == null)
                        {
                            ev = newEv;
                        }
                        else
                        {
                            ev = current;
                            newEv.Dispose();
                        }
                    }
                    try
                    {
                        ev?.Wait();
                    }
                    catch (ObjectDisposedException) { }

                    if (_status is RwTaskStatus.Canceled or RwTaskStatus.Faulted)
                        ExceptionDispatchInfo.Capture(_exception).Throw();
                }
            }
            finally
            {
                Return(this);
            }
        }

        protected virtual void Reset()
        {
            _continuation = null;
            _status = RwTaskStatus.Pending;
            _nextNode = null;
            _isFinished = false;
            _exception = null;
            try { _waitEv?.Dispose(); } catch { }
            _waitEv = null;
            _exception = null;
            _runner = null;
            _forceNextFrame = false;
            _cancellationToken = CancellationToken.None;
            _token++; 
        }
        public void SetException(Exception ex)
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _exception = ex;
                _status = RwTaskStatus.Faulted;
            }
            TriggerCompletion();
        }

        public void SetCancel(CancellationToken? token = null)
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _exception = new OperationCanceledException(token ?? _cancellationToken);
                _status = RwTaskStatus.Canceled;
            }
            TriggerCompletion();
        }

        public void SetResult()
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _status = RwTaskStatus.Succeeded;
            }
            TriggerCompletion();
        }

        private bool Finish()
        {
            _isFinished = true;
            var cont = Interlocked.Exchange(ref _continuation, null);
            cont?.Invoke();
            _ctr.Dispose();
            var ev = Interlocked.Exchange(ref _waitEv, null);
            if (ev != null)
            {
                ev.Set();
                ev.Dispose();
            }
            return true;
        }

        private void TriggerCompletion()
        {
            if (_forceNextFrame)
            {
                (_runner ?? RwLoopRunner.DefaultRunner).Schedule(this);
            }
            else
            {
                if (_runner != null && _runner != RwTaskContext.Current)
                {
                    _runner.Schedule(this);
                }
                else if (RwTaskContext.Current != null)
                {
                    Finish();
                }
                else
                {
                    RwLoopRunner.DefaultRunner.Schedule(this);
                }
            }
        }

        internal short Token => _token;

        public RwTask Task => new RwTask(this, _token);

        public ref IRwTaskSource NextNode => ref _nextNode;
    }


    public class RwTaskPromise<T> : IRwTaskSource<T>
    {
        private static readonly ConcurrentStack<RwTaskPromise<T>> _pool = new ConcurrentStack<RwTaskPromise<T>>();


        public static RwTaskPromise<T> Create(CancellationToken cancellationToken = default, RwLoopRunner runner = null, bool forceNextFrame = false)
        {
            RwTaskPromise<T> promise;
            if (_pool.TryPop(out promise)) { }
            else promise = new RwTaskPromise<T>();
            promise._cancellationToken = cancellationToken;
            promise._runner = runner;
            promise._forceNextFrame = forceNextFrame;
            promise.Setup();
            return promise;
        }

        public static RwTaskPromise<T> CreateCompleted(T value)
        {
            RwTaskPromise<T> promise;
            if (_pool.TryPop(out promise)) { }
            else promise = new RwTaskPromise<T>();
            promise._result = value;
            promise._status = RwTaskStatus.Succeeded;
            return promise;
        }

        public static RwTaskPromise<T> CreateCanceled(CancellationToken token)
        {
            RwTaskPromise<T> promise;
            if (_pool.TryPop(out promise)) { }
            else promise = new RwTaskPromise<T>();
            promise._cancellationToken = token;
            promise._status = RwTaskStatus.Canceled;
            promise._exception = new OperationCanceledException(token);
            return promise;
        }

        protected virtual void Return(RwTaskPromise<T> promise)
        {
            promise.Reset();
            _pool.Push(promise);
        }


        private Action _continuation;
        private RwLoopRunner _runner;
        private Exception _exception;
        private ManualResetEventSlim _waitEv;
        private T _result;
        private RwTaskStatus _status;
        private CancellationToken _cancellationToken;
        private short _token;
        private IRwTaskSource _nextNode;
        private bool _isFinished;
        private bool _forceNextFrame;
        private CancellationTokenRegistration _ctr;

        private RwTaskPromise()
        {
            _token = 0;
            _status = RwTaskStatus.Pending;
        }



        private void Setup()
        {
            _ctr = _cancellationToken.Register(() => SetCancel());
        }


        public virtual bool Execute()
        {
            if (_isFinished) return true;

            if (_status != RwTaskStatus.Pending)
            {
                return Finish();
            }
            return false;

        }


        public RwTaskStatus GetStatus(short token)
        {
            if (token != _token) return RwTaskStatus.Succeeded;
            return _status;
        }

        public void OnCompleted(Action continuation, short token)
        {
            if (token != _token) return;
            if (continuation == null)
            {
                Interlocked.Exchange(ref _continuation, null);
                return;
            }
            Action wrappedAction = continuation;
            var capturedContext = ExecutionContext.Capture();
            if (capturedContext is null)
                wrappedAction = continuation;
            else
                wrappedAction = () => ExecutionContext.Run(capturedContext, _ => continuation(), null);

            var oldContinuation = Interlocked.CompareExchange(ref _continuation, wrappedAction, null);

            if (oldContinuation != null)
            {
                throw new InvalidOperationException("Task already has a continuation.");
            }


            if (_isFinished) //如果_isFinished为true，说明任务已经完成，
                             //需要立刻执行continuation（如果continuation被执行的情况下，_continuation应该为null）
            {
                var span = Interlocked.Exchange(ref _continuation, null);
                span?.Invoke();
            }
        }

        public T GetResult(short token)
        {
            if (token != _token) throw new InvalidOperationException();

            try
            {
                if (_status is RwTaskStatus.Canceled or RwTaskStatus.Faulted)
                    ExceptionDispatchInfo.Capture(_exception).Throw();

                if (_status is not RwTaskStatus.Succeeded)
                {
                    if (SCHelperUtils.IsMainThread)
                        throw new InvalidOperationException("Call GetResult for an uncompleted rwTask in mainThread will cause deadlocked");
                        
                    var ev = Volatile.Read(ref _waitEv);
                    if (ev is null)
                    {
                        var newEv = new ManualResetEventSlim(false);
                        var current = Interlocked.CompareExchange(ref _waitEv, newEv, null);

                        if (current == null)
                        {
                            ev = newEv;
                        }
                        else
                        {
                            ev = current;
                            newEv.Dispose();
                        }

                    }
                    try
                    {
                        ev?.Wait();
                    }
                    catch (ObjectDisposedException)  {  }

                    if (_status is RwTaskStatus.Canceled or RwTaskStatus.Faulted)
                        ExceptionDispatchInfo.Capture(_exception).Throw();

                }
                return _result;
            }
            finally
            {
                Return(this);
            }
        }

        private void Reset()
        {
            _continuation = null;
            _status = RwTaskStatus.Pending;
            _nextNode = null;
            try { _waitEv?.Dispose(); } catch { }
            _waitEv = null;
            _exception = null;
            _runner = null;
            _cancellationToken = CancellationToken.None;
            _isFinished = false;
            _token++;
        }

        public void SetException(Exception ex)
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _exception = ex;
                _status = RwTaskStatus.Faulted;
            }
            TriggerCompletion();
        }

        public void SetResult(T result)
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _result = result;
                _status = RwTaskStatus.Succeeded;
            }
            TriggerCompletion();
        }

        public void SetCancel(CancellationToken? token = null)
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _exception = new OperationCanceledException(token ?? _cancellationToken);
                _status = RwTaskStatus.Canceled;
            }
            TriggerCompletion();
        }

        private bool Finish()
        {
            _isFinished = true;
            _ctr.Dispose();
            var cont = Interlocked.Exchange(ref _continuation, null);
            cont?.Invoke();

            var ev = Interlocked.Exchange(ref _waitEv, null);
            if (ev != null)
            {
                ev.Set();
                ev.Dispose();
            }
            return true;
        }

        private void TriggerCompletion()
        {
            if (_forceNextFrame)
            {
                (_runner ?? RwLoopRunner.DefaultRunner).Schedule(this);
            }
            else
            {
                if (_runner != null && _runner != RwTaskContext.Current)
                {
                    _runner.Schedule(this);
                }
                else if (RwTaskContext.Current != null)
                {
                    Finish();
                }
                else
                {
                    RwLoopRunner.DefaultRunner.Schedule(this);
                }
            }
        }
        internal short Token => _token;

        public RwTask<T> Task => new RwTask<T>(this, _token);

        public ref IRwTaskSource NextNode => ref _nextNode;
    }

    public class RwYieldPromise : RwTaskPromise
    {
        private static readonly ConcurrentStack<RwYieldPromise> _pool = new ConcurrentStack<RwYieldPromise>();

        public static RwTaskPromise CreateYield(CancellationToken cancellationToken = default, RwLoopRunner runner = null)
        {
            RwYieldPromise promise;
            if (_pool.TryPop(out promise)) { }
            else promise = new RwYieldPromise();
            promise._cancellationToken = cancellationToken;
            promise._runner = runner;
            promise._forceNextFrame = true;
            promise.Setup();
            (runner ?? RwLoopRunner.DefaultRunner).Schedule(promise);
            return promise;
        }

        public override bool Execute()
        {
            _status = RwTaskStatus.Succeeded;
            return base.Execute();
        }

        protected override void Return(RwTaskPromise promise)
        {
            _pool.Push((RwYieldPromise)promise);
            ((RwYieldPromise)promise).Reset();
        }
    }
}
