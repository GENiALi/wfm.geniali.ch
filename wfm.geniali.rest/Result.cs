using System;

using RestSharp;

namespace wfm.geniali.rest
{
    public interface IResult
    {
        bool Successful
        {
            get;
            set;
        }

        string Message
        {
            get;
            set;
        }

        string StackTrace
        {
            get;
            set;
        }

        string Raw
        {
            get;
            set;
        }
    }

    public class Result<T> : IResult
    {
        public bool Successful
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string StackTrace
        {
            get;
            set;
        }

        public T Data
        {
            get;
            set;
        }

        public string Raw
        {
            get;
            set;
        }

        public Result(IResult result, T data)
        {
            Successful = result.Successful;
            Message    = result.Message;
            StackTrace = result.StackTrace;
            Data       = data;
            Raw        = result.Raw;
        }

        public Result()
        {
        }

        public Result<T> Success(IRestResponse<T> response)
        {
            return new Result<T>()
                   {
                       Data       = response.Data,
                       Raw        = response.Content,
                       Successful = true,
                       Message    = string.Empty
                   };
        }

        public Result<T> Failure(Exception ex)
        {
            return new Result<T>()
                   {
                       Successful = false,
                       Message    = ex.ToString(),
                       StackTrace = ex.StackTrace
                   };
        }
    }
}
