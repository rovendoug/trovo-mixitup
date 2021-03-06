﻿using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class StreamingPlatformServiceBase
    {
        public abstract string Name { get; }

        protected async Task<Result> AttemptConnect(Func<Task<Result>> connect, int connectionAttempts = 5)
        {
            Result result = new Result();
            for (int i = 0; i < connectionAttempts; i++)
            {
                try
                {
                    result = await connect();
                    if (result.Success)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                await Task.Delay(1000);
            }
            return result;
        }

        protected async Task RunAsync(Task task) { await AsyncRunner.RunAsync(task); }

        protected async Task<T> RunAsync<T>(Task<T> task) { return await AsyncRunner.RunAsync(task); }

        protected async Task RunAsync(Func<Task> task) { await AsyncRunner.RunAsync(task); }

        protected async Task<T> RunAsync<T>(Func<Task<T>> task) { return await AsyncRunner.RunAsync(task); }

        protected async Task<IEnumerable<T>> RunAsync<T>(Task<IEnumerable<T>> task)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return ReflectionHelper.CreateInstanceOf<List<T>>();
        }

        protected async Task<IDictionary<K, V>> RunAsync<K, V>(Task<IDictionary<K, V>> task)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return ReflectionHelper.CreateInstanceOf<Dictionary<K, V>>();
        }
    }
}
