using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace ApiTests.TestData;

internal static class TransactionTestData
{
    public const int MissingAccountId = 999999;
    public const int MissingTransactionId = 999999;
    public const int MissingTransactionItemId = 999999;
    public const string AccountIdQueryKey = "accountId";
    public const string TransactionIdQueryKey = "transactionId";

    public const string TransactionName = "Groceries";
    public const string TransactionDescription = "Monthly grocery shopping";
    public const string UpdatedTransactionName = "Electric Bill";
    public const string UpdatedTransactionDescription = "Monthly electricity bill";

    public const string TransactionItemName = "Milk";
    public const string UpdatedTransactionItemName = "Milk and Bread";

    public static readonly TransactionCreateRequest ValidCreateRequest = new()
    {
        Name = TransactionName,
        Description = TransactionDescription
    };

    public static readonly TransactionItemCreateRequest ValidItemCreateRequest = new()
    {
        Name = TransactionItemName,
        Amount = 12.50m
    };

    public static readonly TransactionItemUpdateRequest ValidItemUpdateRequest = new()
    {
        Name = UpdatedTransactionItemName,
        Amount = 18.75m
    };
}
