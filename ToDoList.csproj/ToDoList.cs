using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToDoList
{
    public class ToDoList : IToDoList
    {

        List<int> dismissedUsers = new List<int>(); // если юзер в списке, то мы не отображаем все что было сделагно им
        public class Datas
        {
            public int entryId; // айдишник тупа
            public long timestamp;
            public int userId; // юзер последнего изменившего

            public Datas(int entryId, int userId, long timestamp)
            {
                this.timestamp = timestamp;
                this.entryId = entryId;
                this.userId = userId;
            }
        }
        
        /*
        HashSet<datas> addSet = new HashSet<datas>();
        HashSet<datas> removeSet = new HashSet<datas>();
        */
        //private Dictionary<int, Entry> addSet = new Dictionary<int, Entry>();
        
        List<Entry> enrtySet = new List<Entry>();
        List<Datas> db = new List<Datas>();
        
        List<Entry> history = new List<Entry>();
        List<Datas> historyDb = new List<Datas>();

        public void AddEntry(int entryId, int userId, string name, long timestamp)
        {
            history.Add(new Entry(entryId, name, EntryState.Undone));
            historyDb.Add(new Datas(entryId, userId, timestamp));

            if (HasUserAccess(userId))
            {
                var existedEntry = enrtySet.Find(x => x.Id == entryId);
                if (existedEntry != null)
                {
                }
                else
                {
                    enrtySet.Add(new Entry(entryId, name, EntryState.Undone));
                    Count++;
                    db.Add(new Datas(entryId, userId, timestamp));
                }

            }
        }

        public void RemoveEntry(int entryId, int userId, long timestamp)
        {
            throw new System.NotImplementedException();
        }

        public void MarkDone(int entryId, int userId, long timestamp)
        {
            var index = enrtySet.IndexOf(enrtySet.Where(entry => entry.Id == entryId).FirstOrDefault());
            enrtySet[index] = new Entry(entryId, enrtySet[index].Name, EntryState.Done);
            db[index].timestamp = timestamp;
            db[index].userId = userId;
        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            var index = enrtySet.IndexOf(enrtySet.Where(entry => entry.Id == entryId).FirstOrDefault());
            enrtySet[index] = new Entry(entryId, enrtySet[index].Name, EntryState.Undone);
            db[index].timestamp = timestamp;
            db[index].userId = userId;
            
        }

        public void DismissUser(int userId)
        {
            dismissedUsers.Add(userId);
            
            
        }

        public void AllowUser(int userId)
        {
            dismissedUsers.Remove(userId);
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            return enrtySet.Take(enrtySet.Count).GetEnumerator();

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public bool HasUserAccess(int userId)
        {
            return !dismissedUsers.Contains(userId);
        }

        public int Count { get; set; }
    }
}