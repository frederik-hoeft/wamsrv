using wamsrv.ApiResponses;
using wamsrv.Database;
using washared.DatabaseServer;
using washared.DatabaseServer.ApiResponses;

namespace wamsrv.ApiRequests
{
    public class ChangeUserPermissionsRequestA : ApiRequest
    {
        public readonly string TargetUserId;
        public readonly Permission Permissions;

        public ChangeUserPermissionsRequestA(ApiRequestId requestId, string targetUserId, Permission permissions)
        {
            RequestId = requestId;
            TargetUserId = targetUserId;
            Permissions = permissions;
        }

        public override void Process(ApiServer server)
        {
            if (server.AssertServerSetup(this) || server.AssertUserOnline())
            {
                return;
            }
            using DatabaseManager databaseManager = new DatabaseManager(server);
            if (databaseManager.AssertHasPermission(Permission.ADJUST_PRIVILEGES))
            {
                return;
            }
            bool userExists = databaseManager.CheckUserExists(TargetUserId, out bool success);
            if (!success)
            {
                return;
            }
            if (!userExists)
            {
                ApiError.Throw(ApiErrorCode.NotFound, server, "User not found.");
                return;
            }
            bool targetIsRoot = databaseManager.UserIsRoot(TargetUserId, out success);
            if (!success)
            {
                return;
            }
            if (targetIsRoot)
            {
                ApiError.Throw(ApiErrorCode.InsufficientPermissions, server, "Cannot adjust permissions of root: is fixed to " + Permission.ALL_ACCESS.ToString());
                return;
            }
            Permission currentPermissions = databaseManager.GetUserPermission(TargetUserId, out success);
            if (!success)
            {
                return;
            }
            if (currentPermissions != Permissions)
            {
                string targetId = databaseManager.UserIdToId(TargetUserId, out success);
                if (!success)
                {
                    return;
                }
                string query;
                if (Permissions == Permission.NONE)
                {
                    query = "DELETE FROM Tbl_admin WHERE userid = " + targetId + ";";
                }
                else if (currentPermissions == Permission.NONE)
                {
                    query = "INSERT INTO Tbl_admin (userid, permissions) VALUES (" + targetId + ", " + ((int)Permissions).ToString() + ");";
                }
                else
                {
                    query = "UPDATE Tbl_admin SET permissions = " + ((int)Permissions).ToString() + " WHERE userid = " + targetId + ";";
                }
                SqlApiRequest sqlRequest = SqlApiRequest.Create(SqlRequestId.ModifyData, query, -1);
                SqlModifyDataResponse modifyDataResponse = databaseManager.AwaitModifyDataResponse(sqlRequest, out success);
                if (!success)
                {
                    return;
                }
            }
            GenericSuccessResponse response = new GenericSuccessResponse(ResponseId.ChangeUserPermissionsA, true);
            SerializedApiResponse serializedApiResponse = SerializedApiResponse.Create(response);
            string json = serializedApiResponse.Serialize();
            server.Send(json);
            server.UnitTesting.MethodSuccess = true;
            return;
        }
    }
}