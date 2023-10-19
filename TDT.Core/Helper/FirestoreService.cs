﻿using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TDT.Core.DTO.Firestore;

namespace TDT.Core.Helper
{
    public class FirestoreService
    {
        private static readonly string CONFIG_PATH = "/Config/cross-platform-music-firebase-adminsdk-6e112-689a7c7543.json";
        private FirestoreDb db;
        private string PATH_CONFIG = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.FullName + "\\TDT.Core\\" + CONFIG_PATH;


        private static FirestoreService _instance;
        public static FirestoreService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FirestoreService();
                }
                return _instance;
            }
        }
        private FirestoreService()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", PATH_CONFIG);
            db = FirestoreDb.Create("cross-platform-music");
            Console.WriteLine("**Init Firestore**");
        }
        public CollectionReference GetCollectionReference(string collectionName)
        {
            return db.Collection(collectionName);
        }
        public DocumentReference GetDocumentReference(string collectionName, string id)
        {
            return db.Collection(collectionName).Document(id);
        }
        public Query OrderBy(string collectionName, string fieldPath)
        {
            return db.Collection(collectionName).OrderBy(fieldPath);
        }
        public Query OrderByDescending(string collectionName, string fieldPath)
        {
            return db.Collection(collectionName).OrderByDescending(fieldPath);
        }
        public Query Where(string collectionName, Filter filter)
        {
            return db.Collection(collectionName).Where(filter);
        }
        
        
        public List<T> Gets<T>(Query query) where T : class, new()
        {
            QuerySnapshot querySnapshot = query.GetSnapshotAsync().Result;
            return Gets<T>(querySnapshot);
        }
        public List<T> Gets<T>(string collectionName) where T : class, new()
        {
            QuerySnapshot querySnapshot = GetCollectionReference(collectionName).GetSnapshotAsync().Result;
            return Gets<T>(querySnapshot);
        }
        public List<T> Gets<T>(QuerySnapshot querySnapshot) where T : class, new()
        {
            List<T> res = new List<T>();
            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                var item = ConvertService.Instance.convertToObjectFromDictionary<T>(documentSnapshot.ToDictionary());
                if (item != null)
                    res.Add(item);
            }
            return res;
        }
        public T Gets<T>(string collectionName, string id) where T : class, new()
        {
            var dic = GetDocumentReference(db.Collection(collectionName).Document(id)).Result;
            return ConvertService.Instance.convertToObjectFromDictionary<T>(dic);
        }
        public async Task<IDictionary<string, object>> Gets(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            return await GetDocumentReference(db.Document(path));
        }
        public async Task<IDictionary<string, object>> Gets(string collectionName, string id)
        {
            return await GetDocumentReference(db.Collection(collectionName).Document(id));
        }
        public async Task<IDictionary<string, object>> GetDocumentReference(DocumentReference docRef)
        {
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            //snapshot.Reference.Database.Document
            if (snapshot.Exists)
            {
                return snapshot.ToDictionary();
            }
            else
            {
                return null;
            }
        }


        public async Task SetAsync<T>(string collectionName, string id, T model)
        {
            DocumentReference docRef = db.Collection(collectionName).Document(id);
            await docRef.SetAsync(model, SetOptions.MergeAll);
        }
        public async Task DeleteAsync(string collectionName)
        {
            var snapshot = db.Collection(collectionName).GetSnapshotAsync();
            await DeleteCollection(db.Collection(collectionName), 100000);
        }
        private async Task DeleteCollection(CollectionReference collectionReference, int batchSize)
        {
            QuerySnapshot snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
            IReadOnlyList<DocumentSnapshot> documents = snapshot.Documents;
            while (documents.Count > 0)
            {
                foreach (DocumentSnapshot document in documents)
                {
                    Console.WriteLine("Deleting document {0}", document.Id);
                    await document.Reference.DeleteAsync();
                }
                snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
                documents = snapshot.Documents;
            }
            Console.WriteLine("Finished deleting all documents from the collection.");
        }


        public async Task<IList<string>> GetKeys(string collectionName, string id = "")
        {
            IList<string> result = new List<string>();
            if (string.IsNullOrEmpty(id))
            {
                var listDoc = db.Collection(collectionName).ListDocumentsAsync();
                await listDoc.ForEachAsync(async doc =>
                {
                    result.Add(doc.Id);
                });
            }
            else
            {
                result.Add(await GetDocumentReferenceKey(db.Collection(collectionName).Document(id)));
            }
            return result;
        }
        public async Task<string> GetDocumentReferenceKey(DocumentReference docRef)
        {
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.Id;
            }
            else
            {
                Console.WriteLine("-Document {0} does not exist!", snapshot.Id);
                return null;
            }
        }
    }
}
