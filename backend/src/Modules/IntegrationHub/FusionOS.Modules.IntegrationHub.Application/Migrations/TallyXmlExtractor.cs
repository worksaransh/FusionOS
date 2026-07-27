using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations;

/// <summary>
/// Reads a real Tally "Data Export" XML document (Gateway of Tally → Export
/// → Data) — the extractor half of 11_SAP_MIGRATION.md §2's
/// "Source System → Extractor → Staging Schema" step, scoped to Tally per
/// §4's stated priority (high-volume, Indian SMB/trading segment).
///
/// A Tally export looks like:
/// <c>&lt;ENVELOPE&gt;&lt;BODY&gt;&lt;IMPORTDATA&gt;&lt;REQUESTDATA&gt;
/// &lt;TALLYMESSAGE&gt;&lt;LEDGER NAME="..."&gt;...&lt;/LEDGER&gt;
/// &lt;/TALLYMESSAGE&gt;...&lt;/REQUESTDATA&gt;&lt;/IMPORTDATA&gt;&lt;/BODY&gt;
/// &lt;/ENVELOPE&gt;</c> — every master/transaction record is wrapped in its
/// own &lt;TALLYMESSAGE&gt;, which contains exactly one of &lt;LEDGER&gt;,
/// &lt;STOCKITEM&gt;, &lt;VOUCHER&gt;, or &lt;GROUP&gt; (checked in that
/// order) as its real payload.
///
/// This extractor is read-only against the source (per §2 — it never writes
/// back to Tally) and deliberately does NOT hardcode a field list per record
/// type: it walks whatever attributes/child elements the export actually
/// contains and lands them as-is into JSON, exactly matching the blueprint's
/// "staging schema... holding raw extracted data as-is, before any
/// transformation" (§2). Mapping/validation/transform happens in later
/// pipeline stages against this raw JSON, not here.
/// </summary>
public sealed class TallyXmlExtractor
{
    /// <summary>Tally record types this extractor recognizes, checked in this exact order per TALLYMESSAGE element.</summary>
    private static readonly string[] KnownRecordElementNames = { "LEDGER", "STOCKITEM", "VOUCHER", "GROUP" };

    /// <summary>
    /// Parses <paramref name="tallyXmlContent"/> and returns one (RecordType, RawDataJson)
    /// pair per recognized TALLYMESSAGE. TALLYMESSAGE elements whose payload is none of
    /// the known record types are silently skipped (e.g. Tally also emits &lt;COMPANY&gt;
    /// or &lt;CURRENCY&gt; master messages this first pass does not attempt to migrate).
    /// </summary>
    public IReadOnlyList<(string RecordType, string RawDataJson)> Extract(string tallyXmlContent)
    {
        if (string.IsNullOrWhiteSpace(tallyXmlContent))
            throw new ArgumentException("Tally XML content is required.", nameof(tallyXmlContent));

        XDocument document;
        try
        {
            document = XDocument.Parse(tallyXmlContent);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidOperationException($"Could not parse the supplied content as Tally export XML: {ex.Message}", ex);
        }

        var results = new List<(string RecordType, string RawDataJson)>();

        // Descendants(), not a fixed ENVELOPE/BODY/IMPORTDATA/REQUESTDATA path, since
        // Tally's export shape varies slightly between "All Masters" exports and
        // "Day Book"/voucher exports — TALLYMESSAGE can nest a level or two differently.
        foreach (var message in document.Descendants("TALLYMESSAGE"))
        {
            XElement? recordElement = null;
            foreach (var candidateName in KnownRecordElementNames)
            {
                recordElement = message.Element(candidateName);
                if (recordElement is not null)
                    break;
            }

            if (recordElement is null)
                continue;

            var recordType = recordElement.Name.LocalName;
            var json = ElementToJsonObject(recordElement);
            results.Add((recordType, json.ToJsonString()));
        }

        return results;
    }

    /// <summary>
    /// Generically converts one XML element's own attributes and child elements into a
    /// flat-ish JSON object — no per-record-type field list. The XML attribute named
    /// NAME (how Tally names every LEDGER/STOCKITEM/GROUP/VOUCHER master) becomes the
    /// JSON field "Name"; every other attribute keeps its own name. Each child element's
    /// text becomes a JSON field keyed by the element's name; a child that itself has
    /// child elements (e.g. VOUCHER's nested ALLLEDGERENTRIES.LIST debit/credit lines)
    /// is recursively converted instead of taken as text; repeated sibling elements with
    /// the same name become a JSON array (this is how a VOUCHER's multiple ledger-entry
    /// lines are captured).
    /// </summary>
    private static JsonObject ElementToJsonObject(XElement element)
    {
        var obj = new JsonObject();

        foreach (var attribute in element.Attributes())
        {
            var key = attribute.Name.LocalName == "NAME" ? "Name" : attribute.Name.LocalName;
            obj[key] = attribute.Value;
        }

        foreach (var group in element.Elements().GroupBy(e => e.Name.LocalName))
        {
            var children = group.ToList();

            if (children.Count == 1)
            {
                obj[group.Key] = ChildToJsonNode(children[0]);
            }
            else
            {
                var array = new JsonArray();
                foreach (var child in children)
                    array.Add(ChildToJsonNode(child));
                obj[group.Key] = array;
            }
        }

        return obj;
    }

    private static JsonNode? ChildToJsonNode(XElement child) =>
        child.HasElements ? ElementToJsonObject(child) : JsonValue.Create(child.Value);
}
