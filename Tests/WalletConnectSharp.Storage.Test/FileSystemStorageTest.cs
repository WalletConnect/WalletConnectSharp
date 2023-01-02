using System;
using System.Collections.Generic;
using System.IO;
using WalletConnectSharp.Tests.Common;
using Xunit;

namespace WalletConnectSharp.Storage.Test
{
    public class FileSystemStorageTest
    {
        [Fact]
        public async void GetSetRemoveTest()
        {
            using (var tempFolder = new TempFolder())
            {
                var testDictStorage = new FileSystemStorage(Path.Combine(tempFolder.Folder.FullName, ".wctestdata"));
                await testDictStorage.Init();
                await testDictStorage.SetItem("somekey", "somevalue");
                Assert.Equal("somevalue", await testDictStorage.GetItem<string>("somekey"));
                await testDictStorage.RemoveItem("somekey");
                await Assert.ThrowsAsync<KeyNotFoundException>(() => testDictStorage.GetItem<string>("somekey"));
            }
        }

        [Fact]
        public async void GetKeysTest()
        {
            using (var tempFolder = new TempFolder())
            {
                var testDictStorage = new FileSystemStorage(Path.Combine(tempFolder.Folder.FullName, ".wctestdata"));
                await testDictStorage.Init();
                await testDictStorage.Clear(); //Clear any persistant state
                await testDictStorage.SetItem("addkey", "testingvalue");
                Assert.Equal(new string[] { "addkey" }, await testDictStorage.GetKeys());
            }
        }

        [Fact]
        public async void GetEntriesTests()
        {
            using (var tempFolder = new TempFolder())
            {
                var testDictStorage = new FileSystemStorage(Path.Combine(tempFolder.Folder.FullName, ".wctestdata"));
                await testDictStorage.Init();
                await testDictStorage.Clear();
                await testDictStorage.SetItem("addkey", "testingvalue");
                Assert.Equal(new object[] { "testingvalue" }, await testDictStorage.GetEntries());
                await testDictStorage.SetItem("newkey", 5);
                Assert.Equal(new int[] { 5 }, await testDictStorage.GetEntriesOfType<int>());
            }
        }

        [Fact]
        public async void HasItemTest()
        {
            using (var tempFolder = new TempFolder())
            {
                var testDictStorage = new FileSystemStorage(Path.Combine(tempFolder.Folder.FullName, ".wctestdata"));
                await testDictStorage.Init();
                await testDictStorage.SetItem("checkedkey", "testingvalue");
                Assert.True(await testDictStorage.HasItem("checkedkey"));
            }
        }
    }
}