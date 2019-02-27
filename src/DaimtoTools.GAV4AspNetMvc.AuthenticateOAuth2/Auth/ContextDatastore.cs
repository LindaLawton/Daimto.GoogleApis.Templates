using Google.Apis.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;


namespace DaimtoTools.GAV4AspNetMvc.AuthenticateOAuth2.Auth
{
    /// <summary>
    /// File data store that implements <see cref="IDataStore"/>. This store creates a different file for each 
    /// combination of type and key. This file data store stores a JSON format of the specified object.
    /// </summary>
    public class HttpContextDataStore : IDataStore
    {
        private readonly HttpContext _context;

        public string AccessToken => _context.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(c => c.Type.Equals("googleaccesstoken"))?.Value;
        public string RefreshToken => _context.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(c => c.Type.Equals("googlerefreshtoken"))?.Value;

        private const string XdgDataHomeSubdirectory = "google-filedatastore";
        private static readonly Task CompletedTask = Task.FromResult(0);


        public HttpContextDataStore(HttpContext context)
        {
            _context = context;
        }
        
        public Task StoreAsync<T>(string key, T value)
        {
            //TODO figuer out how to udpate httpcontext with the new token


            //if (string.IsNullOrEmpty(key))
            //{
            //    throw new ArgumentException("Key MUST have a value");
            //}

            //var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
            //var filePath = Path.Combine(folderPath, GenerateStoredKey(key, typeof(T)));
            //File.WriteAllText(filePath, serialized);
            return CompletedTask;
        }
        
        public Task DeleteAsync<T>(string key)
        {

            //TODO figuer out how to delete the cookie

            //if (string.IsNullOrEmpty(key))
            //{
            //    throw new ArgumentException("Key MUST have a value");
            //}

            //var filePath = Path.Combine(folderPath, GenerateStoredKey(key, typeof(T)));
            //if (File.Exists(filePath))
            //{
            //    File.Delete(filePath);
            //}
            return CompletedTask;
        }

        
        public Task<T> GetAsync<T>(string key)
        {
            //if (string.IsNullOrEmpty(key))
            //{
            //    throw new ArgumentException("Key MUST have a value");
            //}

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            //var filePath = Path.Combine(folderPath, GenerateStoredKey(key, typeof(T)));
            //if (File.Exists(filePath))
            //{
            //    try
            //    {
            //        var obj = File.ReadAllText(filePath);
            //        tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(obj));
            //    }
            //    catch (Exception ex)
            //    {
            //        tcs.SetException(ex);
            //    }
            //}
            //else
            //{
            //    tcs.SetResult(default(T));
            //}
            return tcs.Task;
        }

        /// <summary>
        /// Clears all values in the data store. This method deletes all files in <see cref="FolderPath"/>.
        /// </summary>
        public Task ClearAsync()
        {
            //if (Directory.Exists(folderPath))
            //{
            //    Directory.Delete(folderPath, true);
            //    Directory.CreateDirectory(folderPath);
            //}

            return CompletedTask;
        }

        /// <summary>Creates a unique stored key based on the key and the class type.</summary>
        /// <param name="key">The object key.</param>
        /// <param name="t">The type to store or retrieve.</param>
        public static string GenerateStoredKey(string key, Type t)
        {
            return string.Format("{0}-{1}", t.FullName, key);
        }
    }
}
