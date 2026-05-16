using System.Diagnostics.CodeAnalysis;

namespace CampaignCatalog.Database
{
    [ExcludeFromCodeCoverage]
    public static class MigrationConstantes
    {
        public static readonly string[] Sources = { "Migrations/Tables", "Migrations/Procedures", "Migrations/Inserts" };
        public const string ScriptPrefix = "V";
        public const string RepeatableScriptPrefix = "R";
        public static readonly string[] Commands = { "migrate", "repair", "info" };
        public const string Strategy = "each";
        public const string ConnectionStringKey = "DB-CONNECTION";

    }
}