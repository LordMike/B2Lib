using System;
using System.Threading.Tasks;

namespace B2Lib.SyncExtensions
{
    public static class Utility
    {
        public static T AsyncRunHelper<T>(Func<Task<T>> action)
        {
            try
            {
                return action().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        public static void AsyncRunHelper(Func<Task> action)
        {
            try
            {
                action().Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}