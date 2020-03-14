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

            if (HasUserAccess(userId))
            {
                // проверим, есть ли элемент с таким айдишником
                if (IsElementWithIdExists(entryId)) 
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
                    // а тут нужно сделать запрос и проверить, был ли удален коммит с таким же айдишником
                    
                    history.TryGetValue(entryId, out var existedMarkDone);
                    if (existedMarkDone != null) // обрабатываем ситуацию, когда произошли какие-то изменения над списком, когда не был вызван
                    {
                        var state = existedMarkDone.LastOrDefault(x => x.operation == "done" || x.operation == "remove");
                        historyState = state.state;
                    }
            
                    AddToEntryList(entryId, userId, name, timestamp, historyState);
                    historyState = EntryState.Undone;
                }

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


        public bool IsElementWithIdExists(int entryId) //не должно быть больше 1
            => enrtySet.Where(x => x.Id == entryId).ToList().Count == 1; //мне кажется не оч


        public void RemoveEntry(int entryId, int userId, long timestamp) // TODO будет большая проблема, т.к. я восстаналиваю значения с remove 
        {
            // проверим, есть ли элемент с таким айдишником
            if (IsElementWithIdExists(entryId)) 
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
            if (IsElementWithIdExists(entryId)) 
            {                
                var index = IndexOfElement(entryId);
                var element = enrtySet[index];

                enrtySet[index] = element.MarkDone();
            }
            // а если нет, то порешаем потом при доабавлении этого элемента
            HistoryAdd("done", entryId, userId, timestamp, state: EntryState.Done);

        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            if (IsElementWithIdExists(entryId)) 
            {                
                var index = IndexOfElement(entryId);
                var element = enrtySet[index];

                enrtySet[index] = element.MarkUndone();
            }
            HistoryAdd("undone", entryId, userId, timestamp,  state: EntryState.Undone);

        }
        

        public void DismissUser(int userId)
        {
            foreach (var historyList in history)
            {
                for (int i = 0; i < historyList.Value.Count; i++)
                {
                    if (historyList.Value[i].userId == userId)
                    {
                        if (historyList.Value[i].operation == "add")
                        {
                            RemoveFromEntryList(i);
                        }
                        else if (historyList.Value[i].operation == "done" 
                                 || historyList.Value[i].operation == "undone" 
                                 || historyList.Value[i].operation == "rename")
                        {
                            for (int j = historyList.Value.Count-1; j >= 0 ; j--) // пробегаемся второй раз по всему списку изменений по этому айдишнику 
                            {
                                if (historyList.Value[j].userId != userId)
                                {
                                    var elem = historyList.Value[j];
                                    enrtySet[enrtySet.Count-1] = new Entry(historyList.Key, elem.name, elem.state); // TODO он не всегда будет последним скорее всего
                                    db[enrtySet.Count-1] = new Datas(historyList.Key, elem.userId, elem.timestamp);
                                    break; // TODO неверно априори 
                                }
                            }
                        }
                    }
             
                    
                }
            }
            dismissedUsers.Add(userId);
            
        }

        public void AllowUser(int userId)
        {
            dismissedUsers.Remove(userId);
            foreach (var historyList in history)
            {
                for (int i = 0; i < historyList.Value.Count; i++)
                {
                    if (historyList.Value[i].userId == userId)
                    {
                        if (historyList.Value[i].operation == "add")
                        {
                            var elem = historyList.Value[i];
                            AddToEntryList(historyList.Key, elem.userId, elem.name, elem.timestamp, elem.state);
                        }

                        for (int j = 0; j < historyList.Value.Count ; j++) // пробегаемся второй раз по всему списку изменений по этому айдишнику 
                        {
                            var elem = historyList.Value[j];
                            enrtySet[enrtySet.Count-1] = new Entry(historyList.Key, elem.name, elem.state); // TODO он не всегда будет последним скорее всего
                            db[enrtySet.Count-1] = new Datas(historyList.Key, elem.userId, elem.timestamp);
                        }
                    }
                    
                }
            }
        }

        
        public void HistoryAdd(string operation, int entryId, int userId, long timestamp, string name=" ", EntryState? state=null)//
        {
            if (name == " ")
            {
                history.TryGetValue(entryId, out var histValue);
                if (histValue != null)
                {
                    name = histValue.ToList().LastOrDefault().name;
                }
            }

            if (state is null)
            {
                history.TryGetValue(entryId, out var histValue);
                if (histValue != null)
                {
                    state = histValue.ToList().LastOrDefault().state;
                }
            }
            
                
            var currentAction = new History(operation, timestamp, userId, (EntryState)state, name);


            if (history.TryGetValue(entryId, out List<History> historyList))
            {
                bool isInserted = false;
                // теперь нам нужно инсертнуть в нужное место
                for (int i = 0; i < historyList.Count; i++)
                {
                    var historyTimestamp = historyList[i].timestamp;
                    if (historyTimestamp > timestamp)
                    {
                        isInserted = true;
                        InsertBefore(entryId, i, currentAction);
                        break;
                        
                    }
                }
                
                // теперь правим ремув, если возможно, т.е. если дальше по таймтемпам что-то произошло, но фактичски вызов был раньше, и в ремув не до записались параметры

                if (historyList.Where(x => x.operation == "remove").ToList().Count == 1)
                {
                    var fixRemove = history[entryId].LastOrDefault(x => x.operation == "remove");
                    fixRemove.state = (EntryState)state;
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