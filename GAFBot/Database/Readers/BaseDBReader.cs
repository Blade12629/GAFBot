using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAFBot.Database.Readers
{
    public class BaseDBReader<T> : IDisposable where T : class
    {
        protected GAFContext _context;
        public bool Disposed { get; private set; }

        public BaseDBReader(GAFContext context = null)
        {
            if (context == null)
                _context = new GAFContext();
            else
                _context = context;

            Disposed = false;
        }

        public virtual T Add(T value)
        {
            return (T)_context.Add(value).Entity;
        }

        public virtual T Update(T value)
        {
            return (T)_context.Update(value).Entity;
        }

        public virtual T AddOrUpdate(T value)
        {
            if (Contains(value))
                return Update(value);
            else
                return Add(value);
        }

        public virtual T AddOrUpdate(T value, Func<T, bool> checkForExisting)
        {
            if (Contains(checkForExisting))
                return Update(value);
            else
                return Add(value);
        }

        public virtual DbSet<T> GetSet()
        {
            return _context.Set<T>();
        }

        public virtual bool Contains(T value)
        {
            DbSet<T> dbSet = _context.Set<T>();

            return dbSet.FirstOrDefault(entry => entry.Equals(value)) != null;
        }

        public virtual bool Contains(Func<T, bool> func)
        {
            DbSet<T> dbSet = _context.Set<T>();

            return dbSet.FirstOrDefault(func) != null;
        }

        public virtual T Get(T value)
        {
            DbSet<T> dbSet = _context.Set<T>();

            return dbSet.FirstOrDefault(entry => entry.Equals(value));
        }

        public virtual T Get(Func<T, bool> func)
        {
            DbSet<T> dbSet = _context.Set<T>();

            return dbSet.FirstOrDefault(func);
        }

        public virtual List<T> Where(Func<T, bool> func)
        {
            DbSet<T> dbSet = _context.Set<T>();

            return dbSet.Where(func).ToList();
        }

        public int GetCount()
        {
            var set = GetSet();
            return set.Count();
        }

        public int GetCount(Func<T, bool> func)
        {
            var set = GetSet();
            return set.Count(func);
        }

        public virtual void Remove(Func<T, bool> searchFunc)
        {
            var set = GetSet();

            T obj = set.FirstOrDefault(searchFunc);

            if (obj == null)
                return;

            set.Remove(obj);
        }
        public virtual void RemoveAll(Func<T, bool> searchFunc)
        {
            var set = GetSet();

            List<T> toRemove = set.Where(searchFunc).ToList();

            for (int i = 0; i < toRemove.Count; i++)
                set.Remove(toRemove[i]);
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }

        public virtual void Dispose()
        {
            if (!Disposed)
            {
                _context?.Dispose();
                Disposed = true;
            }
        }

    }
}
