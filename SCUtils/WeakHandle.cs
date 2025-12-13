using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SCUtils
{

    public class WeakHandle : IDisposable, IEquatable<WeakHandle>
    {
        private readonly GCHandle _handle;
        private readonly int _cachedHashCode;

        public WeakHandle(object target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            _handle = GCHandle.Alloc(target, GCHandleType.Weak);
            _cachedHashCode = target.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTarget(out object target)
        {
            if (_handle.IsAllocated)
            {
                target = _handle.Target;
                if (target != null)
                {
                    return true;
                }
            }

            target = null;
            return false;
        }

        public object Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_handle.IsAllocated) return null;

                object target = _handle.Target;
                if (target == null) return null;
                return target;
            }
        }


        public bool IsAlive => _handle.IsAllocated && _handle.Target != null;


        public override int GetHashCode()
        {
            return _cachedHashCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WeakHandle);
        }

        public bool Equals(WeakHandle other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (_cachedHashCode != other._cachedHashCode) return false;

            object targetA = Target;
            object targetB = other.Target;


            if (targetA != null && targetB != null)
            {
                return ReferenceEquals(targetA, targetB);
            }

            return false;
        }
        public static bool operator ==(WeakHandle a, WeakHandle b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(WeakHandle a, WeakHandle b)
        {
            return !a.Equals(b);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        ~WeakHandle()
        {
            Dispose(false);
        }
    }


    public class WeakHandle<T> : IDisposable, IEquatable<WeakHandle<T>> where T : class
    {
        private readonly GCHandle _handle;
        private readonly int _cachedHashCode;

        public WeakHandle(T target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            _handle = GCHandle.Alloc(target, GCHandleType.Weak);
            _cachedHashCode = target.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTarget(out T target)
        {
            if (_handle.IsAllocated)
            {
                object obj = _handle.Target;
                if (obj != null)
                {
                    target = (T)obj;
                    return true;
                }
            }

            target = null;
            return false;
        }

        public T Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_handle.IsAllocated) return null;

                object target = _handle.Target;
                if (target == null) return null;
                return (T)target;
            }
        }


        public bool IsAlive => _handle.IsAllocated && _handle.Target != null;


        public override int GetHashCode()
        {
            return _cachedHashCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WeakHandle<T>);
        }

        public bool Equals(WeakHandle<T> other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (_cachedHashCode != other._cachedHashCode) return false;

            T targetA = Target;
            T targetB = other.Target;

  
            if (targetA != null && targetB != null)
            {
                return ReferenceEquals(targetA, targetB);
            }

            return false;
        }
        public static bool operator ==(WeakHandle<T> a, WeakHandle<T> b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(WeakHandle<T> a, WeakHandle<T> b)
        {
            return !a.Equals(b);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }

        ~WeakHandle()
        {
            Dispose(false);
        }
    }
}
