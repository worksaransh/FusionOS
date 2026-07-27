using System.Text.Json.Nodes;
using FluentAssertions;
using FusionOS.Modules.IntegrationHub.Application.Migrations;
using Xunit;

namespace FusionOS.Modules.IntegrationHub.Tests.Migrations;

public class TallyXmlExtractorTests
{
    private readonly TallyXmlExtractor _extractor = new();

    [Fact]
    public void Extract_LedgerAndVoucherWithNestedEntries_ProducesExpectedJsonShape()
    {
        const string xml = """
            <ENVELOPE>
              <BODY>
                <IMPORTDATA>
                  <REQUESTDATA>
                    <TALLYMESSAGE>
                      <LEDGER NAME="Cash">
                        <PARENT>Cash-in-Hand</PARENT>
                        <OPENINGBALANCE>1000.00</OPENINGBALANCE>
                      </LEDGER>
                    </TALLYMESSAGE>
                    <TALLYMESSAGE>
                      <VOUCHER VCHTYPE="Payment" ACTION="Create">
                        <DATE>20260701</DATE>
                        <ALLLEDGERENTRIES.LIST>
                          <LEDGERNAME>Cash</LEDGERNAME>
                          <ISDEEMEDPOSITIVE>Yes</ISDEEMEDPOSITIVE>
                          <AMOUNT>-500.00</AMOUNT>
                        </ALLLEDGERENTRIES.LIST>
                        <ALLLEDGERENTRIES.LIST>
                          <LEDGERNAME>Office Rent</LEDGERNAME>
                          <ISDEEMEDPOSITIVE>No</ISDEEMEDPOSITIVE>
                          <AMOUNT>500.00</AMOUNT>
                        </ALLLEDGERENTRIES.LIST>
                      </VOUCHER>
                    </TALLYMESSAGE>
                  </REQUESTDATA>
                </IMPORTDATA>
              </BODY>
            </ENVELOPE>
            """;

        var results = _extractor.Extract(xml);

        results.Should().HaveCount(2);

        var ledgerRecord = results.Single(r => r.RecordType == "LEDGER");
        var ledgerJson = JsonNode.Parse(ledgerRecord.RawDataJson)!.AsObject();
        ledgerJson["Name"]!.GetValue<string>().Should().Be("Cash");
        ledgerJson["PARENT"]!.GetValue<string>().Should().Be("Cash-in-Hand");
        ledgerJson["OPENINGBALANCE"]!.GetValue<string>().Should().Be("1000.00");

        var voucherRecord = results.Single(r => r.RecordType == "VOUCHER");
        var voucherJson = JsonNode.Parse(voucherRecord.RawDataJson)!.AsObject();
        voucherJson["VCHTYPE"]!.GetValue<string>().Should().Be("Payment");
        voucherJson["ACTION"]!.GetValue<string>().Should().Be("Create");
        voucherJson["DATE"]!.GetValue<string>().Should().Be("20260701");

        // Two sibling ALLLEDGERENTRIES.LIST elements -> a JSON array of two objects.
        var entries = voucherJson["ALLLEDGERENTRIES.LIST"]!.AsArray();
        entries.Should().HaveCount(2);
        entries[0]!["LEDGERNAME"]!.GetValue<string>().Should().Be("Cash");
        entries[0]!["ISDEEMEDPOSITIVE"]!.GetValue<string>().Should().Be("Yes");
        entries[0]!["AMOUNT"]!.GetValue<string>().Should().Be("-500.00");
        entries[1]!["LEDGERNAME"]!.GetValue<string>().Should().Be("Office Rent");
        entries[1]!["AMOUNT"]!.GetValue<string>().Should().Be("500.00");
    }

    [Fact]
    public void Extract_UnrecognizedTallyMessagePayload_SkipsGracefullyWithoutThrowing()
    {
        const string xml = """
            <ENVELOPE>
              <BODY>
                <IMPORTDATA>
                  <REQUESTDATA>
                    <TALLYMESSAGE>
                      <COMPANY NAME="Acme Trading Co">
                        <STATENAME>Maharashtra</STATENAME>
                      </COMPANY>
                    </TALLYMESSAGE>
                    <TALLYMESSAGE>
                      <STOCKITEM NAME="Widget">
                        <BASEUNITS>Nos</BASEUNITS>
                      </STOCKITEM>
                    </TALLYMESSAGE>
                  </REQUESTDATA>
                </IMPORTDATA>
              </BODY>
            </ENVELOPE>
            """;

        var act = () => _extractor.Extract(xml);

        // COMPANY is not one of the KnownRecordElementNames — its TALLYMESSAGE is silently
        // skipped, not thrown on; the recognized STOCKITEM sibling is still extracted.
        var results = act.Should().NotThrow().Subject;
        results.Should().ContainSingle();
        results.Single().RecordType.Should().Be("STOCKITEM");
    }

    [Fact]
    public void Extract_BlankContent_ThrowsArgumentException()
    {
        var act = () => _extractor.Extract("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Extract_MalformedXml_ThrowsInvalidOperationException()
    {
        var act = () => _extractor.Extract("<ENVELOPE><BODY>not closed");

        act.Should().Throw<InvalidOperationException>();
    }
}
