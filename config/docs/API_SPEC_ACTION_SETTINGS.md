API Specification - Action Settings
======================================

## Get a list of actions settings

The list of configuration settings for all action types.

The ApplicationPermissions attribute indicates whether or not the application has
been given "Owner" permissions in order to make management API calls. If the application
does not have "Owner" permissions, then users will need to check on resources manually from
the Azure portal.

Request:
```
GET /v1/solution-settings/actions
```

Response:
```
200 OK
Content-Type: application/json
```
```json
{
    "Items": [
        {
            "Type": "Email",
            "Settings": {
                "IsEnabled": false,
                "ApplicationPermissions" ["Owner" | "Contributor"]
                "Office365ConnectorUrl": "https://portal.azure.com/#@{tenant}/resource/subscriptions/{subscription}/resourceGroups/{resource-group}/providers/Microsoft.Web/connections/office365-connector/edit"
            }
        }
    ],
    "$metadata": {
        "$type": "ActionSettingsList;1",
        "$url": "/v1/solution-settings/actions"
    }
}
```