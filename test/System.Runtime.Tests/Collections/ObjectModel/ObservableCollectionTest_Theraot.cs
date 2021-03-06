﻿// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
// Copyright 2011 Xamarin Inc.
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Brian O'Keefe (zer0keefie@gmail.com)
//	Marek Safar (marek.safar@gmail.com)
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Tests.Collections.Specialized;

namespace Tests.Collections.ObjectModel
{
    [TestClass]
    public class ObservableCollectionTest
    {
        [TestMethod]
        public void ObservableCollection_Constructor()
        {
            var list = new List<int> { 3 };
            var col = new ObservableCollection<int>(list)
            {
                5
            };
            Assert.AreEqual(1, list.Count, "#1");

            col = new ObservableCollection<int>((IEnumerable<int>)list)
            {
                5
            };
            Assert.AreEqual(1, list.Count, "#2");
        }

        [TestMethod]
        public void ObservableCollection_Constructor_Invalid()
        {
            try
            {
                var x = new ObservableCollection<int>(null);
                GC.KeepAlive(x);
                Assert.Fail("#1");
            }
            catch (ArgumentNullException ex)
            {
                GC.KeepAlive(ex);
            }

            try
            {
                var x = new ObservableCollection<int>((IEnumerable<int>)null);
                GC.KeepAlive(x);
                Assert.Fail("#2");
            }
            catch (ArgumentNullException ex)
            {
                GC.KeepAlive(ex);
            }
        }

