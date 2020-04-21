using System;
using System.Collections.Generic;
using System.Text;

namespace GAFBot.Database.Readers
{
    public class DBSearchFunc<T> : IEquatable<DBSearchFunc<T>> where T : class
    {
        private Func<T, bool> _func;

        public DBSearchFunc()
        {

        }

        public void Add(Func<T, bool> func)
        {
            if (_func == null)
            {
                _func = func;
                return;
            }

            _func += func;
        }

        public void Remove(Func<T, bool> func)
        {
            if (_func == null)
                return;

            _func -= func;
        }

        public Func<T, bool> GetCopy()
        {
            return (Func<T, bool>)_func.Clone();
        }

        public Func<T, bool> Get()
        {
            return _func;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DBSearchFunc<T>);
        }

        public bool Equals(DBSearchFunc<T> other)
        {
            return other != default(DBSearchFunc<T>) &&
                   EqualityComparer<DBSearchFunc<T>>.Default.Equals(this, other);
        }

        public bool Equals(Func<T, bool> other)
        {
            return other != null &&
                   EqualityComparer<Func<T, bool>>.Default.Equals(_func, other);
        }

        public bool IsNull()
        {
            return _func == null;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_func);
        }

        public static DBSearchFunc<T> operator +(DBSearchFunc<T> left, Func<T, bool> right)
        {
            left.Add(right);
            return left;
        }

        public static DBSearchFunc<T> operator -(DBSearchFunc<T> left, Func<T, bool> right)
        {
            left.Remove(right);
            return left;
        }

        public static bool operator ==(DBSearchFunc<T> left, DBSearchFunc<T> right)
        {
            return EqualityComparer<DBSearchFunc<T>>.Default.Equals(left, right);
        }

        public static bool operator ==(DBSearchFunc<T> left, Func<T, bool> right)
        {
            return EqualityComparer<Func<T, bool>>.Default.Equals(left._func, right);
        }

        public static bool operator !=(DBSearchFunc<T> left, DBSearchFunc<T> right)
        {
            return !(left == right);
        }
        public static bool operator !=(DBSearchFunc<T> left, Func<T, bool> right)
        {
            return !(left._func == right);
        }
    }
}
