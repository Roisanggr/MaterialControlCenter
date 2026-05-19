using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using MaterialControlCenter.Models;

namespace MaterialControlCenter.Service
{
    public class DatabaseConnection
    {
        private readonly string connectionString;

        public DatabaseConnection(string connectionName)
        {
            connectionString = ConfigurationManager.ConnectionStrings[connectionName]?.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception($"Connection string '{connectionName}' not found.");
        }
        public async Task<List<ShiftModel>> GetAllShiftsAsync()
        {
            var shifts = new List<ShiftModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                var query = @"SELECT [id], [shiftId], [range_hour] FROM [ToolroomDB].[dbo].[T_shiftId]";
                using (var command = new SqlCommand(query, sqlconn))
                {
                    await sqlconn.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            shifts.Add(new ShiftModel
                            {
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                ShiftId = reader["shiftId"]?.ToString(),
                                RangeHour = reader["range_hour"]?.ToString()
                            });
                        }
                    }
                }
            }

            return shifts;
        }
        public List<EmployeeMasterModelSSO> GetEmployeeMasterSSO()
        {
            List<EmployeeMasterModelSSO> employees = new List<EmployeeMasterModelSSO>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"SELECT [kpk], [name], [birth_date], [email], [supervisor]
                                 FROM [SSO].[dbo].[employee_master]";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employee = new EmployeeMasterModelSSO
                        {
                            Kpk = reader["kpk"] as string,
                            Name = reader["name"] as string,
                            BirthDate = reader["birth_date"] as DateTime?,
                            Email = reader["email"] as string,
                            Supervisor = reader["supervisor"] as string
                        };

                        employees.Add(employee);
                    }
                }
            }

            return employees;
        }
        public EmployeeMasterModelSSO GetUserByKpkSSO(string kpk)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"SELECT [kpk], [name], [birth_date], [email], [supervisor]
                         FROM [SSO].[dbo].[employee_master]
                         WHERE kpk = @kpk";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@kpk", kpk);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new EmployeeMasterModelSSO
                            {
                                Kpk = reader["kpk"] as string,
                                Name = reader["name"] as string,
                                BirthDate = reader["birth_date"] as DateTime?,
                                Email = reader["email"] as string,
                                Supervisor = reader["supervisor"] as string
                            };
                        }
                    }
                }
            }

            return null;
        }

        public List<EmployeeMasterModelSSO> GetUsersBySupervisorKpk(string supervisorKpk)
        {
            var result = new List<EmployeeMasterModelSSO>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"SELECT [kpk], [name], [birth_date], [email], [supervisor]
                         FROM [SSO].[dbo].[employee_master]
                         WHERE supervisor = @supervisorKpk";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@supervisorKpk", supervisorKpk);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new EmployeeMasterModelSSO
                            {
                                Kpk = reader["kpk"] as string,
                                Name = reader["name"] as string,
                                BirthDate = reader["birth_date"] as DateTime?,
                                Email = reader["email"] as string,
                                Supervisor = reader["supervisor"] as string
                            });
                        }
                    }
                }
            }

            return result;
        }


        public List<UserToolRoomModel> GetAllUsersScrap()
        {
            List<UserToolRoomModel> users = new List<UserToolRoomModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"
            SELECT [kpk], [name], [email], [role_id], [hierarchy], [network_id], [facility], [is_active],[ScrapCodeResponsible]
            FROM [Scrap].[dbo].[user] where is_active=1";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new UserToolRoomModel
                        {
                            Kpk = reader["kpk"] as string,
                            Name = reader["name"] as string,
                            Email = reader["email"] as string,
                            RoleId = reader["role_id"] != DBNull.Value ? Convert.ToInt32(reader["role_id"]) : (int?)null,
                            Hierarchy = reader["hierarchy"] as string,
                            NetworkId = reader["network_id"] as string,
                            Facility = reader["facility"] != DBNull.Value
                                ? reader["facility"].ToString().Split(',').Select(x => x.Trim()).ToArray()
                                : Array.Empty<string>(),
                            ScrapCodeResponsible = reader["ScrapCodeResponsible"] != DBNull.Value
                                ? reader["ScrapCodeResponsible"].ToString().Split(',').Select(x => x.Trim()).ToArray()
                                : Array.Empty<string>(),

                            IsActive = reader["is_active"] != DBNull.Value ? (bool?)reader["is_active"] : null
                        });
                    }
                }
            }

            return users;
        }

        public List<UserToolRoomModel> GetAllUsersScrapNoFilter()
        {
            List<UserToolRoomModel> users = new List<UserToolRoomModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"
        SELECT [kpk], [name], [email], [role_id], [hierarchy], 
               [network_id], [facility], [is_active], [TC],[ScrapCodeResponsible]
        FROM [Scrap].[dbo].[user]"; 

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new UserToolRoomModel
                        {
                            Kpk = reader["kpk"] as string,
                            Name = reader["name"] as string,
                            Email = reader["email"] as string,
                            RoleId = reader["role_id"] != DBNull.Value ? Convert.ToInt32(reader["role_id"]) : (int?)null,
                            Hierarchy = reader["hierarchy"] as string,
                            NetworkId = reader["network_id"] as string,
                            Facility = reader["facility"] != DBNull.Value
                                        ? reader["facility"].ToString().Split(',').Select(x => x.Trim()).ToArray()
                                        : Array.Empty<string>(),

                            IsActive = reader["is_active"] != DBNull.Value ? (bool?)reader["is_active"] : null,
                            TC = reader["TC"] != DBNull.Value
                                                ? reader["TC"].ToString().Split(',').Select(x => x.Trim()).ToArray()
                                                : Array.Empty<string>(),
                            ScrapCodeResponsible = reader["ScrapCodeResponsible"] != DBNull.Value
                                                ? reader["ScrapCodeResponsible"].ToString().Split(',').Select(x => x.Trim()).ToArray()
                                                : Array.Empty<string>(),

                        });
                    }
                }
            }

            return users;
        }
        public List<RoleModel> GetAllRoles()
        {
            List<RoleModel> roles = new List<RoleModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"
            SELECT [role_id], [name]
            FROM [Scrap].[dbo].[role]";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new RoleModel
                        {
                            RoleId = reader["role_id"] != DBNull.Value ? Convert.ToInt32(reader["role_id"]) : 0,
                            Name = reader["name"] as string
                        });
                    }
                }
            }

            return roles;
        }
        public UserToolRoomModel GetUserByKpkScrap(string kpk)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"SELECT [kpk], [role_id], [facility],[TC]
                         FROM [Scrap].[dbo].[user]
                         WHERE kpk = @kpk and is_active=1";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@kpk", kpk);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserToolRoomModel
                            {
                                Kpk = reader["kpk"] as string,
                                RoleId = reader["role_id"] != DBNull.Value
                                           ? Convert.ToInt32(reader["role_id"])
                                           : (int?)null,
                                Plant = reader["facility"] as string,
                                TC = reader["TC"] != DBNull.Value
                                                ? reader["TC"].ToString().Split(',').Select(x => x.Trim()).ToArray()
                                                : Array.Empty<string>(),
                            };
                        }
                    }
                }
            }

            return null;
        }
        public async Task<bool> InsertUserAsync(UserModelInsert request)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Cek apakah KPK sudah ada
                    string checkQuery = "SELECT COUNT(*) FROM [Scrap].[dbo].[user] WHERE kpk = @kpk";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@kpk", request.Kpk);
                        int exists = (int)await checkCmd.ExecuteScalarAsync();
                        if (exists > 0)
                        {
                            throw new Exception("KPK already exists");
                        }
                    }

                    // Insert
                    string insertQuery = @"
                        INSERT INTO [Scrap].[dbo].[user] 
                            ([kpk], [name], [role_id], [facility], [TC], [ScrapCodeResponsible])
                        VALUES 
                            (@kpk, @name, @roleId, @facility, @tc, @ScrapCodeResponsible);
                    ";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@kpk", request.Kpk);
                        cmd.Parameters.AddWithValue("@name", request.Name);
                        cmd.Parameters.AddWithValue("@roleId", request.RoleId);
                        cmd.Parameters.AddWithValue("@facility", request.Facility);
                        cmd.Parameters.AddWithValue("@tc", request.TC);

                        object scrapCodeValue;
                        if (string.IsNullOrWhiteSpace(request.ScrapCodeResponsible))
                            scrapCodeValue = DBNull.Value;
                        else
                            scrapCodeValue = request.ScrapCodeResponsible;

                        cmd.Parameters.AddWithValue("@ScrapCodeResponsible", scrapCodeValue);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error inserting user: {ex.Message}");
                throw;
            }
        }

        public bool UpdateUser(UpdateUserRequest request)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                var updates = new List<string>();
                var parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter("@kpk", request.Kpk));

                if (request.RoleId != null)
                {
                    updates.Add("[role_id] = @roleId");
                    parameters.Add(new SqlParameter("@roleId", request.RoleId));
                }

                if (request.Facility != null)
                {
                    updates.Add("[facility] = @facility");
                    parameters.Add(new SqlParameter("@facility", string.Join(",", request.Facility)));
                }

                if (request.IsActive.HasValue)
                {
                    updates.Add("[is_active] = @isActive");
                    parameters.Add(new SqlParameter("@isActive", request.IsActive.Value));
                }

                if (request.TC != null)
                {
                    updates.Add("[TC] = @tc");
                    parameters.Add(new SqlParameter("@tc", string.Join(",", request.TC)));
                }
                if (request.ScrapCodeResponsible != null)
                {
                    updates.Add("[ScrapCodeResponsible] = @ScrapCodeResponsible");
                    parameters.Add(new SqlParameter("@ScrapCodeResponsible", string.Join(",", request.ScrapCodeResponsible)));
                }

                if (!updates.Any())
                {
                    return false; 
                }

                string sql = $@"
                UPDATE [Scrap].[dbo].[user]
                SET {string.Join(", ", updates)}
                WHERE [kpk] = @kpk";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }
        public async Task<bool> AddOrReactivateUserAsync(
            UserUpsertRequest request,
            string addedByKpk)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // cek user
                string checkQuery = @"
            SELECT is_active 
            FROM [Scrap].[dbo].[user] 
            WHERE kpk = @kpk
        ";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@kpk", request.Kpk);
                    var result = await checkCmd.ExecuteScalarAsync();

                    // USER SUDAH ADA
                    if (result != null)
                    {
                        bool isActive = Convert.ToBoolean(result);

                        if (!isActive)
                        {
                            string updateQuery = @"
                        UPDATE [Scrap].[dbo].[user]
                        SET is_active = 1
                        WHERE kpk = @kpk
                    ";

                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@kpk", request.Kpk);
                                var updated = await updateCmd.ExecuteNonQueryAsync() > 0;

                                if (updated)
                                {
                                    await InsertUserAddHistoryAsync(
                                        conn,
                                        addedByKpk,
                                        request.Kpk
                                    );
                                }

                                return updated;
                            }
                        }

                        // sudah aktif
                        return false;
                    }
                }

                // USER BARU → INSERT
                string insertQuery = @"
            INSERT INTO [Scrap].[dbo].[user]
                (kpk, name, role_id, facility, TC, ScrapCodeResponsible, is_active)
            VALUES
                (@kpk, @name, @roleId, @facility, @tc, @scrap, 1)
        ";

                using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@kpk", request.Kpk);
                    insertCmd.Parameters.AddWithValue("@name", request.Name);
                    insertCmd.Parameters.AddWithValue("@roleId", request.RoleId);
                    insertCmd.Parameters.AddWithValue("@facility", request.Facility);
                    insertCmd.Parameters.AddWithValue("@tc", request.TC);
                    insertCmd.Parameters.AddWithValue(
                        "@scrap",
                        (object)request.ScrapCodeResponsible ?? DBNull.Value
                    );

                    var inserted = await insertCmd.ExecuteNonQueryAsync() > 0;

                    if (inserted)
                    {
                        await InsertUserAddHistoryAsync(
                            conn,
                            addedByKpk,
                            request.Kpk
                        );
                    }

                    return inserted;
                }
            }
        }


        public async Task InsertUserAddHistoryAsync(
    SqlConnection conn,
    string addedByKpk,
    string addedUserKpk)
        {
            string insertHistoryQuery = @"
                INSERT INTO [Scrap].[dbo].[user_add_history]
                    (added_by_kpk, added_user_kpk)
                VALUES
                    (@addedBy, @addedUser)
            ";

            using (SqlCommand cmd = new SqlCommand(insertHistoryQuery, conn))
            {
                cmd.Parameters.AddWithValue("@addedBy", addedByKpk);
                cmd.Parameters.AddWithValue("@addedUser", addedUserKpk);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public List<getPartMasterModelUpdated> getPartMasterUpdated(string data, string tc, bool exact)
        {
            List<getPartMasterModelUpdated> partMaster = new List<getPartMasterModelUpdated>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string ftypitFilter = "";
                string typeitFilter = "";
                string prcpitFilter = "";


                // Load filtering rules
                string filterQuery = @"SELECT FtypitFilter, TypeitFilter,PrcpitFilter
                               FROM FilteringTCRules WHERE TC = @tc";

                using (SqlCommand filterCmd = new SqlCommand(filterQuery, sqlconn))
                {
                    filterCmd.Parameters.AddWithValue("@tc", tc);
                    using (SqlDataReader fr = filterCmd.ExecuteReader())
                    {
                        if (fr.Read())
                        {
                            ftypitFilter = fr["FtypitFilter"] as string ?? "";
                            typeitFilter = fr["TypeitFilter"] as string ?? "";
                            prcpitFilter = fr["PrcpitFilter"] as string ?? "";
                        }
                    }
                }

                // Exact vs Like WHERE
                string whereClause = exact
                    ? "WHERE (LTRIM(RTRIM(toynit)) + '-' + LTRIM(RTRIM(partit))) = @data"
                    : "WHERE (LTRIM(RTRIM(toynit)) + '-' + LTRIM(RTRIM(partit))) LIKE @data + '%'";

                string query = $@"
                    SELECT DISTINCT 
                        LTRIM(RTRIM(toynit)) AS toynit,
                        LTRIM(RTRIM(partit)) AS partit,
                        LTRIM(RTRIM(desxit)) AS desxit,
                        baspit,
                        LTRIM(RTRIM(measit)) AS measit,
                        LTRIM(RTRIM(prcpit)) AS prcpit,
                        LTRIM(RTRIM(ftypit)) AS ftypit,
                        LTRIM(RTRIM([commit])) AS commit_it,
                        LTRIM(RTRIM(typeit)) AS typeit,
                        LTRIM(RTRIM(planit)) AS planit,
                        LTRIM(RTRIM(cmidit)) AS cmidit
                    FROM Scrap.dbo.item_master
                    {whereClause}
                ";

                // Filtering rules ()
                if (!string.IsNullOrWhiteSpace(ftypitFilter))
                {
                    var filters = ftypitFilter.Split(',').Select(f => $"'{f.Trim()}'");
                    query += $" AND ftypit IN ({string.Join(",", filters)})";
                }

                if (!string.IsNullOrWhiteSpace(typeitFilter))
                {
                    var filters = typeitFilter.Split(',').Select(f => $"'{f.Trim()}'");
                    query += $" AND typeit IN ({string.Join(",", filters)})";
                }
                if (!string.IsNullOrWhiteSpace(prcpitFilter))
                {
                    var filters = prcpitFilter
                        .Split(',')
                        .Select(f => $"'{f.Trim()}'");

                    query += $" AND prcpit IN ({string.Join(",", filters)})";
                }


                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@data", data);

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            partMaster.Add(new getPartMasterModelUpdated
                            {
                                ToyNum = r["toynit"] as string,
                                PartNum = r["partit"] as string,
                                Description = r["desxit"] as string,
                                Measurement = r["measit"] as string,
                                ProcessPoint = r["prcpit"] as string,
                                ftypit = r["ftypit"] as string,
                                typeit = r["typeit"] as string,
                                commit = r["commit_it"] as string,
                                BasePrice = r.IsDBNull(r.GetOrdinal("baspit"))
                                    ? (decimal?)null
                                    : Convert.ToDecimal(r["baspit"]),
                                planit = r["planit"] as string,
                                cmidit = r["cmidit"] as string
                            });
                        }
                    }
                }
            }

            return partMaster;
        }

        public List<TcAndTypeMaster> getTcAndType()
        {
            List<TcAndTypeMaster> tcAndTypes = new List<TcAndTypeMaster>();
            string query = "SELECT [TC], [Type], [Description], [ShowScrapCode],[Facility] FROM [Scrap].[dbo].[TcAndType]";

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TcAndTypeMaster tcAndType = new TcAndTypeMaster
                            {
                                TC = reader["TC"] != DBNull.Value ? Convert.ToInt32(reader["TC"]) : 0,
                                Type = reader["Type"] != DBNull.Value ? reader["Type"].ToString() : string.Empty,
                                TcDescription = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                ShowScrapCode = reader["ShowScrapCode"] != DBNull.Value ? reader["ShowScrapCode"].ToString() : string.Empty,
                                Facility = reader["Facility"]?.ToString()
                            };

                            tcAndTypes.Add(tcAndType);
                        }
                    }
                }
            }
            return tcAndTypes;
        }
        public List<TcAndTypeMaster> getTcAndTypeByFacility(string facility)
        {
            List<TcAndTypeMaster> list = new List<TcAndTypeMaster>();

            string query = @"
        SELECT [TC], [Type], [Description], [ShowScrapCode], [Facility]
        FROM [Scrap].[dbo].[TcAndType]
        WHERE
            @Facility IS NULL
            OR Facility IS NULL
            OR ',' + Facility + ',' LIKE '%,' + @Facility + ',%'
            ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@Facility",
                        string.IsNullOrEmpty(facility) ? (object)DBNull.Value : facility
                    );

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TcAndTypeMaster
                            {
                                TC = Convert.ToInt32(reader["TC"]),
                                Type = reader["Type"]?.ToString(),
                                TcDescription = reader["Description"]?.ToString(),
                                ShowScrapCode = reader["ShowScrapCode"]?.ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }

        public List<ScrapCode> getScrapCodeByLocation(string location)
        {
            List<ScrapCode> scrapCodeList = new List<ScrapCode>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                string query = "";

                if (string.IsNullOrEmpty(location))
                {
                    query = "SELECT [Name], [Code], [Location] FROM [Scrap].[dbo].[scrap_code]";
                }
                else
                {
                    query = @"
                SELECT [Name], [Code], [Location]
                FROM [Scrap].[dbo].[scrap_code]
                WHERE Location LIKE @location OR Location LIKE @locationWithComma";
                }

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    if (!string.IsNullOrEmpty(location))
                    {
                        cmd.Parameters.AddWithValue("@location", "%" + location + "%");
                        cmd.Parameters.AddWithValue("@locationWithComma", "%, " + location + "%");
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ScrapCode scrapCode = new ScrapCode
                            {
                                Name = reader["Name"].ToString(),
                                Code = reader["Code"].ToString(),
                                Location = reader["Location"].ToString()
                            };

                            scrapCodeList.Add(scrapCode);
                        }
                    }
                }
            }

            return scrapCodeList;
        }

        public List<ScrapCodeRemarkModel> GetScrapCodeRemarks()
        {
            List<ScrapCodeRemarkModel> remarksList = new List<ScrapCodeRemarkModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();

                string query = @"
                SELECT [IdRemarks], [Remarks], [ScrapCode]
                FROM [Scrap].[dbo].[scrap_code_remarks]
                WHERE IsDeleted = 0";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ScrapCodeRemarkModel remark = new ScrapCodeRemarkModel
                            {
                                IdRemarks = Convert.ToInt32(reader["IdRemarks"]),
                                Remarks = reader["Remarks"].ToString(),
                                ScrapCode = reader["ScrapCode"].ToString()
                            };

                            remarksList.Add(remark);
                        }
                    }
                }
            }

            return remarksList;
        }

        public List<WorkCenterModel> GetWorkCenters(string facility = null)
        {
            var workCenters = new List<WorkCenterModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            SELECT [id_workcenter], [Facility], [WC], [Description]
            FROM [Scrap].[dbo].[WorkCenters]
            WHERE (@facility IS NULL OR Facility = @facility)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@facility", (object)facility ?? DBNull.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            workCenters.Add(new WorkCenterModel
                            {
                                Id_WorkCenter = reader.GetInt32(reader.GetOrdinal("id_workcenter")),
                                Facility = reader["Facility"] as string ?? "",
                                WC = reader.GetInt32(reader.GetOrdinal("WC")),
                                Description = reader["Description"] as string ?? ""
                            });
                        }
                    }
                }
            }

            return workCenters;
        }

        public string InsertScrapMaster(ScrapMasterModel model)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

               
                int newId = 1;
                string getMaxIdQuery = "SELECT ISNULL(MAX(CAST(IdScrap AS INT)), 0) + 1 FROM scrap_master";
                using (SqlCommand cmd = new SqlCommand(getMaxIdQuery, conn))
                {
                    newId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                
                string formattedId = newId.ToString("D6");

                string insertQuery = @"
                INSERT INTO scrap_master
                (IdScrap, Facility, Type, TC, InitiatorKpk, isSubmitted, ScrapCode, CreatedDate, isDeleted, TCType, CurrentStatus, WC, Type_ID, SpecialCodeRemarks,specialCodeTcCompanion)
                VALUES (@IdScrap, @Facility, @Type, @TC, @InitiatorKpk, @isSubmitted, @ScrapCode, @CreatedDate, @isDeleted, @TCType, @CurrentStatus, @WC, @Type_ID,@SpecialCodeRemarks, @specialCodeTcCompanion)";


                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@IdScrap", formattedId);
                    cmd.Parameters.AddWithValue("@Facility", model.Facility ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Type", model.Type ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TC", model.TC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InitiatorKpk", model.InitiatorKpk ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@isSubmitted", model.isSubmitted == 0 ? 1 : model.isSubmitted);
                    cmd.Parameters.AddWithValue("@ScrapCode", model.ScrapCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@isDeleted", 0);
                    cmd.Parameters.AddWithValue("@TCType", model.TCType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentStatus", 1);
                    cmd.Parameters.AddWithValue("@WC", model.WC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Type_ID", model.Type_ID.HasValue ? (object)model.Type_ID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue(
                        "@SpecialCodeRemarks",
                        string.IsNullOrWhiteSpace(model.SpecialCodeRemarks)
                            ? (object)DBNull.Value
                            : model.SpecialCodeRemarks
                    );
                    cmd.Parameters.AddWithValue("@specialCodeTcCompanion", model.SpecialCodeTcCompanion ?? (object)DBNull.Value);




                    cmd.ExecuteNonQuery();
                }

                return formattedId;
            }
        }



        public void InsertScrapPart(ScrapPartModel model)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

               
                int newIdPart = 1;
                string getMaxIdQuery = "SELECT ISNULL(MAX(IdPart), 0) + 1 FROM scrap_part";
                using (SqlCommand cmd = new SqlCommand(getMaxIdQuery, conn))
                {
                    newIdPart = Convert.ToInt32(cmd.ExecuteScalar());
                }

                string insertQuery = @"
            INSERT INTO scrap_part
                (IdScrap,
                 PartNum,
                 Description,
                 Qty,
                 Value,
                 Remarks,
                 IdPart,
                 ProcessPoint,
                 ftypit,
                 typeit,
                 CurrentStatus,
                 RnNumber,
                 RespCode,
                 measit,
                 planit,
                 cmidit,
                 baspit,
                [commit],
                LeaderKPK,
                [specialcodeRemarksParts])
            VALUES
                (@IdScrap,
                 @PartNum,
                 @Description,
                 @Qty,
                 @Value,
                 @Remarks,
                 @IdPart,
                 @ProcessPoint,
                 @Ftypit,
                 @Typeit,
                 @CurrentStatus,
                 @RnNumber,
                 @RespCode,
                 @measit,
                 @planit,
                 @cmidit,
                 @baspit,
                @commit,
                @LeaderKPK,
                @specialcodeRemarksParts)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@IdScrap", model.IdScrap);
                    cmd.Parameters.AddWithValue("@PartNum", model.PartNum ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", model.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Qty", model.Qty);
                    cmd.Parameters.AddWithValue("@Value", model.Value);
                    cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdPart", newIdPart); // auto increment manual
                    cmd.Parameters.AddWithValue("@ProcessPoint", model.ProcessPoint ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Ftypit", model.Ftypit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Typeit", model.Typeit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CurrentStatus", model.CurrentStatus == 0 ? 1 : model.CurrentStatus);
                    cmd.Parameters.AddWithValue("@RnNumber", model.RnNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RespCode", model.RespCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@measit", model.Measit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@planit", model.Planit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@cmidit", model.Cmidit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@baspit", model.BasePrice ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@commit", model.Commit ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LeaderKPK", model.LeaderKPK ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@specialcodeRemarksParts", model.SpecialcodeRemarksParts ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int InsertCentralizedSourceData(CentralizedSourceDataModel data)
        {
            int newId = 0;


            string query = @"
        INSERT INTO [CentralizedNotification].[dbo].[Centralized_SourceData]
        (
            Centralized_SystemList_ID,
            Centralized_SourceData_TableName,
            Centralized_SourceData_Master_ID,
            Centralized_SourceData_Master_Title,
            Centralized_SourceData_Master_Desc,
            Centralized_SourceData_Master_Status,
            Centralized_SourceData_Master_CreatedDate
        )
        OUTPUT INSERTED.Centralized_SourceData_ID
        VALUES
        (
            @Centralized_SystemList_ID,
            @Centralized_SourceData_TableName,
            @Centralized_SourceData_Master_ID,
            @Centralized_SourceData_Master_Title,
            @Centralized_SourceData_Master_Desc,
            @Centralized_SourceData_Master_Status,
            @Centralized_SourceData_Master_CreatedDate
        )";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@Centralized_SystemList_ID", data.Centralized_SystemList_ID);
                    cmd.Parameters.AddWithValue("@Centralized_SourceData_TableName", data.Centralized_SourceData_TableName);
                    cmd.Parameters.AddWithValue("@Centralized_SourceData_Master_ID", data.Centralized_SourceData_Master_ID);
                    cmd.Parameters.AddWithValue("@Centralized_SourceData_Master_Title", data.Centralized_SourceData_Master_Title);
                    cmd.Parameters.AddWithValue("@Centralized_SourceData_Master_Desc", data.Centralized_SourceData_Master_Desc);
                    cmd.Parameters.AddWithValue("@Centralized_SourceData_Master_Status", data.Centralized_SourceData_Master_Status);
                    cmd.Parameters.AddWithValue("@Centralized_SourceData_Master_CreatedDate", data.Centralized_SourceData_Master_CreatedDate);

                    conn.Open();
                    newId = Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch (SqlException ex)
                {
                   
                    foreach (SqlError err in ex.Errors)
                    {
                        Debug.Print($"Line {err.LineNumber}: {err.Message}");
                    }
                    throw;
                }
            }


            return newId; // Kembalikan ID baru
        }

        public void InsertCentralizedInitiator(CentralizedInitiator model)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"INSERT INTO Centralized_Initiator 
                (Centralized_SourceData_ID, Centralized_Initiator_KPK) 
                VALUES (@SourceDataID, @KPK)", conn))
                {
                    cmd.Parameters.AddWithValue("@SourceDataID", model.Centralized_SourceData_ID);
                    cmd.Parameters.AddWithValue("@KPK", model.Centralized_Initiator_KPK ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public async Task<int> InsertApprovalListAsync(CentralizedApprovalListModel model)
        {
            string query = @"INSERT INTO [dbo].[Centralized_ApprovalList]
        ([Centralized_SourceData_ID],
         [Centralized_ApprovalList_Step],
         [Centralized_StatusList_ID],
         [Centralized_ApprovalList_Date],
         [Centralized_ApprovalList_Link],
         [Centralized_ApprovalList_KpkApproval],
         [Centralized_ApprovalList_Base64])
        VALUES
        (@Centralized_SourceData_ID,
         @Centralized_ApprovalList_Step,
         @Centralized_StatusList_ID,
         @Centralized_ApprovalList_Date,
         @Centralized_ApprovalList_Link,
         @Centralized_ApprovalList_KpkApproval,
         @Centralized_ApprovalList_Base64);
        SELECT SCOPE_IDENTITY();";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Centralized_SourceData_ID", model.Centralized_SourceData_ID);
                cmd.Parameters.AddWithValue("@Centralized_ApprovalList_Step", model.Centralized_ApprovalList_Step);
                cmd.Parameters.AddWithValue("@Centralized_StatusList_ID", model.Centralized_StatusList_ID);
                cmd.Parameters.AddWithValue("@Centralized_ApprovalList_Date",
                    (object)model.Centralized_ApprovalList_Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Centralized_ApprovalList_Link",
                    (object)model.Centralized_ApprovalList_Link ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Centralized_ApprovalList_KpkApproval",
                    (object)model.Centralized_ApprovalList_KpkApproval ?? DBNull.Value);
                var paramBase64 = cmd.Parameters.Add("@Centralized_ApprovalList_Base64", SqlDbType.NVarChar, -1);
                paramBase64.Value = (object)model.Centralized_ApprovalList_Base64 ?? DBNull.Value;

                try
                {
                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    Debug.Print($"[InsertApprovalList] Step {model.Centralized_ApprovalList_Step} — Base64 {(model.Centralized_ApprovalList_Base64?.Length ?? 0)} chars");
                    return Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                   
                    if (ex.InnerException != null)
                        Debug.Print("Inner: " + ex.InnerException.Message);
                    throw;
                }
            }
        }






        public List<ScrapCodeSpecialCaseApprovalRequirement> GetAllSpecialCase()
        {
            var list = new List<ScrapCodeSpecialCaseApprovalRequirement>();

            string query = @"
                SELECT
                    [Id],
                    [ScrapCode],
                    [Role_Id],
                    [RequiredApproverCount],
                    [ScrapTcType],
                    [minValue],
                    [maxValue],
                    CAST([commit] AS VARCHAR(1)) AS [commit],
                    [Priority],
                    [TC]
                FROM [Scrap].[dbo].[ScrapCodeSpecialCaseApprovalRequirements]
                ORDER BY [Priority] ASC, [Id] ASC
            ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ScrapCodeSpecialCaseApprovalRequirement
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ScrapCode = reader["ScrapCode"] as string,
                            Role_Id = reader.GetInt32(reader.GetOrdinal("Role_Id")),
                            RequiredApproverCount = reader.GetInt32(reader.GetOrdinal("RequiredApproverCount")),
                            ScrapTcType = reader["ScrapTcType"] as string,
                            minValue = reader["minValue"] != DBNull.Value ? (int?)reader["minValue"] : null,
                            maxValue = reader["maxValue"] != DBNull.Value ? (int?)reader["maxValue"] : null,
                            commit = reader["commit"] as string,
                            PriorityScrapCase = !reader.IsDBNull(reader.GetOrdinal("Priority"))
                                    ? reader.GetInt32(reader.GetOrdinal("Priority"))
                                    : (int?)null,
                            Tc = reader["TC"] as string,

                        };

                        list.Add(item);
                    }
                }
            }

            return list;
        }

        public List<CentralizedSourceDataModel> GetSourceDataSystemList()
        {
            var list = new List<CentralizedSourceDataModel>();

            string query = @"
                SELECT 
                    [Centralized_SourceData_ID],
                    [Centralized_SystemList_ID],
                    [Centralized_SourceData_Master_ID],
                    [Centralized_SourceData_Master_Status],
                    [Centralized_SourceData_Master_CreatedDate]
                FROM [CentralizedNotification].[dbo].[Centralized_SourceData]
                WHERE [Centralized_SystemList_ID] = 2
                ORDER BY [Centralized_SourceData_ID] DESC
            ";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new CentralizedSourceDataModel
                        {
                            Centralized_SourceData_ID = reader.GetInt32(reader.GetOrdinal("Centralized_SourceData_ID")),
                            Centralized_SystemList_ID = reader.GetInt32(reader.GetOrdinal("Centralized_SystemList_ID")),
                            Centralized_SourceData_Master_ID = reader.GetInt32(reader.GetOrdinal("Centralized_SourceData_Master_ID")),
                            Centralized_SourceData_Master_Status = reader.GetInt32(reader.GetOrdinal("Centralized_SourceData_Master_Status")),
                            Centralized_SourceData_Master_CreatedDate = reader.GetDateTime(reader.GetOrdinal("Centralized_SourceData_Master_CreatedDate"))
                        };

                        list.Add(item);
                    }

                }
            }

            return list;
        }

        public async Task<List<CentralizedApprovalListModel>> GetApprovalListAsync()
        {
            var list = new List<CentralizedApprovalListModel>();

            string query = @"
        SELECT
            [Centralized_ApprovalList_ID],
            [Centralized_SourceData_ID],
            [Centralized_ApprovalList_Step],
            [Centralized_StatusList_ID],
            [Centralized_ApprovalList_Date],
            [Centralized_ApprovalList_KpkApproval]
        FROM [CentralizedNotification].[dbo].[Centralized_ApprovalList]
        ORDER BY [Centralized_ApprovalList_ID] DESC
    ";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new CentralizedApprovalListModel
                        {
                            Centralized_ApprovalList_ID = reader.GetInt32(reader.GetOrdinal("Centralized_ApprovalList_ID")),
                            Centralized_SourceData_ID = reader.GetInt32(reader.GetOrdinal("Centralized_SourceData_ID")),
                            Centralized_ApprovalList_Step = reader.GetInt32(reader.GetOrdinal("Centralized_ApprovalList_Step")),
                            Centralized_StatusList_ID = reader["Centralized_StatusList_ID"] != DBNull.Value
                                ? (int)reader["Centralized_StatusList_ID"]
                                : 0,
                            Centralized_ApprovalList_Date = reader["Centralized_ApprovalList_Date"] != DBNull.Value
                                ? (DateTime?)reader["Centralized_ApprovalList_Date"]
                                : null,
                            Centralized_ApprovalList_KpkApproval = reader["Centralized_ApprovalList_KpkApproval"] as string
                        };

                        list.Add(item);
                    }
                }
            }

            return list;
        }

        public async Task<List<FetchingScrapMasterModel>> GetScrapMasterAsync()
        {
            var list = new List<FetchingScrapMasterModel>();

            string query = @"
        SELECT
            [IdScrap],
            [Facility],
            [TC],
            [InitiatorKpk],
            [isSubmitted],
            [ScrapCode],
            [CreatedDate],
            [isDeleted],
            [DeletedAt],
            [CurrentStatus],
            [WC],
            [Type_ID],
            [SpecialCodeRemarks],
            [specialCodeTcCompanion]
        FROM [Scrap].[dbo].[scrap_master]
        ORDER BY [IdScrap] DESC
    ";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new FetchingScrapMasterModel
                        {
                            IdScrap = reader["IdScrap"].ToString(),
                            Facility = reader["Facility"] as string,
                            TC = reader["TC"] as string,
                            InitiatorKpk = reader["InitiatorKpk"] as string,
                            isSubmitted = reader["isSubmitted"] != DBNull.Value && (bool)reader["isSubmitted"],
                            ScrapCode = reader["ScrapCode"] as string,
                            CreatedDate = reader["CreatedDate"] != DBNull.Value ? (DateTime?)reader["CreatedDate"] : null,
                            isDeleted = reader["isDeleted"] != DBNull.Value && (bool)reader["isDeleted"],
                            DeletedAt = reader["DeletedAt"] != DBNull.Value ? (DateTime?)reader["DeletedAt"] : null,
                            CurrentStatus = reader["CurrentStatus"] != DBNull.Value ? (int?)reader["CurrentStatus"] : null,
                            WC = reader["WC"] as string,
                            Type_ID = reader["Type_ID"] != DBNull.Value ? Convert.ToInt32(reader["Type_ID"]) : 0,
                            SpecialCodeRemarks = reader["SpecialCodeRemarks"] as string,
                            SpecialCodeTcCompanion = reader["specialCodeTcCompanion"] as string
                        };

                        list.Add(item);
                    }
                }
            }

            return list;
        }
        public async Task<List<ScrapPartModel>> GetScrapPartsAllAsync()
        {
            var result = new List<ScrapPartModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT [IdScrap],
                   [PartNum],
                   [Description],
                   [Qty],
                   [Value],
                   [Remarks],
                   [measit],
                   [planit],
                   [cmidit],
                   [commit],
                   [baspit],
                   [RnNumber],
                   [CurrentStatus],
                   [LeaderKPK],
                   [keyin_datetime],
                   [specialcodeRemarksParts]
            FROM [Scrap].[dbo].[scrap_part]";

                 using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                 using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new ScrapPartModel
                        {
                            IdScrap = reader["IdScrap"].ToString(),
                            PartNum = reader["PartNum"]?.ToString(),
                            Description = reader["Description"]?.ToString(),
                            Qty = reader["Qty"] != DBNull.Value ? Convert.ToDecimal(reader["Qty"]) : 0m,
                            BasePrice = reader["baspit"] != DBNull.Value ? Convert.ToDecimal(reader["baspit"]) : 0,
                            Value = reader["Value"] != DBNull.Value ? Convert.ToDecimal(reader["Value"]) : 0,
                            Remarks = reader["Remarks"]?.ToString(),
                            Measit = reader["measit"]?.ToString(),
                            Planit = reader["planit"]?.ToString(),
                            Cmidit = reader["cmidit"]?.ToString(),
                            Commit = reader["commit"]?.ToString(),
                            RnNumber = reader["RnNumber"]?.ToString(),
                            CurrentStatus = reader["CurrentStatus"] != DBNull.Value ? Convert.ToInt32(reader["CurrentStatus"]) : 0,
                            LeaderKPK = reader["LeaderKPK"]?.ToString(),
                            KeyInDateTime = reader["keyin_datetime"] != DBNull.Value
                                ? (DateTime?)Convert.ToDateTime(reader["keyin_datetime"])
                                : null,
                            SpecialcodeRemarksParts = reader["specialcodeRemarksParts"]?.ToString()
                        });
                    }
                 }
            }

            return result;
        }
        public async Task<List<ScrapPartSummaryModel>> GetScrapPartsSummaryAsync()
        {
            var result = new List<ScrapPartSummaryModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT 
                IdScrap,
                SUM(Qty)   AS TotalQty,
                SUM(Value) AS TotalValue
            FROM [Scrap].[dbo].[scrap_part]
            GROUP BY IdScrap";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new ScrapPartSummaryModel
                        {
                            IdScrap = reader["IdScrap"].ToString(),
                            TotalQty = reader["TotalQty"] != DBNull.Value ? Convert.ToDecimal(reader["TotalQty"]) : 0m,
                            TotalValue = reader["TotalValue"] != DBNull.Value ? Convert.ToDecimal(reader["TotalValue"]) : 0
                        });
                    }
                }
            }

            return result;
        }


        public async Task<List<ScrapPartModel>> GetScrapPartsByScrapIdAsync(string idScrap)
        {
            var result = new List<ScrapPartModel>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT [IdScrap],
                   [PartNum],
                   [Description],
                   [Qty],
                   [Value],
                   [Measit],
                   [Commit],
                   [Planit],
                   [RnNumber],
                   [Remarks],
                   [CurrentStatus],
                   [LeaderKPK]
            FROM [Scrap].[dbo].[scrap_part]
            WHERE IdScrap = @idScrap";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@idScrap", idScrap);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new ScrapPartModel
                            {
                                IdScrap = reader["IdScrap"].ToString(),
                                PartNum = reader["PartNum"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                Qty = reader["Qty"] != DBNull.Value ? Convert.ToDecimal(reader["Qty"]) : 0m,
                                Value = reader["Value"] != DBNull.Value ? Convert.ToDecimal(reader["Value"]) : 0,
                                Measit = reader["Measit"]?.ToString(),
                                Planit = reader["Planit"]?.ToString(),
                                Commit = reader["Commit"]?.ToString(),
                                RnNumber = reader["RnNumber"]?.ToString(),
                                Remarks = reader["Remarks"]?.ToString(),
                                CurrentStatus = reader["CurrentStatus"] != DBNull.Value ? Convert.ToInt32(reader["CurrentStatus"]) : 0,
                                LeaderKPK = reader["LeaderKPK"]?.ToString()
                            });
                        }
                    }
                }
            }

            return result;
        }

        public bool UpdateMultipleApprovals(List<(int ApprovalListId, string NewKpk)> updates)
        {
            string query = @"UPDATE [CentralizedNotification].[dbo].[Centralized_ApprovalList]
                     SET [Centralized_ApprovalList_KpkApproval] = @NewKpk
                     WHERE [Centralized_ApprovalList_ID] = @ApprovalListId";

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                foreach (var upd in updates)
                {
                    using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                    {
                        cmd.Parameters.AddWithValue("@NewKpk", upd.NewKpk ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ApprovalListId", upd.ApprovalListId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return true;
        }


        public void UpdateScrapStatusBatch(List<string> scrapIds, int newStatus)
        {
            if (scrapIds == null || scrapIds.Count == 0) return;

            var ids = string.Join(",", scrapIds.Select(id => $"'{id}'"));

            var query = $@"
        UPDATE Scrap.dbo.scrap_part
        SET CurrentStatus = @newStatus
        WHERE IdScrap IN ({ids})";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@newStatus", newStatus);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }



        public bool UpdateSourceDataStatus(int sourceDataId, int newStatus)
        {
            string query = @"UPDATE [CentralizedNotification].[dbo].[Centralized_SourceData]
                     SET [Centralized_SourceData_Master_Status] = @NewStatus
                     WHERE [Centralized_SourceData_ID] = @SourceDataId";

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                    cmd.Parameters.AddWithValue("@SourceDataId", sourceDataId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<List<DisposalItem>> GetAllDisposalItemsAsync()
        {
            var items = new List<DisposalItem>();

            string query = @"
        SELECT [Disposal_ID], [Disposal_Code], [Disposal_Desc], [Disposal_Unit]
        FROM [Scrap].[dbo].[disposal_item]";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new DisposalItem
                        {
                            Disposal_ID = reader["Disposal_ID"] != DBNull.Value ? Convert.ToInt32(reader["Disposal_ID"]) : 0,
                            Disposal_Code = reader["Disposal_Code"]?.ToString(),
                            Disposal_Desc = reader["Disposal_Desc"]?.ToString(),
                            Disposal_Unit = reader["Disposal_Unit"]?.ToString()
                        });
                    }
                }
            }

            return items;
        }

        public bool InsertScrapDisposal(ScrapDisposalModel model)
        {
            string query = @"
            INSERT INTO [Scrap].[dbo].[scrap_disposal]
            ([Scrap_ID], [Disposal_ID], [Disposal_Quantity], [Disposal_Remarks], [Disposal_In], [Disposal_Out], [Disposal_B3], [Disposal_NonB3])
            VALUES (@Scrap_ID, @Disposal_ID, @Disposal_Quantity, @Disposal_Remarks, @Disposal_In, @Disposal_Out, @Disposal_B3, @Disposal_NonB3)";

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@Scrap_ID", model.Scrap_ID);
                    cmd.Parameters.AddWithValue("@Disposal_ID", model.Disposal_ID);
                    cmd.Parameters.AddWithValue("@Disposal_Quantity", model.Disposal_Quantity);
                    cmd.Parameters.AddWithValue("@Disposal_Remarks", (object)model.Disposal_Remarks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Disposal_In", model.Disposal_In);
                    cmd.Parameters.AddWithValue("@Disposal_Out", model.Disposal_Out);
                    cmd.Parameters.AddWithValue("@Disposal_B3", model.Disposal_B3);
                    cmd.Parameters.AddWithValue("@Disposal_NonB3", model.Disposal_NonB3);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        public async Task<List<ScrapDisposalModel>> GetScrapDisposalsByScrapIdAsync(string scrapId)
        {
            var result = new List<ScrapDisposalModel>();

            string query = @"
        SELECT 
            [Scrap_Disposal_ID],
            [Scrap_ID],
            [Disposal_ID],
            [Disposal_Quantity],
            [Disposal_Remarks],
            [Disposal_In],
            [Disposal_Out],
            [Disposal_B3],
            [Disposal_NonB3]
        FROM [Scrap].[dbo].[scrap_disposal]
        WHERE [Scrap_ID] = @ScrapID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ScrapID", scrapId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new ScrapDisposalModel
                            {
                                Scrap_Disposal_ID = reader["Scrap_Disposal_ID"] != DBNull.Value ? Convert.ToInt32(reader["Scrap_Disposal_ID"]) : 0,
                                Scrap_ID = reader["Scrap_ID"]?.ToString(),
                                Disposal_ID = reader["Disposal_ID"] != DBNull.Value ? Convert.ToInt32(reader["Disposal_ID"]) : 0,
                                Disposal_Quantity = reader["Disposal_Quantity"] != DBNull.Value ? Convert.ToDecimal(reader["Disposal_Quantity"]) : 0,
                                Disposal_Remarks = reader["Disposal_Remarks"]?.ToString(),
                                Disposal_In = reader["Disposal_In"] != DBNull.Value ? Convert.ToBoolean(reader["Disposal_In"]) : false,
                                Disposal_Out = reader["Disposal_Out"] != DBNull.Value ? Convert.ToBoolean(reader["Disposal_Out"]) : false,
                                Disposal_B3 = reader["Disposal_B3"] != DBNull.Value ? Convert.ToBoolean(reader["Disposal_B3"]) : false,
                                Disposal_NonB3 = reader["Disposal_NonB3"] != DBNull.Value ? Convert.ToBoolean(reader["Disposal_NonB3"]) : false
                            });
                        }
                    }
                }
            }

            return result;
        }

        public List<TypeScrapModel> GetAllTypeScrap()
        {
            var list = new List<TypeScrapModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT [Type_ID], [Type_Desc], [IsDelete]
                                 FROM [Scrap].[dbo].[TypeScrap]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TypeScrapModel
                        {
                            Type_ID = (int)reader["Type_ID"],
                            Type_Desc = reader["Type_Desc"].ToString(),
                            IsDelete = (bool)reader["IsDelete"]
                        });
                    }
                }
            }

            return list;
        }
        public bool SubmitScrapCode(ScrapCode model, string createdByKpk, string createdByName)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                sqlconn.Open();
                using (SqlTransaction transaction = sqlconn.BeginTransaction())
                {
                    try
                    {
                        string application = (model.Application ?? "SCRAP").Trim().ToUpperInvariant();
                        string description = string.IsNullOrWhiteSpace(model.Description) ? model.Name : model.Description;
                        string facility = string.IsNullOrWhiteSpace(model.Facility) ? model.Location : model.Facility;

                        if (application == "SCRAP")
                        {
                            string scrapCodeQuery = @"INSERT INTO [Scrap].[dbo].[scrap_code]
                                    ([Name], [Code], [Location])
                                    VALUES (@name, @code, @location)";

                            using (SqlCommand cmd = new SqlCommand(scrapCodeQuery, sqlconn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@name", string.IsNullOrEmpty(description) ? DBNull.Value : (object)description);
                                cmd.Parameters.AddWithValue("@code", string.IsNullOrEmpty(model.Code) ? DBNull.Value : (object)model.Code);
                                cmd.Parameters.AddWithValue("@location", string.IsNullOrEmpty(facility) ? DBNull.Value : (object)facility);

                                int result = cmd.ExecuteNonQuery();
                                if (result != 1)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        else if (application == "PIA")
                        {
                            string piaCodeQuery = @"INSERT INTO [Scrap].[dbo].[pia_code]
                            ([facility], [code], [area], [description], [created_at], [is_deleted])
                            VALUES (@facility, @code, @area, @description, GETDATE(), 0)";

                            using (SqlCommand cmd = new SqlCommand(piaCodeQuery, sqlconn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@facility", string.IsNullOrWhiteSpace(facility) ? DBNull.Value : (object)facility);
                                cmd.Parameters.AddWithValue("@code", string.IsNullOrWhiteSpace(model.Code) ? DBNull.Value : (object)model.Code);
                                cmd.Parameters.AddWithValue("@area", string.IsNullOrWhiteSpace(model.Area) ? DBNull.Value : (object)model.Area);
                                cmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : (object)description);

                                int result = cmd.ExecuteNonQuery();
                                if (result != 1)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        else if (application == "TPR")
                        {
                            string tprCodeQuery = @"INSERT INTO [Scrap].[dbo].[tpr_code]
                            ([facility], [code], [area], [description], [created_by_kpk], [created_by_name], [created_at], [is_deleted])
                            VALUES (@facility, @code, @area, @description, @createdByKpk, @createdByName, GETDATE(), 0)";

                            using (SqlCommand cmd = new SqlCommand(tprCodeQuery, sqlconn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@facility", string.IsNullOrWhiteSpace(facility) ? DBNull.Value : (object)facility);
                                cmd.Parameters.AddWithValue("@code", string.IsNullOrWhiteSpace(model.Code) ? DBNull.Value : (object)model.Code);
                                cmd.Parameters.AddWithValue("@area", string.IsNullOrWhiteSpace(model.Area) ? DBNull.Value : (object)model.Area);
                                cmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : (object)description);

                                if (int.TryParse(createdByKpk, out int parsedKpk))
                                {
                                    cmd.Parameters.AddWithValue("@createdByKpk", parsedKpk);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@createdByKpk", DBNull.Value);
                                }

                                cmd.Parameters.AddWithValue("@createdByName", string.IsNullOrWhiteSpace(createdByName) ? DBNull.Value : (object)createdByName);

                                int result = cmd.ExecuteNonQuery();
                                if (result != 1)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            return false;
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                        }

                        throw;
                    }
                }
            }
        }



        public bool SubmitScrapCodeRemark(ScrapCodeRemarkModel model)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlconn.Open();

                    // Get new IdRemarks
                    string getMaxIdQuery = "SELECT ISNULL(MAX(IdRemarks), 0) FROM [Scrap].[dbo].[scrap_code_remarks]";
                    int newId = (int)new SqlCommand(getMaxIdQuery, sqlconn).ExecuteScalar() + 1;

                    string insertQuery = @"INSERT INTO [Scrap].[dbo].[scrap_code_remarks] 
                                   (IdRemarks, Remarks, ScrapCode, IsDeleted) 
                                   VALUES (@IdRemarks, @Remarks, @ScrapCode, 0)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, sqlconn))
                    {
                        cmd.Parameters.AddWithValue("@IdRemarks", newId);
                        cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(model.Remarks) ? DBNull.Value : (object)model.Remarks);
                        cmd.Parameters.AddWithValue("@ScrapCode", string.IsNullOrEmpty(model.ScrapCode) ? DBNull.Value : (object)model.ScrapCode);

                        int result = cmd.ExecuteNonQuery();
                        return result == 1;
                    }
                }
                catch (Exception ex)
                {
                    
                    return false;
                }
            }
        }


    
        public bool DeleteRemark(int idRemarks)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE [Scrap].[dbo].[scrap_code_remarks] SET IsDeleted = 1 WHERE IdRemarks = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", idRemarks);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }
        public bool DeleteScrapCode(int idRemarks)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlconn.Open();

                    string query = @"DELETE FROM [Scrap].[dbo].[scrap_code] WHERE [IdRemarks] = @idRemarks";

                    using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                    {
                        cmd.Parameters.AddWithValue("@idRemarks", idRemarks);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch
                {
                    throw;
                }
            }
        }


        public List<ScrapCode> GetAllScrapCodes()
        {
            var result = new List<ScrapCode>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlconn.Open();

                    string query = @"
                        SELECT [Name], [Code], [Location], [IdRemarks]
                        FROM [Scrap].[dbo].[scrap_code]";

                    using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ScrapCode
                            {
                                Name = reader["Name"]?.ToString(),
                                Code = reader["Code"]?.ToString(),
                                Location = reader["Location"]?.ToString(),
                                Application = "SCRAP",
                                IdRemarks = reader["IdRemarks"] != DBNull.Value ? (int)reader["IdRemarks"] : 0
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Debug.Print($"SQL ERROR {ex.Number}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.Print($"General ERROR: {ex.Message}");
                }
            }

            return result;
        }

        public List<ScrapCode> GetAllPiaCodes()
        {
            var result = new List<ScrapCode>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlconn.Open();

                    string query = @"
                        SELECT ROW_NUMBER() OVER(ORDER BY [code]) AS [TempId], [facility], [code], [area], [description]
                        FROM [Scrap].[dbo].[pia_code]
                        WHERE ISNULL([is_deleted], 0) = 0";

                    using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ScrapCode
                            {
                                Name = reader["description"]?.ToString(),
                                Code = reader["code"]?.ToString(),
                                Location = reader["facility"]?.ToString(),
                                Area = reader["area"]?.ToString(),
                                Application = "PIA",
                                IdRemarks = reader["TempId"] != DBNull.Value ? Convert.ToInt32(reader["TempId"]) : 0
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Debug.Print($"SQL ERROR {ex.Number}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.Print($"General ERROR: {ex.Message}");
                }
            }

            return result;
        }

        public List<ScrapCode> GetAllTprCodes()
        {
            var result = new List<ScrapCode>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlconn.Open();

                    string query = @"
                        SELECT ROW_NUMBER() OVER(ORDER BY [code]) AS [TempId], [facility], [code], [area], [description]
                        FROM [Scrap].[dbo].[tpr_code]
                        WHERE ISNULL([is_deleted], 0) = 0";

                    using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ScrapCode
                            {
                                Name = reader["description"]?.ToString(),
                                Code = reader["code"]?.ToString(),
                                Location = reader["facility"]?.ToString(),
                                Area = reader["area"]?.ToString(),
                                Application = "TPR",
                                IdRemarks = reader["TempId"] != DBNull.Value ? Convert.ToInt32(reader["TempId"]) : 0
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Debug.Print($"SQL ERROR {ex.Number}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.Print($"General ERROR: {ex.Message}");
                }
            }

            return result;
        }
        public List<ScrapCodeSpecialCaseApprovalRequirement> GetAllScrapCodeSpecialApprovals()
        {
            var result = new List<ScrapCodeSpecialCaseApprovalRequirement>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlconn.Open();

                    string query = @"
                SELECT [Id]
                      ,[ScrapCode]
                      ,[Role_Id]
                      ,[RequiredApproverCount]
                      ,[ScrapTcType]
                      ,[minValue]
                      ,[maxValue]
                      ,[commit]
                      ,[Priority]
                FROM [Scrap].[dbo].[ScrapCodeSpecialCaseApprovalRequirements]";

                    using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new ScrapCodeSpecialCaseApprovalRequirement
                            {
                                Id = reader["Id"] != DBNull.Value ? (int)reader["Id"] : 0,
                                ScrapCode = reader["ScrapCode"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["ScrapCode"].ToString())
                                    ? reader["ScrapCode"].ToString()
                                    : "All Scrap Code",
                                Role_Id = reader["Role_Id"] != DBNull.Value ? (int)reader["Role_Id"] : 0,
                                RequiredApproverCount = reader["RequiredApproverCount"] != DBNull.Value ? (int)reader["RequiredApproverCount"] : 0,
                                ScrapTcType = reader["ScrapTcType"] != DBNull.Value
                                ? reader["ScrapTcType"].ToString()
                                : "All Type",

                                minValue = reader["minValue"] != DBNull.Value ? Convert.ToInt32(reader["minValue"]) : 0, // ✅ default 0
                                maxValue = reader["maxValue"] != DBNull.Value ? Convert.ToInt32(reader["maxValue"]) : 0, // ✅ default 0
                                commit = reader["commit"] != DBNull.Value ? reader["commit"].ToString() : "All Commit",
                                PriorityScrapCase = reader["Priority"] != DBNull.Value ? (int)reader["Priority"] : 0
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Debug.Print($"SQL ERROR {ex.Number}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.Print($"General ERROR: {ex.Message}");
                }
            }

            return result;
        }
        public async Task<List<TypeScrapModel>> GetTypeScrapAsync()
        {
            var list = new List<TypeScrapModel>();

            string query = @"
            SELECT [Type_ID],
                   [Type_Desc],
                   [IsDelete]
            FROM [Scrap].[dbo].[TypeScrap]
            ORDER BY [Type_ID] ASC
        ";

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new TypeScrapModel
                        {
                            Type_ID = reader["Type_ID"] != DBNull.Value ? Convert.ToInt32(reader["Type_ID"]) : 0,
                            Type_Desc = reader["Type_Desc"] as string,
                            IsDelete = reader["IsDelete"] != DBNull.Value && (bool)reader["IsDelete"]
                        };

                        list.Add(item);
                    }
                }
            }

            return list;
        }

        public async Task<int> InsertUserDelegateAsync(UserDelegate model)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            INSERT INTO [Scrap].[dbo].[user_delegate]
                (user_kpk, delegate_kpk, delegate_time)
            VALUES (@UserKpk, @DelegateKpk, @DelegateTime);

            SELECT SCOPE_IDENTITY();";


                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@UserKpk", model.UserKpk);
                    cmd.Parameters.AddWithValue("@DelegateKpk", model.DelegateKpk);
                    cmd.Parameters.AddWithValue("@DelegateTime", model.DelegateTime);

                    object result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<List<UserDelegate>> GetUserDelegatesAsync()
        {
            var delegates = new List<UserDelegate>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT [id],
                   [user_kpk],
                   [delegate_kpk],
                   [delegate_time]
            FROM [Scrap].[dbo].[user_delegate]
            ORDER BY id DESC;";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        delegates.Add(new UserDelegate
                        {
                            Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                            UserKpk = reader["user_kpk"]?.ToString() ?? string.Empty,
                            DelegateKpk = reader["delegate_kpk"]?.ToString() ?? string.Empty,
                            DelegateTime = reader["delegate_time"]?.ToString()
                        });
                    }
                }
            }

            return delegates;
        }

        public async Task<int> InsertDelegateApprovalRecordAsync(DelegateApprovalRecord model)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
        INSERT INTO [Scrap].[dbo].[Delegate_Approval_Record]
            (Centralized_ApprovalList_ID, Centralized_StatusList_ID, Delegate_ApprovalList_KpkApproval)
        VALUES (@ApprovalListId, @StatusListId, @DelegateKpk);

        SELECT SCOPE_IDENTITY();"; // biar dapat Id terakhir

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@ApprovalListId", model.Centralized_ApprovalList_ID);
                    cmd.Parameters.AddWithValue("@StatusListId", model.Centralized_StatusList_ID);
                    cmd.Parameters.AddWithValue("@DelegateKpk", model.Delegate_ApprovalList_KpkApproval);

                    object result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        public async Task<List<DelegateApprovalInfo>> GetDelegateApprovalInfosAsync()
        {
            var results = new List<DelegateApprovalInfo>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT Centralized_ApprovalList_ID, Delegate_ApprovalList_KpkApproval
            FROM [Scrap].[dbo].[Delegate_Approval_Record]";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new DelegateApprovalInfo
                            {
                                Centralized_ApprovalList_ID = Convert.ToInt32(reader["Centralized_ApprovalList_ID"]),
                                Delegate_ApprovalList_KpkApproval = reader["Delegate_ApprovalList_KpkApproval"].ToString()
                            });
                        }
                    }
                }
            }

            return results;
        }
        public async Task<bool> DeleteUserDelegateAsync(int id)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            DELETE FROM [Scrap].[dbo].[user_delegate]
            WHERE id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    // Return true if at least one row was deleted
                    return rowsAffected > 0;
                }
            }
        }
        public async Task<bool> UpdateApprovalListStatusAsync(int approvalListId, int status)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
    UPDATE [CentralizedNotification].[dbo].[Centralized_ApprovalList]
    SET 
        Centralized_StatusList_ID = @Status,
        Centralized_ApprovalList_Date = @Date,
        Centralized_ApprovalList_Base64=NULL
    WHERE Centralized_ApprovalList_ID = @ApprovalListID";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ApprovalListID", approvalListId);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
        }

        public async Task<string> GetNextPendingApprovalBase64Async(int sourceDataId)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT TOP 1 Centralized_ApprovalList_Base64
            FROM [CentralizedNotification].[dbo].[Centralized_ApprovalList]
            WHERE Centralized_SourceData_ID = @SourceDataID
              AND Centralized_StatusList_ID = 1
            ORDER BY Centralized_ApprovalList_Step ASC";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@SourceDataID", sourceDataId);
                    var result = await cmd.ExecuteScalarAsync();
                    return (result != null && result != DBNull.Value) ? result as string : null;
                }
            }
        }

        public async Task<List<int>> GetStatusesBySourceDataIdAsync(int sourceDataId)
        {
            var result = new List<int>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT Centralized_StatusList_ID 
            FROM [CentralizedNotification].[dbo].[Centralized_ApprovalList]
            WHERE Centralized_SourceData_ID = @SourceDataID";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@SourceDataID", sourceDataId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(reader.GetInt32(0));
                        }
                    }
                }
            }

            return result;
        }

        public async Task<bool> UpdateMasterStatusAsync(int sourceDataId, int newStatus)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            UPDATE [CentralizedNotification].[dbo].[Centralized_SourceData]
            SET Centralized_SourceData_Master_Status = @Status
            WHERE Centralized_SourceData_ID = @SourceDataID";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@SourceDataID", sourceDataId);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }
        public async Task<List<TCCompanion>> GetTCCompanionsAsync()
        {
            var list = new List<TCCompanion>();

            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {
                await sqlconn.OpenAsync();

                string query = @"
            SELECT
                   [Id],
                   [TC],
                   [Name],
                   [TypeTC],
                   [CreatedAt]
            FROM [Scrap].[dbo].[TCCompanion]
            ORDER BY Id DESC;
        ";

                using (SqlCommand cmd = new SqlCommand(query, sqlconn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new TCCompanion
                        {
                            Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                            TC = reader["TC"]?.ToString() ?? string.Empty,
                            Name = reader["Name"]?.ToString() ?? string.Empty,
                            TypeTC = reader["TypeTC"]?.ToString() ?? string.Empty,
                            CreatedAt = reader["CreatedAt"] != DBNull.Value
                                ? Convert.ToDateTime(reader["CreatedAt"])
                                : DateTime.MinValue
                        });
                    }
                }
            }

            return list;
        }

        public async Task RollbackCentralizedDataAsync(int centralizedId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                var queries = new[]
                {
                    "DELETE FROM [dbo].[Centralized_ApprovalList] WHERE Centralized_SourceData_ID = @Id",
                    "DELETE FROM [dbo].[Centralized_Initiator]   WHERE Centralized_SourceData_ID = @Id",
                    "DELETE FROM [CentralizedNotification].[dbo].[Centralized_SourceData] WHERE Centralized_SourceData_ID = @Id"
                };

                foreach (var q in queries)
                {
                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", centralizedId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task RollbackScrapDataAsync(string idScrap)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                var queries = new[]
                {
                    "DELETE FROM [dbo].[scrap_part]   WHERE IdScrap = @IdScrap",
                    "DELETE FROM [dbo].[scrap_master] WHERE IdScrap = @IdScrap"
                };

                foreach (var q in queries)
                {
                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdScrap", idScrap);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<bool> SoftDeleteScrapMasterAsync(string idScrap)
        {
            string query = @"
                UPDATE [Scrap].[dbo].[scrap_master]
                SET [isDeleted] = 1,
                    [DeletedAt] = GETDATE(),
                    [CurrentStatus] = 16
                WHERE [IdScrap] = @IdScrap";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdScrap", idScrap);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public bool InsertScrapCodeRemark(string scrapCode, string remarks)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Cek IdRemarks terakhir
                int nextId = 1;
                string maxQuery = "SELECT ISNULL(MAX(IdRemarks), 0) + 1 FROM [Scrap].[dbo].[scrap_code_remarks]";
                using (SqlCommand cmd = new SqlCommand(maxQuery, conn))
                    nextId = (int)cmd.ExecuteScalar();

                string insertQuery = @"INSERT INTO [Scrap].[dbo].[scrap_code_remarks] 
                               (IdRemarks, ScrapCode, Remarks) 
                               VALUES (@IdRemarks, @ScrapCode, @Remarks)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@IdRemarks", nextId);
                    cmd.Parameters.AddWithValue("@ScrapCode", scrapCode);
                    cmd.Parameters.AddWithValue("@Remarks", remarks);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool InsertPiaCodeRemark(int piaCode, string remarks)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string insertQuery = @"
            INSERT INTO [Scrap].[dbo].[pia_code_remarks] 
                (pia_code, remarks, created_at) 
            VALUES 
                (@PiaCode, @Remarks, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@PiaCode", piaCode);
                    cmd.Parameters.AddWithValue("@Remarks", remarks);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool InsertTprCodeRemark(string code, string remarks)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string insertQuery = @"INSERT INTO [Scrap].[dbo].[tpr_code_remarks] 
                               (Code, Remarks) 
                               VALUES (@Code, @Remarks)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@Remarks", remarks);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<ScrapCodeRemarkModel> GetPiaCodeRemarks()
        {
            var result = new List<ScrapCodeRemarkModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT r.id AS IdRemarks, r.remarks, p.code AS ScrapCode
            FROM [Scrap].[dbo].[pia_code_remarks] r
            INNER JOIN [Scrap].[dbo].[pia_code] p ON r.pia_code = p.id
            WHERE ISNULL(r.is_deleted, 0) = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ScrapCodeRemarkModel
                        {
                            IdRemarks = reader["IdRemarks"] != DBNull.Value ? Convert.ToInt32(reader["IdRemarks"]) : 0,
                            Remarks = reader["remarks"]?.ToString(),
                            ScrapCode = reader["ScrapCode"]?.ToString()
                        });
                    }
                }
            }
            return result;
        }

        public List<ScrapCodeRemarkModel> GetTprCodeRemarks()
        {
            var result = new List<ScrapCodeRemarkModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            SELECT r.id AS IdRemarks, r.remarks, t.code AS ScrapCode
            FROM [Scrap].[dbo].[tpr_code_remarks] r
            INNER JOIN [Scrap].[dbo].[tpr_code] t ON r.tpr_code = t.id
            WHERE ISNULL(r.is_deleted, 0) = 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ScrapCodeRemarkModel
                        {
                            IdRemarks = reader["IdRemarks"] != DBNull.Value ? Convert.ToInt32(reader["IdRemarks"]) : 0,
                            Remarks = reader["remarks"]?.ToString(),
                            ScrapCode = reader["ScrapCode"]?.ToString()
                        });
                    }
                }
            }
            return result;
        }

    }

}


