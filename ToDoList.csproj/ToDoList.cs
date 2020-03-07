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
            public int authorId;
            
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

        private Dictionary<int, List<History>> history = new Dictionary<int, List<History>>();
        
        
        public void HistoryAdd(int entryId, int userId, string name, long timestamp, EntryState state)
        {
            
            var currentAction = new History(timestamp, userId, state, name);
            if (history.TryGetValue(entryId, out List<History> historyList))
            {
                bool isInserted = false;
                // теперь нам нужно инсертнуть в нужное место
                for (int i = 0; i < history[entryId].Count; i++)
                {
                    var historyTimestamp = history[entryId][i].timestamp;
                    if (historyTimestamp > timestamp)
                    {
                        isInserted = true;
                        InsertBefore(entryId, i, currentAction);
                        break;
                    }
                }

                if (!isInserted)
                {
                    history[entryId].Add(currentAction);
                }
            }
            else
            {
                history.Add(entryId, new List<History>());
                history[entryId].Add(currentAction);
            }

    
            
        }
        

        public void AddEntry(int entryId, int userId, string name, long timestamp)
        {
            HistoryAdd(entryId, userId, name, timestamp, EntryState.Undone);

            if (HasUserAccess(userId))
            {
                var existedEntry =  enrtySet.Find(x => x.Id == entryId);
                
                if (existedEntry != null)
                {
                    var index = IndexOfElement(entryId);
                    enrtySet[index] = new Entry(entryId, name, EntryState.Undone);
                    db[index] = new Datas(entryId,userId, timestamp);
                    
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
            if (index != -1)
            {
                enrtySet[index] = new Entry(entryId, enrtySet[index].Name, EntryState.Done);
                db[index].timestamp = timestamp;
                db[index].userId = userId;
                HistoryAdd(entryId, userId, enrtySet[index].Name, timestamp, EntryState.Done);

            }
            else
            {
                HistoryAdd(entryId, userId, " ", timestamp, EntryState.Done);
            }

        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            var index = IndexOfElement(entryId);
            if (index != -1)
            {
                enrtySet[index] = new Entry(entryId, enrtySet[index].Name, EntryState.Undone);
                db[index].timestamp = timestamp;
                db[index].userId = userId;
                HistoryAdd(entryId, userId, enrtySet[index].Name, timestamp, EntryState.Undone);

            }
            else
            {
                HistoryAdd(entryId, userId, " ", timestamp, EntryState.Undone);
            }

        }

        public void DismissUser(int userId)
        {
            dismissedUsers.Add(userId);
            for (int entryIterator = 0; entryIterator < enrtySet.Count; entryIterator++)
            {
                if (db[entryIterator].userId == userId) // если у нас есть коммит отзамьюченного юзера
                {
                    //мы не должны удалять, а просто заменять запись на запись из истории с меньшим таймстемпов и другом юзерайди
                    var ticket = history[enrtySet[entryIterator].Id];
                    bool isEdited = false;
                    for (int historyIterator = ticket.Count-1; historyIterator >= 0 ; historyIterator--) // то мы идем по истории от самой последней, и ищем коммит от другого чела
                    {
                        if (ticket[historyIterator].userId != userId)
                        {
                            isEdited = true;
                            enrtySet[entryIterator] = new Entry(
                                enrtySet[entryIterator].Id, 
                                ticket[historyIterator].name, 
                                ticket[historyIterator].state );
                            db[entryIterator].timestamp = ticket[historyIterator].timestamp;
                            db[entryIterator].userId = userId;

                            break; // TODO вероятнее всего тут такого быть не должно
                        }
                        else // если все коммиты были сделаны от забанненого юзера, удалим запись
                        {
                            //entryIteratorGlobal = entryIterator;

                        }
                    }
                    if (!isEdited)
                    {
                        db.RemoveAt(entryIterator);
                        enrtySet.RemoveAt(entryIterator);
                        Count--; 
                    }

                }
            }
        }

        public void AllowUser(int userId)
        {

            foreach (var hist in history)
            {
                foreach (var action in hist.Value)
                {
                    if (dismissedUsers.Contains(userId))
                    {
                        AddToEntryList(hist.Key, action.userId, action.name, action.timestamp, action.state);
                    }
                }

            }
            
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
        
        
        void InsertBefore(int entryCode, int index, History action)
        {
            history[entryCode].Add(action);
            for (int i = history[entryCode].Count -1; i > index ; i--)
            {
                history[entryCode][i] = history[entryCode][i - 1];
            }

            history[entryCode][index] = action;
        }
    }
}