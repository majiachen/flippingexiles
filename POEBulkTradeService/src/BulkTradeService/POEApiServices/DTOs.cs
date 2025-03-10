using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TradeResponse
{
    public string id { get; set; }
    public object complexity { get; set; }
    public Dictionary<string, TradeResult> result { get; set; }
    public int total { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

public class TradeResult
{
    public string id { get; set; }
    public object item { get; set; }
    public Listing listing { get; set; }

    public override string ToString()
    {
        return $"ID: {id}\nItem: {item}\nListing: {listing}";
    }
}

public class Listing
{
    public string indexed { get; set; }
    public Account account { get; set; }
    public List<Offer> offers { get; set; }
    public string whisper { get; set; }

    public override string ToString()
    {
        return $"Indexed: {indexed}\nAccount: {account}\nOffers: {string.Join(", ", offers)}\nWhisper: {whisper}";
    }
}

public class Account
{
    public string name { get; set; }
    public OnlineStatus online { get; set; }
    public string lastCharacterName { get; set; }
    public string language { get; set; }
    public string realm { get; set; }

    public override string ToString()
    {
        return $"Name: {name}, Online: {online}, Last Character: {lastCharacterName}, Language: {language}, Realm: {realm}";
    }
}

public class OnlineStatus
{
    public string league { get; set; }
    public string status { get; set; }

    public override string ToString()
    {
        return $"League: {league}, Status: {status}";
    }
}

public class Offer
{
    public Exchange exchange { get; set; }
    public OfferItem item { get; set; }

    public override string ToString()
    {
        return $"Exchange: {exchange}, Item: {item}";
    }
}

public class Exchange
{
    public string currency { get; set; }
    public decimal amount { get; set; }
    public string whisper { get; set; }

    public override string ToString()
    {
        return $"Currency: {currency}, Amount: {amount}, Whisper: {whisper}";
    }
}

public class OfferItem
{
    public string currency { get; set; }
    public decimal amount { get; set; }
    public int stock { get; set; }
    public string id { get; set; }
    public string whisper { get; set; }

    public override string ToString()
    {
        return $"Currency: {currency}, Amount: {amount}, Stock: {stock}, ID: {id}, Whisper: {whisper}";
    }
}

public class TradeRequest
{
    public string engine { get; set; } = "new";
    public TradeQuery query { get; set; }
    public SortOptions sort { get; set; } = new() { have = "asc" };

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

public class TradeQuery
{
    public TradeStatus status { get; set; }
    public string[] have { get; set; }
    public string[] want { get; set; }
    public int? minimum { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

public class TradeStatus
{
    public string option { get; set; }

    public override string ToString()
    {
        return $"Status: {option}";
    }
}

public class SortOptions
{
    public string have { get; set; }

    public override string ToString()
    {
        return $"Sort By: {have}";
    }
}