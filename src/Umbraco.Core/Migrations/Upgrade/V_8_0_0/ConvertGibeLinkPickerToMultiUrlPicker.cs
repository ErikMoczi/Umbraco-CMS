using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class ConvertGibeLinkPickerToMultiUrlPicker : MigrationBase
    {
        public ConvertGibeLinkPickerToMultiUrlPicker(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var sqlDataTypes = Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == "Gibe.LinkPicker");
            var dataTypes = Database.Fetch<DataTypeDto>(sqlDataTypes);
            var dataTypeIds = dataTypes.Select(x => x.NodeId).ToList();
            if (dataTypeIds.Count == 0) return;
            foreach (var dataType in dataTypes)
            {
                dataType.EditorAlias = Constants.PropertyEditors.Aliases.MultiUrlPicker;
                dataType.Configuration = "{\"minNumber\":0,\"maxNumber\":1,\"ignoreUserStartNodes\":false,\"hideAnchor\":false}";
                Database.Update(dataType);
            }

            var sqlPropertyTpes = Sql()
                .Select<PropertyTypeDto>()
                .From<PropertyTypeDto>()
                .Where<PropertyTypeDto>(x => dataTypeIds.Contains(x.DataTypeId));

            var propertyTypeIds = Database.Fetch<PropertyTypeDto>(sqlPropertyTpes).Select(x => x.Id).ToList();

            if (propertyTypeIds.Count == 0) return;

            var sqlPropertyData = Sql()
                .Select<PropertyDataDto>()
                .From<PropertyDataDto>()
                .Where<PropertyDataDto>(x => propertyTypeIds.Contains(x.PropertyTypeId));

            var properties = Database.Fetch<PropertyDataDto>(sqlPropertyData);


            foreach (var property in properties)
            {
                var value = property.Value?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                property.TextValue = NestedContentPropertyEditorsMigration.ConvertGibeLinkPickerToMultiUrlPicker(Database, value, Sql);
                Database.Update(property);
            }
        }
    }
}
