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
            public string operation;
            public long timestamp;
            public int userId;
            public EntryState state;
            public string name;
            public int authorId;

            public History(string operation, long timestamp, int userId, EntryState state, string name)
            {
                this.operation = operation;
                this.timestamp = timestamp;
                this.userId = userId;
                this.state = state;
                this.name = name;
            }
        }
        
        List<Entry> enrtySet = new List<Entry>();
        List<Datas> db = new List<Datas>();

        private Dictionary<int, List<History>> history = new Dictionary<int, List<History>>();
        
        

        public void AddEntry(int entryId, int userId, string name, long timestamp)
        {
            var historyState = EntryState.Undone;

            // проверим, есть ли элемент с таким айдишником
            if (enrtySet.Where(x => x.Id == entryId).ToList().Count == 1) //не должно быть больше 1
            {
                var index = IndexOfElement(entryId);
                var element = enrtySet[index];
                var metaelement = db[index];
                if (metaelement.timestamp == timestamp) // а с таким же таймстемпом
                {
                    var el = history[entryId].Where(x => x.timestamp == timestamp).ToList();
                    //го пока проанализируем первый, если не зайдет, будем думать
                    var historyElement = el[0];

                    if (historyElement.operation == "rename" || historyElement.operation == "add")
                    {
                        if (userId < metaelement.userId)
                        {
                            enrtySet[index] = new Entry(element.Id, name, element.State);
                        }
                    }
                }
                else if (metaelement.timestamp < timestamp) // а теперь посмотрим, может были изменения раньше по времени
                {
                    enrtySet[index] = new Entry(element.Id, name, element.State);

                }
                
            }
            else 
            {
                history.TryGetValue(entryId, out var existedMarkDone);
                if (existedMarkDone != null) // обрабатываем ситуацию, когда произошли какие-то изменения над списком, когда не был вызван
                {
                    var state = existedMarkDone.LastOrDefault(x => x.operation == "done");
                    historyState = EntryState.Done;
                }
        
                AddToEntryList(entryId, userId, name, timestamp, historyState);
            }

            
            if (history.ContainsKey(entryId))
            {
                HistoryAdd("rename", entryId, userId, timestamp, name, historyState);
            }
            else
            {
                HistoryAdd("add", entryId, userId, timestamp, name, historyState);
            }

        }



        public void RemoveEntry(int entryId, int userId, long timestamp)
        {
            
            // проверим, есть ли элемент с таким айдишником
            if (enrtySet.Where(x => x.Id == entryId).ToList().Count == 1) //не должно быть больше 1
            {                
                var index = IndexOfElement(entryId);
                var element = enrtySet[index];
                var metaelement = db[index];
                if (metaelement.timestamp <= timestamp) // а с таким же таймстемпом
                {
                    RemoveFromEntryList(index); // преимущество удаления
                }
            }

            HistoryAdd("remove", entryId, userId, timestamp);
        }

        

        public void MarkDone(int entryId, int userId, long timestamp)
        {            
            // проверим, есть ли элемент с таким айдишником
            if (enrtySet.Where(x => x.Id == entryId).ToList().Count == 1) //не должно быть больше 1
            {                
                var index = IndexOfElement(entryId);
                var element = enrtySet[index];
                var metaelement = db[index];
                if (metaelement.timestamp <= timestamp) // а с таким же таймстемпом
                {
                    enrtySet[index] = element.MarkDone();
                }
                else //скорее всего неверно
                {
                    enrtySet[index] = element.MarkDone();

                }

            }
            // а если нет, то порешаем потом при доабавлении этого жлемента
            HistoryAdd("done", entryId, userId, timestamp);

        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {

            HistoryAdd("undone", entryId, userId, timestamp);

        }
        

        public void DismissUser(int userId)
        {
            dismissedUsers.Add(userId);
            
        }

        public void AllowUser(int userId)
        {
            dismissedUsers.Remove(userId);

        }

        public bool IsThisInHistory(int id)
        {
            return history.ContainsKey(id);
        }
        
        public void HistoryAdd(string operation, int entryId, int userId, long timestamp, string name=" ", EntryState state=EntryState.Undone)
        {
            
            var currentAction = new History(operation, timestamp, userId, state, name);
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


        public IEnumerator<Entry> GetEnumerator()
            =>  enrtySet.Take(enrtySet.Count).GetEnumerator();

        

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public bool HasUserAccess(int userId)
        {
            return !dismissedUsers.Contains(userId);
        }

        public int Count { get; set; }
        
        public int IndexOfElement(int entryId)
            => enrtySet.IndexOf(enrtySet.Where(entry => entry.Id == entryId).FirstOrDefault());
        
        public void AddToEntryList(int entryId, int userId, string name, long timestamp, EntryState state)
        {
            enrtySet.Add(new Entry(entryId, name, state));
            Count++;
            db.Add(new Datas(entryId, userId, timestamp));
        }

        public void RemoveFromEntryList(int entryId)
        {
            enrtySet.RemoveAt(entryId);
            db.RemoveAt(entryId);
            Count--;
        }

        
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