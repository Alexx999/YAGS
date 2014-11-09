using Yags.Annotations;

namespace Yags.Core
{
    public class MethodResult
    {
        public static readonly MethodResult Ok = new MethodResult(MethodErrorCode.Ok);
        public static readonly MethodResult Fail = new MethodResult<string>(MethodErrorCode.CallFailed, "Method call failed");
        public static readonly MethodResult SessionInvalid = new MethodResult<string>(MethodErrorCode.SessionInvalid, "Session invalid");
        public static readonly MethodResult WrongArguments = new MethodResult<string>(MethodErrorCode.WrongArguments, "Wrong Arguments");

        public int ResultCode { get; private set; }

        public MethodResult(MethodErrorCode code)
        {
            ResultCode = (int)code;
        }
    }

    public class MethodResult<T> : MethodResult
    {
        public T Data { [UsedImplicitly] get; private set; }

        public MethodResult(T data)
            : base(MethodErrorCode.Ok)
        {
            Data = data;
        }

        public MethodResult(MethodErrorCode code, T data)
            : base(code)
        {
            Data = data;
        }
    }
}
