using Newtonsoft.Json.Linq;
using System.Linq;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Persistence;
using ProWorks.Umbraco8.Migrations.Dtos;
using Umbraco.Core;

namespace ProWorks.Umbraco8.Migrations.Migrations
{
    /// <summary>
    /// Updates the pre-values on the Slider data type to preserve value
    /// settings from v7
    /// </summary>
    public class SliderPreValueMigrator : MigrationBase
    {
        public SliderPreValueMigrator(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var sql = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(d => d.EditorAlias == Constants.PropertyEditors.Aliases.Slider);

            var dtos = Database.Fetch<DataTypeDto>(sql);
            var changes = false;

            foreach (var dto in dtos)
            {
                if (!Migrate(dto)) continue;

                changes = true;
                Database.Update(dto);
            }

            // if some data types have been updated directly in the database (editing DataTypeDto and/or PropertyDataDto),
            // bypassing the services, then we need to rebuild the cache entirely, including the umbracoContentNu table
            if (changes)
                Context.AddPostMigration<RebuildPublishedSnapshot>();
        }

        // Convert from old names to new names
        private bool Migrate(DataTypeDto dto)
        {
            if (dto.Configuration.IsNullOrWhiteSpace() || dto.Configuration[0] != '{') return false;
            var config = JObject.Parse(dto.Configuration);
            var changes = false;

            if (config.ContainsKey("EnableRange"))
            {
                changes = true;
                config["enableRange"] = config["EnableRange"];
                config.Remove("EnableRange");
            }

            if (config.ContainsKey("InitialValue"))
            {
                changes = true;
                config["initVal1"] = config["InitialValue"];
                config.Remove("InitialValue");
            }

            if (config.ContainsKey("InitialValue2"))
            {
                changes = true;
                config["initVal2"] = config["InitialValue2"];
                config.Remove("InitialValue2");
            }

            if (config.ContainsKey("MinimumValue"))
            {
                changes = true;
                config["minVal"] = config["MinimumValue"];
                config.Remove("MinimumValue");
            }

            if (config.ContainsKey("MaximumValue"))
            {
                changes = true;
                config["maxVal"] = config["MaximumValue"];
                config.Remove("MaximumValue");
            }

            if (config.ContainsKey("StepIncrements"))
            {
                changes = true;
                config["step"] = config["StepIncrements"];
                config.Remove("StepIncrements");
            }

            if (changes)
            {
                dto.Configuration = config.ToString();
            }

            return changes;
        }
    }
}
