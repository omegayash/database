using System;
namespace xx.DataAccessLayer.Contracts
{
    public interface INumericKey
    {
        int ID { get; set; }
    }

    public interface IAlphanumericKey
    {
        string ID { get; set; }
    }

    public interface IResourceKey
    {
        string resource_key { get; set; }
    }

    public interface ILanguageKey
    {
        string languageKey { get; set; }
    }

    public interface ILanguageID
    {
        string language_id { get; set; }
    }
}
