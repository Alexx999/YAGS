namespace Yags.Core
{
    public enum MethodErrorCode
    {
        WrongArguments = -3,
        SessionInvalid = -2,
        CallFailed = -1,
        Ok = 0,
        InvalidBuildingId,
        InvalidCityId,
        InvalidCityOwner,
        InvalidBuildingType,
        InvalidUserId,
        InvalidBuildingPosition,
        NotEnoughMoney
    }
}
