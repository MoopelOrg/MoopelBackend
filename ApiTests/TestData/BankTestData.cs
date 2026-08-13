using MoopelObjects.Requests.Creation;
using MoopelObjects.Requests.Update;

namespace ApiTests.TestData;

internal static class BankAccountTestData
{
    public static readonly BankAccountCreateRequest ValidCreateRequest = new()
    {
        Name = "BankAccountName",
        Description = "Description"
    };
    public static readonly BankAccountUpdateRequest ValidUpdateRequest = new()
    {
        Name = "BankAccountNewName",
        Description = "NewDescription"
    };

    public static TheoryData<BankAccountCreateRequest, BankAccountUpdateRequest> ValidBankAccountUpdateCreateRequests
    {
        get
        {
            TheoryData<BankAccountCreateRequest, BankAccountUpdateRequest> data = [];

            foreach (BankAccountCreateRequest createRequest in ValidBankAccountCreateRequestValues())
            {
                foreach (BankAccountUpdateRequest updateRequest in ValidBankAccountUpdateRequestValues())
                {
                    data.Add(createRequest, updateRequest);
                }
            }

            return data;
        }
    }

    public static TheoryData<BankAccountCreateRequest> ValidBankAccountCreateRequests
    {
        get
        {
            TheoryData<BankAccountCreateRequest> data = [];

            foreach (BankAccountCreateRequest request in ValidBankAccountCreateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static TheoryData<BankAccountUpdateRequest> ValidBankAccountUpdateRequests
    {
        get
        {
            TheoryData<BankAccountUpdateRequest> data = [];

            foreach (BankAccountUpdateRequest request in ValidBankAccountUpdateRequestValues())
            {
                data.Add(request);
            }

            return data;
        }
    }

    public static IEnumerable<BankAccountUpdateRequest> ValidBankAccountUpdateRequestValues()
    {
        yield return new BankAccountUpdateRequest
        {
            Name = "Updated Checking",
            Description = "Updated checking account description"
        };

        yield return new BankAccountUpdateRequest
        {
            Name = "Updated Savings",
            Description = "Updated savings account description"
        };

        yield return new BankAccountUpdateRequest
        {
            Name = "A",
            Description = "Minimum length name"
        };

        yield return new BankAccountUpdateRequest
        {
            Name = "Account With Empty Description",
            Description = ""
        };
    }

    public static IEnumerable<BankAccountCreateRequest> ValidBankAccountCreateRequestValues()
    {
        yield return new BankAccountCreateRequest
        {
            Name = "Checking",
            Description = "Checking account description"
        };

        yield return new BankAccountCreateRequest
        {
            Name = "Savings",
            Description = "Savings account description"
        };

        yield return new BankAccountCreateRequest
        {
            Name = "Credit Card",
            Description = "Credit card account description"
        };

        yield return new BankAccountCreateRequest
        {
            Name = "A",
            Description = "Minimum length name"
        };

        yield return new BankAccountCreateRequest
        {
            Name = "Account With Empty Description",
            Description = ""
        };
    }

}