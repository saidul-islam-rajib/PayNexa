using System.Text.Json.Nodes;
using PayNexa.Logging.Masking;

namespace PayNexa.BuildingBlocks.UnitTests.Logging;

public sealed class JsonBodyMaskerTests
{
    [Fact]
    public void TryMask_CustomerPayload_MasksPersonalDataAndKeepsTheRest()
    {
        const string json = """
            {
              "id": "58c49479-ec65-4de2-86e7-033c546291aa",
              "firstName": "Saidul",
              "lastName": "Rajib",
              "email": "rajib@example.com",
              "phoneNumber": "+8801700001234",
              "dateOfBirth": "1995-05-20",
              "address": { "line1": "Mirpur Road", "city": "Dhaka", "postalCode": "1207", "countryCode": "BD" },
              "status": "Active"
            }
            """;

        JsonBodyMasker.TryMask(json, out var masked).ShouldBeTrue();
        var node = JsonNode.Parse(masked)!;

        node["id"]!.GetValue<string>().ShouldBe("58c49479-ec65-4de2-86e7-033c546291aa");
        node["firstName"]!.GetValue<string>().ShouldBe("S***");
        node["lastName"]!.GetValue<string>().ShouldBe("R***");
        node["email"]!.GetValue<string>().ShouldBe("r***@example.com");
        node["phoneNumber"]!.GetValue<string>().ShouldBe("*********1234");
        node["dateOfBirth"]!.GetValue<string>().ShouldBe("1***");
        node["address"]!["line1"]!.GetValue<string>().ShouldBe("M***");
        node["address"]!["postalCode"]!.GetValue<string>().ShouldBe("1***");
        node["address"]!["city"]!.GetValue<string>().ShouldBe("Dhaka");
        node["status"]!.GetValue<string>().ShouldBe("Active");
    }

    [Fact]
    public void TryMask_Secrets_AreRedactedWhateverTheirType()
    {
        const string json = """{ "username": "rajib", "password": "Test@123", "cvv": 123, "refreshToken": { "value": "abc" } }""";

        JsonBodyMasker.TryMask(json, out var masked).ShouldBeTrue();
        var node = JsonNode.Parse(masked)!;

        node["username"]!.GetValue<string>().ShouldBe("rajib");
        node["password"]!.GetValue<string>().ShouldBe(SensitiveDataMaskingEnricher.Redacted);
        node["cvv"]!.GetValue<string>().ShouldBe(SensitiveDataMaskingEnricher.Redacted);
        node["refreshToken"]!.GetValue<string>().ShouldBe(SensitiveDataMaskingEnricher.Redacted);
    }

    [Fact]
    public void TryMask_ArraysOfObjects_AreMaskedPerItem()
    {
        const string json = """{ "items": [ { "email": "a@x.io" }, { "email": "b@y.io" } ], "totalCount": 2 }""";

        JsonBodyMasker.TryMask(json, out var masked).ShouldBeTrue();
        var node = JsonNode.Parse(masked)!;

        node["items"]![0]!["email"]!.GetValue<string>().ShouldBe("a***@x.io");
        node["items"]![1]!["email"]!.GetValue<string>().ShouldBe("b***@y.io");
        node["totalCount"]!.GetValue<int>().ShouldBe(2);
    }

    [Fact]
    public void TryMask_InvalidJson_ReturnsFalse()
    {
        JsonBodyMasker.TryMask("{ not json", out var masked).ShouldBeFalse();
        masked.ShouldBeEmpty();
    }
}
