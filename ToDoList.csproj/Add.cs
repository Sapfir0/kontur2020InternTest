using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ToDoList
{
    public class Add
    {
        private const int userA = 10;
        private const int userB = 11;
        private const int userC = 14;

        private IToDoList list;

        [SetUp]
        public void SetUp()
        {
            list = new ToDoList();
        }

        [Test]
        public void SimpleAdd()
        {
            list.AddEntry(42, userA, "iv", 100);
            
            AssertEntries(Entry.Undone(42, "iv"));
        }
        
        [Test]
        public void TimestampIsExistsUserIdLower()
        {
            list.AddEntry(42, userB, "iv", 100);
            list.AddEntry(42, userA, "sing", 100);

            AssertEntries(Entry.Undone(42, "sing"));
        }
        
        [Test]
        public void TimestampIsExistsUserIdBigger()
        {
            list.AddEntry(42, userA, "iv", 100);
            list.AddEntry(42, userC, "sing", 100);

            AssertEntries(Entry.Undone(42, "iv"));
        }
        
        [Test]
        public void TimestampIsExistsRemove()
        {
            list.AddEntry(42, userA, "iv", 100);
            list.RemoveEntry(42, userB, 100);

            AssertListEmpty();
        }
        
        [Test]
        public void TimestampIsExistsMarkDone()
        {
            list.AddEntry(42, userA, "iv", 100);
            list.MarkDone(42, userB, 100);

            AssertEntries(Entry.Done(42, "iv"));
        }
        
        [Test]
        public void TimestampIsExistsMarkDoneBefore()
        {
            list.MarkDone(42, userB, 100);
            list.AddEntry(42, userA, "iv", 100);

            AssertEntries(Entry.Done(42, "iv"));
        }

        
        [Test]
        public void Mark_Done_After_Removal(
            [Values(userA, userB, userC)] int removingUserId,
            [Values(99, 101, 100)] long removingTimestamp)
        {
            list.AddEntry(10, removingUserId, "MyCase", 99);
            list.RemoveEntry(10, removingUserId, removingTimestamp);
            list.MarkDone(10, removingUserId, 99);

            list.AddEntry(10, removingUserId, "MyCase", 104);
            
            AssertEntries(Entry.Done(10, "MyCase"));
        } 
        
        
        [Test]
        public void Mark_Done_Before_Removal(
            [Values(userA, userB, userC)] int removingUserId,
            [Values(99, 101, 100)] long removingTimestamp)
        {
            list.MarkDone(10, removingUserId, 99);
            list.AddEntry(10, removingUserId, "MyCase", 99);
            list.RemoveEntry(10, removingUserId, removingTimestamp);

            list.AddEntry(10, removingUserId, "MyCase", 104);
            
            AssertEntries(Entry.Done(10, "MyCase"));
        } 
        
        [Test]
        public void Mark_Undone_After_Removal()
        {
            list.AddEntry(10, userA, "MyCase", 100);
            list.MarkDone(10, userA, 101);
            list.MarkUndone(10, userA, 102);
            list.RemoveEntry(10, userA, 102);
            list.AddEntry(10, userA, "MyCase", 104);
            
            AssertEntries(Entry.Undone(10, "MyCase"));
        }
        
        
        [Test]
        public void  Not_Add_Item_When_It_Was_Removed_With_Greater_Timestamp(
            [Values(userA, userB, userC)] int removingUserId,
            [Values(200, 100, 203)] long removingTimestamp)
        {
            list.AddEntry(10, userA, "MyCase", 100);
            list.RemoveEntry(10, userA, 400);
            list.AddEntry(10, userA, "MyCase", 104);
            
            AssertListEmpty();
        }
        
        [Test]
        public void MarkDoneAfterRemove()
        {
            list.AddEntry(10, userA, "MyCase", 100);
            list.MarkDone(10, userA, 101);
            list.RemoveEntry(10, userA, 102);
            list.AddEntry(10, userA, "MyCase", 104);


            AssertEntries(Entry.Done(10, "MyCase"));
        }
        
        [Test]
        public void DissmissBeforeCreation()
        {
            list.DismissUser(userA);
            list.AddEntry(10, userA, "MyCase", 100);

            AssertListEmpty();
        }
        
        [Test]
        public void DissmissThanAllowCreation()
        {
            list.DismissUser(userA);
            list.AddEntry(10, userA, "MyCase", 100);
            list.AllowUser(userA);

            AssertEntries(Entry.Undone(10, "MyCase"));
        }
        
        [Test]
        public void MyCase()
        {
            list.AddEntry(42, userA, "Introduce autotests0", 100);
            list.AddEntry(42, userA, "Introduce autotests2", 120);
            list.AddEntry(42, userA, "Introduce autotests3", 50);
            list.AddEntry(42, userA, "Introduce autotests4", 50);
            list.AddEntry(42, userA, "Introduce autotests5", 500);
            list.AddEntry(42, userA, "Introduce autotests6", 110);
            // мне лень писать ассерт
        }
        
        [Test]
        public void Not_Mark_Undone_When_Timestamp_Less_Than_Done_Mark_Timestamp()
        {
            list.AddEntry(42, userA, "iv", 100);
            list.MarkDone(42, userB, 102);
            list.MarkUndone(42, userB, 101);

            AssertEntries(Entry.Done(42, "iv"));
        }

        
        private void AssertListEmpty()
        {
            list.Should().BeEmpty();
            list.Count.Should().Be(0);
        }

        private void AssertEntries(params Entry[] expected)
        {
            list.Should().BeEquivalentTo(expected.AsEnumerable());
            list.Count.Should().Be(expected.Length);
        }
        
        
        
    }
}