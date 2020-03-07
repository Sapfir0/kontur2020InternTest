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
        
        public class History
        {
            public long timestamp;
            public int userId;
            public EntryState state;
            public string name;

            public History(long timestamp, int userId, EntryState state, string name)
            {
                this.timestamp = timestamp;
                this.userId = userId;
                this.state = state;
                this.name = name;
            }
        }
        
        List<Entry> enrtySet = new List<Entry>();
        List<Datas> db = new List<Datas>();

        private Dictionary<int, LinkedList<History>> history = new Dictionary<int, LinkedList<History>>();
        
        
        public void HistoryAdd(int entryId, int userId, string name, long timestamp, EntryState state)
        {
            
            var currentAction = new History(timestamp, userId, state, name);
            if (history.TryGetValue(entryId, out LinkedList<History> historyList))
            {
                // теперь нам нужно инсертнуть в нужное место
                foreach (var action in history[entryId])
                {
                    if (action.timestamp < timestamp)
                    {
                        continue;
                    }
                    else
                    {
                        history[entryId].AddBefore(new LinkedListNode<History>(action), new LinkedListNode<History>(currentAction));
                    }
                }
            }
            else
            {
                history.Add(entryId, new LinkedList<History>());
                history[entryId].AddLast(currentAction);
            }

    
            
        }
        

        public void AddEntry(int entryId, int userId, string name, long timestamp)
        {
            HistoryAdd(entryId, userId, name, timestamp, EntryState.Undone);

            if (HasUserAccess(userId))
            {
                var existedEntry = enrtySet.Find(x => x.Id == entryId);
                
                if (existedEntry != null)
                {
                    var index = IndexOfElement(entryId);
                    enrtySet[index] = new Entry(entryId, name, EntryState.Undone);
                }
                else
                {
                    AddToEntryList(entryId, userId, name, timestamp, EntryState.Undone);
                }

            }
        }

        public void RemoveEntry(int entryId, int userId, long timestamp)
        {
            var index = IndexOfElement(entryId);
            if (timestamp > db[index].timestamp)
            {
                enrtySet.RemoveAt(index);
                db.RemoveAt(index);
            }
            
        }

        public void AddToEntryList(int entryId, int userId, string name, long timestamp, EntryState state)
        {
            enrtySet.Add(new Entry(entryId, name, state));
            Count++;
            db.Add(new Datas(entryId, userId, timestamp));

        }

        public void UpdateItemByEntryList(int entryId, int userId, string name, long timestamp, EntryState state)
        {
            
        }

        public void RemoveFromEntryList(int index)
        {
            
        }

        public int IndexOfElement(int entryId)
        {
            return enrtySet.IndexOf(enrtySet.Where(entry => entry.Id == entryId).FirstOrDefault());

        }

        public void MarkDone(int entryId, int userId, long timestamp)
        {
            var index = IndexOfElement(entryId);
            enrtySet[index] = new Entry(entryId, enrtySet[index].Name, EntryState.Done);
            db[index].timestamp = timestamp;
            db[index].userId = userId;
        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            var index = IndexOfElement(entryId);
            enrtySet[index] = new Entry(entryId, enrtySet[index].Name, EntryState.Undone);
            db[index].timestamp = timestamp;
            db[index].userId = userId;
            
        }

        public void DismissUser(int userId)
        {
            dismissedUsers.Add(userId);

            for (int i = 0; i < enrtySet.Count; i++)
            {
                if (db[i].userId == userId)
                {
                    db.RemoveAt(i);
                    enrtySet.RemoveAt(i);
                }
            }
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