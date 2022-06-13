using JobsityNetChallenge.Domain;
using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Storage
{
    public interface IChatStorage
    {
        User FetchUser(string id);
        List<Message> LoadAllMessages();
        List<Message> LoadLastMessages(int size);
        Message SaveMessage(Message data);
        User SaveUser(User data);
    }

    public class ChatStorage : IChatStorage
    {
        private const string DB_TABLE_USERS = "db_users";
        private const string DB_TABLE_MESSAGES = "db_messages";

        private LiteDatabase _liteDb;

        public ChatStorage(IConfiguration configuration)
        {
            _liteDb = new LiteDatabase(configuration["DataBaseConfig"]);
        }

        #region Messages
        public List<Message> LoadAllMessages()
        {
            List<Message> storedValue = null;
            var simpleStorage = _liteDb.GetCollection<Message>(DB_TABLE_MESSAGES);
            storedValue = simpleStorage.FindAll().ToList();
            return storedValue;
        }
        public List<Message> LoadLastMessages(int size)
        {
            List<Message> storedValue = null;
            var simpleStorage = _liteDb.GetCollection<Message>(DB_TABLE_MESSAGES);
            storedValue = simpleStorage.FindAll().OrderByDescending(x=>x.Id).Take(size).ToList();
            return storedValue;
        }

        public Message FetchMessage(string id)
        {
            Message storedValue = null;
            var simpleStorage = _liteDb.GetCollection<Message>(DB_TABLE_MESSAGES);
            var dbResult = simpleStorage.FindById(id);
            if (dbResult != null)
            {
                storedValue = dbResult;
            }
            return storedValue;
        }

        public Message SaveMessage(Message data)
        {
            Message storedValue = null;
            // Get customer collection
            var simpleStorage = _liteDb.GetCollection<Message>(DB_TABLE_MESSAGES);
            simpleStorage.Upsert(data);
            simpleStorage.EnsureIndex(x => x.Id);
            return storedValue;
        }
        #endregion

        #region User
        public List<User> LoadAllUsers()
        {
            List<User> storedValue = null;
            var simpleStorage = _liteDb.GetCollection<User>(DB_TABLE_USERS);
            storedValue = simpleStorage.FindAll().ToList();
            return storedValue;
        }

        public User FetchUser(string id)
        {
            User storedValue = null;
            var simpleStorage = _liteDb.GetCollection<User>(DB_TABLE_USERS);
            var dbResult = simpleStorage.FindById(id);
            if (dbResult != null)
            {
                storedValue = dbResult;
            }
            return storedValue;
        }

        public User SaveUser(User data)
        {
            User storedValue = null;
            // Get customer collection
            var simpleStorage = _liteDb.GetCollection<User>(DB_TABLE_USERS);
            simpleStorage.Upsert(data);
            simpleStorage.EnsureIndex(x => x.Id);
            return storedValue;
        }
        #endregion

    }
}
