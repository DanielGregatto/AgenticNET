using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using Util.Interfaces;

namespace Util
{
    public class HandlerCookie : IHandlerTempInfo
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHandlerBase64String _handlerBase64String;
        private readonly static int expirationDefault = 1;

        public HandlerCookie(IHttpContextAccessor contextAccessor, IHandlerBase64String handlerBase64String)
        {
            this._contextAccessor = contextAccessor;
            _handlerBase64String = handlerBase64String;
        }

        public string GetValue(string key)
        {
            var value = _contextAccessor.HttpContext.Request.Cookies[key];

            return (!string.IsNullOrEmpty(value)) ? _handlerBase64String.DecodeFrom64(value) : null;
        }

        public T GetValueDescerialized<T>(string key)
        {
            try
            {
                var value = _contextAccessor.HttpContext.Request.Cookies[key];

                if (value == null)
                {
                    if (typeof(T).Equals(typeof(System.String)))
                        return (T)Activator.CreateInstance(typeof(T), new object[] { new char[] { } });
                    else
                        return (T)Activator.CreateInstance(typeof(T), new object[] { });
                }
                return JsonConvert.DeserializeObject<T>(_handlerBase64String.DecodeFrom64(value));
            }
            catch (Exception e)
            {
                return (T)Activator.CreateInstance(typeof(T), new object[] { });
            }
        }

        public void SetValue(string key, string value, int? addMinutes)
        {
            CookieOptions option = new CookieOptions();

            if (addMinutes.HasValue)
                option.Expires = DateTime.Now.AddMinutes(addMinutes.Value);
            else
                option.Expires = DateTime.Now.AddHours(expirationDefault);

            _contextAccessor.HttpContext.Response.Cookies.Append(key, _handlerBase64String.DecodeFrom64(value), option);
        }

        public void SetValueSerialized(string key, object value, int? addMinutes)
        {
            CookieOptions option = new CookieOptions();

            if (addMinutes.HasValue)
                option.Expires = DateTime.Now.AddMinutes(addMinutes.Value);
            else
                option.Expires = DateTime.Now.AddHours(expirationDefault);

            var valueSerialzied = JsonConvert.SerializeObject(value);

            _contextAccessor.HttpContext.Response.Cookies.Append(key, _handlerBase64String.DecodeFrom64(valueSerialzied), option);
        }

        public void Remove(string key)
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete(key);
        }

        public bool Exists(string key)
        {
            var value = _contextAccessor.HttpContext.Request.Cookies[key];
            if (value == null)
                return false;
            return true;
        }
    }
}