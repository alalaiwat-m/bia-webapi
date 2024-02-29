using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web.Http.ModelBinding;
using BusinessClassLibrary.EntityClass;

namespace BACWebAPI.Models.Common
{
    public class clsCommonFunction
    {
        public static void SetModelError(ModelStateDictionary objModelState, ref List<clsMessage> objError)
        {
            foreach (var modelState in objModelState.Values)
            foreach (var modelError in modelState.Errors)
                //objError.Add(new clsMessage { code = 500, message = modelError.ErrorMessage });
                if (!string.IsNullOrEmpty(modelError.ErrorMessage))
                    objError.Add(new clsMessage {code = 500, message = modelError.ErrorMessage});
                else if (!string.IsNullOrEmpty(modelError.Exception.Message))
                    objError.Add(new clsMessage {code = 500, message = modelError.Exception.Message});
        }

        public static void AddInCacheMemory(string key, object value, int validMinutes = 5)
        {
            var cache = MemoryCache.Default;
            try
            {
                if (cache.Get(key) == null)
                    cache.Add(key, value,
                        new CacheItemPolicy {AbsoluteExpiration = DateTime.Now.AddMinutes(validMinutes)});
                else
                    cache.Set(key, value,
                        new CacheItemPolicy {AbsoluteExpiration = DateTime.Now.AddMinutes(validMinutes)});
            }
            catch (Exception ex)
            {
                //
            }
        }

        public static object GetCacheValue(string key)
        {
            var cache = MemoryCache.Default;
            try
            {
                return cache.Get(key);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void ClearCache(string strKey = "")
        {
            var cache = MemoryCache.Default;

            try
            {
                if (!string.IsNullOrEmpty(strKey))
                    cache.Remove(strKey, CacheEntryRemovedReason.Expired);
                else
                    foreach (var item in cache)
                        cache.Remove(item.Key, CacheEntryRemovedReason.Expired);
            }
            catch (Exception)
            {
            }
        }
    }
}