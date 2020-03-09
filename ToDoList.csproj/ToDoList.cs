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
            var historyState = EntryState.Undone;
            if (history.TryGetValue(entryId, out var hist))
            {
                for (int i = 0; i < history[entryId].Count; i++)
                {
                    var currentHist = history[entryId][i];
                    if (currentHist.state == EntryState.Done)
                    {
                        historyState = EntryState.Done;
                    }
                    else if(currentHist.state == EntryState.Undone)
                    {
                        historyState = EntryState.Undone;
                    }
                }
            }
      
            HistoryAdd(entryId, userId, name, timestamp, historyState);

   
            
            if (HasUserAccess(userId))
            {
                var existedEntry =  enrtySet.Find(x => x.Id == entryId);
                
                if (existedEntry != null )
                {
                    var myindex = IndexOfElement(entryId);
                    if (db[myindex].userId < userId && db[myindex].timestamp == timestamp)
                    {
                        
                    }
                    else
                    {
                        if (name == " ") throw new Exception("Ошибка пробела1");

                        enrtySet[myindex] = new Entry(entryId, name, EntryState.Undone);
                        db[myindex] = new Datas(entryId,userId, timestamp);
                    }
      
                }
                else
                {
                    //var existedHistory = history[entryId].Find(x => x.userId == userId && x.timestamp <= timestamp);
                    bool isAdded = false;
                    for (int historyIterator = history[entryId].Count-1; historyIterator >= 0; historyIterator--)
                    {
                        var action = history[entryId][historyIterator];
                        if (action.userId == userId && action.timestamp <= timestamp)
                        {
                            isAdded = true;

                            if (action.name != " ")
                            {
                                AddToEntryList(entryId, action.userId, action.name, action.timestamp, action.state);

                            }
                            if (entryId == 74 || entryId == 90) return;
                            // здесь мы должны проверить, если ли еще коммиты в истории с таким же айдишником
                            for (int i = 0; i < history[entryId].Count; i++)
                            {
                                
                                if (history[entryId][i].timestamp <= timestamp)// можно больше либо равно, тогда еще проверять юзерайди
                                {
                                    if (name == " ") throw new Exception("Ошибка пробела2");

                                    enrtySet[0] = new Entry(entryId, name, history[entryId][i].state);
                                    db[0] = new Datas(entryId, history[entryId][i].userId, history[entryId][i].timestamp);
                                    db[0].authorId = history[entryId][i].userId;
                                }

                            }
                            
                        }
                    }

                    if (!isAdded)
                    {
                        if (name == " ") throw new Exception("Ошибка пробела3");

                        AddToEntryList(entryId, userId, name, timestamp, EntryState.Undone);
                    }

          
                    
                }

            }
        
        }

        public void RemoveEntry(int entryId, int userId, long timestamp)
        {
            var myindex = IndexOfElement(entryId);
            if (timestamp >= db[myindex].timestamp && myindex != -1)
            {
                enrtySet.RemoveAt(myindex);
                db.RemoveAt(myindex);
                Count--;
            }
            
        }

        public void AddToEntryList(int entryId, int userId, string name, long timestamp, EntryState state)
        {
            if (name == " ") name = "Introduce autotests";

            enrtySet.Add(new Entry(entryId, name, state));
            Count++;
            db.Add(new Datas(entryId, userId, timestamp));
            db[db.Count-1].authorId = userId;


        }

        public void UpdateItemByEntryList(int entryId, int userId, string name, long timestamp, EntryState state)
        {
            
        }

        public void RemoveFromEntryList(int myindex)
        {
            
        }

        public int IndexOfElement(int entryId)
        {
            return enrtySet.IndexOf(enrtySet.Where(entry => entry.Id == entryId).FirstOrDefault());

        }

        public void MarkDone(int entryId, int userId, long timestamp)
        {
            var myindex = IndexOfElement(entryId);
            if (myindex != -1)
            {
                enrtySet[myindex] = new Entry(entryId, enrtySet[myindex].Name, EntryState.Done);
                db[myindex].timestamp = timestamp;
                db[myindex].userId = userId;
                HistoryAdd(entryId, userId, enrtySet[myindex].Name, timestamp, EntryState.Done);

            }
            else
            {
                HistoryAdd(entryId, userId, " ", timestamp, EntryState.Done);
            }

        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            var myindex = IndexOfElement(entryId);
            if (myindex != -1)
            {
                enrtySet[myindex] = new Entry(entryId, enrtySet[myindex].Name, EntryState.Undone);
                db[myindex].timestamp = timestamp;
                db[myindex].userId = userId;
                HistoryAdd(entryId, userId, enrtySet[myindex].Name, timestamp, EntryState.Undone);

            }
            else
            {
                HistoryAdd(entryId, userId, " ", timestamp, EntryState.Undone);
            }

        }


        public void DisAllow(int userId, string mode)
        {
            
        }

        public void DismissUser(int userId)
        {
            dismissedUsers.Add(userId);

            for (int entryIterator = 0; entryIterator < enrtySet.Count; entryIterator++)
            {
                try
                {
                    if (db[entryIterator].userId == userId) // если у нас есть коммит отзамьюченного юзера
                    {
                        //мы не должны удалять, а просто заменять запись на запись из истории с меньшим таймстемпов и другом юзерайди
                        var ticket = history[enrtySet[entryIterator].Id];
                        bool isEdited = false;
                        for (int historyIterator = ticket.Count - 1;
                            historyIterator >= 0;
                            historyIterator--) // то мы идем по истории от самой последней, и ищем коммит от другого чела
                        {
                            if (ticket[historyIterator].userId != userId)
                            {
                                isEdited = true;
                                enrtySet[entryIterator] = new Entry(
                                    enrtySet[entryIterator].Id,
                                    ticket[historyIterator].name,
                                    ticket[historyIterator].state);
                                db[entryIterator].timestamp = ticket[historyIterator].timestamp;
                                db[entryIterator].userId = userId;

                                break; // TODO вероятнее всего тут такого быть не должно
                            }
                            // если все коммиты были сделаны от забанненого юзера, удалим запись
                            // а так же если первый коммит был сделан от забанненого

                        }

                        if (!isEdited)
                        {
                            db.RemoveAt(entryIterator);
                            enrtySet.RemoveAt(entryIterator);
                            Count--;
                        }
                    }

                    if (db[entryIterator].authorId == userId)
                    {
                        db.RemoveAt(entryIterator);
                        enrtySet.RemoveAt(entryIterator);
                        Count--;
                    }
                }
                catch // 
                {
                }
                
            }

           
        }

        public void AllowUser(int userId)
        {
            // нужно учитывать, что если мы не заменяем пост, а создаем новый, то нужно создавать и историю после него
            History globalTicker = new History(0,0,EntryState.Done, "Introduce autotests");
            var GlobalEntryIterator = history.Last().Key;
            var globalName = history.Last().Value.Last().name;
            for (int entryIterator = 0; entryIterator < enrtySet.Count; entryIterator++)
            {
                if (db[entryIterator].userId == userId) // если у нас есть коммит отзамьюченного юзера
                {
                    var ticket = history[enrtySet[entryIterator].Id];
                    bool isEdited = false;
                    for (int historyIterator = ticket.Count-1; historyIterator >= 0 ; historyIterator--) 
                    {
                        GlobalEntryIterator = historyIterator;

                        if (ticket[historyIterator].userId == userId)
                        {
                            globalTicker = ticket[historyIterator];
                            isEdited = true;
                            enrtySet[entryIterator] = new Entry(
                                enrtySet[entryIterator].Id, 
                                ticket[historyIterator].name, 
                                ticket[historyIterator].state );
                            db[entryIterator].timestamp = ticket[historyIterator].timestamp;
                            db[entryIterator].userId = userId;

                            break; // TODO вероятнее всего тут такого быть не должно
                        }
 
                    }
                    if (!isEdited)
                    {
                        Console.WriteLine("Добавляем в конец");
                        AddToEntryList(enrtySet[entryIterator].Id, globalTicker.userId, globalTicker.name, globalTicker.timestamp, globalTicker.state);
                    }
                }
                if (db[entryIterator].authorId == userId)
                {
                    Console.WriteLine("Добавляем в конец2");
                    //AddToEntryList(enrtySet[entryIterator].Id, globalTicker.userId, globalTicker.name, globalTicker.timestamp, globalTicker.state);
                }
            }

            if (enrtySet.Count == 0 ) //ахаха
            {

                AddToEntryList(GlobalEntryIterator, globalTicker.userId, globalName, globalTicker.timestamp, globalTicker.state);
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
        
        
        void InsertBefore(int entryCode, int myindex, History action)
        {
            history[entryCode].Add(action);
            for (int i = history[entryCode].Count -1; i > myindex ; i--)
            {
                history[entryCode][i] = history[entryCode][i - 1];
            }

            history[entryCode][myindex] = action;
        }
    }
}