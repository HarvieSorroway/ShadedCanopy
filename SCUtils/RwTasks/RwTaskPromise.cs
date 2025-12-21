using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Stack<RwTaskPromise> _pool = new Stack<RwTaskPromise>();
        private static object _poolLock = new();



        public static RwTaskPromise Create(int delayCount, CancellationToken cancellationToken)
        {
            RwTaskPromise promise;
            lock (_poolLock)
            {
                if (_pool.Count > 0) promise = _pool.Pop();
                else promise = new RwTaskPromise();
            }
            promise._cancellationToken = cancellationToken;
            promise._remainingFrames = delayCount;
            return promise;
        }

        private static void Return(RwTaskPromise promise)
        {
            promise.Reset();
            lock (_poolLock)
            {
                _pool.Push(promise);
            }
        }


        private Action _continuation;
        private Exception _exception;
        private ManualResetEventSlim _waitEv;
        private RwTaskStatus _status;
        private CancellationToken _cancellationToken;
        private int _remainingFrames;
        private short _token;
        private IRwTaskSource _nextNode;
        private bool _isFinished;

        private RwTaskPromise()
        {
            _token = 0;
            _status = RwTaskStatus.Pending;
        }


        public bool Execute()
        {
            if (_isFinished) return true;

            if(_status != RwTaskStatus.Pending)
            {
                return Finish();
            }

            if (_cancellationToken.IsCancellationRequested)
            {
                _status = RwTaskStatus.Canceled;
                SetException(new OperationCanceledException(_cancellationToken));
                return Finish();
            }

            if (_remainingFrames == -1)
            {
                return RwTaskStatus.Succeeded == _status;
            }
            if(_remainingFrames-- == 0)
            {
                _status = RwTaskStatus.Succeeded;
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
            _continuation = continuation;
            if (_status != RwTaskStatus.Pending)  //已经完成
            {
                continuation?.Invoke();
                return;
            }
        }

        public void GetResult(short token)
        {
            try
            {
                if (token != _token) throw new InvalidOperationException();

                if (_status is RwTaskStatus.Canceled or RwTaskStatus.Faulted)
                    throw _exception;
                if (_status is not RwTaskStatus.Succeeded)
                {
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
                        throw _exception;
                }
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
            _isFinished = false;
            _exception = null;
            try { _waitEv?.Dispose(); } catch { }
            _waitEv = null;
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
            if (RwTaskContext.Current != null) //在同步上下文
            {
                Finish();
            }
        }
        public void SetResult()
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _status = RwTaskStatus.Succeeded;
            }
            if (RwTaskContext.Current != null) //在同步上下文
            {
                Finish();
            }
        }
        private bool Finish()
        {
            _isFinished = true;
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

        public RwTask Task => new RwTask(this, _token);

        public ref IRwTaskSource NextNode => ref _nextNode;
    }


    public class RwTaskPromise<T> : IRwTaskSource<T>
    {
        private static readonly Stack<RwTaskPromise<T>> _pool = new Stack<RwTaskPromise<T>>();
        private static object _poolLock = new();


        public static RwTaskPromise<T> Create(CancellationToken cancellationToken)
        {
            RwTaskPromise<T> promise;
            lock (_poolLock)
            {
                if (_pool.Count > 0) promise = _pool.Pop();
                else promise = new RwTaskPromise<T>();
            }
            promise._cancellationToken = cancellationToken;
            return promise;
        }

        private static void Return(RwTaskPromise<T> promise)
        {
            promise.Reset();
            lock (_poolLock)
            {
                _pool.Push(promise);
            }
        }


        private Action _continuation;
        private Exception _exception;
        private ManualResetEventSlim _waitEv;
        private T _result;
        private RwTaskStatus _status;
        private CancellationToken _cancellationToken;
        private short _token;
        private IRwTaskSource _nextNode;
        private bool _isFinished;

        private RwTaskPromise()
        {
            _token = 0;
            _status = RwTaskStatus.Pending;
        }


        public bool Execute()
        {
            if (_isFinished) return true;

            if (_status != RwTaskStatus.Pending)
            {
                return Finish();
            }

            if (_cancellationToken.IsCancellationRequested)
            {
                _status = RwTaskStatus.Canceled;
                SetException(new OperationCanceledException(_cancellationToken));
                return Finish();
            }

            if (_status == RwTaskStatus.Succeeded)
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
            _continuation = continuation;
            if (_status != RwTaskStatus.Pending) //已经完成
            {
                continuation?.Invoke();
                return;
            }
        }

        public T GetResult(short token)
        {
            try
            {
                if (token != _token) throw new InvalidOperationException();

                if (_status is RwTaskStatus.Canceled or RwTaskStatus.Faulted)
                    throw _exception;
                if (_status is not RwTaskStatus.Succeeded)
                {
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
                        throw _exception;

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

            if (RwTaskContext.Current != null) //在同步上下文
            {
                Finish();
            }
        }

        public void SetResult(T result)
        {
            lock (this)
            {
                if (_status != RwTaskStatus.Pending) return;
                _result = result;
                _status = RwTaskStatus.Succeeded;
            }
            if (RwTaskContext.Current != null) //在同步上下文
            {
                Finish();
            }
        }

        private bool Finish()
        {
            _isFinished = true;
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

        public RwTask<T> Task => new RwTask<T>(this, _token);

        public ref IRwTaskSource NextNode => ref _nextNode;
    }
}
