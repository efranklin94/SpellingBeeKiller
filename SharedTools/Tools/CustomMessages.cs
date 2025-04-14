namespace SharedTools.Tools;

public static class CustomMessages
{
    # region Properties

    public static readonly string UserNotFound = "User not found!";
    public static readonly string UserNotExist = "User not exist!";
    public static readonly string LimitReached = "You have reached your renaming limit!";
    public static readonly string UserNameNotAvailable = "UserName is not available!";
    public static readonly string RefreshTokenNotValid = "Refresh Token Is Not Valid!";
    public static readonly string CheckInput = "Check input!";
    public static readonly string InvalidRequest = "Invalid request!";

    #endregion


    #region Methods

    public static string ItemIsNotValid(string itemName) => $"{itemName} is not valid!";
    public static string DoesNotHaveEnoughItems(string itemName) => $"User does not have enough {itemName}!";
    public static string DoesNotHaveEnoughItemsTo(string itemName, string reason) => $"User does not have enough {itemName}!";
    public static string LengthShoulBeGreaterThan(byte digits) => $"Length should be greater than 4 {digits} digits!";
    public static string LengthShoulNotBeGreaterThan(byte digits) => $"Length should not be greater than 4 {digits} digits!";
    public static string ItemAlreadyExists(string item) => $"This {item} is already exist!";

    #endregion
}