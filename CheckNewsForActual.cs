using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Aktion.Clones.ExternalCache;
using Aktion.Clones.ExternalCache.Objects;
using Aktion.Clones3.BaseWeb;
using Aktion.Clones3.FrontEnd.Controls;
using Aktion.Commons.Data.Objects;
using Aktion.Commons.Logging;
using Aktion.Commons.Data.Objects;
using Aktion.Commons.Utils;
using Aktion.eContexts;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Aktion.Clones3.FrontEnd.AsyncWorker
{
    public sealed class CheckNewsForActual
    {
        volatile static ConcurrentDictionary<string, ReaderWriterLockSlim> _newsKeys = new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        public void CheckNews(object info)
        {
            var workerInfo = info as NewsWorkerInfo;

            if (workerInfo != null && workerInfo.Cache != null && workerInfo.Context != null &&
                workerInfo.PageParams != null && workerInfo.NewsListControl != null)
            {
                HttpContext.Current = workerInfo.Context;

                if (EvaluateNewsValueForBd(workerInfo))
                    EventContext.FireEvent(EventTypeEnum.NewsChanged);
            }
            else
                Log.Write(LogType.Error, "Wrong parametrs for CheckNewsForActual");
        }

        /// <summary>
        /// Возвращает информацию об изменении новостей
        /// </summary>
        /// <param name="workerInfo"></param>
        /// <returns></returns>
        bool EvaluateNewsValueForBd(NewsWorkerInfo workerInfo)
        {
            var result = false;
            var key = workerInfo.Cache.Key;

            if (!_newsKeys.ContainsKey(key) || !_newsKeys[key].IsWriteLockHeld)
            {
                var locker = !_newsKeys.ContainsKey(key) ? new ReaderWriterLockSlim() : _newsKeys[key];

                try
                {
                    locker.EnterWriteLock();
                    _newsKeys[key] = locker;

                    var data = ExternalCache.GetData(workerInfo.Cache);
                    if (data != null)
                    {
                        workerInfo.Cache.RawValue = data.RawValue;
                        workerInfo.Cache.Value = data.Value;
                    }

                    var obj = ParseNews(workerInfo.Cache, workerInfo.PageParams, workerInfo.NewsListControl,
                        workerInfo.Context);

                    if (obj.ErrorType == eExternalErrorType.None)
                    {
                        if (obj.RawValue != workerInfo.Cache.RawValue)
                        {
                            ExternalCache.SaveValue(obj);
                            result = true;
                        }
                    }
                    else
                        ExternalCache.SaveError(obj);
                }
                catch (Exception e)
                {
                    Log.Write(LogType.Error, "Ошибка при обработке новостей {0}", e.ToString());
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            return result;
        }


        ExternalObject ParseNews(ExternalObject obj, PageParameters parametrs, BaseNewsControl control, HttpContext context)
        {
            var result = new ExternalObject
            {
                ActivityType = obj.ActivityType,
                Partner = obj.Partner,
                ErrorType = eExternalErrorType.None,
                Key = String.Format(Constants.NewsAttributes.NewsListKey, parametrs.PageNum, parametrs.PageSize)
            };


            var newsRequest = EvaluateNewsRequestParametrs(parametrs);
            newsRequest.Url = context.Request.Url.ToString();
            var responseInfo = control.GetResponse(newsRequest);

            if (responseInfo.ErrorType == eExternalErrorType.None)
            {
                if (result.RawValue != responseInfo.Response)
                {
                    result.RawValue = responseInfo.Response;

                    try
                    {
                        var parseResult = control.ParseResponse(context, responseInfo.Response.ConvertStringByCharset(responseInfo.Charset, Constants.NewsAttributes.DefaultCharset));

                        if (parseResult.Count > 0)
                            result.Value = parseResult.XmlSerialize();
                        else
                            result.ErrorType = eExternalErrorType.Parse;
                    }
                    catch (Exception e)
                    {
                        result.ErrorType = eExternalErrorType.Parse;
                        Log.Write(LogType.Error, "Ошибка при обработке новостей: {0}", e.ToString());
                    }
                }
            }
            else
                result.ErrorType = responseInfo.ErrorType;

            return result;
        }

        BaseNewsControl.NewsRequest EvaluateNewsRequestParametrs(PageParameters pageParams)
        {
            var newsRequest = new BaseNewsControl.NewsRequest
            {
                StartIndex = pageParams.PageSize * (pageParams.PageNum - 1),
                PageSize = pageParams.PageSize
            };

            if (pageParams.PageNum > 1)
                newsRequest.StartIndex--;

            return newsRequest;
        }

        public class NewsWorkerInfo
        {
            public ExternalObject Cache { get; set; }
            public PageParameters PageParams { get; set; }
            public HttpContext Context { get; set; }
            public BaseNewsControl NewsListControl { get; set; }
        }
    }
}