        [TestMethod]
        public void ObservableCollection_Insert()
        {
            var reached = false;
            var col = new ObservableCollection<int>();
            col.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                reached = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action, "INS_1");
                Assert.AreEqual(0, e.NewStartingIndex, "INS_2");
                Assert.AreEqual(-1, e.OldStartingIndex, "INS_3");
                Assert.AreEqual(1, e.NewItems.Count, "INS_4");
                Assert.AreEqual(5, (int)e.NewItems[0], "INS_5");
                Assert.AreEqual(null, e.OldItems, "INS_6");
            };
            col.Insert(0, 5);
            Assert.IsTrue(reached, "INS_5");
        }

        [TestMethod]
        public void ObservableCollection_RemoveAt()
        {
            var reached = false;
            var col = new ObservableCollection<int>();
            col.Insert(0, 5);
            col.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                reached = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action, "REMAT_1");
                Assert.AreEqual(-1, e.NewStartingIndex, "REMAT_2");
                Assert.AreEqual(0, e.OldStartingIndex, "REMAT_3");
                Assert.AreEqual(null, e.NewItems, "REMAT_4");
                Assert.AreEqual(1, e.OldItems.Count, "REMAT_5");
                Assert.AreEqual(5, (int)e.OldItems[0], "REMAT_6");
            };
            col.RemoveAt(0);
            Assert.IsTrue(reached, "REMAT_7");
        }

        [TestMethod]
        public void ObservableCollection_Move()
        {
            var reached = false;
            var col = new ObservableCollection<int>();
            col.Insert(0, 0);
            col.Insert(1, 1);
            col.Insert(2, 2);
            col.Insert(3, 3);
            col.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                reached = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Move, e.Action, "MOVE_1");
                Assert.AreEqual(3, e.NewStartingIndex, "MOVE_2");
                Assert.AreEqual(1, e.OldStartingIndex, "MOVE_3");
                Assert.AreEqual(1, e.NewItems.Count, "MOVE_4");
                Assert.AreEqual(1, e.NewItems[0], "MOVE_5");
                Assert.AreEqual(1, e.OldItems.Count, "MOVE_6");
                Assert.AreEqual(1, e.OldItems[0], "MOVE_7");
            };
            col.Move(1, 3);
            Assert.IsTrue(reached, "MOVE_8");
        }

        [TestMethod]
        public void ObservableCollection_Add()
        {
            var collection = new ObservableCollection<char>();
            var propertyChanged = false;
            var changedProps = new List<string>();
            NotifyCollectionChangedEventArgs args = null;

            ((INotifyPropertyChanged)collection).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                propertyChanged = true;
                changedProps.Add(e.PropertyName);
            };

            collection.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                args = e;
            };

            collection.Add('A');

            Assert.IsTrue(propertyChanged, "ADD_1");
            Assert.IsTrue(changedProps.Contains("Count"), "ADD_2");
            Assert.IsTrue(changedProps.Contains("Item[]"), "ADD_3");

            CollectionChangedEventValidators.ValidateAddOperation(args, new char[] { 'A' }, 0, "ADD_4");
        }

        [TestMethod]
        public void ObservableCollection_Remove()
        {
            var collection = new ObservableCollection<char>();
            var propertyChanged = false;
            var changedProps = new List<string>();
            NotifyCollectionChangedEventArgs args = null;

            collection.Add('A');
            collection.Add('B');
            collection.Add('C');

            ((INotifyPropertyChanged)collection).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                propertyChanged = true;
                changedProps.Add(e.PropertyName);
            };

            collection.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                args = e;
            };

            collection.Remove('B');

            Assert.IsTrue(propertyChanged, "REM_1");
            Assert.IsTrue(changedProps.Contains("Count"), "REM_2");
            Assert.IsTrue(changedProps.Contains("Item[]"), "REM_3");

            CollectionChangedEventValidators.ValidateRemoveOperation(args, new char[] { 'B' }, 1, "REM_4");
        }

        [TestMethod]
        public void ObservableCollection_Set()
        {
            var collection = new ObservableCollection<char>();
            var propertyChanged = false;
            var changedProps = new List<string>();
            NotifyCollectionChangedEventArgs args = null;

            collection.Add('A');
            collection.Add('B');
            collection.Add('C');

            ((INotifyPropertyChanged)collection).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                propertyChanged = true;
                changedProps.Add(e.PropertyName);
            };

            collection.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                args = e;
            };

            collection[2] = 'I';

            Assert.IsTrue(propertyChanged, "SET_1");
            Assert.IsTrue(changedProps.Contains("Item[]"), "SET_2");

            CollectionChangedEventValidators.ValidateReplaceOperation(args, new char[] { 'C' }, new char[] { 'I' }, 2, "SET_3");
        }

        [TestMethod]
        public void ObservableCollection_Reentrant()
        {
            var collection = new ObservableCollection<char>();
            var propertyChanged = false;
            var changedProps = new List<string>();
            NotifyCollectionChangedEventArgs args = null;

            collection.Add('A');
            collection.Add('B');
            collection.Add('C');

            PropertyChangedEventHandler pceh = (object sender, PropertyChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                propertyChanged = true;
                changedProps.Add(e.PropertyName);
            };

            // Adding a PropertyChanged event handler
            ((INotifyPropertyChanged)collection).PropertyChanged += pceh;

            collection.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                args = e;
            };

            collection.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                GC.KeepAlive(e);
                // This one will attempt to break reentrancy
                try
                {
                    collection.Add('X');
                    Assert.Fail("Reentrancy should not be allowed.");
                }
                catch (InvalidOperationException ex)
                {
                    GC.KeepAlive(ex);
                }
            };

            collection[2] = 'I';

            Assert.IsTrue(propertyChanged, "REENT_1");
            Assert.IsTrue(changedProps.Contains("Item[]"), "REENT_2");

            CollectionChangedEventValidators.ValidateReplaceOperation(args, new char[] { 'C' }, new char[] { 'I' }, 2, "REENT_3");

            // Removing the PropertyChanged event handler should work as well:
            ((INotifyPropertyChanged)collection).PropertyChanged -= pceh;
        }

        //Private test class for protected members of ObservableCollection
        private class ObservableCollectionTestHelper : ObservableCollection<char>
        {
            internal void DoubleEnterReentrant()
            {
                var object1 = BlockReentrancy();
                var object2 = BlockReentrancy();

                Assert.AreSame(object1, object2);

                //With double block, try the reentrant:
                NotifyCollectionChangedEventArgs args = null;

                CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
                {
                    GC.KeepAlive(sender);
                    args = e;
                };

                // We need a second callback for reentrancy to matter
                CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
                {
                    GC.KeepAlive(sender);
                    GC.KeepAlive(e);
                    // Doesn't need to do anything; just needs more than one callback registered.
                };

                // Try adding - this should cause reentrancy, and fail
                try
                {
                    Add('I');
                    Assert.Fail("Reentrancy should not be allowed. -- #2");
                }
                catch (InvalidOperationException ex)
                {
                    GC.KeepAlive(ex);
                }

                // Release the first reentrant
                object1.Dispose();

                // Try adding again - this should cause reentrancy, and fail again
                try
                {
                    Add('J');
                    Assert.Fail("Reentrancy should not be allowed. -- #3");
                }
                catch (InvalidOperationException ex)
                {
                    GC.KeepAlive(ex);
                }

                // Release the reentrant a second time
                object1.Dispose();

                // This last add should work fine.
                Add('K');
                CollectionChangedEventValidators.ValidateAddOperation(args, new char[] { 'K' }, 0, "REENTHELP_1");
            }
        }

        [TestMethod]
        public void ObservableCollection_ReentrantReuseObject()
        {
            var helper = new ObservableCollectionTestHelper();

            helper.DoubleEnterReentrant();
        }

        [TestMethod]
        public void ObservableCollection_Clear()
        {
            var initial = new List<char>
            {
                'A',
                'B',
                'C'
            };

            var collection = new ObservableCollection<char>(initial);
            var propertyChanged = false;
            var changedProps = new List<string>();
            NotifyCollectionChangedEventArgs args = null;

            ((INotifyPropertyChanged)collection).PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                propertyChanged = true;
                changedProps.Add(e.PropertyName);
            };

            collection.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                GC.KeepAlive(sender);
                args = e;
            };

            collection.Clear();

            Assert.IsTrue(propertyChanged, "CLEAR_1");
            Assert.IsTrue(changedProps.Contains("Count"), "CLEAR_2");
            Assert.IsTrue(changedProps.Contains("Item[]"), "CLEAR_3");

            CollectionChangedEventValidators.ValidateResetOperation(args, "CLEAR_4");
        }
    }
}
