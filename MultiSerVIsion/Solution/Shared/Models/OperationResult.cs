using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSerVIsion.Solution.Shared.Models
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

       
        public static OperationResult<T> Succes(T data)
        {
            return new OperationResult<T>
            {
                Success = true,
                Data = data,
              
            };
        }
        public static OperationResult<T> SuccessEmpty(string message="")
        {
            return new OperationResult<T>
            {
                Success = true,
                Data = default,
                Message = message
            };
        }
        public static OperationResult<T> Fail(string message)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message
            };
        }
    }
    public class OperationResult : OperationResult<object>
    {
        public static OperationResult Succes(string message = "")
        {
            return new OperationResult
            {
                Success = true,
                Message = message,
            };
        }
        public static new OperationResult Fail(string message)
        {
            return new OperationResult
            {
                Success = false,
                Message = message,
            };
        }
    }
}
